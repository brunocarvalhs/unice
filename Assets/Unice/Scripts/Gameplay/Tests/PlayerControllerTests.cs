using NUnit.Framework;

namespace Unice
{
    /// <summary>Moving the player, the noise it makes and the hide/pass/laugh/open actions, host side and guest side.</summary>
    [TestFixture]
    public class PlayerControllerTests
    {
        Game game;
        Character me;
        FakeStage stage;
        bool paused;
        int published;
        PlayerController controller;

        [SetUp]
        public void SetUp()
        {
            game = MatchFixture.NewMatch();
            me = MatchFixture.PlainHider(game);
            stage = new FakeStage();
            paused = false; published = 0;
            controller = new PlayerController(game, () => me, stage, () => paused, () => published++);
        }

        // ---------- Direction ----------

        [Test]
        public void Direction_InsideTheDeadZone_IsZero()
        {
            Assert.AreEqual((0f, 0f), PlayerController.Direction(0, 0, .1f, .1f));
        }

        [Test]
        public void Direction_OneKeyHeld_IsAUnitVectorOnThatAxis()
        {
            Assert.AreEqual((1f, 0f), PlayerController.Direction(1, 0, 0, 0));
        }

        [Test]
        public void Direction_TwoKeys_IsNormalizedSoDiagonalsAreNotFaster()
        {
            var (x, y) = PlayerController.Direction(1, 1, 0, 0);

            Assert.AreEqual(1f, System.MathF.Sqrt(x * x + y * y), .001f);
        }

        [Test]
        public void Direction_KeysAndJoystickPullingOppositeWays_CancelOut()
        {
            Assert.AreEqual((0f, 0f), PlayerController.Direction(1, 0, -1, 0));
        }

        // ---------- Move ----------

        [Test]
        public void Move_HiderWithInput_MovesAndIsLoudWhenRunning()
        {
            Map.Load(MapHouse.Build());
            float startX = me.x;

            controller.Move(.05f, (1, 0), false);

            Assert.IsTrue(me.moving, "moving");
            Assert.IsTrue(me.loud, "loud");
            Assert.Greater(me.x, startX, "moved right");
            Assert.AreEqual(1f, me.faceX, "faces right");
        }

        [Test]
        public void Move_HiderSneaking_MovesQuietly()
        {
            controller.Move(.05f, (1, 0), true);

            Assert.IsTrue(me.sneaking, "sneaking");
            Assert.IsFalse(me.loud, "not loud");
        }

        [Test]
        public void Move_NoInput_StandsStill()
        {
            float startX = me.x;

            controller.Move(.05f, (0, 0), false);

            Assert.IsFalse(me.moving, "moving");
            Assert.AreEqual(startX, me.x, "x");
        }

        [Test]
        public void Move_Paused_DoesNotMoveAndIsSilent()
        {
            paused = true; float startX = me.x;
            me.loud = true;

            controller.Move(.05f, (1, 0), false);

            Assert.AreEqual(startX, me.x, "x");
            Assert.IsFalse(me.loud, "loud");
        }

        [Test]
        public void Move_TheUnice_NeverSneaksAndIsNeverLoud()
        {
            me = game.unice;

            controller.Move(.05f, (1, 0), true);

            Assert.IsFalse(me.sneaking, "sneaking");
            Assert.IsFalse(me.loud, "loud");
        }

        [Test]
        public void Move_AfterHiding_IgnoresInputUntilTheFreezeEnds()
        {
            HideInNearestSpot();
            me.hidingSpot = null;   // out again, but the freeze keeps the character still
            float x = me.x;

            controller.Move(.05f, (1, 0), false);
            Assert.AreEqual(x, me.x, "frozen");

            controller.TickCooldowns(PlayerController.HideFreezeSeconds + .01f);
            controller.Move(.05f, (1, 0), false);
            Assert.AreNotEqual(x, me.x, "free again");
        }

        [Test]
        public void Move_GuestUniceTouchingAHider_AsksTheHostToCatchOnce()
        {
            game.isHost = false; game.phase = Phase.Hunt;
            me = game.unice;
            var hider = MatchFixture.PlainHider(game);
            hider.x = me.x; hider.y = me.y; hider.hidingSpot = null;

            controller.Move(.05f, (0, 0), false);
            controller.Move(.05f, (0, 0), false);

            Assert.AreEqual(PlayerAction.Catch, controller.action, "action");
            Assert.AreEqual(hider.index, controller.actionArg, "target");
            Assert.AreEqual(1, controller.actionSeq, "one request: the second step is inside the cooldown");
            Assert.AreEqual(1, published, "sent once");
        }

        // ---------- UpdateNoiseLevel ----------

        [Test]
        public void UpdateNoiseLevel_RunningCharacter_MovesTowardsOne()
        {
            me.loud = true; me.noiseLevel = 0;

            controller.UpdateNoiseLevel(.1f);

            Assert.Greater(me.noiseLevel, 0f);
            Assert.Less(me.noiseLevel, 1f);
        }

        [Test]
        public void UpdateNoiseLevel_QuietCharacter_FallsTowardsZero()
        {
            me.loud = false; me.noiseLevel = 1;

            controller.UpdateNoiseLevel(.1f);

            Assert.Less(me.noiseLevel, 1f);
        }

        [Test]
        public void UpdateNoiseLevel_NoCharacter_DoesNothing()
        {
            me = null;

            Assert.DoesNotThrow(() => controller.UpdateNoiseLevel(.1f));
        }

        // ---------- Reset ----------

        [Test]
        public void Reset_AfterAnAction_ClearsIt()
        {
            game.isHost = false;
            controller.TryLaugh();

            controller.Reset();

            Assert.AreEqual(0, controller.actionSeq, "seq");
            Assert.AreEqual(PlayerAction.None, controller.action, "action");
            Assert.AreEqual(-1, controller.actionArg, "arg");
        }

        // ---------- TryLaugh ----------

        [Test]
        public void TryLaugh_Ready_LogsItAndTheHostAppliesIt()
        {
            controller.TryLaugh();

            CollectionAssert.Contains(stage.LogNames, "player_action");
            Assert.AreEqual("laugh", stage.LogValue("player_action", "type"));
            Assert.AreEqual(0, controller.actionSeq, "a host applies directly, nothing to publish");
        }

        [Test]
        public void TryLaugh_Guest_QueuesTheActionAndAsksForAPublish()
        {
            game.isHost = false;

            controller.TryLaugh();

            Assert.AreEqual(PlayerAction.Laugh, controller.action, "action");
            Assert.AreEqual(1, controller.actionSeq, "seq");
            Assert.AreEqual(1, published, "published");
        }

        [Test]
        public void TryLaugh_OnCooldown_WarnsAndPlaysTheBadSound()
        {
            me.laughCooldown = 3;

            controller.TryLaugh();

            Assert.AreEqual(Localization.Get("act.laugh.cooldown", 3), stage.Toasts[0], "toast");
            CollectionAssert.Contains(stage.Sounds, "bad");
            CollectionAssert.DoesNotContain(stage.LogNames, "player_action");
        }

        [Test]
        public void TryLaugh_TheUnice_DoesNothing()
        {
            me = game.unice;

            controller.TryLaugh();

            Assert.IsEmpty(stage.Logs);
            Assert.IsEmpty(stage.Toasts);
        }

        [Test]
        public void TryLaugh_Paused_DoesNothing()
        {
            paused = true;

            controller.TryLaugh();

            Assert.IsEmpty(stage.Logs);
        }

        [Test]
        public void TryLaugh_CaughtPlayer_DoesNothing()
        {
            me.outOfRound = true;

            controller.TryLaugh();

            Assert.IsEmpty(stage.Logs);
        }

        // ---------- TryPassDoll ----------

        [Test]
        public void TryPassDoll_WithoutTheDoll_SaysSo()
        {
            controller.TryPassDoll();

            Assert.AreEqual(Localization.Get("act.pass.no_doll"), stage.Toasts[0]);
        }

        [Test]
        public void TryPassDoll_HolderOnCooldown_WarnsAndPlaysTheBadSound()
        {
            me = game.doll.holder; me.passCooldown = 2;

            controller.TryPassDoll();

            Assert.AreEqual(Localization.Get("act.pass.cooldown", 2), stage.Toasts[0], "toast");
            CollectionAssert.Contains(stage.Sounds, "bad");
        }

        [Test]
        public void TryPassDoll_HolderWithNobodyClose_SaysSo()
        {
            me = game.doll.holder; me.passCooldown = 0;
            foreach (var other in game.characters) if (other != me) { other.x = me.x + 2000; other.y = me.y; }

            controller.TryPassDoll();

            Assert.AreEqual(Localization.Get("act.pass.no_target"), stage.Toasts[0]);
        }

        [Test]
        public void TryPassDoll_HolderWithSomeoneClose_PassesTheDoll()
        {
            me = game.doll.holder; me.passCooldown = 0;
            var receiver = MatchFixture.OtherHider(game, me);
            receiver.x = me.x + 5; receiver.y = me.y; receiver.hidingSpot = null;

            controller.TryPassDoll();

            Assert.AreEqual("pass", stage.LogValue("player_action", "type"), "logged");
            Assert.AreSame(receiver, game.doll.holder, "the doll changed hands");
        }

        [Test]
        public void TryPassDoll_TheUnice_DoesNothing()
        {
            me = game.unice;

            controller.TryPassDoll();

            Assert.IsEmpty(stage.Toasts);
        }

        // ---------- UseNearbySpot ----------

        void PutMeAt(Furniture spot) { me.x = spot.standX; me.y = spot.standY; }

        Furniture FreeSpot() => System.Array.Find(Map.Spots, spot => spot.occupant == null);

        void HideInNearestSpot()
        {
            PutMeAt(FreeSpot());
            controller.UseNearbySpot();
        }

        [Test]
        public void UseNearbySpot_HiderNextToAFreeSpot_HidesAndMakesTheCreak()
        {
            HideInNearestSpot();

            Assert.IsNotNull(me.hidingSpot, "hiding");
            CollectionAssert.Contains(stage.Sounds, "creak");
            Assert.AreEqual("hide", stage.LogValue("player_action", "type"));
        }

        [Test]
        public void UseNearbySpot_AlreadyHiding_LeavesTheSpot()
        {
            HideInNearestSpot();
            stage.Logs.Clear();

            controller.UseNearbySpot();

            Assert.AreEqual("exit", stage.LogValue("player_action", "type"));
        }

        [Test]
        public void UseNearbySpot_NothingNear_SaysSo()
        {
            me.x = 20; me.y = 20;

            controller.UseNearbySpot();

            Assert.AreEqual(Localization.Get("act.hide.need_near"), stage.Toasts[0]);
        }

        [Test]
        public void UseNearbySpot_SpotTakenBySomeoneElse_NamesThemAndPlaysTheBadSound()
        {
            var spot = FreeSpot();
            var other = MatchFixture.OtherHider(game, me);
            spot.occupant = other;
            PutMeAt(spot);

            controller.UseNearbySpot();

            Assert.AreEqual(Localization.Get("act.hide.occupied", other.name), stage.Toasts[0], "toast");
            CollectionAssert.Contains(stage.Sounds, "bad");
        }

        [Test]
        public void UseNearbySpot_UniceDuringTheHideTime_DoesNothing()
        {
            me = game.unice; game.phase = Phase.Hide;

            controller.UseNearbySpot();

            Assert.IsEmpty(stage.Logs);
            Assert.IsEmpty(stage.Toasts);
        }

        [Test]
        public void UseNearbySpot_UniceHuntingWithNothingNear_SaysSo()
        {
            me = game.unice; game.phase = Phase.Hunt; me.stunTime = 0;
            me.x = 20; me.y = 20;

            controller.UseNearbySpot();

            Assert.AreEqual(Localization.Get("act.unice.need_near"), stage.Toasts[0]);
        }

        [Test]
        public void UseNearbySpot_UniceHuntingNextToASpot_OpensIt()
        {
            me = game.unice; game.phase = Phase.Hunt; me.stunTime = 0;
            PutMeAt(FreeSpot());

            controller.UseNearbySpot();

            Assert.AreEqual("open", stage.LogValue("player_action", "type"));
        }

        [Test]
        public void UseNearbySpot_StunnedUnice_DoesNothing()
        {
            me = game.unice; game.phase = Phase.Hunt; me.stunTime = 2;
            PutMeAt(FreeSpot());

            controller.UseNearbySpot();

            Assert.IsEmpty(stage.Logs);
        }
    }
}
