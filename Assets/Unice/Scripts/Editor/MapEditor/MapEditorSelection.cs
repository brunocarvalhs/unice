namespace Unice
{
    /// <summary>What kind of map item is selected in the map editor canvas.</summary>
    public enum MapEditorSelectionKind { None, Room, VerticalDoor, HorizontalDoor, Furniture, Spawn, Entry }

    /// <summary>
    /// The current selection in the map editor: a kind plus the array index (for rooms, furniture and doors), or
    /// -1 for the single spawn/entry points. Index-based rather than a reference into the map, so a selection
    /// survives undo/redo, where the whole <see cref="MapDefinition"/> is swapped for a different clone.
    /// </summary>
    public readonly struct MapEditorSelection
    {
        public readonly MapEditorSelectionKind kind;
        public readonly int index;

        public MapEditorSelection(MapEditorSelectionKind kind, int index) { this.kind = kind; this.index = index; }

        public static readonly MapEditorSelection None = new MapEditorSelection(MapEditorSelectionKind.None, -1);

        /// <summary>Rooms, furniture and doors live in an array and can be resized or removed; spawn/entry are single points on the map.</summary>
        public bool IsArrayItem =>
            kind == MapEditorSelectionKind.Room || kind == MapEditorSelectionKind.Furniture ||
            kind == MapEditorSelectionKind.VerticalDoor || kind == MapEditorSelectionKind.HorizontalDoor;
    }
}
