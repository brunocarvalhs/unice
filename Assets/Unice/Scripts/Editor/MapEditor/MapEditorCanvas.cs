using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The map editor's left-hand canvas: draws the map (world bounds, rooms, doors, furniture, spawn/entry) and handles
    /// its own input (pan, zoom, select, drag/resize). View state (zoom/pan/drag) lives here; the data it draws and the
    /// selection/undo it writes into live on the owning <see cref="MapEditorWindow"/>. Rooms are flat colored rectangles,
    /// except the house map's, which show the real painted floor (<see cref="FloorTexture"/>) under a faint tint — other
    /// maps have no painted art. World/room/door/prop coordinates are drawn in their own Y-down pixel space, scaled
    /// by <see cref="zoom"/> and offset by <see cref="pan"/>; no Y-flip.
    /// </summary>
    sealed class MapEditorCanvas
    {
        const float MinZoom = 0.08f, MaxZoom = 6f;
        const float GridSize = 8;                 // world units; also the drag/resize snap
        const float HandleTolerancePx = 8;         // screen pixels, scaled by 1/zoom for hit testing
        const float PointTolerancePx = 10;
        const float GridFadeStartPx = 3, GridFadeEndPx = 14, GridMaxAlpha = 0.12f;

        static readonly Color BackgroundColor = new Color(0.11f, 0.11f, 0.11f);
        static readonly Color WorldBoundsColor = new Color(0.45f, 0.45f, 0.45f);
        static readonly Color DoorColor = new Color(0.95f, 0.8f, 0.2f);
        static readonly Color FurnitureColor = new Color(0.5f, 0.38f, 0.3f);
        static readonly Color HidingSpotColor = new Color(0.4f, 0.65f, 0.35f);
        static readonly Color SelectionColor = new Color(1f, 1f, 0.3f);
        static readonly Color SpawnColor = new Color(0.3f, 0.75f, 1f);
        static readonly Color EntryColor = new Color(1f, 0.55f, 0.2f);

        float zoom = 0.5f;
        Vector2 pan = new Vector2(40, 40);
        Rect lastRect;

        bool panning;
        Vector2 panStartMouse, panStartPan;

        ResizeHandle dragHandle = ResizeHandle.None;
        bool dragging, dragRecorded;
        Vector2 dragStartWorld;
        Rect dragOriginalRect;        // room/furniture being resized/moved
        Vector2 dragOriginalPoint;    // door/spawn/entry being moved

        /// <summary>By room id; null cached once a room has no "floor_&lt;id&gt;" texture (a brand-new room, or a non-house map).</summary>
        readonly Dictionary<string, Texture2D> floorTextureCache = new Dictionary<string, Texture2D>();

        /// <summary>The world point at the center of the canvas right now — where newly added items are placed, so they land where the user is looking.</summary>
        public Vector2 ViewCenterWorld => MapEditorMath.ScreenToWorld(lastRect.size / 2f, pan, zoom);

        public void Draw(Rect rect, MapEditorWindow window)
        {
            lastRect = rect;
            var map = window.Working;
            if (map == null) return;

            GUI.BeginClip(rect);
            var local = new Rect(Vector2.zero, rect.size);
            EditorGUI.DrawRect(local, BackgroundColor);
            DrawGrid(local);
            DrawOutline(WorldToScreenRect(new Rect(0, 0, map.worldW, map.worldH)), WorldBoundsColor, 2);

            bool isHouse = map.id == MapHouse.Id;
            for (int i = 0; i < map.rooms.Length; i++)
                DrawRoom(map.rooms[i], window.Selection.kind == MapEditorSelectionKind.Room && window.Selection.index == i, isHouse);
            DrawDoors(map.verticalDoors, true, window.Selection);
            DrawDoors(map.horizontalDoors, false, window.Selection);
            for (int i = 0; i < map.furniture.Length; i++)
                DrawFurniture(map.furniture[i], window.Selection.kind == MapEditorSelectionKind.Furniture && window.Selection.index == i);
            DrawPoint(new Vector2(map.spawnX, map.spawnY), SpawnColor, "início", window.Selection.kind == MapEditorSelectionKind.Spawn);
            DrawPoint(new Vector2(map.entryX, map.entryY), EntryColor, "entrada", window.Selection.kind == MapEditorSelectionKind.Entry);

            HandleInput(local, window);
            GUI.EndClip();
        }

        Vector2 WorldToScreen(Vector2 world) => MapEditorMath.WorldToScreen(world, pan, zoom);
        Vector2 ScreenToWorld(Vector2 screen) => MapEditorMath.ScreenToWorld(screen, pan, zoom);
        Rect WorldToScreenRect(Rect world) => new Rect(WorldToScreen(world.position), world.size * zoom);

        void DrawGrid(Rect local)
        {
            float spacingPx = GridSize * zoom;
            if (spacingPx < GridFadeStartPx) return;
            float alpha = Mathf.Clamp01((spacingPx - GridFadeStartPx) / (GridFadeEndPx - GridFadeStartPx)) * GridMaxAlpha;
            var color = new Color(1, 1, 1, alpha);

            var topLeftWorld = ScreenToWorld(Vector2.zero);
            var bottomRightWorld = ScreenToWorld(local.size);
            for (float worldX = Mathf.Floor(topLeftWorld.x / GridSize) * GridSize; worldX <= bottomRightWorld.x; worldX += GridSize)
                EditorGUI.DrawRect(new Rect(WorldToScreen(new Vector2(worldX, 0)).x, 0, 1, local.height), color);
            for (float worldY = Mathf.Floor(topLeftWorld.y / GridSize) * GridSize; worldY <= bottomRightWorld.y; worldY += GridSize)
                EditorGUI.DrawRect(new Rect(0, WorldToScreen(new Vector2(0, worldY)).y, local.width, 1), color);
        }

        static void DrawOutline(Rect r, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), color);
            EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), color);
        }

        void DrawRoom(Room room, bool selected, bool isHouse)
        {
            var r = WorldToScreenRect(new Rect(room.x, room.y, room.w, room.h));
            var baseColor = room.tint != null && ColorUtility.TryParseHtmlString(room.tint, out var tintColor) ? tintColor : ColorForId(room.id);
            var floorArt = isHouse ? FloorTexture(room.id) : null;

            if (floorArt != null)
            {
                GUI.DrawTexture(r, floorArt, ScaleMode.StretchToFill);
                // a faint tint on top of the real art, just enough to show selection and keep the id label legible
                EditorGUI.DrawRect(r, new Color(baseColor.r, baseColor.g, baseColor.b, selected ? 0.22f : 0.1f));
            }
            else
            {
                EditorGUI.DrawRect(r, new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f));
            }
            DrawOutline(r, selected ? SelectionColor : new Color(baseColor.r, baseColor.g, baseColor.b, 0.9f), selected ? 3 : 1.5f);
            GUI.Label(new Rect(r.x + 4, r.y + 2, Mathf.Max(0, r.width - 8), 16), room.id, EditorStyles.whiteMiniLabel);
        }

        /// <summary>The house's painted floor for a room id (Resources/Art/floor_&lt;id&gt;.png), same file as <c>MapScenery</c> loads in-game; null when there is none (cached either way).</summary>
        Texture2D FloorTexture(string roomId)
        {
            if (floorTextureCache.TryGetValue(roomId, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("Art/floor_" + roomId);
            floorTextureCache[roomId] = texture;
            return texture;
        }

        void DrawFurniture(MapProp prop, bool selected)
        {
            var r = WorldToScreenRect(new Rect(prop.x, prop.y, prop.w, prop.h));
            bool hideable = prop.hideSide != '\0';
            var fill = hideable ? HidingSpotColor : FurnitureColor;
            EditorGUI.DrawRect(r, new Color(fill.r, fill.g, fill.b, 0.85f));
            DrawOutline(r, selected ? SelectionColor : Color.black, selected ? 3 : 1);
            if (hideable) DrawHideSideTick(r, prop.hideSide);
            if (r.width > 24 && r.height > 14) GUI.Label(new Rect(r.x + 2, r.yMax - 14, r.width - 4, 14), prop.type, EditorStyles.whiteMiniLabel);
        }

        /// <summary>A short tick on the side a child approaches a hiding spot from.</summary>
        static void DrawHideSideTick(Rect r, char side)
        {
            const float thickness = 4;
            Rect tick = side switch
            {
                'n' => new Rect(r.x, r.y - thickness, r.width, thickness),
                's' => new Rect(r.x, r.yMax, r.width, thickness),
                'w' => new Rect(r.x - thickness, r.y, thickness, r.height),
                _ => new Rect(r.xMax, r.y, thickness, r.height),
            };
            EditorGUI.DrawRect(tick, new Color(1f, 1f, 1f, 0.85f));
        }

        void DrawDoors(MapDoor[] doors, bool vertical, MapEditorSelection selection)
        {
            var kind = vertical ? MapEditorSelectionKind.VerticalDoor : MapEditorSelectionKind.HorizontalDoor;
            for (int i = 0; i < doors.Length; i++)
            {
                bool selected = selection.kind == kind && selection.index == i;
                var center = WorldToScreen(new Vector2(doors[i].x, doors[i].y));
                float half = Map.DoorWidth / 2f * zoom;
                var color = selected ? SelectionColor : DoorColor;
                // a short line across the gap it crosses, plus a small marker at its exact (x, y)
                EditorGUI.DrawRect(vertical ? new Rect(center.x - half, center.y - 2, half * 2, 4) : new Rect(center.x - 2, center.y - half, 4, half * 2), color);
                EditorGUI.DrawRect(new Rect(center.x - 3, center.y - 3, 6, 6), color);
            }
        }

        void DrawPoint(Vector2 world, Color color, string label, bool selected)
        {
            var p = WorldToScreen(world);
            float size = selected ? 9 : 6;
            var drawColor = selected ? SelectionColor : color;
            EditorGUI.DrawRect(new Rect(p.x - size, p.y - 1.5f, size * 2, 3), drawColor);
            EditorGUI.DrawRect(new Rect(p.x - 1.5f, p.y - size, 3, size * 2), drawColor);
            GUI.Label(new Rect(p.x + size + 2, p.y - 8, 60, 16), label, EditorStyles.whiteMiniLabel);
        }

        /// <summary>Deterministic color from a room's id, so rooms stay recognizable across repaints without a palette to maintain.</summary>
        static Color ColorForId(string id)
        {
            int hash = id?.GetHashCode() ?? 0;
            float hue = Mathf.Abs(hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.45f, 0.85f);
        }

        void HandleInput(Rect local, MapEditorWindow window)
        {
            var e = Event.current;
            bool overCanvas = local.Contains(e.mousePosition);
            if (!overCanvas && e.type != EventType.MouseUp && !panning && !dragging) return;

            switch (e.type)
            {
                case EventType.ScrollWheel:
                    if (!overCanvas) break;
                    float factor = Mathf.Pow(1.1f, -e.delta.y);
                    float newZoom = Mathf.Clamp(zoom * factor, MinZoom, MaxZoom);
                    pan = MapEditorMath.ZoomAroundCursor(e.mousePosition, pan, zoom, newZoom);
                    zoom = newZoom;
                    e.Use();
                    window.Repaint();
                    break;

                case EventType.MouseDown:
                    if (!overCanvas) break;
                    if (e.button == 2 || (e.button == 0 && window.spaceHeld))
                    {
                        panning = true; panStartMouse = e.mousePosition; panStartPan = pan; e.Use();
                    }
                    else if (e.button == 0)
                    {
                        BeginSelectAndDrag(window, e.mousePosition);
                        e.Use();
                        window.Repaint();
                    }
                    break;

                case EventType.MouseDrag:
                    if (panning) { pan = panStartPan + (e.mousePosition - panStartMouse); e.Use(); window.Repaint(); }
                    else if (dragging && window.Selection.kind != MapEditorSelectionKind.None)
                    {
                        if (!dragRecorded) { window.RecordUndo(); dragRecorded = true; }
                        var delta = ScreenToWorld(e.mousePosition) - dragStartWorld;
                        ApplyDrag(window.Working, window.Selection, delta, !e.shift);
                        e.Use();
                        window.Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (panning) { panning = false; e.Use(); }
                    if (dragging) { dragging = false; dragHandle = ResizeHandle.None; e.Use(); }
                    break;
            }
        }

        void BeginSelectAndDrag(MapEditorWindow window, Vector2 mouseScreen)
        {
            var world = ScreenToWorld(mouseScreen);
            float tolerance = PointTolerancePx / zoom;
            var hit = MapEditorMath.HitTestSelection(window.Working, world, tolerance);
            window.Selection = hit;
            if (hit.kind == MapEditorSelectionKind.None) return;

            dragging = true; dragRecorded = false; dragStartWorld = world;
            dragHandle = DetermineHandle(hit, window.Working, world);
            switch (hit.kind)
            {
                case MapEditorSelectionKind.Room: dragOriginalRect = RectOf(window.Working.rooms[hit.index]); break;
                case MapEditorSelectionKind.Furniture: dragOriginalRect = RectOf(window.Working.furniture[hit.index]); break;
                case MapEditorSelectionKind.VerticalDoor: dragOriginalPoint = PointOf(window.Working.verticalDoors[hit.index]); break;
                case MapEditorSelectionKind.HorizontalDoor: dragOriginalPoint = PointOf(window.Working.horizontalDoors[hit.index]); break;
                case MapEditorSelectionKind.Spawn: dragOriginalPoint = new Vector2(window.Working.spawnX, window.Working.spawnY); break;
                case MapEditorSelectionKind.Entry: dragOriginalPoint = new Vector2(window.Working.entryX, window.Working.entryY); break;
            }
        }

        /// <summary>Only rooms and furniture resize; doors, spawn and entry are points that only move.</summary>
        ResizeHandle DetermineHandle(MapEditorSelection selection, MapDefinition map, Vector2 world)
        {
            float tolerance = HandleTolerancePx / zoom;
            if (selection.kind == MapEditorSelectionKind.Room) return MapEditorMath.HitTestHandle(RectOf(map.rooms[selection.index]), world, tolerance);
            if (selection.kind == MapEditorSelectionKind.Furniture) return MapEditorMath.HitTestHandle(RectOf(map.furniture[selection.index]), world, tolerance);
            return ResizeHandle.Move;
        }

        void ApplyDrag(MapDefinition map, MapEditorSelection selection, Vector2 delta, bool snap)
        {
            switch (selection.kind)
            {
                case MapEditorSelectionKind.Room:
                {
                    var r = MapEditorMath.ApplyHandleDrag(dragOriginalRect, dragHandle, delta, GridSize, snap);
                    var room = map.rooms[selection.index];
                    room.x = r.x; room.y = r.y; room.w = r.width; room.h = r.height;
                    break;
                }
                case MapEditorSelectionKind.Furniture:
                {
                    var r = MapEditorMath.ApplyHandleDrag(dragOriginalRect, dragHandle, delta, GridSize, snap);
                    var prop = map.furniture[selection.index];
                    prop.x = r.x; prop.y = r.y; prop.w = r.width; prop.h = r.height;
                    break;
                }
                case MapEditorSelectionKind.VerticalDoor:
                {
                    var p = SnapPoint(dragOriginalPoint + delta, snap);
                    map.verticalDoors[selection.index] = new MapDoor(p.x, p.y);
                    break;
                }
                case MapEditorSelectionKind.HorizontalDoor:
                {
                    var p = SnapPoint(dragOriginalPoint + delta, snap);
                    map.horizontalDoors[selection.index] = new MapDoor(p.x, p.y);
                    break;
                }
                case MapEditorSelectionKind.Spawn:
                {
                    var p = SnapPoint(dragOriginalPoint + delta, snap);
                    map.spawnX = p.x; map.spawnY = p.y;
                    break;
                }
                case MapEditorSelectionKind.Entry:
                {
                    var p = SnapPoint(dragOriginalPoint + delta, snap);
                    map.entryX = p.x; map.entryY = p.y;
                    break;
                }
            }
        }

        static Vector2 SnapPoint(Vector2 point, bool snap) => snap ? MapEditorMath.SnapToGrid(point, GridSize) : point;
        static Rect RectOf(Room room) => new Rect(room.x, room.y, room.w, room.h);
        static Rect RectOf(MapProp prop) => new Rect(prop.x, prop.y, prop.w, prop.h);
        static Vector2 PointOf(MapDoor door) => new Vector2(door.x, door.y);
    }
}
