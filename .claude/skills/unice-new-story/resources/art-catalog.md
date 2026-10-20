# Art you can use in a story's comic (`Assets/Unice/Resources/Art`)

Use these names in `ComicFigure.Sprite("<file>", width, x, y)` and as `ComicPanel` backdrops / `Story.cardArt`.
`width` is in panel pixels (a comic panel is ~230–300 px wide); the height follows the image. Sensible widths are given.
Only files listed here are guaranteed to exist (`StoryContentTests.Comic_EveryPictureExists` checks them).

## Backdrops (floor pictures — house rooms), also valid for `cardArt`
`floor_master` (bedroom), `floor_bath`, `floor_kids`, `floor_library`, `floor_hall` (corridor, very wide), `floor_living`,
`floor_dining`, `floor_kitchen`, `floor_estufa` (greenhouse), `floor_entrance`, `floor_pantry`, `floor_laundry`.
Pass `null` as the backdrop to use the story map's flat ground colour (right for outdoor or non-house stories).

## Props / furniture sprites (`ComicFigure.Sprite`)
| File | What | Good width |
|---|---|---|
| `furn_0` | bed | 90–110 |
| `furn_1` | wardrobe (tall) | 55–70 |
| `furn_2` | dresser | 60–80 |
| `furn_3` | bathtub | 90–110 |
| `furn_4` | sink | 40–55 |
| `furn_5` | bunk bed | 80–100 |
| `furn_6` | chest / trunk | 70–90 |
| `furn_7` | dollhouse | 70–90 |
| `furn_8` | wall shelves (very wide) | 200–260 |
| `furn_9` | desk | 100–130 |
| `furn_10` | grandfather clock | 40–55 |
| `furn_11` | armchair | 55–70 |
| `furn_12` | closet | 70–90 |
| `furn_13` | commode | 70–90 |
| `furn_14` | sofa | 110–140 |
| `furn_15` | fireplace | 80–100 |
| `furn_16` | piano | 90–110 |
| `furn_17` | long table | 120–160 |
| `furn_18` | cabinet (tall, thin) | 35–45 |
| `furn_19` | sink cabinet | 90–110 |
| `furn_20` | stove | 70–90 |
| `furn_21` | kitchen table | 90–110 |
| `furn_22` | ferns / plants | 70–90 |
| `furn_23` | planters | 100–120 |
| `furn_24` | stairs | 70–90 |
| `furn_25` | coat rack | 70–90 |
| `furn_26` | barrel | 40–55 |
| `furn_27` | pantry shelves (wide) | 160–200 |
| `furn_28` | laundry hamper | 45–55 |
| `furn_29` | water tank | 45–55 |

## Other
- `doll` — the cursed doll (24–32 px). `ring` — glow ring (use under the doll, ~90 px). `dot` — soft round dot (sun, steam, hearts; tint not available in comic figures).
- Characters: `ComicFigure.Kid(cousin, x, y)` — five fixed cousins (0 Márcio with straw hat, 1 pigtails, 2 leather hat, 3 cap and glasses, 4 cape);
  `ComicFigure.Unice(x, y)` — the aunt/seeker, standing. Use `flip: true` to face left. These are the **only** character sprites.

## Not available (do not invent)
Animals, vehicles, trees, food, tools or any outdoor props have **no sprite**. For a story with no matching sprite, build the panel
from the closest props (`furn_22` ferns for plants, `furn_26` barrel, `furn_17` long table for a market stall), kids and the Unice, and
rely on the caption to say the rest. Listing missing art in the report ("needs: umbu tree, well") is the right move for the art team.
