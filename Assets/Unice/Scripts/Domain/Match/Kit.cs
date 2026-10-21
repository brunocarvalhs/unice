namespace Unice
{
    /// <summary>
    /// An option's extra drawing behavior, looked up by <see cref="Kit.OutfitSpecial"/>/<see cref="Kit.HatSpecial"/>/
    /// <see cref="Kit.AccessorySpecial"/> (parallel to <see cref="Kit.Outfits"/>/<see cref="Kit.Hats"/>/
    /// <see cref="Kit.Accessories"/>) instead of <see cref="KidSprites"/> comparing a raw option number. <see cref="Cap"/>
    /// and <see cref="Beanie"/> share the outfit-colored tint on the hat's own layer; <see cref="Beanie"/> and
    /// <see cref="Ears"/> also draw an extra overlay layer (pompom / inner ear) on top of it.
    /// </summary>
    public enum KitSpecial { None, Cape, Backpack, Glasses, Teddy, Cap, Beanie, Ears }

    /// <summary>
    /// One option within a kit slot. <see cref="id"/> is a stable name for the option (reordering the catalog in the
    /// Inspector never changes which entry is "the cape"); <see cref="value"/> is the hex color for a color slot
    /// (skin/hair color/outfit color) and unused for a style slot. <see cref="special"/> is <see cref="KitSpecial.None"/>
    /// for every color option and for most style options.
    /// </summary>
    public readonly struct KitOptionData
    {
        public readonly string id, value;
        public readonly KitSpecial special;
        public KitOptionData(string id, string value, KitSpecial special = KitSpecial.None) { this.id = id; this.value = value; this.special = special; }
    }

    /// <summary>One kit slot's full option list, in <see cref="Kit"/>'s slot order (skin, hair, hair color, outfit, outfit color, hat, accessory).</summary>
    public readonly struct KitSlotData
    {
        public readonly KitOptionData[] options;
        public KitSlotData(KitOptionData[] options) { this.options = options; }
    }

    /// <summary>
    /// A child's look: one option index per slot (skin, hair style, hair color, outfit, outfit color, hat, accessory).
    /// The option lists below start out holding today's built-in values (see <see cref="ApplyDefaults"/>); the Core layer
    /// overwrites them at boot from <c>KitCatalogAsset</c> (see <see cref="Apply"/>) so new options can be added in the
    /// Inspector without touching this file. <see cref="Hairs"/>/<see cref="Outfits"/>/<see cref="Hats"/>/
    /// <see cref="Accessories"/> hold each option's id, not its display name — the name players see comes from
    /// <c>Localization.KitOptionName</c>.
    /// </summary>
    public static class Kit
    {
        public const int SkinSlot = 0, HairSlot = 1, HairColorSlot = 2, OutfitSlot = 3, OutfitColorSlot = 4, HatSlot = 5, AccessorySlot = 6;
        public const int Slots = 7;
        /// <summary>A kit is packed 3 bits per slot into one int for the network (see <see cref="Pack"/>), so no slot can have more than this many options.</summary>
        public const int MaxOptionsPerSlot = 8;

        public static string[] Skins, Hairs, HairColors, Outfits, OutfitColors, Hats, Accessories;
        /// <summary>How many options each slot has, in slot order.</summary>
        public static int[] OptionCount;
        /// <summary>Parallel to <see cref="Outfits"/>/<see cref="Hats"/>/<see cref="Accessories"/>: each option's extra drawing behavior (see <see cref="KitSpecial"/>).</summary>
        public static KitSpecial[] OutfitSpecial, HatSpecial, AccessorySpecial;

        static Kit() => ApplyDefaults();

        /// <summary>
        /// Resets every slot to today's built-in options (the same ids/hex/specials <c>KitCatalogSeed</c> migrates into
        /// <c>KitCatalogAsset</c>). Used as the fallback if <c>Resources/Kit/KitCatalog</c> can't be loaded at boot, and
        /// by any test that calls <see cref="Apply"/> with synthetic data and needs to restore the real catalog afterward.
        /// </summary>
        public static void ApplyDefaults() => Apply(DefaultSlots());

        static KitSlotData[] DefaultSlots() => new[]
        {
            ColorSlot("#f3d3b8", "#dcaa82", "#b07a52", "#6f4a33"),
            StyleSlot(("short", KitSpecial.None), ("ponytail", KitSpecial.None), ("pigtails", KitSpecial.None), ("bun", KitSpecial.None)),
            ColorSlot("#20150f", "#5b3a22", "#b5532a", "#e0b95a", "#4a6fb5"),
            StyleSlot(("plain", KitSpecial.None), ("striped", KitSpecial.None), ("polka", KitSpecial.None), ("dress", KitSpecial.None), ("cape", KitSpecial.Cape)),
            ColorSlot("#e9795f", "#6fa8dc", "#9fcf7a", "#f2c14e", "#b98ad8", "#5fc9b8", "#ef8fb4", "#e6e0d4"),
            StyleSlot(("none", KitSpecial.None), ("cap", KitSpecial.Cap), ("beanie", KitSpecial.Beanie), ("straw", KitSpecial.None), ("leather", KitSpecial.None), ("ears", KitSpecial.Ears)),
            StyleSlot(("none", KitSpecial.None), ("backpack", KitSpecial.Backpack), ("glasses", KitSpecial.Glasses), ("teddy", KitSpecial.Teddy)),
        };

        static KitSlotData ColorSlot(params string[] hexes)
        {
            var options = new KitOptionData[hexes.Length];
            for (int i = 0; i < hexes.Length; i++) options[i] = new KitOptionData(hexes[i], hexes[i]);
            return new KitSlotData(options);
        }

        static KitSlotData StyleSlot(params (string id, KitSpecial special)[] entries)
        {
            var options = new KitOptionData[entries.Length];
            for (int i = 0; i < entries.Length; i++) options[i] = new KitOptionData(entries[i].id, "", entries[i].special);
            return new KitSlotData(options);
        }

        static readonly string[] SlotNames = { "skin", "hair", "hairColor", "outfit", "outfitColor", "hat", "accessory" };

        /// <summary>
        /// Replaces every slot's options (ids/values/specials) with <paramref name="slots"/>'s, in this exact order: skin,
        /// hair, hair color, outfit, outfit color, hat, accessory. The Core layer calls this at boot from the loaded
        /// <c>KitCatalogAsset</c> (see <c>KitCatalogLoader</c>), before anything reads <see cref="OptionCount"/>/
        /// <see cref="Skins"/>/etc. Throws if a slot is missing, empty, or has more than <see cref="MaxOptionsPerSlot"/>
        /// options — a broken catalog must fail loudly instead of silently corrupting saved kits.
        /// </summary>
        public static void Apply(KitSlotData[] slots)
        {
            if (slots == null || slots.Length != Slots)
                throw new System.InvalidOperationException("Kit.Apply needs exactly " + Slots + " slots, got " + (slots?.Length ?? 0));
            for (int slot = 0; slot < Slots; slot++)
            {
                var options = slots[slot].options;
                if (options == null || options.Length == 0)
                    throw new System.InvalidOperationException("Kit.Apply: " + SlotNames[slot] + " has no options");
                if (options.Length > MaxOptionsPerSlot)
                    throw new System.InvalidOperationException(
                        "Kit.Apply: " + SlotNames[slot] + " has " + options.Length + " options, max is " + MaxOptionsPerSlot + " (a kit is packed 3 bits/slot for the network)");
            }

            var optionCount = new int[Slots];
            for (int slot = 0; slot < Slots; slot++) optionCount[slot] = slots[slot].options.Length;
            OptionCount = optionCount;

            Skins = ValuesOf(slots[SkinSlot]);
            Hairs = IdsOf(slots[HairSlot]);
            HairColors = ValuesOf(slots[HairColorSlot]);
            Outfits = IdsOf(slots[OutfitSlot]); OutfitSpecial = SpecialsOf(slots[OutfitSlot]);
            OutfitColors = ValuesOf(slots[OutfitColorSlot]);
            Hats = IdsOf(slots[HatSlot]); HatSpecial = SpecialsOf(slots[HatSlot]);
            Accessories = IdsOf(slots[AccessorySlot]); AccessorySpecial = SpecialsOf(slots[AccessorySlot]);
        }

        static string[] IdsOf(KitSlotData slot) { var r = new string[slot.options.Length]; for (int i = 0; i < r.Length; i++) r[i] = slot.options[i].id; return r; }
        static string[] ValuesOf(KitSlotData slot) { var r = new string[slot.options.Length]; for (int i = 0; i < r.Length; i++) r[i] = slot.options[i].value; return r; }
        static KitSpecial[] SpecialsOf(KitSlotData slot) { var r = new KitSpecial[slot.options.Length]; for (int i = 0; i < r.Length; i++) r[i] = slot.options[i].special; return r; }

        /// <summary>A kit with every slot in range (missing or invalid slots become 0). Kits from other players go through here.</summary>
        public static int[] Sanitize(int[] kit)
        {
            var clean = new int[Slots];
            for (int slot = 0; slot < Slots; slot++)
            {
                int option = kit != null && slot < kit.Length ? kit[slot] : 0;
                clean[slot] = option >= 0 && option < OptionCount[slot] ? option : 0;
            }
            return clean;
        }

        public static int[] Random()
        {
            var kit = new int[Slots];
            for (int slot = 0; slot < Slots; slot++) kit[slot] = GameRandom.Index(OptionCount[slot]);
            return kit;
        }

        /// <summary>3 bits per slot fits the whole kit in one int for the network.</summary>
        public static int Pack(int[] kit)
        {
            int packed = 0;
            for (int slot = 0; slot < Slots; slot++) packed |= (kit[slot] & 7) << (slot * 3);
            return packed;
        }

        public static int[] Unpack(int packed)
        {
            var kit = new int[Slots];
            for (int slot = 0; slot < Slots; slot++) kit[slot] = (packed >> (slot * 3)) & 7;
            return Sanitize(kit);
        }

        public static string OutfitColor(int[] kit) => OutfitColors[kit[OutfitColorSlot]];
    }
}
