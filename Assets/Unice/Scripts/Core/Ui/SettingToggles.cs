using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The sound / mic / whisper / language buttons. The same setting appears on several screens (menu, waiting room,
    /// pause), so every copy is registered here and they all update together.
    /// </summary>
    sealed class SettingToggles : UiPart
    {
        readonly List<Button> soundButtons = new List<Button>(), micButtons = new List<Button>(), whisperButtons = new List<Button>();
        readonly System.Action onLanguageChanged;
        /// <summary>Buttons that show an icon instead of text: repainted (and their tooltip refreshed) with the settings.</summary>
        readonly List<(VisualElement icon, Button button, Func<string> label)> iconButtons = new List<(VisualElement, Button, Func<string>)>();
        readonly List<Label> languageCodes = new List<Label>();

        public SettingToggles(Ui ui, System.Action onLanguageChanged) : base(ui) { this.onLanguageChanged = onLanguageChanged; }

        public Button AddSoundButton(VisualElement parent, params string[] classes) => Register(soundButtons, AddButton(parent, "", ToggleSound, classes));
        /// <summary>Personal choice: when on, the room hears you through proximity voice chat as long as you're not hidden. Not added (null) without voice.</summary>
        public Button AddMicButton(VisualElement parent, params string[] classes) => app.voice == null ? null : Register(micButtons, AddButton(parent, "", ToggleMic, classes));
        /// <summary>Personal choice: "whisper" transmits quieter and never counts as noise for the Unice. Needs the mic on. Not added (null) without voice.</summary>
        public Button AddWhisperButton(VisualElement parent, params string[] classes) => app.voice == null ? null : Register(whisperButtons, AddButton(parent, "", ToggleWhisper, classes));

        /// <summary>The sound toggle as a speaker icon (crossed out when off); the text is only its tooltip.</summary>
        public Button AddSoundIconButton(VisualElement parent, params string[] classes) =>
            AddIconButton(parent, ToggleSound, () => app.sfx.Enabled ? ActionIcon.SoundOn : ActionIcon.SoundOff,
                () => Localization.Get(app.sfx.Enabled ? "menu.sound_on" : "menu.sound_off"), classes);

        /// <summary>The language toggle as a globe followed by the current language code (PT / EN / ES).</summary>
        public Button AddLanguageIconButton(VisualElement parent, params string[] classes)
        {
            var button = AddIconButton(parent, CycleLanguage, () => ActionIcon.Globe, () => Localization.Get("menu.lang_prefix") + ": " + Localization.Current.ToString().ToUpperInvariant(), classes);
            button.style.flexDirection = FlexDirection.Row;
            languageCodes.Add(AddLabel(button, "", "mpill-code"));
            return button;
        }

        /// <summary>A button with a vector icon child instead of text. <paramref name="icon"/> is read at every repaint, so it follows the setting.</summary>
        Button AddIconButton(VisualElement parent, Action onClick, Func<ActionIcon> icon, Func<string> label, string[] classes)
        {
            var button = AddButton(parent, "", onClick, classes);
            button.AddToClassList("mpill-icon");
            var glyph = IgnorePointer(AddElement(button, "mpill-glyph"));
            glyph.generateVisualContent += context => ActionIcons.Draw(context, icon(), button.resolvedStyle.color);
            // the glyph takes its color from the button, which changes with hover and focus
            button.RegisterCallback<PointerEnterEvent>(e => glyph.MarkDirtyRepaint()); button.RegisterCallback<PointerLeaveEvent>(e => glyph.MarkDirtyRepaint());
            button.RegisterCallback<FocusInEvent>(e => glyph.MarkDirtyRepaint()); button.RegisterCallback<FocusOutEvent>(e => glyph.MarkDirtyRepaint());
            iconButtons.Add((glyph, button, label));
            return button;
        }

        static Button Register(List<Button> list, Button button) { list.Add(button); return button; }

        void ToggleSound() { app.sfx.Enabled = !app.sfx.Enabled; Refresh(); }
        void ToggleMic() { app.MicOn = !app.MicOn; Refresh(); }
        void ToggleWhisper() { app.Whisper = !app.Whisper; Refresh(); }
        void CycleLanguage()
        {
            var next = (Language)(((int)Localization.Current + 1) % 3);
            Localization.Set(next);
            onLanguageChanged();
        }

        /// <summary>Re-applies every button's text or icon (and the whisper buttons' enabled state) from the current settings.</summary>
        public void Refresh()
        {
            string sound = Localization.Get(app.sfx.Enabled ? "menu.sound_on" : "menu.sound_off");
            string mic = Localization.Get(app.MicOn ? "menu.mic_on" : "menu.mic_off");
            string whisper = Localization.Get(app.Whisper ? "menu.voice_whisper" : "menu.voice_normal");
            foreach (var button in soundButtons) button.text = sound;
            foreach (var button in micButtons) button.text = mic;
            foreach (var button in whisperButtons) { button.text = whisper; button.SetEnabled(app.MicOn); }
            foreach (var code in languageCodes) code.text = Localization.Current.ToString().ToUpperInvariant();
            foreach (var (glyph, button, label) in iconButtons) { button.tooltip = label(); glyph.MarkDirtyRepaint(); }
        }
    }
}
