using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The paper-doll sprite layers of a child, back to front, for a given kit and pose. Used by the in-game
    /// <see cref="CharacterView"/> and the menu <see cref="CharacterPortrait"/>.
    /// </summary>
    public static class KidSprites
    {
        /// <summary>Layer names, in drawing order (back to front).</summary>
        public static readonly string[] LayerOrder = { "cape", "packside", "legs", "body", "arm", "teddy", "packback", "head", "hair0", "hairX", "face", "glasses", "hat", "hatX" };

        /// <summary>Fills <paramref name="layers"/> (one entry per <see cref="LayerOrder"/> item): the sprite file (null = layer not used) and its tint.
        /// The six special sprites (cape, backpack, glasses, teddy, beanie, ears) and the cap's outfit-colored tint are picked by the
        /// selected option's <see cref="KitSpecial"/> (see <see cref="Kit.OutfitSpecial"/>/<see cref="Kit.HatSpecial"/>/<see cref="Kit.AccessorySpecial"/>),
        /// not by its raw index — a catalog entry keeps its special behavior even if it's reordered in the Inspector.</summary>
        /// <param name="showBack">Seen from behind.</param>
        /// <param name="frame">Walk animation frame (0 = standing).</param>
        public static void Build(int[] kit, bool showBack, int frame, (string file, Color tint)[] layers)
        {
            string view = showBack ? "b" : "s";
            Color skin = Art.HexColor(Kit.Skins[kit[Kit.SkinSlot]]), hair = Art.HexColor(Kit.HairColors[kit[Kit.HairColorSlot]]);
            Color outfit = Art.HexColor(Kit.OutfitColors[kit[Kit.OutfitColorSlot]]), white = Color.white;
            int hairStyle = kit[Kit.HairSlot], outfitStyle = kit[Kit.OutfitSlot], hat = kit[Kit.HatSlot], accessory = kit[Kit.AccessorySlot];
            KitSpecial outfitSpecial = Kit.OutfitSpecial[outfitStyle], hatSpecial = Kit.HatSpecial[hat], accessorySpecial = Kit.AccessorySpecial[accessory];

            layers[0] = (outfitSpecial == KitSpecial.Cape ? $"kid_cape_{view}_{frame}" : null, outfit);
            layers[1] = (accessorySpecial == KitSpecial.Backpack && !showBack ? "kid_packside_s" : null, white);
            layers[2] = ($"kid_legs_{view}_{frame}", white);
            layers[3] = ($"kid_body{outfitStyle}_{view}", outfit);
            layers[4] = ($"kid_arm_{view}_{frame}", skin);
            layers[5] = (accessorySpecial == KitSpecial.Teddy && !showBack ? "kid_teddy_s" : null, white);
            layers[6] = (accessorySpecial == KitSpecial.Backpack && showBack ? "kid_packback_b" : null, white);
            layers[7] = ($"kid_head_{view}", skin);
            layers[8] = ($"kid_hair0_{view}", hair);
            layers[9] = (hairStyle > 0 ? $"kid_hair{hairStyle}_{view}" : null, hair);
            layers[10] = (showBack ? null : "kid_face_s", white);
            layers[11] = (accessorySpecial == KitSpecial.Glasses && !showBack ? "kid_glasses_s" : null, white);
            layers[12] = (hat > 0 ? $"kid_hat{hat}_{view}" : null, hatSpecial == KitSpecial.Cap || hatSpecial == KitSpecial.Beanie ? outfit : hatSpecial == KitSpecial.Ears ? hair : white);
            layers[13] = (hatSpecial == KitSpecial.Beanie ? $"kid_hat2pom_{view}" : hatSpecial == KitSpecial.Ears ? $"kid_hat5in_{view}" : null, white);
        }
    }
}
