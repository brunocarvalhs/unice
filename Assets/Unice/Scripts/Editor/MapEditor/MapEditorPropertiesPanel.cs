using System;
using UnityEditor;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The map editor's right-hand panel: map-level settings, the add/delete buttons, and the fields of the current
    /// selection. Two-way with the canvas: dragging an item updates these fields next repaint, and editing a field
    /// here mutates the same in-memory <see cref="MapDefinition"/> the canvas draws.
    /// </summary>
    static class MapEditorPropertiesPanel
    {
        /// <summary><see cref="MapProp.hideSide"/>'s five values, with their popup labels.</summary>
        static readonly (char value, string label)[] HideSides =
        {
            ('\0', "Nenhum (decorativo)"), ('n', "Norte (n)"), ('s', "Sul (s)"), ('e', "Leste (e)"), ('w', "Oeste (w)"),
        };

        public static void Draw(MapEditorWindow window)
        {
            var map = window.Working;
            if (map == null) return;

            EditorGUILayout.LabelField("Mapa", EditorStyles.boldLabel);
            DrawMapSettings(window, map);

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Sala")) window.AddRoom();
            if (GUILayout.Button("+ Móvel")) window.AddFurniture();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Porta vertical")) window.AddDoor(true);
            if (GUILayout.Button("+ Porta horizontal")) window.AddDoor(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Seleção", EditorStyles.boldLabel);
            DrawSelection(window, map);
        }

        static void DrawMapSettings(MapEditorWindow window, MapDefinition map)
        {
            EditorGUI.BeginChangeCheck();
            float worldW = EditorGUILayout.FloatField("Largura do mundo", map.worldW);
            float worldH = EditorGUILayout.FloatField("Altura do mundo", map.worldH);
            string groundTint = ColorField("Cor do piso", map.groundTint, out bool groundTintChanged);
            if (EditorGUI.EndChangeCheck() || groundTintChanged)
            {
                window.RecordUndo();
                map.worldW = worldW; map.worldH = worldH; map.groundTint = groundTint;
            }
        }

        static void DrawSelection(MapEditorWindow window, MapDefinition map)
        {
            switch (window.Selection.kind)
            {
                case MapEditorSelectionKind.Room: DrawRoom(window, map); break;
                case MapEditorSelectionKind.Furniture: DrawFurniture(window, map); break;
                case MapEditorSelectionKind.VerticalDoor: DrawDoor(window, map.verticalDoors, "porta vertical"); break;
                case MapEditorSelectionKind.HorizontalDoor: DrawDoor(window, map.horizontalDoors, "porta horizontal"); break;
                case MapEditorSelectionKind.Spawn:
                    DrawPoint(window, () => (map.spawnX, map.spawnY), (x, y) => { map.spawnX = x; map.spawnY = y; });
                    break;
                case MapEditorSelectionKind.Entry:
                    DrawPoint(window, () => (map.entryX, map.entryY), (x, y) => { map.entryX = x; map.entryY = y; });
                    break;
                default:
                    EditorGUILayout.HelpBox("Clique numa sala, móvel, porta, no início (spawn) ou na entrada, no mapa à esquerda.", MessageType.None);
                    break;
            }
        }

        static void DrawRoom(MapEditorWindow window, MapDefinition map)
        {
            var room = map.rooms[window.Selection.index];
            EditorGUI.BeginChangeCheck();
            string id = EditorGUILayout.TextField("Id", room.id);
            float x = EditorGUILayout.FloatField("X", room.x);
            float y = EditorGUILayout.FloatField("Y", room.y);
            float w = EditorGUILayout.FloatField("Largura", room.w);
            float h = EditorGUILayout.FloatField("Altura", room.h);
            string tint = ColorField("Cor própria", room.tint, out bool tintChanged);
            if (EditorGUI.EndChangeCheck() || tintChanged)
            {
                window.RecordUndo();
                room.id = id; room.x = x; room.y = y; room.w = w; room.h = h; room.tint = tint;
            }
            if (GUILayout.Button("Remover sala")) window.DeleteSelected();
        }

        static void DrawFurniture(MapEditorWindow window, MapDefinition map)
        {
            var prop = map.furniture[window.Selection.index];
            EditorGUI.BeginChangeCheck();
            string type = EditorGUILayout.TextField("Tipo", prop.type);
            float x = EditorGUILayout.FloatField("X", prop.x);
            float y = EditorGUILayout.FloatField("Y", prop.y);
            float w = EditorGUILayout.FloatField("Largura", prop.w);
            float h = EditorGUILayout.FloatField("Altura", prop.h);
            int sideIndex = Math.Max(0, Array.FindIndex(HideSides, s => s.value == prop.hideSide));
            int newSideIndex = EditorGUILayout.Popup("Lado do esconderijo", sideIndex, Array.ConvertAll(HideSides, s => s.label));
            if (EditorGUI.EndChangeCheck())
            {
                window.RecordUndo();
                prop.type = type; prop.x = x; prop.y = y; prop.w = w; prop.h = h; prop.hideSide = HideSides[newSideIndex].value;
            }
            if (GUILayout.Button("Remover móvel")) window.DeleteSelected();
        }

        static void DrawDoor(MapEditorWindow window, MapDoor[] doors, string label)
        {
            var door = doors[window.Selection.index];
            EditorGUI.BeginChangeCheck();
            float x = EditorGUILayout.FloatField("X", door.x);
            float y = EditorGUILayout.FloatField("Y", door.y);
            if (EditorGUI.EndChangeCheck())
            {
                window.RecordUndo();
                doors[window.Selection.index] = new MapDoor(x, y);
            }
            if (GUILayout.Button("Remover " + label)) window.DeleteSelected();
        }

        static void DrawPoint(MapEditorWindow window, Func<(float x, float y)> get, Action<float, float> set)
        {
            var (px, py) = get();
            EditorGUI.BeginChangeCheck();
            float x = EditorGUILayout.FloatField("X", px);
            float y = EditorGUILayout.FloatField("Y", py);
            if (EditorGUI.EndChangeCheck())
            {
                window.RecordUndo();
                set(x, y);
            }
        }

        /// <summary>A hex color field that can also be "none" (null = fall back to the map's ground color). Returns the new hex string (or null) and whether anything changed.</summary>
        static string ColorField(string label, string hex, out bool changed)
        {
            bool hadColor = hex != null;
            var color = hadColor && ColorUtility.TryParseHtmlString(hex, out var parsed) ? parsed : Color.white;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            bool hasColor = EditorGUILayout.ToggleLeft(label, hadColor, GUILayout.Width(140));
            using (new EditorGUI.DisabledScope(!hasColor)) color = EditorGUILayout.ColorField(hasColor ? color : Color.white);
            EditorGUILayout.EndHorizontal();
            changed = EditorGUI.EndChangeCheck();
            return hasColor ? "#" + ColorUtility.ToHtmlStringRGB(color) : null;
        }
    }
}
