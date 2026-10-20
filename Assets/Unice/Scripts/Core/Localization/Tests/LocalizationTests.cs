using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Unice
{
    /// <summary>The text table and the lookups on top of it: every key in every language, same placeholders, and a key for everything the game content names.</summary>
    [TestFixture]
    public class LocalizationTests
    {
        const string LanguagePrefKey = "unice.lang";
        int? savedLanguage;
        Language languageBefore;

        [SetUp]
        public void SetUp()
        {
            languageBefore = Localization.Current;
            savedLanguage = PlayerPrefs.HasKey(LanguagePrefKey) ? PlayerPrefs.GetInt(LanguagePrefKey) : (int?)null;
            Localization.Set(Language.Pt);
        }

        [TearDown]
        public void TearDown()
        {
            Localization.Set(languageBefore);
            if (savedLanguage.HasValue) PlayerPrefs.SetInt(LanguagePrefKey, savedLanguage.Value); else PlayerPrefs.DeleteKey(LanguagePrefKey);
            PlayerPrefs.Save();
        }

        static IEnumerable<KeyValuePair<string, string[]>> Entries => Localization.Strings;

        [Test]
        public void Strings_EveryKey_HasOneNonEmptyTextPerLanguage()
        {
            foreach (var entry in Entries)
            {
                Assert.AreEqual(3, entry.Value.Length, entry.Key + " must have pt, en and es");
                foreach (var text in entry.Value) Assert.IsFalse(string.IsNullOrWhiteSpace(text), entry.Key + " has an empty text");
            }
        }

        [Test]
        public void Strings_EveryKey_UsesTheSamePlaceholdersInEveryLanguage()
        {
            foreach (var entry in Entries)
            {
                var perLanguage = entry.Value.Select(text => string.Join(",", Regex.Matches(text, @"\{\d+\}").Cast<Match>().Select(match => match.Value).Distinct().OrderBy(value => value))).ToArray();
                Assert.AreEqual(perLanguage[0], perLanguage[1], entry.Key + ": pt and en differ");
                Assert.AreEqual(perLanguage[0], perLanguage[2], entry.Key + ": pt and es differ");
            }
        }

        [Test]
        public void Get_UnknownKey_ReturnsAVisiblePlaceholderInsteadOfThrowing()
        {
            Assert.AreEqual("⟨missing:no.such.key⟩", Localization.Get("no.such.key"));
        }

        [TestCase(Language.Pt, 0)]
        [TestCase(Language.En, 1)]
        [TestCase(Language.Es, 2)]
        public void Get_KnownKey_ReturnsTheCurrentLanguage(Language language, int index)
        {
            Localization.Set(language);

            Assert.AreEqual(Localization.Strings["hud.map_btn"][index], Localization.Get("hud.map_btn"));
        }

        [Test]
        public void Get_WithArguments_FillsThePlaceholders()
        {
            Assert.AreEqual("Sala ABCD", Localization.Get("hud.room_online", "ABCD"));
        }

        [Test]
        public void Set_NewLanguage_IsRememberedAndAnnounced()
        {
            int announced = 0;
            void Count() => announced++;
            Localization.LanguageChanged += Count;

            Localization.Set(Language.Es);
            Localization.LanguageChanged -= Count;

            Assert.AreEqual(Language.Es, Localization.Current, "current");
            Assert.AreEqual((int)Language.Es, PlayerPrefs.GetInt(LanguagePrefKey), "saved");
            Assert.AreEqual(1, announced, "announced once");
        }

        [TestCase(1, "1st")] [TestCase(2, "2nd")] [TestCase(3, "3rd")] [TestCase(4, "4th")]
        [TestCase(11, "11th")] [TestCase(12, "12th")] [TestCase(13, "13th")]
        [TestCase(21, "21st")] [TestCase(22, "22nd")] [TestCase(23, "23rd")] [TestCase(101, "101st")] [TestCase(111, "111th")]
        public void Ordinal_English_FollowsTheSuffixRules(int number, string expected)
        {
            Localization.Set(Language.En);

            Assert.AreEqual(expected, Localization.Ordinal(number));
        }

        [TestCase(Language.Pt)] [TestCase(Language.Es)]
        public void Ordinal_PortugueseAndSpanish_UseTheOrdinalSign(Language language)
        {
            Localization.Set(language);

            Assert.AreEqual("3º", Localization.Ordinal(3));
        }

        static IEnumerable<MapDefinition> AllMaps() => Maps.AllIds.Select(Maps.ById);

        [TestCaseSource(nameof(AllMaps))]
        public void MapContent_EveryRoomAndSpot_HasText(MapDefinition definition)
        {
            Map.Load(definition);

            foreach (var room in Map.Rooms)
            {
                StringAssert.DoesNotContain("missing", Localization.RoomPhrase(room), room.id + " phrase");
            }
            foreach (var spot in Map.Spots) StringAssert.DoesNotContain("missing", Localization.SpotPhrase(spot), "spot " + spot.type);
        }

        [Test]
        public void KitOptionName_EveryNamedSlotAndOption_HasText()
        {
            foreach (int slot in new[] { Kit.HairSlot, Kit.OutfitSlot, Kit.HatSlot, Kit.AccessorySlot })
                for (int option = 0; option < Kit.OptionCount[slot]; option++)
                    StringAssert.DoesNotContain("missing", Localization.KitOptionName(slot, option), $"slot {slot} option {option}");
        }
    }
}
