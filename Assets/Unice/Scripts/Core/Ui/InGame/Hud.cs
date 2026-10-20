using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// In-game HUD: the match card (top left, with the giggle meter), the map/menu icon buttons (top right) with the role card under them, and the
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
            // laid out in Resources/UI/Screens/Hud.uxml (UI Builder)
            Root = IgnorePointer(ui.LoadScreen("Hud", ui.safeArea));
            matchCard = new MatchCard(ui, Root.Q("match-card"));
            roleCard = new RoleCard(ui, Root.Q("role-card"));
            IgnorePointer(Root.Q("buttons"));
            BindGlyphButton(Root.Q<Button>(MapButtonName), ActionIcon.Map, () => playerList.Toggle());
            BindGlyphButton(Root.Q<Button>(MenuButtonName), ActionIcon.Menu, () => app.TogglePause());
            playerList = new PlayerListPanel(ui, Root.Q("plist"));
        }

        /// <summary>With keyboard and mouse the house plan and the player list stay open on the side; on touch the map button opens them.</summary>
        public void SetDocked(bool docked) => playerList.SetOpen(docked);

        /// <summary>Every 0.2 s while playing.</summary>
        public void Tick()
        {
            var role = HudModels.RoleCard(game, app.myCharacter, app.perspective);
            matchCard.Refresh(role); roleCard.Refresh(role); playerList.Refresh();
        }

        /// <summary>Rebuild the player list on the next tick even if nothing seems to have changed (e.g. the language did).</summary>
        public void Invalidate() => playerList.Invalidate();
    }
}
