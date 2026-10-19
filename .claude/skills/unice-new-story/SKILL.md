---
name: unice-new-story
description: >-
  Turn a story the user tells into a new playable Unice story: a spec in docs/stories/<id>.md, a new map (rooms, doors,
  hiding spots), the story card, the intro comic, texts in pt/en/es and tests. Use whenever the user says things like
  "nova história", "cria um mapa", "new story/map", "conto uma história", or tells a memory/place they want as a game map.
---

# Creating a new story (and its map) from what the user tells

The user tells a story in plain words. You produce a working, tested story. The process is spec-driven: the spec comes first
(`docs/stories/<id>.md`, template in `docs/stories/_template.md`, process in `docs/stories/README.md`), the code follows the spec,
the tests prove it. Read `docs/stories/README.md` and `resources/map-design-guide.md` before you start; `resources/art-catalog.md`
lists the sprites you may use. Follow the `unity-code` skill for all C#.

## The idea
Every story the user tells becomes a **hide-and-seek game**: the place becomes the map, the people become the hiders, one person
(the aunt, the grandfather, the neighbour, the teacher… whoever fits) becomes "the Unice", the seeker who waits with her eyes closed
and then hunts, and the things in the place become hiding spots. The rules of the match never change (`Rules`, `Game`); only the
setting, names, texts and comic do. Keep the user's real details (names of places, objects, nicknames, sayings) — that is what makes the game theirs.

## 0. Ask only what you cannot deduce
Do not interview. Infer everything you can and state your assumptions in the final summary. Ask (one short message, max 3 questions)
only if the story lacks: **where** it happens (nothing to build a map from), or **who is the "Unice"** (the seeker) if it's unclear.
Everything else (names, rooms, hiding spots, tone) you invent from the story, staying faithful to the details the user gave.
The Unice is a playful seeker, never violent or scary-for-real; keep the tone of the user's story.

## 1. Spec — write `docs/stories/<id>.md` first
Copy `docs/stories/_template.md` and fill every section from the story: brief, rooms table with coordinates, hiding-spot table,
HQ (comic) panels with captions, texts in pt/en/es, tasks. Pick:
- `<id>`: story id, lowercase ASCII, one word (`feira`, `engenho`). Must not exist in `Stories.All`.
- `<mapId>`: map id, lowercase ASCII, unique in `Maps` (often the same word as `<id>`).
- Class name `Map<Name>` (e.g. `MapFeira`) in `Assets/Unice/Scripts/Domain/Maps/Map<Name>.cs`.
The cordel (5 verses, `story.N.*`) is optional and only exists for `doll`; do not build a new cordel screen unless the user asks.

## 2. Design the map (geometry rules are tested — get them right on paper first)
Follow `resources/map-design-guide.md`: 1720×1180 world, 6–12 rooms in a grid with exactly **24 px** between neighbours,
a door in every gap you want walkable (door ≠ gap means a locked-out room), the entry on the story's natural door,
`spawn` on free floor reachable from the entry, **at least 8 hiding spots** (`hideSide` n/s/e/w) spread over the rooms
(never all in one room), plus decorative props. Props must stay inside the world and must not block doors or the spawn.

## 3. Implement
1. `Domain/Maps/Map<Name>.cs`: a `public static class` with `const string Id` and `Build()` returning a `MapDefinition`
   (see `MapHouse.cs` for a full example and `resources/map-design-guide.md` for the shape; comment the *why* of the entry point). No `UnityEngine` here.
2. Register the map: add `Map<Name>.Build()` to `Maps.InOrder` in `Domain/Maps/MapDefinition.cs` (the only place).
3. Register the story: add a `new Story(id, Map<Name>.Id, released: …, comic: …, cardArt: …)` to `Stories.All` in `Domain/Maps/Stories.cs`.
   - `comic`: 5 `ComicPanel`s (3–6 allowed). Backdrop = an `Art` floor name or `null` (map ground color). Figures use only sprites from
     `resources/art-catalog.md`; kids `ComicFigure.Kid(cousin 0–4, x, y)`, aunt `ComicFigure.Unice(x, y)`. Keep figures at y 0.6–0.95
     (feet line) and x 0.1–0.9 so the caption box at the top does not cover them.
   - `released`: use `true` only if the user wants it playable by everyone now. The map has flat placeholder art (no painted floors/walls), so
     default to `released: false` ("coming soon" card, playable in dev builds and the Editor) and say how to flip it. If the user
     said "pode lançar", use `true`.
4. Texts in `Core/Localization/Localization.Strings.cs` (pt, en, es — always all three, same `{0}` placeholders):
   `map.<mapId>.name`; for **every room** `room.<roomId>.name` and `room.<roomId>.in` ("na Cozinha" / "in the Kitchen" / "en la Cocina");
   for **every hideable prop type** `spot.<type>` ("atrás do pé de umbu"); `stories.<id>.name`, `stories.<id>.blurb` (one sentence), and
   `comic.<id>.1…N` (captions ≤ 70 characters). Reuse an existing key only if the text fits exactly (`room.kitchen.*`, `spot.bed`, …); never rename existing keys.
5. New `.cs` files need `.meta` files with fresh GUIDs (see `unity-code` skill). Follow the file/class naming of `MapHouse.cs`.
6. Docs: add the story to the table in `docs/stories/README.md`, a line in `CHANGELOG.md` (Adicionado), and tick the spec's tasks.

Do **not** change `MapHouse`, existing ids/keys, `Rules`, `Game`, or snapshot formats. If the story needs something the engine lacks
(vehicles, weather, new mechanics), say so in the summary and keep the map within the existing mechanics.

## 4. Verify (you are not done until this is green)
Run the whole Edit Mode suite (`unice-run-tests` skill). Without writing any new test, these cover a new story automatically:
- `MapDefinitionsTests`: rooms inside the world with unique ids; furniture inside the world; ≥ 8 spots; spawn and entrance on floor and
  connected; every spot's stand point is free and reachable from the spawn; every room reachable.
- `LocalizationTests.MapContent_*`: every room name/phrase and spot phrase exists in all languages.
- `StoryContentTests`: story name, blurb (if released), comic captions in all languages; every sprite/backdrop exists; figures within the panel.
- `StoriesTests`: map exists, ids unique.
Fix failures in the map data (not the tests). If a test fails and you believe the test is wrong, stop and tell the user why.
Never add the new map to `MapDefinitionsTests.UnfinishedMaps`.

## 5. Report
Reply in the user's language, short: the story id and map id, the room list in one line, the number of hiding spots, how to play it
(Editor/dev build: Jogar solo → pick the card; or lobby map stepper), the assumptions you made, what is NOT verified (looks, feel, balance:
no one has played it; art is placeholder), and test numbers (`total / passed / failed`). Do not commit unless asked.
