using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>The pause sheet: continue, sound setting, leave. Solo it freezes the match; online it only covers the screen.</summary>
    sealed class PauseMenu : UiPart
    {
        /// <summary>The full-screen dim; <see cref="Ui"/> pads it so the sheet stays inside the safe area.</summary>
        public readonly VisualElement Root;
        readonly Label titleLabel, noteLabel, roomLabel;
        readonly Button leaveButton;

        public PauseMenu(Ui ui) : base(ui)
        {
            // laid out in Resources/UI/Screens/PauseMenu.uxml (UI Builder)
            Root = ui.LoadScreen("PauseMenu", ui.root);
            titleLabel = Root.Q<Label>("title"); noteLabel = Root.Q<Label>("note"); roomLabel = Root.Q<Label>("room");
            ui.Localize(Root.Q<Button>("continue"), "pause.continue").clicked += () => app.TogglePause();
            var settingsRow = Root.Q("settings");
            ui.settings.AddSoundButton(settingsRow, "btn", "btn-ghost");
            leaveButton = Root.Q<Button>("leave"); leaveButton.clicked += () => app.LeaveRoom();
            UiKit.Show(Root, false);
        }

        public void Show(bool visible, bool online)
        {
            UiKit.Show(Root, visible);
            titleLabel.text = Localization.Get(online ? "pause.online_title" : "pause.title");
            noteLabel.text = Localization.Get(online ? "pause.note_online" : "pause.note_local");
            roomLabel.text = online
                ? Localization.Get("hud.room_online", app.roomCode) + (app.game.isHost ? Localization.Get("hud.room_host_suffix") : "")
                : Localization.Get("hud.room_solo");
            leaveButton.text = Localization.Get(online ? "res.leave_room" : "pause.leave_match");
        }
    }
}
