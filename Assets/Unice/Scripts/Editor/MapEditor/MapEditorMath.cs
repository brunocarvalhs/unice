using System;
using UnityEngine;

namespace Unice
{
    /// <summary>A drag handle on a room or furniture rect: a move (inside, away from every edge), an edge, or a corner.</summary>
    public enum ResizeHandle { None, Move, N, S, E, W, NE, NW, SE, SW }

    /// <summary>
    /// Pure math for the map editor canvas: grid snapping, the world/screen view transform, hit testing and the deep
    /// copy used by undo/redo. Kept free of <c>EditorWindow</c>/<c>OnGUI</c> so it can be unit tested without a GUI context.
    /// Screen space here always means "canvas-relative pixels" (the canvas's own top-left is the origin), and world
    /// space is the same Y-down pixel space <see cref="MapDefinition"/> is written in — no Y-flip anywhere.
    /// </summary>
    public static class MapEditorMath
    {
        /// <summary>The smallest a room or prop may shrink to while resizing, in world (house) pixels.</summary>
        public const float MinSize = 8;

        /// <summary>Rounds to the nearest multiple of <paramref name="grid"/> (the 8 px drag/resize snap; a non-positive grid disables snapping).</summary>
        public static float SnapToGrid(float value, float grid) => grid <= 0 ? value : Mathf.Round(value / grid) * grid;

        public static Vector2 SnapToGrid(Vector2 value, float grid) => new Vector2(SnapToGrid(value.x, grid), SnapToGrid(value.y, grid));

        public static Vector2 WorldToScreen(Vector2 world, Vector2 pan, float zoom) => pan + world * zoom;

        public static Vector2 ScreenToWorld(Vector2 screen, Vector2 pan, float zoom) => (screen - pan) / zoom;

        /// <summary>The pan that keeps the world point under <paramref name="cursorScreen"/> in the same place after the zoom changes from <paramref name="oldZoom"/> to <paramref name="newZoom"/>.</summary>
        public static Vector2 ZoomAroundCursor(Vector2 cursorScreen, Vector2 pan, float oldZoom, float newZoom)
        {
            var worldUnderCursor = ScreenToWorld(cursorScreen, pan, oldZoom);
            return cursorScreen - worldUnderCursor * newZoom;
        }

        /// <summary>
        /// Which edge/corner of <paramref name="rect"/> is within <paramref name="tolerance"/> of <paramref name="point"/>; <see cref="ResizeHandle.Move"/>
        /// if the point is inside the rect away from every edge; <see cref="ResizeHandle.None"/> if it's outside the rect by more than the tolerance.
        /// </summary>
        public static ResizeHandle HitTestHandle(Rect rect, Vector2 point, float tolerance)
        {
            bool nearLeft = Mathf.Abs(point.x - rect.x) <= tolerance;
            bool nearRight = Mathf.Abs(point.x - (rect.x + rect.width)) <= tolerance;
            bool nearTop = Mathf.Abs(point.y - rect.y) <= tolerance;
            bool nearBottom = Mathf.Abs(point.y - (rect.y + rect.height)) <= tolerance;
            bool withinX = point.x >= rect.x - tolerance && point.x <= rect.x + rect.width + tolerance;
            bool withinY = point.y >= rect.y - tolerance && point.y <= rect.y + rect.height + tolerance;

            if (nearTop && nearLeft) return ResizeHandle.NW;
            if (nearTop && nearRight) return ResizeHandle.NE;
            if (nearBottom && nearLeft) return ResizeHandle.SW;
            if (nearBottom && nearRight) return ResizeHandle.SE;
            if (nearTop && withinX) return ResizeHandle.N;
            if (nearBottom && withinX) return ResizeHandle.S;
            if (nearLeft && withinY) return ResizeHandle.W;
            if (nearRight && withinY) return ResizeHandle.E;
            if (point.x > rect.x && point.x < rect.x + rect.width && point.y > rect.y && point.y < rect.y + rect.height) return ResizeHandle.Move;
            return ResizeHandle.None;
        }

        /// <summary>
        /// Applies one drag gesture's world-space delta (from the gesture's start, not incremental, so rounding never drifts) to a rect
        /// through the given handle, snapping to <paramref name="grid"/> unless <paramref name="snap"/> is false, and never shrinking past <see cref="MinSize"/>.
        /// </summary>
        public static Rect ApplyHandleDrag(Rect original, ResizeHandle handle, Vector2 worldDelta, float grid, bool snap)
        {
            float dx = snap ? SnapToGrid(worldDelta.x, grid) : worldDelta.x;
            float dy = snap ? SnapToGrid(worldDelta.y, grid) : worldDelta.y;
            float left = original.x, top = original.y, right = original.x + original.width, bottom = original.y + original.height;

            if (handle == ResizeHandle.Move) { left += dx; top += dy; right += dx; bottom += dy; }
            bool west = handle == ResizeHandle.W || handle == ResizeHandle.NW || handle == ResizeHandle.SW;
            bool east = handle == ResizeHandle.E || handle == ResizeHandle.NE || handle == ResizeHandle.SE;
            bool north = handle == ResizeHandle.N || handle == ResizeHandle.NW || handle == ResizeHandle.NE;
            bool south = handle == ResizeHandle.S || handle == ResizeHandle.SW || handle == ResizeHandle.SE;
            // the opposite edge stays put: clamp against it rather than the new edge, so a fast drag can't flip the rect inside out
            if (west) left = Mathf.Min(left + dx, right - MinSize);
            if (east) right = Mathf.Max(right + dx, left + MinSize);
            if (north) top = Mathf.Min(top + dy, bottom - MinSize);
            if (south) bottom = Mathf.Max(bottom + dy, top + MinSize);

            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// The nearest selectable item to <paramref name="world"/> within <paramref name="tolerance"/> world units, or <see cref="MapEditorSelection.None"/>.
        /// Spawn/entry and doors (small, easy to miss) win over furniture, which wins over rooms (so a chair inside a room is still selectable,
        /// and a room is only picked when nothing smaller is under the cursor).
        /// </summary>
        public static MapEditorSelection HitTestSelection(MapDefinition map, Vector2 world, float tolerance)
        {
            if (Vector2.Distance(world, new Vector2(map.entryX, map.entryY)) <= tolerance) return new MapEditorSelection(MapEditorSelectionKind.Entry, -1);
            if (Vector2.Distance(world, new Vector2(map.spawnX, map.spawnY)) <= tolerance) return new MapEditorSelection(MapEditorSelectionKind.Spawn, -1);

            int vertical = NearestDoor(map.verticalDoors, world, tolerance);
            if (vertical >= 0) return new MapEditorSelection(MapEditorSelectionKind.VerticalDoor, vertical);
            int horizontal = NearestDoor(map.horizontalDoors, world, tolerance);
            if (horizontal >= 0) return new MapEditorSelection(MapEditorSelectionKind.HorizontalDoor, horizontal);

            int furniture = NearestRect(map.furniture, prop => new Rect(prop.x, prop.y, prop.w, prop.h), world, tolerance);
            if (furniture >= 0) return new MapEditorSelection(MapEditorSelectionKind.Furniture, furniture);

            for (int i = 0; i < (map.rooms?.Length ?? 0); i++)
                if (HitTestHandle(new Rect(map.rooms[i].x, map.rooms[i].y, map.rooms[i].w, map.rooms[i].h), world, tolerance) != ResizeHandle.None)
                    return new MapEditorSelection(MapEditorSelectionKind.Room, i);
            return MapEditorSelection.None;
        }

        static int NearestDoor(MapDoor[] doors, Vector2 world, float tolerance)
        {
            int best = -1; float bestDistance = tolerance;
            for (int i = 0; i < (doors?.Length ?? 0); i++)
            {
                float distance = Vector2.Distance(world, new Vector2(doors[i].x, doors[i].y));
                if (distance <= bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        /// <summary>The closest-to-center item (by its <paramref name="toRect"/> rect) that <paramref name="world"/> is inside or within <paramref name="tolerance"/> of an edge of.</summary>
        static int NearestRect<T>(T[] items, Func<T, Rect> toRect, Vector2 world, float tolerance)
        {
            int best = -1; float bestDistance = float.MaxValue;
            for (int i = 0; i < (items?.Length ?? 0); i++)
            {
                var rect = toRect(items[i]);
                if (HitTestHandle(rect, world, tolerance) == ResizeHandle.None) continue;
                float distance = Vector2.Distance(world, rect.center);
                if (distance < bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        /// <summary>Deep copy: the undo stack's snapshots, and the editor's working copy, never alias the shared <see cref="Maps"/> instance the game uses.</summary>
        public static MapDefinition Clone(MapDefinition map) => new MapDefinition
        {
            id = map.id, worldW = map.worldW, worldH = map.worldH, groundTint = map.groundTint,
            entryX = map.entryX, entryY = map.entryY, spawnX = map.spawnX, spawnY = map.spawnY,
            rooms = Array.ConvertAll(map.rooms ?? Array.Empty<Room>(), r => new Room(r.id, r.x, r.y, r.w, r.h, r.tint)),
            verticalDoors = Array.ConvertAll(map.verticalDoors ?? Array.Empty<MapDoor>(), d => new MapDoor(d.x, d.y)),
            horizontalDoors = Array.ConvertAll(map.horizontalDoors ?? Array.Empty<MapDoor>(), d => new MapDoor(d.x, d.y)),
            furniture = Array.ConvertAll(map.furniture ?? Array.Empty<MapProp>(), p => new MapProp(p.type, p.x, p.y, p.w, p.h, p.hideSide)),
        };
    }
}
