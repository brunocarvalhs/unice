using System.Collections.Generic;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// Turns a <see cref="KitCatalogAsset"/> into <see cref="Kit"/>'s plain data (<see cref="Kit.Apply"/>) and feeds each
    /// style entry's label into <see cref="Localization"/>'s string table under the same "kit.&lt;slot&gt;.&lt;index&gt;"
    /// keys <see cref="Localization.KitOptionName"/> already reads — so that method needs no change, and the asset's
    /// labels are a drop-in replacement for <c>Localization.Strings.cs</c>'s hardcoded ones.
    /// </summary>
    public static class KitCatalogLoader
    {
        const string ResourcesPath = "Kit/KitCatalog";

        /// <summary>
        /// Loads Resources/Kit/KitCatalog and applies it to <see cref="Kit"/> and <see cref="Localization"/>. Call this as
        /// the very first thing in <c>App.Awake</c>, before anything (including <c>ProfileStore</c>'s constructor) reads
        /// <see cref="Kit.OptionCount"/>/<see cref="Kit.Skins"/>/etc.
        /// If the asset can't be found, logs an error and leaves <see cref="Kit"/> on its built-in defaults
        /// (<see cref="Kit.ApplyDefaults"/>, already applied by Kit's static constructor) rather than leaving it empty —
        /// a missing Resources folder should degrade to "today's game", not a crash at the next screen that reads Kit.
        /// </summary>
        public static void LoadAndApply()
        {
            var catalog = Resources.Load<KitCatalogAsset>(ResourcesPath);
            if (catalog == null)
            {
                Debug.LogError("KitCatalogLoader: Resources/" + ResourcesPath + " not found — using Kit's built-in defaults.");
                Kit.ApplyDefaults();
                return;
            }
            Apply(catalog);
        }

        /// <summary>The conversion itself, split out from <see cref="LoadAndApply"/> so a test can run it on a synthetic
        /// catalog (<c>ScriptableObject.CreateInstance</c>) without touching <c>Resources</c>.</summary>
        public static void Apply(KitCatalogAsset catalog)
        {
            Kit.Apply(new[]
            {
                ColorSlot(catalog.Skins),
                StyleSlot(catalog.Hairs, "hair"),
                ColorSlot(catalog.HairColors),
                StyleSlot(catalog.Outfits, "outfit"),
                ColorSlot(catalog.OutfitColors),
                StyleSlot(catalog.Hats, "hat"),
                StyleSlot(catalog.Accessories, "accessory"),
            });
        }

        static KitSlotData ColorSlot(List<KitCatalogAsset.Entry> entries)
        {
            var options = new KitOptionData[entries.Count];
            for (int i = 0; i < entries.Count; i++) options[i] = new KitOptionData(entries[i].id, entries[i].hex);
            return new KitSlotData(options);
        }

        /// <summary>A style slot's options (hair/outfit/hat/accessory): besides building the options, registers each
        /// entry's pt/en/es label into Localization under "kit.&lt;slotKey&gt;.&lt;index&gt;".</summary>
        static KitSlotData StyleSlot(List<KitCatalogAsset.Entry> entries, string slotKey)
        {
            var options = new KitOptionData[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                options[i] = new KitOptionData(entry.id, "", entry.special);
                Localization.AddOrReplace("kit." + slotKey + "." + i, entry.labelPt, entry.labelEn, entry.labelEs);
            }
            return new KitSlotData(options);
        }
    }
}
