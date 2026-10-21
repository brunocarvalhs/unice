# Art available in `Assets/Unice/Resources/Art`

> This catalog used to size furniture sprites for the intro comic's panels (`ComicFigure`/`ComicPanel`), which was removed from the
> game with no replacement. What's left below (the backdrops) is what a new story actually still uses, for `Story.cardArt`.

Only files listed here are guaranteed to exist (`StoryContentTests.Story_CardArtExists` checks `cardArt`).

## Backdrops (floor pictures — house rooms), valid for `cardArt`
`floor_master` (bedroom), `floor_bath`, `floor_kids`, `floor_library`, `floor_hall` (corridor, very wide), `floor_living`,
`floor_dining`, `floor_kitchen`, `floor_estufa` (greenhouse), `floor_entrance`, `floor_pantry`, `floor_laundry`.
Pass `null` as the backdrop to use the story map's flat ground colour (right for outdoor or non-house stories).
