using System.Collections.Generic;

namespace Unice
{
    /// <summary>
    /// A simple undo/redo stack of whole <see cref="MapDefinition"/> snapshots (deep copies via <see cref="MapEditorMath.Clone"/>),
    /// used instead of Unity's <c>Undo</c> system because the map editor mutates a plain C# object graph, not a <c>UnityEngine.Object</c>.
    /// Capped at <see cref="Capacity"/> entries so a long editing session doesn't grow without bound.
    /// </summary>
    public sealed class MapEditorUndoStack
    {
        const int Capacity = 100;

        readonly List<MapDefinition> undoStack = new List<MapDefinition>();
        readonly List<MapDefinition> redoStack = new List<MapDefinition>();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        /// <summary>Call right before mutating the in-memory map (a drag's first move, or a properties-panel edit): snapshots the state as it is now, and drops the redo branch.</summary>
        public void RecordSnapshot(MapDefinition current)
        {
            undoStack.Add(MapEditorMath.Clone(current));
            if (undoStack.Count > Capacity) undoStack.RemoveAt(0);
            redoStack.Clear();
        }

        /// <summary>Pops the last snapshot and returns it, pushing <paramref name="current"/> onto the redo stack; null if there's nothing to undo.</summary>
        public MapDefinition Undo(MapDefinition current)
        {
            if (undoStack.Count == 0) return null;
            redoStack.Add(MapEditorMath.Clone(current));
            var previous = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            return previous;
        }

        /// <summary>Pops the last redo snapshot and returns it, pushing <paramref name="current"/> back onto the undo stack; null if there's nothing to redo.</summary>
        public MapDefinition Redo(MapDefinition current)
        {
            if (redoStack.Count == 0) return null;
            undoStack.Add(MapEditorMath.Clone(current));
            var next = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            return next;
        }
    }
}
