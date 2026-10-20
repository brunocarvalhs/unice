using NUnit.Framework;

namespace Unice
{
    /// <summary>The words and numbers on the HUD's top cards, for each role and phase.</summary>
    [TestFixture]
    public class HudModelsTests
    {
        Game game;
        Character me;
        Perspective perspective;

        [SetUp]
        public void SetUp()
        {
            game = MatchFixture.NewMatch();
            me = MatchFixture.PlainHider(game);
            perspective = new Perspective(game, () => me);
        }

        RoleCardData Role() => HudModels.RoleCard(game, me, perspective);

        // ---------- match card ----------

        [Test]
        public void MatchCard_RoundOneOfFive_ShowsTheRound()
        {
            Assert.AreEqual("1/5", HudModels.MatchCard(game, me).round);
        }

        [Test]
        public void MatchCard_LiveRound_ShowsTheTimeLeftAsMinutesAndSeconds()
        {
            game.timeLeft = 83;

            Assert.AreEqual(TextFormat.FormatMinutesSeconds(83), HudModels.MatchCard(game, me).clock);
        }

        [Test]
        public void MatchCard_RoundIsOver_ShowsAZeroClock()
        {
            game.phase = Phase.Result; game.timeLeft = 50;

            Assert.AreEqual(TextFormat.FormatMinutesSeconds(0), HudModels.MatchCard(game, me).clock);
        }

        [TestCase(Phase.Hunt, 10f, true)]
        [TestCase(Phase.Hunt, 15f, true)]
        [TestCase(Phase.Hunt, 16f, false)]
        [TestCase(Phase.Hide, 5f, false)]
        public void MatchCard_ClockLow_OnlyInTheLastSecondsOfTheHunt(Phase phase, float timeLeft, bool expected)
        {
            game.phase = phase; game.timeLeft = timeLeft;

            Assert.AreEqual(expected, HudModels.MatchCard(game, me).clockLow);
        }

        [Test]
        public void MatchCard_HidePhase_SaysItIsTimeToHide()
        {
            game.phase = Phase.Hide;

            Assert.AreEqual(Localization.Get("hud.phase_hide"), HudModels.MatchCard(game, me).phase);
        }

        [Test]
        public void MatchCard_HuntAsTheUnice_SaysYouAreHunting()
        {
            game.phase = Phase.Hunt; me = game.unice;

            Assert.AreEqual(Localization.Get("hud.phase_hunt_you"), HudModels.MatchCard(game, me).phase);
        }

        [Test]
        public void MatchCard_HuntAsAHider_SaysTheUniceIsLooking()
        {
            game.phase = Phase.Hunt;

            Assert.AreEqual(Localization.Get("hud.phase_hunt_other"), HudModels.MatchCard(game, me).phase);
        }

        [Test]
        public void MatchCard_Result_SaysTheRoundEnded()
        {
            game.phase = Phase.Result;

            Assert.AreEqual(Localization.Get("hud.phase_result"), HudModels.MatchCard(game, me).phase);
        }

        [Test]
        public void MatchCard_PhaseWithNoLabel_ShowsNothing()
        {
            game.phase = Phase.Lobby;

            Assert.AreEqual("", HudModels.MatchCard(game, me).phase);
        }

        // ---------- role ----------

        [Test]
        public void RoleCard_Spectator_IsToldToWatch()
        {
            me = null;

            var data = Role();

            Assert.AreEqual(Localization.Get("hud.role.watch"), data.role, "role");
            Assert.AreEqual(Localization.Get("hud.goal.watch"), data.goal, "goal");
            Assert.AreEqual(RoleStyle.None, data.style, "style");
        }

        [Test]
        public void RoleCard_UniceDuringTheHideTime_ShowsTheWaitingGoal()
        {
            me = game.unice; game.phase = Phase.Hide;

            var data = Role();

            Assert.AreEqual(Localization.Get("hud.goal.unice_hide"), data.goal, "goal");
            Assert.AreEqual(RoleStyle.Unice, data.style, "style");
        }

        [Test]
        public void RoleCard_UniceHunting_ShowsTheHuntGoal()
        {
            me = game.unice; game.phase = Phase.Hunt;

            Assert.AreEqual(Localization.Get("hud.goal.unice_hunt"), Role().goal);
        }

        [Test]
        public void RoleCard_CaughtHider_IsOut()
        {
            me.outOfRound = true;

            var data = Role();

            Assert.AreEqual(Localization.Get("hud.role.caught"), data.role, "role");
            Assert.AreEqual(RoleStyle.Out, data.style, "style");
        }

        [Test]
        public void RoleCard_DollHolder_ShowsTheGiggleIntervalInTheGoal()
        {
            me = game.doll.holder;

            var data = Role();

            Assert.AreEqual(Localization.Get("hud.goal.holder", game.giggleInterval.ToString("0")), data.goal, "goal");
            Assert.AreEqual(RoleStyle.Holder, data.style, "style");
        }

        [TestCase(false, "hud.goal.hiding_out")]
        [TestCase(true, "hud.goal.hiding_spot")]
        public void RoleCard_Hider_GoalDependsOnBeingInASpot(bool inSpot, string goalKey)
        {
            if (inSpot) me.hidingSpot = Map.Spots[0];

            Assert.AreEqual(Localization.Get(goalKey), Role().goal);
        }

        // ---------- doll ----------

        [Test]
        public void RoleCard_TeamSeesTheGiggleMeter()
        {
            var data = Role();

            Assert.IsTrue(data.showGiggleMeter, "meter");
        }

        [Test]
        public void RoleCard_HuntingHalfwayToTheGiggle_MeterIsHalfFull()
        {
            game.phase = Phase.Hunt; game.giggleInterval = 10; game.doll.giggleTimer = 5;

            Assert.AreEqual(.5f, Role().giggleProgress, .001f);
        }

        [Test]
        public void RoleCard_HideTime_MeterIsEmpty()
        {
            game.phase = Phase.Hide;

            Assert.AreEqual(0f, Role().giggleProgress);
        }

        [Test]
        public void RoleCard_DollHidingInASpotDuringTheHunt_NotesTheCountdown()
        {
            game.phase = Phase.Hunt; game.doll.giggleTimer = 4.2f;
            game.doll.holder.hidingSpot = Map.Spots[0];

            Assert.AreEqual(Localization.Get("hud.giggle_in", Localization.SpotPhrase(Map.Spots[0]), 5), Role().note);
        }

        [Test]
        public void RoleCard_DollOutOfHiding_HasNoNote()
        {
            game.phase = Phase.Hunt; game.doll.holder.hidingSpot = null;

            Assert.AreEqual("", Role().note);
        }

        [Test]
        public void RoleCard_HolderOnPassCooldown_NotesWhenItCanBePassedAgain()
        {
            me = game.doll.holder; me.passCooldown = 2.5f; me.hidingSpot = null;
            game.phase = Phase.Hunt;

            Assert.AreEqual(Localization.Get("hud.repass_in", 3).TrimStart(), Role().note);
        }

        [Test]
        public void RoleCard_Unice_HasNoGiggleMeter()
        {
            me = game.unice; game.phase = Phase.Hunt;

            var data = Role();

            Assert.IsFalse(data.showGiggleMeter, "meter");
        }

        [Test]
        public void RoleCard_UniceAfterAGiggle_SeesWhereAndHowLongAgo()
        {
            me = game.unice; game.phase = Phase.Hunt;
            game.doll.hasLastGiggle = true; game.doll.lastGiggleX = 400; game.doll.lastGiggleY = 500;
            game.doll.lastGiggleTime = 10; game.roundClock = 17.9f;

            Assert.AreEqual(Localization.Get("hud.giggle_last", Localization.RoomPhrase(Map.RoomAt(400, 500)), 7), Role().note);
        }

        [Test]
        public void RoleCard_UniceBeforeAnyGiggle_HasNoNote()
        {
            me = game.unice; game.phase = Phase.Hunt; game.doll.hasLastGiggle = false;

            Assert.AreEqual("", Role().note);
        }

        // ---------- noise ----------

        [Test]
        public void RoleCard_RunningHider_ShowsTheNoiseMeterAsLoud()
        {
            me.moving = true; me.noiseLevel = .8f;

            var data = Role();

            Assert.IsTrue(data.showNoise, "shown");
            Assert.AreEqual(.8f, data.noiseProgress, .001f, "level");
            Assert.AreEqual(Localization.Get("hud.noise_loud"), data.noiseText, "text");
        }

        [Test]
        public void RoleCard_WalkingQuietly_ReadsAsQuiet()
        {
            me.moving = true; me.noiseLevel = .2f;

            Assert.AreEqual(Localization.Get("hud.noise_quiet"), Role().noiseText);
        }

        [Test]
        public void RoleCard_StandingStillAndSilent_HidesTheNoiseMeter()
        {
            me.moving = false; me.noiseLevel = 0;

            Assert.IsFalse(Role().showNoise);
        }

        [Test]
        public void RoleCard_NoiseStillFadingAfterStopping_KeepsTheMeterVisible()
        {
            me.moving = false; me.noiseLevel = .3f;

            Assert.IsTrue(Role().showNoise);
        }

        [Test]
        public void RoleCard_HiddenInASpot_HidesTheNoiseMeter()
        {
            me.hidingSpot = Map.Spots[0]; me.moving = true; me.noiseLevel = .9f;

            Assert.IsFalse(Role().showNoise);
        }

        [Test]
        public void RoleCard_TheUnice_HasNoNoiseMeter()
        {
            me = game.unice; me.moving = true;

            Assert.IsFalse(Role().showNoise);
        }

        [Test]
        public void RoleCard_NoiseLevelAboveOne_IsClamped()
        {
            me.moving = true; me.noiseLevel = 1.7f;

            Assert.AreEqual(1f, Role().noiseProgress);
        }

        // ---------- player list ----------

        [Test]
        public void PlayerStatusKey_Unice_IsTheUniceStatus()
        {
            Assert.AreEqual("hud.st_unice", HudModels.PlayerStatusKey(game.unice, false));
        }

        [Test]
        public void PlayerStatusKey_CaughtHider_IsOut()
        {
            me.outOfRound = true;

            Assert.AreEqual("hud.st_out", HudModels.PlayerStatusKey(me, true));
        }

        [Test]
        public void PlayerStatusKey_NoTeamInfo_HidersAreSomewhere()
        {
            Assert.AreEqual("hud.st_somewhere", HudModels.PlayerStatusKey(me, false));
        }

        [Test]
        public void PlayerStatusKey_HiderInASpot_IsHidden()
        {
            me.hidingSpot = Map.Spots[0];

            Assert.AreEqual("hud.st_hidden", HudModels.PlayerStatusKey(me, true));
        }

        [Test]
        public void PlayerStatusKey_StandingStill_IsStill()
        {
            me.moving = false;

            Assert.AreEqual("hud.st_still", HudModels.PlayerStatusKey(me, true));
        }

        [TestCase(true, "hud.st_sneaking")]
        [TestCase(false, "hud.st_running")]
        public void PlayerStatusKey_Moving_IsSneakingOrRunning(bool sneaking, string expected)
        {
            me.moving = true; me.sneaking = sneaking;

            Assert.AreEqual(expected, HudModels.PlayerStatusKey(me, true));
        }
    }
}
