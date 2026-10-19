using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>What each match event makes this player see, hear and log, for each role (the Unice, the doll holder, a hider, a spectator).</summary>
    [TestFixture]
    public class EventFeedTests
    {
        Game game;
        Character me;
        FakeStage stage;
        Perspective perspective;
        EventFeed feed;

        [SetUp]
        public void SetUp()
        {
            game = MatchFixture.NewMatch();
            me = MatchFixture.PlainHider(game);
            stage = new FakeStage();
            perspective = new Perspective(game, () => me);
            perspective.Update();
            feed = new EventFeed(game, () => me, perspective, stage, stage);
            feed.PlayNewEvents();   // the first call only catches up with the round-start event NewMatch added
        }

        [Test]
        public void PlayNewEvents_FirstCall_OnlyCatchesUp()
        {
            Assert.IsEmpty(stage.Banners);
            Assert.IsEmpty(stage.Logs);
        }

        [Test]
        public void PlayNewEvents_SameEventTwice_PlaysItOnce()
        {
            game.AddEvent(GameEventType.Left, actor: MatchFixture.OtherHider(game, me).index);

            feed.PlayNewEvents();
            feed.PlayNewEvents();

            Assert.AreEqual(1, stage.Toasts.Count);
        }

        [Test]
        public void PlayNewEvents_AfterReset_CatchesUpAgainWithoutReplaying()
        {
            game.AddEvent(GameEventType.Left, actor: MatchFixture.OtherHider(game, me).index);

            feed.Reset();
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Toasts);
        }

        [Test]
        public void PlayNewEvents_NewHostRestartedTheNumbering_FollowsItAndPlaysTheNextEvent()
        {
            for (int i = 0; i < 60; i++) game.AddEvent(GameEventType.None);
            feed.PlayNewEvents();
            game.events.Clear(); game.eventSeq = 0;
            feed.PlayNewEvents();

            game.AddEvent(GameEventType.Left, actor: MatchFixture.OtherHider(game, me).index);
            feed.PlayNewEvents();

            Assert.AreEqual(1, stage.Toasts.Count);
        }

        // ---------- round start ----------

        [Test]
        public void RoundStart_PlayerIsTheUnice_GetsTheUniceBanner()
        {
            me = game.unice;

            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: game.doll.holder.index, value: 2);
            feed.PlayNewEvents();

            Assert.AreEqual((Localization.Get("ev.start.you_unice"), BannerStyle.Rose), (stage.Banners[0].text, stage.Banners[0].style));
        }

        [Test]
        public void RoundStart_PlayerHoldsTheDoll_GetsTheHolderBanner()
        {
            me = game.doll.holder;

            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: me.index, value: 2);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.start.you_holder"), stage.Banners[0].text);
        }

        [Test]
        public void RoundStart_PlayerIsAnotherHider_IsToldWhoTheUniceIs()
        {
            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: game.doll.holder.index, value: 2);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.start.other", game.unice.name), stage.Banners[0].text, "text");
            Assert.AreEqual(BannerStyle.Candle, stage.Banners[0].style, "style");
        }

        [Test]
        public void RoundStart_Spectator_GetsAnInfoBannerWithTheRound()
        {
            me = null;

            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: game.doll.holder.index, value: 3);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.start.watch", 3, game.unice.name), stage.Banners[0].text, "text");
            Assert.AreEqual(BannerStyle.Info, stage.Banners[0].style, "style");
        }

        [Test]
        public void RoundStart_Always_LogsTheRoundWithTheRole()
        {
            me = game.unice;

            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: game.doll.holder.index, value: 2);
            feed.PlayNewEvents();

            Assert.AreEqual("unice", stage.LogValue("round_start", "role"));
        }

        // ---------- the Unice comes in ----------

        [Test]
        public void UniceEnters_ForTheOthers_PlaysTheBoomAndTheCreakAtTheFrontDoor()
        {
            game.AddEvent(GameEventType.Enter);
            feed.PlayNewEvents();

            CollectionAssert.Contains(stage.Sounds, "boom");
            Assert.AreEqual(("creak", Map.Entrance.x), (stage.PlacedSounds[0].sound, stage.PlacedSounds[0].x));
            Assert.AreEqual(Localization.Get("ev.enter.others"), stage.Banners[0].text);
        }

        [Test]
        public void UniceEnters_ForTheUnice_GetsItsOwnBanner()
        {
            me = game.unice;

            game.AddEvent(GameEventType.Enter);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.enter.unice"), stage.Banners[0].text);
        }

        // ---------- giggle ----------

        [Test]
        public void Giggle_TheDollHolderGiggles_ShowsARippleATextAndATellToTheOthers()
        {
            var holder = game.doll.holder;

            game.AddEvent(GameEventType.Giggle, actor: holder.index, x: 400, y: 500);
            feed.PlayNewEvents();

            CollectionAssert.Contains(stage.Ripples, RippleKind.Giggle);
            Assert.AreEqual(1, stage.WorldTexts.Count, "floating text");
            Assert.AreEqual(Localization.Get("ev.giggle.toast_doll", Localization.RoomPhrase(Map.RoomAt(400, 500))), stage.Toasts[0]);
        }

        [Test]
        public void Giggle_SomeoneLaughsOnPurpose_NamesThemInTheToast()
        {
            var laugher = MatchFixture.OtherHider(game, me);
            if (laugher == game.doll.holder) laugher = game.characters.First(c => !c.IsUnice && c != me && c != game.doll.holder);

            game.AddEvent(GameEventType.Giggle, actor: laugher.index, x: 400, y: 500);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.giggle.toast_who", laugher.name, Localization.RoomPhrase(Map.RoomAt(400, 500))), stage.Toasts[0]);
        }

        [Test]
        public void Giggle_ThePlayerLaughsOnPurpose_GetsABanner()
        {
            game.AddEvent(GameEventType.Giggle, actor: me.index, x: 400, y: 500);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.giggle.laughed"), stage.Banners[0].text);
        }

        [Test]
        public void Giggle_ThePlayerHoldsTheDoll_GetsTheHolderBanner()
        {
            me = game.doll.holder;

            game.AddEvent(GameEventType.Giggle, actor: me.index, x: 400, y: 500);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.giggle.holder"), stage.Banners[0].text);
        }

        [Test]
        public void Giggle_ForTheUniceInALiveRound_TellsTheRoom()
        {
            me = game.unice;

            game.AddEvent(GameEventType.Giggle, actor: game.doll.holder.index, x: 400, y: 500);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.giggle.unice", Localization.RoomPhrase(Map.RoomAt(400, 500))), stage.Banners[0].text);
        }

        // ---------- pass ----------

        [Test]
        public void Pass_PlayerReceivesTheDoll_GetsABanner()
        {
            var giver = game.doll.holder;
            me = MatchFixture.OtherHider(game, giver);

            game.AddEvent(GameEventType.Pass, actor: giver.index, target: me.index);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.pass.you_got"), stage.Banners[0].text);
        }

        [Test]
        public void Pass_PlayerGivesTheDoll_GetsABannerWithTheReceiversName()
        {
            me = game.doll.holder;
            var receiver = MatchFixture.OtherHider(game, me);

            game.AddEvent(GameEventType.Pass, actor: me.index, target: receiver.index);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.pass.you_gave", receiver.name), stage.Banners[0].text);
        }

        [Test]
        public void Pass_BetweenOthers_TheTeamGetsAToast()
        {
            var giver = game.doll.holder;
            var receiver = game.characters.First(c => !c.IsUnice && c != giver && c != me);

            game.AddEvent(GameEventType.Pass, actor: giver.index, target: receiver.index);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.pass.toast", giver.name, receiver.name), stage.Toasts[0]);
        }

        [Test]
        public void Pass_BetweenOthers_TheUniceGetsNoToast()
        {
            me = game.unice;
            var giver = game.doll.holder;
            var receiver = game.characters.First(c => !c.IsUnice && c != giver);

            game.AddEvent(GameEventType.Pass, actor: giver.index, target: receiver.index);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Toasts);
        }

        [Test]
        public void Pass_WithAnUnknownCharacter_IsIgnored()
        {
            game.AddEvent(GameEventType.Pass, actor: 99, target: 98);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Toasts);
            Assert.IsEmpty(stage.Errors);
        }

        // ---------- open ----------

        [Test]
        public void Open_EmptySpotInView_ShowsTheEmptyMessage()
        {
            var spot = Map.Spots[0];
            me.x = spot.standX; me.y = spot.standY; perspective.Update();

            game.AddEvent(GameEventType.Open, actor: game.unice.index, target: -1, value: spot.spotIndex);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.open.empty"), stage.WorldTexts[0].text);
        }

        [Test]
        public void Open_SpotIndexOutOfRange_IsIgnored()
        {
            game.AddEvent(GameEventType.Open, actor: game.unice.index, target: -1, value: 999);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.PlacedSounds);
        }

        // ---------- catch ----------

        [Test]
        public void Catch_PlayerCaughtWithTheDoll_GetsTheStrongestFeedback()
        {
            game.AddEvent(GameEventType.Catch, actor: game.unice.index, target: me.index, value: 1, x: 300, y: 300);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.catch.you_doll"), stage.Banners[0].text, "banner");
            Assert.AreEqual(BannerStyle.Blood, stage.Banners[0].style, "style");
            Assert.AreEqual(16f, stage.Shakes[0], "shake");
            Assert.AreEqual(.4f, stage.Flashes[0].strength, .001f, "flash");
            CollectionAssert.Contains(stage.Sounds, "sting");
            CollectionAssert.Contains(stage.HitFlashes, me.index);
            Assert.AreEqual("caught", stage.LogValue("catch", "side"));
        }

        [Test]
        public void Catch_PlayerCaughtWithoutTheDoll_GetsTheOutBannerAndALighterShake()
        {
            game.AddEvent(GameEventType.Catch, actor: game.unice.index, target: me.index, value: 0, x: 300, y: 300);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.catch.you_out"), stage.Banners[0].text, "banner");
            Assert.AreEqual(10f, stage.Shakes[0], "shake");
        }

        [Test]
        public void Catch_TheUniceCatchesTheWrongHider_IsToldThemNotTheHolder()
        {
            me = game.unice;
            var caught = MatchFixture.PlainHider(game);

            game.AddEvent(GameEventType.Catch, actor: me.index, target: caught.index, value: 0, x: 300, y: 300);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.catch.not_holder", caught.name), stage.Banners[0].text, "banner");
            Assert.AreEqual("catcher", stage.LogValue("catch", "side"));
        }

        [Test]
        public void Catch_SomeoneElseCaught_TheTeamGetsAToastWithTheRoom()
        {
            var caught = game.characters.First(c => !c.IsUnice && c != me && c != game.doll.holder);

            game.AddEvent(GameEventType.Catch, actor: game.unice.index, target: caught.index, value: 0, x: 300, y: 300);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.catch.toast", caught.name, Localization.RoomPhrase(Map.RoomAt(300, 300))), stage.Toasts[0]);
            CollectionAssert.DoesNotContain(stage.LogNames, "catch");
        }

        [Test]
        public void Catch_NobodyCaught_IsIgnored()
        {
            game.AddEvent(GameEventType.Catch, actor: game.unice.index, target: -1, value: 0);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Sounds);
        }

        // ---------- round end / match end ----------

        [Test]
        public void RoundEnd_TimeRanOut_ShowsTheTimeBannerAndStopsTheHeartbeat()
        {
            game.AddEvent(GameEventType.End, value: 0);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.end.time"), stage.Banners[0].text, "banner");
            Assert.AreEqual(1, stage.HeartbeatStops, "heartbeat");
            Assert.AreEqual(false, stage.LogValue("round_end", "unice_won"), "logged as a hider win");
        }

        [Test]
        public void RoundEnd_UniceWon_HidersHearTheLoseJingle()
        {
            game.AddEvent(GameEventType.End, value: 1);
            feed.PlayNewEvents();

            CollectionAssert.Contains(stage.Sounds, "lose");
        }

        [Test]
        public void RoundEnd_UniceLost_HidersHearTheWinJingle()
        {
            game.AddEvent(GameEventType.End, value: 0);
            feed.PlayNewEvents();

            CollectionAssert.Contains(stage.Sounds, "win");
        }

        [Test]
        public void RoundEnd_Spectator_HearsNoJingle()
        {
            me = null;

            game.AddEvent(GameEventType.End, value: 1);
            feed.PlayNewEvents();

            CollectionAssert.DoesNotContain(stage.Sounds, "win");
            CollectionAssert.DoesNotContain(stage.Sounds, "lose");
        }

        [Test]
        public void RoundEnd_Duration_IsMeasuredFromTheRoundStart()
        {
            stage.Now = 100;
            game.AddEvent(GameEventType.Start, actor: game.unice.index, target: game.doll.holder.index, value: 2);
            feed.PlayNewEvents();

            stage.Now = 145;
            game.AddEvent(GameEventType.End, value: 0);
            feed.PlayNewEvents();

            Assert.AreEqual(45, stage.LogValue("round_end", "duration_s"));
        }

        [Test]
        public void MatchEnd_Player_LogsThePlaceAndTheFirstMatch()
        {
            stage.FirstMatch = true; me.score = 100;
            foreach (var other in game.characters) if (other != me) other.score = 0;

            game.AddEvent(GameEventType.Final);
            feed.PlayNewEvents();

            Assert.AreEqual(1, stage.LogValue("match_end", "place"), "place");
            CollectionAssert.Contains(stage.LogNames, "first_match_end");
        }

        [Test]
        public void MatchEnd_NotTheFirstMatch_DoesNotLogTheFirstMatch()
        {
            stage.FirstMatch = false;

            game.AddEvent(GameEventType.Final);
            feed.PlayNewEvents();

            CollectionAssert.DoesNotContain(stage.LogNames, "first_match_end");
        }

        [Test]
        public void MatchEnd_Spectator_LogsNothing()
        {
            me = null;

            game.AddEvent(GameEventType.Final);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Logs);
        }

        // ---------- host and leavers ----------

        [Test]
        public void Host_SomeoneElseTookOver_ToastsTheirName()
        {
            game.AddEvent(GameEventType.Host, text: "Beto");
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.host.other", "Beto"), stage.Toasts[0]);
            CollectionAssert.DoesNotContain(stage.LogNames, "host_changed");
        }

        [Test]
        public void Host_ThisDeviceTookOver_ToastsItAndLogsTheHandoverOnce()
        {
            stage.MyName = "Ana";

            game.AddEvent(GameEventType.Host, text: "Ana");
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.host.you"), stage.Toasts[0], "toast");
            CollectionAssert.Contains(stage.LogNames, "host_changed");
        }

        [Test]
        public void Left_KnownPlayer_ToastsTheirName()
        {
            var leaver = MatchFixture.OtherHider(game, me);

            game.AddEvent(GameEventType.Left, actor: leaver.index);
            feed.PlayNewEvents();

            Assert.AreEqual(Localization.Get("ev.left", leaver.name), stage.Toasts[0]);
        }

        [Test]
        public void Left_UnknownPlayer_ShowsNothing()
        {
            game.AddEvent(GameEventType.Left, actor: 99);
            feed.PlayNewEvents();

            Assert.IsEmpty(stage.Toasts);
        }
    }
}
