using System;
using NUnit.Framework;

namespace Unice
{
    [TestFixture]
    public class KitTests
    {
        [Test]
        public void Clean_ValidValues_ReturnsUnchanged()
        {
            var k = new[] { 1, 2, 3, 4, 5, 2, 1 };
            CollectionAssert.AreEqual(k, Kit.Sanitize(k));
        }

        [Test]
        public void Clean_NullArray_ReturnsAllZeros() => CollectionAssert.AreEqual(new int[Kit.Slots], Kit.Sanitize(null));

        [Test]
        public void Clean_ShorterArray_PadsRemainderWithZeros()
        {
            var r = Kit.Sanitize(new[] { 1, 2 });
            CollectionAssert.AreEqual(new[] { 1, 2, 0, 0, 0, 0, 0 }, r);
        }

        [Test]
        public void Clean_LongerArray_IgnoresExtraElements()
        {
            var r = Kit.Sanitize(new[] { 1, 2, 3, 4, 5, 2, 1, 999, 999 });
            Assert.AreEqual(Kit.Slots, r.Length);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 2, 1 }, r);
        }

        [Test]
        public void Clean_NegativeValue_ClampsToZero() => CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 0, 0, 0 }, Kit.Sanitize(new[] { -1, -5, 0, 0, 0, 0, 0 }));

        [Test]
        public void Clean_ValueAtOrAboveMax_ClampsToZero()
        {
            // Max = { 4, 4, 5, 5, 8, 6, 4 }: values equal to Max (one past the valid range) fall back to 0.
            var r = Kit.Sanitize(new[] { 4, 4, 5, 5, 8, 6, 4 });
            CollectionAssert.AreEqual(new int[Kit.Slots], r);
        }

        [Test]
        public void Random_ReturnsArrayOfSlotsLength() => Assert.AreEqual(Kit.Slots, Kit.Random().Length);

        [Test]
        public void Random_EachSlotStaysWithinItsMax()
        {
            for (int n = 0; n < 200; n++)
            {
                var r = Kit.Random();
                for (int i = 0; i < Kit.Slots; i++) Assert.That(r[i], Is.InRange(0, Kit.OptionCount[i] - 1));
            }
        }

        [Test]
        public void Pack_Unpack_RoundTripsCleanValues()
        {
            var k = new[] { 3, 1, 4, 2, 7, 5, 3 };
            Assert.Less(Kit.Pack(k), 1 << (3 * Kit.Slots)); // fits in 3 bits/slot as documented
            CollectionAssert.AreEqual(k, Kit.Unpack(Kit.Pack(k)));
        }

        [Test]
        public void Pack_Unpack_RoundTripsAllZeros() => CollectionAssert.AreEqual(new int[Kit.Slots], Kit.Unpack(Kit.Pack(new int[Kit.Slots])));

        [Test]
        public void Unpack_AnyIntValue_AlwaysReturnsACleanArray()
        {
            // Unpack runs its 3-bit extraction through Clean(), so even garbage ints come back valid.
            foreach (int v in new[] { 0, -1, int.MaxValue, int.MinValue, 12345678 })
            {
                var r = Kit.Unpack(v);
                Assert.AreEqual(Kit.Slots, r.Length);
                for (int i = 0; i < Kit.Slots; i++) Assert.That(r[i], Is.InRange(0, Kit.OptionCount[i] - 1));
            }
        }

        [Test]
        public void Unpack_BitsBeyondSlotRange_AreIgnored()
        {
            // Slots * 3 = 21 bits are consumed; anything packed above bit 21 has no effect.
            int highBitNoise = 1 << 25;
            CollectionAssert.AreEqual(Kit.Unpack(0), Kit.Unpack(highBitNoise));
        }

        [Test]
        public void Pack_NullArray_ThrowsNullReferenceException()
        {
            // Unlike Clean(), Pack() has no null-guard: documenting the current (crashy) behavior, not fixing it.
            Assert.Throws<NullReferenceException>(() => Kit.Pack(null));
        }

        [Test]
        public void Pack_ArrayShorterThanSlots_ThrowsIndexOutOfRangeException()
        {
            // Unlike Clean(), Pack() doesn't bounds-check the input length.
            Assert.Throws<IndexOutOfRangeException>(() => Kit.Pack(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Pack_ValueAboveSevenForASlot_SilentlyWrapsViaBitmask()
        {
            // Pack masks each slot with "& 7": an unclamped value of 9 wraps around to 1 instead of
            // being rejected. Only reachable if a caller skips Clean()/Random() before packing.
            var k = new[] { 9, 0, 0, 0, 0, 0, 0 };
            Assert.AreEqual(1, Kit.Pack(k) & 7);
        }

        [Test]
        public void OutfitColor_ValidIndex_ReturnsMatchingColor()
        {
            var k = new[] { 0, 0, 0, 0, 3, 0, 0 };
            Assert.AreEqual(Kit.OutfitColors[3], Kit.OutfitColor(k));
        }

        [Test]
        public void OutfitColor_NullArray_ThrowsNullReferenceException() => Assert.Throws<NullReferenceException>(() => Kit.OutfitColor(null));

        [Test]
        public void OutfitColor_IndexOutOfRange_ThrowsIndexOutOfRangeException()
        {
            // OutfitColor() does no bounds-check of its own; it relies on callers having run Clean() first.
            var k = new[] { 0, 0, 0, 0, 99, 0, 0 };
            Assert.Throws<IndexOutOfRangeException>(() => Kit.OutfitColor(k));
        }

        // ---------- Apply (the catalog asset's loader calls this at boot) ----------

        [TearDown]
        public void RestoreDefaults() => Kit.ApplyDefaults();   // every Apply test below must not leak into later tests

        static KitSlotData Slot(params (string id, string value, KitSpecial special)[] options)
        {
            var array = new KitOptionData[options.Length];
            for (int i = 0; i < options.Length; i++) array[i] = new KitOptionData(options[i].id, options[i].value, options[i].special);
            return new KitSlotData(array);
        }

        static KitSlotData[] SyntheticCatalog(int outfitOptions = 2, KitSpecial outfit1Special = KitSpecial.Cape)
        {
            var outfitOptionsArray = new KitOptionData[outfitOptions];
            for (int i = 0; i < outfitOptions; i++) outfitOptionsArray[i] = new KitOptionData("outfit" + i, "", i == 1 ? outfit1Special : KitSpecial.None);
            return new[]
            {
                Slot(("skinA", "#111111", KitSpecial.None), ("skinB", "#222222", KitSpecial.None)),
                Slot(("hairA", "", KitSpecial.None)),
                Slot(("hcA", "#333333", KitSpecial.None)),
                new KitSlotData(outfitOptionsArray),
                Slot(("ocA", "#444444", KitSpecial.None)),
                Slot(("hatA", "", KitSpecial.None), ("hatB", "", KitSpecial.Beanie), ("hatC", "", KitSpecial.Ears)),
                Slot(("accA", "", KitSpecial.None), ("accB", "", KitSpecial.Backpack)),
            };
        }

        [Test]
        public void Apply_SyntheticCatalog_SetsOptionCountAndArraysFromIt()
        {
            Kit.Apply(SyntheticCatalog());

            CollectionAssert.AreEqual(new[] { 2, 1, 1, 2, 1, 3, 2 }, Kit.OptionCount);
            CollectionAssert.AreEqual(new[] { "#111111", "#222222" }, Kit.Skins);
            CollectionAssert.AreEqual(new[] { "hairA" }, Kit.Hairs);
            CollectionAssert.AreEqual(new[] { "outfit0", "outfit1" }, Kit.Outfits);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Cape }, Kit.OutfitSpecial);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Beanie, KitSpecial.Ears }, Kit.HatSpecial);
            CollectionAssert.AreEqual(new[] { KitSpecial.None, KitSpecial.Backpack }, Kit.AccessorySpecial);
        }

        [Test]
        public void Apply_MoreThanEightOptionsInASlot_Throws()
        {
            var tooMany = SyntheticCatalog(outfitOptions: Kit.MaxOptionsPerSlot + 1, outfit1Special: KitSpecial.None);
            Assert.Throws<InvalidOperationException>(() => Kit.Apply(tooMany));
        }

        [Test]
        public void Apply_ExactlyEightOptionsInASlot_Succeeds()
        {
            Assert.DoesNotThrow(() => Kit.Apply(SyntheticCatalog(outfitOptions: Kit.MaxOptionsPerSlot, outfit1Special: KitSpecial.None)));
            Assert.AreEqual(Kit.MaxOptionsPerSlot, Kit.OptionCount[Kit.OutfitSlot]);
        }

        [Test]
        public void Apply_WrongNumberOfSlots_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => Kit.Apply(new[] { Slot(("a", "", KitSpecial.None)) }));
        }

        [Test]
        public void Apply_EmptySlot_Throws()
        {
            var slots = SyntheticCatalog();
            slots[0] = new KitSlotData(new KitOptionData[0]);
            Assert.Throws<InvalidOperationException>(() => Kit.Apply(slots));
        }

        [Test]
        public void Apply_ThenSanitizeRandomPackUnpack_BehaveAgainstTheAppliedData()
        {
            Kit.Apply(SyntheticCatalog());

            CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 0, 0, 0 }, Kit.Sanitize(null));
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 1, 0, 0, 0 }, Kit.Sanitize(new[] { 0, 0, 0, 1, 0, 0, 0 }));
            // slot 5 (hat) only has 3 options now; index 5 is out of range and falls back to 0
            CollectionAssert.AreEqual(new int[Kit.Slots], Kit.Sanitize(new[] { 0, 0, 0, 0, 0, 5, 0 }));

            for (int n = 0; n < 50; n++)
            {
                var random = Kit.Random();
                for (int slot = 0; slot < Kit.Slots; slot++) Assert.That(random[slot], Is.InRange(0, Kit.OptionCount[slot] - 1));
            }

            var k = new[] { 1, 0, 0, 1, 0, 2, 1 };
            CollectionAssert.AreEqual(k, Kit.Unpack(Kit.Pack(k)));
            Assert.AreEqual("#444444", Kit.OutfitColor(new[] { 0, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void ApplyDefaults_AfterApply_RestoresTheBuiltInSlotsAndSpecials()
        {
            Kit.Apply(SyntheticCatalog());

            Kit.ApplyDefaults();

            CollectionAssert.AreEqual(new[] { 4, 4, 5, 5, 8, 6, 4 }, Kit.OptionCount);
            Assert.AreEqual(KitSpecial.Cape, Kit.OutfitSpecial[4]);
            Assert.AreEqual(KitSpecial.Cap, Kit.HatSpecial[1]);
            Assert.AreEqual(KitSpecial.Beanie, Kit.HatSpecial[2]);
            Assert.AreEqual(KitSpecial.Ears, Kit.HatSpecial[5]);
            Assert.AreEqual(KitSpecial.Backpack, Kit.AccessorySpecial[1]);
            Assert.AreEqual(KitSpecial.Glasses, Kit.AccessorySpecial[2]);
            Assert.AreEqual(KitSpecial.Teddy, Kit.AccessorySpecial[3]);
        }
    }
}
