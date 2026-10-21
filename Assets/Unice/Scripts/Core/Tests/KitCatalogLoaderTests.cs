using NUnit.Framework;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// Headless coverage for <see cref="KitCatalogLoader.Apply"/> (the conversion from a <see cref="KitCatalogAsset"/> to
    /// <see cref="Kit"/>'s data and Localization's string table) on a synthetic asset built with
    /// <see cref="ScriptableObject.CreateInstance{T}"/> — no Resources, no Play Mode needed.
    /// Restores both Kit and Localization's real "kit.*" entries afterward so this doesn't leak into other tests.
    /// </summary>
    [TestFixture]
    public class KitCatalogLoaderTests
    {
        const string LanguagePrefKey = "unice.lang";
        int? savedLanguage;
        Language languageBefore;

        [SetUp]
        public void SetUp()
        {
            languageBefore = Localization.Current;
            savedLanguage = PlayerPrefs.HasKey(LanguagePrefKey) ? PlayerPrefs.GetInt(LanguagePrefKey) : (int?)null;
        }

        [TearDown]
        public void RestoreRealCatalog()
        {
            Kit.ApplyDefaults();
            Localization.Set(languageBefore);
            if (savedLanguage.HasValue) PlayerPrefs.SetInt(LanguagePrefKey, savedLanguage.Value); else PlayerPrefs.DeleteKey(LanguagePrefKey);
            PlayerPrefs.Save();
            Localization.AddOrReplace("kit.hair.0", "Curto", "Short", "Corto");
            Localization.AddOrReplace("kit.hair.1", "Rabo de cavalo", "Ponytail", "Cola de caballo");
            Localization.AddOrReplace("kit.hair.2", "Maria-chiquinha", "Pigtails", "Colitas");
            Localization.AddOrReplace("kit.hair.3", "Coque", "Bun", "Moño");
            Localization.AddOrReplace("kit.outfit.0", "Lisa", "Plain", "Lisa");
            Localization.AddOrReplace("kit.outfit.1", "Listrada", "Striped", "A rayas");
            Localization.AddOrReplace("kit.outfit.2", "Bolinhas", "Polka dot", "A lunares");
            Localization.AddOrReplace("kit.outfit.3", "Vestido", "Dress", "Vestido");
            Localization.AddOrReplace("kit.outfit.4", "Capa", "Cape", "Capa");
            Localization.AddOrReplace("kit.hat.0", "Nenhum", "None", "Ninguno");
            Localization.AddOrReplace("kit.hat.1", "Boné", "Cap", "Gorra");
            Localization.AddOrReplace("kit.hat.2", "Gorro", "Beanie", "Gorro");
            Localization.AddOrReplace("kit.hat.3", "Chapéu de palha", "Straw hat", "Sombrero de paja");
            Localization.AddOrReplace("kit.hat.4", "Chapéu de couro", "Leather hat", "Sombrero de cuero");
            Localization.AddOrReplace("kit.hat.5", "Orelhinhas", "Little ears", "Orejitas");
            Localization.AddOrReplace("kit.accessory.0", "Nenhum", "None", "Ninguno");
            Localization.AddOrReplace("kit.accessory.1", "Bornal", "Cloth bag", "Morral");
            Localization.AddOrReplace("kit.accessory.2", "Óculos", "Glasses", "Lentes");
            Localization.AddOrReplace("kit.accessory.3", "Boneca de pano", "Rag doll", "Muñeca de trapo");
        }

        static KitCatalogAsset BuildSyntheticCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<KitCatalogAsset>();
            catalog.Skins.Add(new KitCatalogAsset.Entry { id = "pale", hex = "#aaaaaa" });
            catalog.Skins.Add(new KitCatalogAsset.Entry { id = "deep", hex = "#333333" });

            catalog.Hairs.Add(new KitCatalogAsset.Entry { id = "spiky", labelPt = "Espetado", labelEn = "Spiky", labelEs = "Puntiagudo" });

            catalog.HairColors.Add(new KitCatalogAsset.Entry { id = "green", hex = "#00ff00" });

            catalog.Outfits.Add(new KitCatalogAsset.Entry { id = "plain", labelPt = "Lisa", labelEn = "Plain", labelEs = "Lisa" });
            catalog.Outfits.Add(new KitCatalogAsset.Entry { id = "cape", labelPt = "Capa", labelEn = "Cape", labelEs = "Capa", special = KitSpecial.Cape });

            catalog.OutfitColors.Add(new KitCatalogAsset.Entry { id = "red", hex = "#ff0000" });

            catalog.Hats.Add(new KitCatalogAsset.Entry { id = "none", labelPt = "Nenhum", labelEn = "None", labelEs = "Ninguno" });
            catalog.Hats.Add(new KitCatalogAsset.Entry { id = "beanie", labelPt = "Gorro", labelEn = "Beanie", labelEs = "Gorro", special = KitSpecial.Beanie });

            catalog.Accessories.Add(new KitCatalogAsset.Entry { id = "none", labelPt = "Nenhum", labelEn = "None", labelEs = "Ninguno" });
            catalog.Accessories.Add(new KitCatalogAsset.Entry { id = "teddy", labelPt = "Boneca de pano", labelEn = "Rag doll", labelEs = "Muñeca de trapo", special = KitSpecial.Teddy });

            return catalog;
        }

        [Test]
        public void Apply_SyntheticAsset_SetsKitArraysFromEntries()
        {
            KitCatalogLoader.Apply(BuildSyntheticCatalog());

            CollectionAssert.AreEqual(new[] { "#aaaaaa", "#333333" }, Kit.Skins);
            CollectionAssert.AreEqual(new[] { "spiky" }, Kit.Hairs);
            CollectionAssert.AreEqual(new[] { "#00ff00" }, Kit.HairColors);
            CollectionAssert.AreEqual(new[] { "plain", "cape" }, Kit.Outfits);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Cape }, Kit.OutfitSpecial);
            CollectionAssert.AreEqual(new[] { "#ff0000" }, Kit.OutfitColors);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Beanie }, Kit.HatSpecial);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Teddy }, Kit.AccessorySpecial);
            CollectionAssert.AreEqual(new[] { 2, 1, 1, 2, 1, 2, 2 }, Kit.OptionCount);
        }

        [Test]
        public void Apply_SyntheticAsset_RegistersEachStyleEntrysLabelIntoLocalization()
        {
            KitCatalogLoader.Apply(BuildSyntheticCatalog());

            Localization.Set(Language.En);
            Assert.AreEqual("Spiky", Localization.KitOptionName(Kit.HairSlot, 0));
            Assert.AreEqual("Cape", Localization.KitOptionName(Kit.OutfitSlot, 1));
            Assert.AreEqual("Beanie", Localization.KitOptionName(Kit.HatSlot, 1));
            Assert.AreEqual("Rag doll", Localization.KitOptionName(Kit.AccessorySlot, 1));

            Localization.Set(Language.Pt);
            Assert.AreEqual("Espetado", Localization.KitOptionName(Kit.HairSlot, 0));
        }
    }
}
