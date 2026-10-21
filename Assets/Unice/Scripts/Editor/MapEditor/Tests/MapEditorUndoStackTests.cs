using NUnit.Framework;

namespace Unice
{
    [TestFixture]
    public class MapEditorUndoStackTests
    {
        static MapDefinition MapWithRoomX(float x) => new MapDefinition
        {
            id = "m", worldW = 100, worldH = 100,
            rooms = new[] { new Room("a", x, 0, 10, 10) },
            verticalDoors = new MapDoor[0], horizontalDoors = new MapDoor[0], furniture = new MapProp[0],
        };

        [Test]
        public void NewStack_CannotUndoOrRedo()
        {
            var stack = new MapEditorUndoStack();
            Assert.IsFalse(stack.CanUndo);
            Assert.IsFalse(stack.CanRedo);
        }

        [Test]
        public void Undo_AfterOneRecordedSnapshot_RestoresThePreviousState()
        {
            var stack = new MapEditorUndoStack();
            var before = MapWithRoomX(0);
            stack.RecordSnapshot(before);
            var after = MapWithRoomX(100); // the "current" state, as if the mutation already happened

            var restored = stack.Undo(after);

            Assert.AreEqual(0, restored.rooms[0].x);
            Assert.IsFalse(stack.CanUndo);
            Assert.IsTrue(stack.CanRedo);
        }

        [Test]
        public void Redo_AfterUndo_RestoresTheStateThatWasUndone()
        {
            var stack = new MapEditorUndoStack();
            var before = MapWithRoomX(0);
            stack.RecordSnapshot(before);
            var after = MapWithRoomX(100);

            var undone = stack.Undo(after);
            var redone = stack.Redo(undone);

            Assert.AreEqual(100, redone.rooms[0].x);
            Assert.IsTrue(stack.CanUndo);
            Assert.IsFalse(stack.CanRedo);
        }

        [Test]
        public void RecordSnapshot_AfterAnUndo_DropsTheRedoBranch()
        {
            var stack = new MapEditorUndoStack();
            stack.RecordSnapshot(MapWithRoomX(0));
            var current = stack.Undo(MapWithRoomX(100));
            Assert.IsTrue(stack.CanRedo);

            stack.RecordSnapshot(current); // a fresh edit after undoing should abandon the redo branch
            Assert.IsFalse(stack.CanRedo);
        }

        [Test]
        public void Undo_ReturnsASnapshotIndependentFromWhatWasPassedIn()
        {
            var stack = new MapEditorUndoStack();
            var before = MapWithRoomX(0);
            stack.RecordSnapshot(before);
            var current = MapWithRoomX(100);

            var restored = stack.Undo(current);
            restored.rooms[0].x = 999;

            Assert.AreEqual(0, before.rooms[0].x); // the stack cloned "before"; mutating the result doesn't reach back into it
        }

        [Test]
        public void Undo_WithNothingRecorded_ReturnsNull() => Assert.IsNull(new MapEditorUndoStack().Undo(MapWithRoomX(0)));

        [Test]
        public void Redo_WithNothingToRedo_ReturnsNull() => Assert.IsNull(new MapEditorUndoStack().Redo(MapWithRoomX(0)));

        [Test]
        public void ManySnapshots_UndoStillWalksBackInOrder()
        {
            var stack = new MapEditorUndoStack();
            var current = MapWithRoomX(0);
            for (int i = 1; i <= 5; i++)
            {
                stack.RecordSnapshot(current);
                current = MapWithRoomX(i * 10);
            }

            for (int i = 4; i >= 0; i--)
                current = stack.Undo(current);
            Assert.AreEqual(0, current.rooms[0].x);
            Assert.IsFalse(stack.CanUndo);
        }
    }
}
