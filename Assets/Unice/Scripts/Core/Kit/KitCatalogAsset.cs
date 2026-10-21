using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// Every kit option in the game — skin, hair, hair color, outfit, outfit color, hat, accessory — editable in the
    /// Inspector instead of hardcoded in <see cref="Kit"/>. Loaded once at boot by <c>KitCatalogLoader</c> from
    /// <c>Resources/Kit/KitCatalog</c>.
    ///
    /// Two rules for editing this asset:
    /// - At most <see cref="Kit.MaxOptionsPerSlot"/> entries per list — a kit is packed 3 bits/slot to fit one network int.
    /// - Always add a new entry at the END of its list. A player's chosen look is saved on their device as the option's
    ///   INDEX (see <c>ProfileStore</c>, <c>PlayerPrefs</c>); reordering or removing an entry silently changes what a
    ///   returning player sees.
    /// </summary>
    [CreateAssetMenu(fileName = "KitCatalog", menuName = "Unice/Kit Catalog")]
    public sealed class KitCatalogAsset : ScriptableObject
    {
        /// <summary>
        /// One option. <see cref="hex"/> is only read for the three color lists (<see cref="Skins"/>/<see cref="HairColors"/>/
        /// <see cref="OutfitColors"/>); <see cref="labelPt"/>/<see cref="labelEn"/>/<see cref="labelEs"/> only for the four
        /// style lists (<see cref="Hairs"/>/<see cref="Outfits"/>/<see cref="Hats"/>/<see cref="Accessories"/>), which have
        /// no name shown to players for color swatches. <see cref="special"/> gives KidSprites an extra drawing behavior
        /// (cape, backpack, glasses, teddy, cap, beanie, ears) to look up instead of hardcoding this option's position.
        /// </summary>
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("Stable name for this option; keeps its special behavior attached even if the list is reordered.")]
            public string id;
            [Tooltip("Hex color (e.g. #f3d3b8) — only used by Skins/Hair colors/Outfit colors.")]
            public string hex;
            [Tooltip("Shown to players — only used by Hairs/Outfits/Hats/Accessories.")]
            public string labelPt, labelEn, labelEs;
            [Tooltip("An extra drawing behavior KidSprites.Build looks for by this, not by index. None for colors and most style options.")]
            public KitSpecial special = KitSpecial.None;
        }

        [SerializeField] List<Entry> skins = new List<Entry>();
        [SerializeField] List<Entry> hairs = new List<Entry>();
        [SerializeField] List<Entry> hairColors = new List<Entry>();
        [SerializeField] List<Entry> outfits = new List<Entry>();
        [SerializeField] List<Entry> outfitColors = new List<Entry>();
        [SerializeField] List<Entry> hats = new List<Entry>();
        [SerializeField] List<Entry> accessories = new List<Entry>();

        public List<Entry> Skins => skins;
        public List<Entry> Hairs => hairs;
        public List<Entry> HairColors => hairColors;
        public List<Entry> Outfits => outfits;
        public List<Entry> OutfitColors => outfitColors;
        public List<Entry> Hats => hats;
        public List<Entry> Accessories => accessories;

        /// <summary>Catches an over-full list in the Inspector early; the real, fail-loud enforcement is <see cref="Kit.Apply"/>.</summary>
        void OnValidate()
        {
            CheckMax(skins, "Skins"); CheckMax(hairs, "Hairs"); CheckMax(hairColors, "Hair colors");
            CheckMax(outfits, "Outfits"); CheckMax(outfitColors, "Outfit colors"); CheckMax(hats, "Hats"); CheckMax(accessories, "Accessories");
        }

        void CheckMax(List<Entry> list, string listName)
        {
            if (list != null && list.Count > Kit.MaxOptionsPerSlot)
                Debug.LogWarning($"KitCatalogAsset: {listName} has {list.Count} options, more than the max of {Kit.MaxOptionsPerSlot} " +
                    "(a kit is packed 3 bits/slot for the network) — loading this catalog will fail.");
        }
    }
}
