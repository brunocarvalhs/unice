using NUnit.Framework;

namespace Unice
{
    /// <summary>Where this device looks from, what it may see, what it may know, and how others are named.</summary>
    [TestFixture]
    public class PerspectiveTests
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

        [Test]
        public void ShowsTeamInfo_PlayerIsAHiderInALiveRound_IsTrue()
        {
            Assert.IsTrue(perspective.ShowsTeamInfo);
        }

        [Test]
        public void ShowsTeamInfo_PlayerIsTheUniceInALiveRound_IsFalse()
        {
            me = game.unice;

            Assert.IsFalse(perspective.ShowsTeamInfo);
        }

        [Test]
        public void ShowsTeamInfo_PlayerIsTheUniceButTheRoundIsOver_IsTrue()
        {
            me = game.unice;
            game.phase = Phase.Result;

            Assert.IsTrue(perspective.ShowsTeamInfo);
        }

        [Test]
        public void ShowsTeamInfo_PlayerWasCaught_IsTrue()
        {
            me.outOfRound = true;

            Assert.IsTrue(perspective.ShowsTeamInfo);
        }

        [Test]
        public void ShowsTeamInfo_NoCharacter_IsTrue()
        {
            me = null;

            Assert.IsTrue(perspective.ShowsTeamInfo);
        }

        [Test]
        public void CanSee_BeforeTheFirstUpdate_IsFalse()
        {
            Assert.IsFalse(perspective.CanSee(me.x, me.y));
        }

        [Test]
        public void Update_Hider_ViewIsTheirPositionWithTheHiderRadiusAndLineOfSight()
        {
            perspective.Update();

            Assert.AreEqual(me.x, perspective.view.x, "x");
            Assert.AreEqual(Rules.HiderViewRange, perspective.view.radius, "radius");
            Assert.IsTrue(perspective.view.usesLineOfSight, "line of sight");
            Assert.IsFalse(perspective.view.spectating, "spectating");
        }

        [Test]
        public void Update_Unice_UsesTheUniceRadius()
        {
            me = game.unice;

            perspective.Update();

            Assert.AreEqual(Rules.UniceViewRange, perspective.view.radius);
        }

        [Test]
        public void Update_HiddenInASpot_UsesTheShortHiddenRadius()
        {
            me.hidingSpot = Map.Spots[0];

            perspective.Update();

            Assert.AreEqual(Rules.HiddenViewRange, perspective.view.radius);
        }

        [Test]
        public void Update_CaughtPlayer_SpectatesTheDollHolderWithAPlainCircle()
        {
            me.outOfRound = true;
            var holder = game.doll.holder;

            perspective.Update();

            Assert.IsTrue(perspective.view.spectating, "spectating");
            Assert.IsFalse(perspective.view.usesLineOfSight, "plain circle");
            Assert.AreEqual(Rules.SpectatorViewRange, perspective.view.radius, "radius");
            Assert.AreEqual(holder.x, perspective.view.x, "follows the doll holder");
        }

        [Test]
        public void Update_SpectatorWithAChosenTarget_FollowsThatPlayer()
        {
            me.outOfRound = true;
            var target = MatchFixture.OtherHider(game, me);
            if (target == game.doll.holder) target = game.characters.Find(c => !c.IsUnice && c != me && c != game.doll.holder);
            perspective.spectateTargetId = target.playerId;

            perspective.Update();

            Assert.AreEqual(target.x, perspective.view.x);
        }

        [Test]
        public void Update_AtHomeWithNoCharacter_NeverGetsAView()
        {
            me = null;
            game.phase = Phase.Home;

            perspective.Update();

            Assert.IsFalse(perspective.hasView);
        }

        [Test]
        public void CanSee_PointInsideTheRadiusInTheOpen_IsTrue()
        {
            perspective.Update();

            Assert.IsTrue(perspective.CanSee(me.x, me.y));
        }

        [Test]
        public void CanSee_PointFarBeyondTheRadius_IsFalse()
        {
            perspective.Update();

            Assert.IsFalse(perspective.CanSee(me.x + Rules.HiderViewRange * 3, me.y));
        }

        [Test]
        public void NameFor_Nobody_IsSomeone()
        {
            Assert.AreEqual(Localization.Get("app.someone"), perspective.NameFor(null));
        }

        [Test]
        public void NameFor_ThePlayer_IsYou()
        {
            Assert.AreEqual(Localization.Get("app.you"), perspective.NameFor(me));
        }

        [Test]
        public void NameFor_AnotherCharacter_IsTheirName()
        {
            var other = MatchFixture.OtherHider(game, me);

            Assert.AreEqual(other.name, perspective.NameFor(other));
        }

        [Test]
        public void NameForCapitalized_ThePlayer_IsYouUpperCase()
        {
            Assert.AreEqual(Localization.Get("app.you.up"), perspective.NameForCapitalized(me));
        }

        [Test]
        public void NameForCapitalized_Nobody_IsSomeoneUpperCase()
        {
            Assert.AreEqual(Localization.Get("app.someone.up"), perspective.NameForCapitalized(null));
        }

        [Test]
        public void DrawnView_FollowingACharacter_MovesToWhereItIsDrawn()
        {
            perspective.Update();
            me.KeepPrevious();
            me.x += 10;

            var drawn = perspective.DrawnView(0);

            Assert.AreEqual(me.DrawX(0), drawn.x);
        }
    }
}
