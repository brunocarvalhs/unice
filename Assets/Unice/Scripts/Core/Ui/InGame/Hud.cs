using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// In-game HUD: the match card (top left), the map/menu icon buttons (top right) with the role card under them, and the
    /// centered panel with the house plan and the player list. Refreshed a few times a second, not every frame.
    /// </summary>
    sealed class Hud : UiPart
    {
        /// <summary>Element names of the icon buttons (they have no text to find them by).</summary>
        public const string MapButtonName = "hud-map", MenuButtonName = "hud-menu";

        public readonly VisualElement Root;
        readonly MatchCard matchCard;
        readonly RoleCard roleCard;
        readonly PlayerListPanel playerList;

        public Hud(Ui ui) : base(ui)
        {
            Root = IgnorePointer(AddElement(ui.safeArea, "layer"));
            matchCard = new MatchCard(ui, Root);
            roleCard = new RoleCard(ui, Root);
            var buttons = IgnorePointer(AddElement(Root, "hud-btns"));
            AddGlyphButton(buttons, ActionIcon.Map, () => playerList.Toggle(), "hud-btn", "hud-icon").name = MapButtonName;
            AddGlyphButton(buttons, ActionIcon.Menu, () => app.TogglePause(), "hud-btn", "hud-icon").name = MenuButtonName;
            playerList = new PlayerListPanel(ui, Root);
        }

        /// <summary>Every 0.2 s while playing.</summary>
        public void Tick() { matchCard.Refresh(); roleCard.Refresh(); playerList.Refresh(); }

        /// <summary>Rebuild the player list on the next tick even if nothing seems to have changed (e.g. the language did).</summary>
        public void Invalidate() => playerList.Invalidate();
    }
}
