using NUnit.Framework;
using UnityEngine;

namespace Unice
{
    [TestFixture]
    public class MapEditorMathTests
    {
        static MapDefinition SampleMap() => new MapDefinition
        {
            id = "sample", worldW = 400, worldH = 300, groundTint = "#112233",
            entryX = 10, entryY = 10, spawnX = 390, spawnY = 290,
            rooms = new[] { new Room("a", 0, 0, 100, 100), new Room("b", 100, 0, 100, 100, tint: "#ff0000") },
            verticalDoors = new[] { new MapDoor(100, 40) },
            horizontalDoors = new MapDoor[0],
            furniture = new[] { new MapProp("bed", 10, 10, 20, 20, 'n'), new MapProp("chair", 150, 10, 10, 10) },
        };

        // SnapToGrid

        [Test]
        public void SnapToGrid_RoundsToNearestMultiple()
        {
            Assert.AreEqual(8, MapEditorMath.SnapToGrid(5, 8));
            Assert.AreEqual(16, MapEditorMath.SnapToGrid(13, 8));
            Assert.AreEqual(0, MapEditorMath.SnapToGrid(3, 8));
            Assert.AreEqual(-8, MapEditorMath.SnapToGrid(-5, 8));
        }

        [Test]
        public void SnapToGrid_NonPositiveGrid_ReturnsValueUnchanged() => Assert.AreEqual(5.5f, MapEditorMath.SnapToGrid(5.5f, 0));

        // World/screen transform

        [Test]
        public void ScreenToWorld_UndoesWorldToScreen()
        {
            var pan = new Vector2(37, -12); const float zoom = 1.7f;
            var world = new Vector2(123, 45);
            var screen = MapEditorMath.WorldToScreen(world, pan, zoom);
            var back = MapEditorMath.ScreenToWorld(screen, pan, zoom);
            Assert.AreEqual(world.x, back.x, 1e-3f);
            Assert.AreEqual(world.y, back.y, 1e-3f);
        }

        [Test]
        public void ZoomAroundCursor_KeepsWorldPointUnderCursorFixed()
        {
            var pan = new Vector2(10, 10); const float oldZoom = 1f;
            var cursor = new Vector2(60, 80);
            var worldUnderCursorBefore = MapEditorMath.ScreenToWorld(cursor, pan, oldZoom);

            float newZoom = 2.5f;
            var newPan = MapEditorMath.ZoomAroundCursor(cursor, pan, oldZoom, newZoom);
            var worldUnderCursorAfter = MapEditorMath.ScreenToWorld(cursor, newPan, newZoom);

            Assert.AreEqual(worldUnderCursorBefore.x, worldUnderCursorAfter.x, 1e-3f);
            Assert.AreEqual(worldUnderCursorBefore.y, worldUnderCursorAfter.y, 1e-3f);
        }

        // HitTestHandle

        [Test]
        public void HitTestHandle_PointWellInside_ReturnsMove() =>
            Assert.AreEqual(ResizeHandle.Move, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(50, 50), 5));

        [Test]
        public void HitTestHandle_PointFarAway_ReturnsNone() =>
            Assert.AreEqual(ResizeHandle.None, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(500, 500), 5));

        [Test]
        public void HitTestHandle_NearTopEdgeAwayFromCorners_ReturnsN() =>
            Assert.AreEqual(ResizeHandle.N, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(50, 1), 5));

        [Test]
        public void HitTestHandle_NearTopLeftCorner_ReturnsNW() =>
            Assert.AreEqual(ResizeHandle.NW, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(1, 1), 5));

        [Test]
        public void HitTestHandle_NearBottomRightCorner_ReturnsSE() =>
            Assert.AreEqual(ResizeHandle.SE, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(99, 99), 5));

        [Test]
        public void HitTestHandle_NearRightEdge_ReturnsE() =>
            Assert.AreEqual(ResizeHandle.E, MapEditorMath.HitTestHandle(new Rect(0, 0, 100, 100), new Vector2(99, 50), 5));

        // ApplyHandleDrag

        [Test]
        public void ApplyHandleDrag_Move_TranslatesWithoutResizing()
        {
            var result = MapEditorMath.ApplyHandleDrag(new Rect(10, 20, 30, 40), ResizeHandle.Move, new Vector2(5, -5), 1, false);
            Assert.AreEqual(new Rect(15, 15, 30, 40), result);
        }

        [Test]
        public void ApplyHandleDrag_North_MovesTopEdgeAndKeepsBottomFixed()
        {
            var result = MapEditorMath.ApplyHandleDrag(new Rect(0, 0, 50, 50), ResizeHandle.N, new Vector2(0, 10), 1, false);
            Assert.AreEqual(10, result.y);
            Assert.AreEqual(40, result.height);
            Assert.AreEqual(50, result.yMax); // bottom edge unchanged
        }

        [Test]
        public void ApplyHandleDrag_West_NeverShrinksPastMinSize()
        {
            // dragging the west edge far past the east edge must clamp, not invert the rect
            var result = MapEditorMath.ApplyHandleDrag(new Rect(0, 0, 50, 50), ResizeHandle.W, new Vector2(1000, 0), 1, false);
            Assert.AreEqual(MapEditorMath.MinSize, result.width);
            Assert.AreEqual(50 - MapEditorMath.MinSize, result.x);
        }

        [Test]
        public void ApplyHandleDrag_SnapsToGridWhenRequested()
        {
            var result = MapEditorMath.ApplyHandleDrag(new Rect(0, 0, 50, 50), ResizeHandle.Move, new Vector2(13, 0), 8, true);
            Assert.AreEqual(16, result.x); // 13 snaps to 16
        }

        [Test]
        public void ApplyHandleDrag_ShiftHeld_SkipsSnapping()
        {
            var result = MapEditorMath.ApplyHandleDrag(new Rect(0, 0, 50, 50), ResizeHandle.Move, new Vector2(13, 0), 8, false);
            Assert.AreEqual(13, result.x);
        }

        [Test]
        public void ApplyHandleDrag_SoutheastCorner_GrowsBothDimensions()
        {
            var result = MapEditorMath.ApplyHandleDrag(new Rect(0, 0, 50, 50), ResizeHandle.SE, new Vector2(10, 20), 1, false);
            Assert.AreEqual(new Rect(0, 0, 60, 70), result);
        }

        // HitTestSelection

        [Test]
        public void HitTestSelection_NearEntry_ReturnsEntry()
        {
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(11, 11), 5);
            Assert.AreEqual(MapEditorSelectionKind.Entry, selection.kind);
        }

        [Test]
        public void HitTestSelection_NearSpawn_ReturnsSpawn()
        {
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(389, 289), 5);
            Assert.AreEqual(MapEditorSelectionKind.Spawn, selection.kind);
        }

        [Test]
        public void HitTestSelection_NearVerticalDoor_ReturnsVerticalDoorByIndex()
        {
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(101, 41), 5);
            Assert.AreEqual(MapEditorSelectionKind.VerticalDoor, selection.kind);
            Assert.AreEqual(0, selection.index);
        }

        [Test]
        public void HitTestSelection_InsideFurnitureInsideARoom_PrefersFurnitureOverRoom()
        {
            // (15,15) is inside both furniture[0] ("bed", 10..30) and room "a" (0..100) — furniture must win
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(15, 15), 2);
            Assert.AreEqual(MapEditorSelectionKind.Furniture, selection.kind);
            Assert.AreEqual(0, selection.index);
        }

        [Test]
        public void HitTestSelection_InsideRoomAwayFromFurniture_ReturnsRoomByIndex()
        {
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(170, 50), 2);
            Assert.AreEqual(MapEditorSelectionKind.Room, selection.kind);
            Assert.AreEqual(1, selection.index);
        }

        [Test]
        public void HitTestSelection_FarFromEverything_ReturnsNone()
        {
            var selection = MapEditorMath.HitTestSelection(SampleMap(), new Vector2(-500, -500), 5);
            Assert.AreEqual(MapEditorSelectionKind.None, selection.kind);
        }

        // Clone

        [Test]
        public void Clone_CopiesEveryField()
        {
            var clone = MapEditorMath.Clone(SampleMap());
            Assert.AreEqual("sample", clone.id);
            Assert.AreEqual(400, clone.worldW);
            Assert.AreEqual("#112233", clone.groundTint);
            Assert.AreEqual(2, clone.rooms.Length);
            Assert.AreEqual(1, clone.verticalDoors.Length);
            Assert.AreEqual(2, clone.furniture.Length);
            Assert.AreEqual('n', clone.furniture[0].hideSide);
        }

        [Test]
        public void Clone_MutatingTheCloneDoesNotAffectTheOriginal()
        {
            var original = SampleMap();
            var clone = MapEditorMath.Clone(original);

            clone.rooms[0].x = 999;
            clone.furniture[0].type = "changed";
            clone.id = "different";

            Assert.AreEqual(0, original.rooms[0].x);
            Assert.AreEqual("bed", original.furniture[0].type);
            Assert.AreEqual("sample", original.id);
        }

        [Test]
        public void Clone_ReplacingAnArrayOnTheCloneDoesNotAffectTheOriginal()
        {
            var original = SampleMap();
            var clone = MapEditorMath.Clone(original);
            clone.rooms = new[] { new Room("only", 0, 0, 1, 1) };
            Assert.AreEqual(2, original.rooms.Length);
        }
    }
}
