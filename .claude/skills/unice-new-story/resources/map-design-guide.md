# Designing a Unice map

Units are "house pixels". The characters are ~28 px wide (`Map.BodyRadius = 14`). Everything is axis-aligned rectangles.

## The world
- `worldW = 1720`, `worldH = 1180` for every map (same round length as the house). Rooms live inside `[40 .. 1680] × [40 .. 1140]`.
- Hiders run at 150 px/s (sneak 78), the Unice patrols at 138 and chases at 152–165 (`Rules`); a room 400–550 px wide takes about 3 s to cross. Rounds are 120 s: ~8–12 rooms is right.

## Rooms and gaps
`new Room(id, x, y, w, h)`. Neighbouring rooms are separated by a wall gap of **exactly 24 px** (`Map.DoorGap`):
- side by side: `next.x == prev.x + prev.w + 24`
- stacked: `next.y == prev.y + prev.h + 24`
Rooms that do not have a 24 px gap cannot be linked by a door and are unreachable.
Room ids: lowercase ASCII, unique, each needs `room.<id>.name` and `room.<id>.in` texts.

Reference layout (the house, 4 columns × 3 bands):
```
y=40   [master 440×350][bath 220×350][kids 420×350][library 488×350]    x: 40, 504, 748, 1192
y=414  [hall 1640×120 — the long corridor linking everything]
y=558  [living 520×342][dining 480×342][kitchen 592×342]                x: 40, 584, 1088
y=924  [estufa 520×216][entrance 480×216][pantry 300×216][laundry 268×216]
```
A **corridor/yard/street room** that every band opens onto keeps routes short and gives the Unice a place to patrol.
Do not make the map a single long chain: every room should be reachable in ≤ 3–4 door crossings from the spawn.

## Doors
A door is a `MapDoor(x, y)` listed in one of two arrays; the gap is always 24 px and the opening is 64 px wide:
- `verticalDoors` — in a **horizontal** wall (a stacked pair; you walk up/down). `x` = door centre along the wall, `y` = bottom edge of the upper room.
  Example: master (bottom edge 390) over hall → `new MapDoor(260, 390)`.
- `horizontalDoors` — in a **vertical** wall (a side-by-side pair; you walk left/right). `x` = right edge of the left room, `y` = door centre along the wall.
  Example: master (right edge 480) next to bath → `new MapDoor(480, 215)`.
The 64 px opening (plus 30 px of overlap into each room) must lie inside the overlap of both rooms' extents; keep ≥ 50 px of wall
beside it. Every room needs at least one door; give loops (2 doors) to large rooms so hiders and the Unice can circle around.

## Entry and spawn
- `entryX, entryY`: where the Unice appears when the hunt starts — the story's natural door (front door, gate, back door).
  Put it inside a room near its outer edge, on free floor. It is always the *far side* from `spawn`.
- `spawnX, spawnY`: where hiders line up; central, on free floor, not inside a prop.
- `Navigation.FindPath(entry → spawn)` must exist (tested).

## Props (`MapProp(type, x, y, w, h, hideSide)`)
- `type`: lowercase ASCII; also the localization key `spot.<type>` when hideable. Reusing an existing type reuses its text (`bed`, `wardrobe`, `table`, `chest`…).
- `hideSide` (`'n' 's' 'e' 'w'`): the side a child approaches from; the child stands 22 px outside that side → that point must be free floor
  inside a room (not in a wall gap, not inside another prop, not outside the room). Put the prop against the *opposite* wall when it looks natural
  (a bed against the north wall is hidden-from `'s'`).
- No `hideSide` = decorative obstacle (blocks walking; keep it out of doorways).
- **≥ 8 hiding spots** (max players); aim for 12–18 and ≥ 1 per room in most rooms. Vary the risk: some in corners (safe, far), some
  in the open (close to the Unice's route). A spot should be a bit smaller than a room, not a whole wall.
- Keep 60 px of free floor in front of doorways and around the spawn.
- Props must lie fully inside the world (tested) and should lie inside one room.
- Optional: a prop of type `"clock"` is treated as the room clock (only the house has one).

## Ground colour
`groundTint = "#rrggbb"` fills every room (no painted art for new maps yet). Props are drawn as darker silhouettes of it.
Choose a colour that says the place: grass `#4a7c3f`, pavement `#6b6a63`, red clay `#8a4b32`, wood `#5a3d28`.

## Skeleton
```csharp
namespace Unice
{
    /// <summary>One sentence: the place, the time of day, and why the entry is where it is.</summary>
    public static class MapFeira
    {
        public const string Id = "feira";

        public static MapDefinition Build() => new MapDefinition
        {
            id = Id, worldW = 1720, worldH = 1180, groundTint = "#8a4b32",
            entryX = 860, entryY = 60, spawnX = 860, spawnY = 700,
            rooms = new[] { new Room("fruits", 40, 40, 540, 380), /* … */ },
            verticalDoors = new[] { new MapDoor(300, 420) /* … */ },
            horizontalDoors = new[] { new MapDoor(580, 230) /* … */ },
            furniture = new[] { new MapProp("stall", 80, 140, 140, 90, 'n') /* … */ },
        };
    }
}
```
After writing it, run the tests: `MapDefinitionsTests` names exactly which room/spot/door is wrong.
