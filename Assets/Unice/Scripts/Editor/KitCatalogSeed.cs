using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// One-time migration: writes Assets/Unice/Resources/Kit/KitCatalog.asset with today's hardcoded kit values —
    /// the same ids/hex as <see cref="Kit"/>'s built-in defaults, and the same labels as
    /// <c>Localization.Strings.cs</c>'s "kit.*" entries, copied verbatim (not retranslated). Run once via
    /// <c>-executeMethod Unice.KitCatalogSeed.CreateDefaultKitCatalog</c> (or the menu item below); the asset is the
    /// catalog's source of truth from here on — running this again overwrites it back to these literal values, so
    /// don't run it after an Inspector edit you want to keep.
    /// </summary>
    public static class KitCatalogSeed
    {
        const string AssetPath = "Assets/Unice/Resources/Kit/KitCatalog.asset";

        [MenuItem("Unice/Kit/Recriar catálogo padrão")]
        public static void CreateDefaultKitCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<KitCatalogAsset>();

            AddColors(catalog.Skins, "light", "#f3d3b8", "tan", "#dcaa82", "brown", "#b07a52", "dark", "#6f4a33");

            AddStyles(catalog.Hairs,
                ("short", "Curto", "Short", "Corto"),
                ("ponytail", "Rabo de cavalo", "Ponytail", "Cola de caballo"),
                ("pigtails", "Maria-chiquinha", "Pigtails", "Colitas"),
                ("bun", "Coque", "Bun", "Moño"));

            AddColors(catalog.HairColors, "black", "#20150f", "brown", "#5b3a22", "auburn", "#b5532a", "blonde", "#e0b95a", "blue", "#4a6fb5");

            AddStyles(catalog.Outfits,
                ("plain", "Lisa", "Plain", "Lisa"),
                ("striped", "Listrada", "Striped", "A rayas"),
                ("polka", "Bolinhas", "Polka dot", "A lunares"),
                ("dress", "Vestido", "Dress", "Vestido"),
                ("cape", "Capa", "Cape", "Capa"));
            catalog.Outfits[4].special = KitSpecial.Cape;

            AddColors(catalog.OutfitColors,
                "coral", "#e9795f", "sky", "#6fa8dc", "mint", "#9fcf7a", "gold", "#f2c14e",
                "lilac", "#b98ad8", "teal", "#5fc9b8", "pink", "#ef8fb4", "sand", "#e6e0d4");

            AddStyles(catalog.Hats,
                ("none", "Nenhum", "None", "Ninguno"),
                ("cap", "Boné", "Cap", "Gorra"),
                ("beanie", "Gorro", "Beanie", "Gorro"),
                ("straw", "Chapéu de palha", "Straw hat", "Sombrero de paja"),
                ("leather", "Chapéu de couro", "Leather hat", "Sombrero de cuero"),
                ("ears", "Orelhinhas", "Little ears", "Orejitas"));
            catalog.Hats[1].special = KitSpecial.Cap;
            catalog.Hats[2].special = KitSpecial.Beanie;
            catalog.Hats[5].special = KitSpecial.Ears;

            AddStyles(catalog.Accessories,
                ("none", "Nenhum", "None", "Ninguno"),
                ("backpack", "Bornal", "Cloth bag", "Morral"),
                ("glasses", "Óculos", "Glasses", "Lentes"),
                ("teddy", "Boneca de pano", "Rag doll", "Muñeca de trapo"));
            catalog.Accessories[1].special = KitSpecial.Backpack;
            catalog.Accessories[2].special = KitSpecial.Glasses;
            catalog.Accessories[3].special = KitSpecial.Teddy;

            Directory.CreateDirectory("Assets/Unice/Resources/Kit");
            if (AssetDatabase.LoadAssetAtPath<KitCatalogAsset>(AssetPath) != null) AssetDatabase.DeleteAsset(AssetPath);
            AssetDatabase.CreateAsset(catalog, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Unice: default kit catalog written to " + AssetPath);
        }

        static void AddColors(List<KitCatalogAsset.Entry> list, params string[] idsAndHexes)
        {
            for (int i = 0; i < idsAndHexes.Length; i += 2) list.Add(new KitCatalogAsset.Entry { id = idsAndHexes[i], hex = idsAndHexes[i + 1] });
        }

        static void AddStyles(List<KitCatalogAsset.Entry> list, params (string id, string pt, string en, string es)[] entries)
        {
            foreach (var (id, pt, en, es) in entries) list.Add(new KitCatalogAsset.Entry { id = id, labelPt = pt, labelEn = en, labelEs = es });
        }
    }
}
