using System;
using UnityEditor;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// <c>Unice &gt; Editor de mapas</c>: pick a map, see it drawn (rooms, doors, furniture, spawn, entry) and drag it
    /// into shape. Edits happen on an in-memory copy (<see cref="Working"/>, deep-cloned via <see cref="MapEditorMath.Clone"/>)
    /// — the shared <see cref="Maps"/> instance the game reads is never mutated. Save writes it back to the map's
    /// <c>Map&lt;Name&gt;.cs</c> file through <see cref="MapSourceFiles.SaveOne"/>.
    /// Drawing/input is split out: <see cref="MapEditorCanvas"/> (left) and <see cref="MapEditorPropertiesPanel"/> (right).
    /// </summary>
    sealed class MapEditorWindow : EditorWindow
    {
        const float PropertiesWidth = 300;

        [SerializeField] string selectedMapId;

        internal MapDefinition Working;
        internal MapEditorSelection Selection = MapEditorSelection.None;
        internal bool Dirty;
        internal bool spaceHeld;
        string statusMessage;

        MapEditorUndoStack undo = new MapEditorUndoStack();
        readonly MapEditorCanvas canvas = new MapEditorCanvas();

        [MenuItem("Unice/Editor de mapas")]
        static void Open() => GetWindow<MapEditorWindow>("Editor de mapas");

        void OnEnable()
        {
            titleContent = new GUIContent("Editor de mapas");
            if (Working == null) LoadMap(string.IsNullOrEmpty(selectedMapId) ? Maps.AllIds[0] : selectedMapId);
        }

        void OnGUI()
        {
            HandleGlobalInput(Event.current);
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            var canvasRect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            canvas.Draw(canvasRect, this);
            EditorGUILayout.BeginVertical(GUILayout.Width(PropertiesWidth), GUILayout.ExpandHeight(true));
            MapEditorPropertiesPanel.Draw(this);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void HandleGlobalInput(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Space) spaceHeld = true;
                else if (e.control && e.keyCode == KeyCode.Z && !e.shift) { PerformUndo(); e.Use(); }
                else if (e.control && (e.keyCode == KeyCode.Y || (e.keyCode == KeyCode.Z && e.shift))) { PerformRedo(); e.Use(); }
            }
            else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Space) spaceHeld = false;
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            int index = Math.Max(0, Array.IndexOf(Maps.AllIds, selectedMapId));
            int newIndex = EditorGUILayout.Popup(index, Maps.AllIds, EditorStyles.toolbarPopup, GUILayout.Width(160));
            if (newIndex != index) SwitchMap(Maps.AllIds[newIndex]);

            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(!undo.CanUndo))
                if (GUILayout.Button("Desfazer", EditorStyles.toolbarButton, GUILayout.Width(80))) PerformUndo();
            using (new EditorGUI.DisabledScope(!undo.CanRedo))
                if (GUILayout.Button("Refazer", EditorStyles.toolbarButton, GUILayout.Width(80))) PerformRedo();
            GUILayout.Space(8);
            if (GUILayout.Button("Salvar", EditorStyles.toolbarButton, GUILayout.Width(80))) SaveCurrentMap();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage)) EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }

        void SwitchMap(string id)
        {
            if (Dirty && !EditorUtility.DisplayDialog("Trocar de mapa", "Há alterações não salvas no mapa atual. Trocar mesmo assim?", "Trocar", "Cancelar")) return;
            LoadMap(id);
        }

        void LoadMap(string id)
        {
            selectedMapId = id;
            Working = MapEditorMath.Clone(Maps.ById(id));
            Selection = MapEditorSelection.None;
            undo = new MapEditorUndoStack();
            Dirty = false;
            statusMessage = null;
            Repaint();
        }

        internal void SaveCurrentMap()
        {
            if (Working == null) return;
            var path = MapSourceFiles.SaveOne(selectedMapId, Working);
            if (path == null) { statusMessage = "Não consegui salvar: mapa não registrado em MapSourceFiles."; return; }
            Dirty = false;
            statusMessage = "Salvo em " + path + ".";
            Debug.Log("[Unice] Mapa \"" + selectedMapId + "\" salvo em " + path + ".");
        }

        /// <summary>Call right before mutating <see cref="Working"/> (a drag's first move, or a properties-panel edit).</summary>
        internal void RecordUndo()
        {
            undo.RecordSnapshot(Working);
            Dirty = true;
            statusMessage = null;
        }

        internal void PerformUndo()
        {
            var previous = undo.Undo(Working);
            if (previous == null) return;
            Working = previous; Selection = MapEditorSelection.None; Dirty = true; Repaint();
        }

        internal void PerformRedo()
        {
            var next = undo.Redo(Working);
            if (next == null) return;
            Working = next; Selection = MapEditorSelection.None; Dirty = true; Repaint();
        }

        internal void AddRoom()
        {
            RecordUndo();
            var center = canvas.ViewCenterWorld;
            var room = new Room("sala" + (Working.rooms.Length + 1),
                MapEditorMath.SnapToGrid(center.x - 100, 8), MapEditorMath.SnapToGrid(center.y - 75, 8), 200, 150);
            Working.rooms = Append(Working.rooms, room);
            Selection = new MapEditorSelection(MapEditorSelectionKind.Room, Working.rooms.Length - 1);
        }

        internal void AddFurniture()
        {
            RecordUndo();
            var center = canvas.ViewCenterWorld;
            var prop = new MapProp("novo", MapEditorMath.SnapToGrid(center.x - 20, 8), MapEditorMath.SnapToGrid(center.y - 20, 8), 40, 40);
            Working.furniture = Append(Working.furniture, prop);
            Selection = new MapEditorSelection(MapEditorSelectionKind.Furniture, Working.furniture.Length - 1);
        }

        internal void AddDoor(bool vertical)
        {
            RecordUndo();
            var center = canvas.ViewCenterWorld;
            var door = new MapDoor(MapEditorMath.SnapToGrid(center.x, 8), MapEditorMath.SnapToGrid(center.y, 8));
            if (vertical)
            {
                Working.verticalDoors = Append(Working.verticalDoors, door);
                Selection = new MapEditorSelection(MapEditorSelectionKind.VerticalDoor, Working.verticalDoors.Length - 1);
            }
            else
            {
                Working.horizontalDoors = Append(Working.horizontalDoors, door);
                Selection = new MapEditorSelection(MapEditorSelectionKind.HorizontalDoor, Working.horizontalDoors.Length - 1);
            }
        }

        /// <summary>
        /// Removes the selected room/furniture/door after confirming for rooms and furniture (other systems address them by array
        /// position: Navigation/Game by room id is fine, but the house's Art/manifest.json furn[].i indexes furniture positionally —
        /// deleting or reordering among its first 30 entries desyncs the painted art from the pieces, hence the stronger warning below).
        /// </summary>
        internal void DeleteSelected()
        {
            if (!Selection.IsArrayItem) return;
            string title = null, message = null;
            if (Selection.kind == MapEditorSelectionKind.Room)
            {
                title = "Remover sala";
                message = "Remover a sala \"" + Working.rooms[Selection.index].id + "\"?";
            }
            else if (Selection.kind == MapEditorSelectionKind.Furniture)
            {
                bool positionalArt = Working.id == MapHouse.Id && Selection.index < 30;
                title = "Remover móvel";
                message = positionalArt
                    ? "Este é um dos 30 primeiros móveis da casa: Resources/Art/manifest.json aponta para eles pela posição no array. " +
                      "Remover este vai desalinhar a arte pintada de todos os móveis depois dele. Remover mesmo assim?"
                    : "Remover o móvel \"" + Working.furniture[Selection.index].type + "\"?";
            }
            if (title != null && !EditorUtility.DisplayDialog(title, message, "Remover", "Cancelar")) return;

            RecordUndo();
            switch (Selection.kind)
            {
                case MapEditorSelectionKind.Room: Working.rooms = Remove(Working.rooms, Selection.index); break;
                case MapEditorSelectionKind.Furniture: Working.furniture = Remove(Working.furniture, Selection.index); break;
                case MapEditorSelectionKind.VerticalDoor: Working.verticalDoors = Remove(Working.verticalDoors, Selection.index); break;
                case MapEditorSelectionKind.HorizontalDoor: Working.horizontalDoors = Remove(Working.horizontalDoors, Selection.index); break;
            }
            Selection = MapEditorSelection.None;
        }

        static T[] Append<T>(T[] array, T item)
        {
            var result = new T[array.Length + 1];
            Array.Copy(array, result, array.Length);
            result[array.Length] = item;
            return result;
        }

        static T[] Remove<T>(T[] array, int index)
        {
            var result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, index);
            Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            return result;
        }
    }
}
