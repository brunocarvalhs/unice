using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>What the player hears and is shown for sounds in the house, and the heartbeat of a hider with the Unice near.</summary>
    [TestFixture]
    public class SoundscapeTests
    {
        /// <summary>Records what the soundscape sent to the speakers, the cue and the caption.</summary>
        sealed class FakeOutput : ISoundOutput
        {
            public readonly List<(string sound, float volume, float pan, bool muffled)> Played = new List<(string, float, float, bool)>();
            public readonly List<(float angle, float loudness)> Cues = new List<(float, float)>();
            public readonly List<string> Captions = new List<string>();
            public void Play(string sound, float volume, float pan, bool muffled) => Played.Add((sound, volume, pan, muffled));
            public void ShowCue(float angle, float loudness, StageColor color) => Cues.Add((angle, loudness));
            public void Caption(string text) => Captions.Add(text);
        }

        Game game;
        Character me;
        FakeOutput output;
        Perspective perspective;
        Soundscape soundscape;

        /// <summary>The listener's own spot: open floor in the hall.</summary>
        static Character Place(Character character, float x, float y) { character.x = x; character.y = y; return character; }

        [SetUp]
        public void SetUp()
        {
            game = MatchFixture.NewMatch();
            me = MatchFixture.PlainHider(game);
            // everyone else far away so only what a test places can be heard
            foreach (var other in game.characters) if (other != me) Place(other, 5000, 5000);
            output = new FakeOutput();
            perspective = new Perspective(game, () => me);
            perspective.Update();
            soundscape = new Soundscape(game, () => me, perspective, new FakeStage(), output);
        }

        // ---------- PlayFrom ----------

        [Test]
        public void PlayFrom_NoViewYet_IsSilent()
        {
            var fresh = new Perspective(game, () => me);
            var silent = new Soundscape(game, () => me, fresh, new FakeStage(), output);

            Assert.AreEqual(0f, silent.PlayFrom(me.x, me.y, 300, "x", StageColor.Rose, "step"));
            Assert.IsEmpty(output.Played);
        }

        [Test]
        public void PlayFrom_RightOnTheListener_IsAtFullLoudness()
        {
            float loudness = soundscape.PlayFrom(me.x, me.y, 300, null, StageColor.Rose, "step");

            Assert.AreEqual(1f, loudness, .001f);
            Assert.AreEqual("step", output.Played[0].sound);
        }

        [Test]
        public void PlayFrom_BeyondTheRange_IsSilent()
        {
            float loudness = soundscape.PlayFrom(me.x + 400, me.y, 300, null, StageColor.Rose, "step");

            Assert.AreEqual(0f, loudness);
            Assert.IsEmpty(output.Played);
        }

        [Test]
        public void PlayFrom_BeyondTheRangeWithAMinimum_StillPlaysAtTheMinimum()
        {
            float loudness = soundscape.PlayFrom(me.x + 4000, me.y, 300, null, StageColor.Rose, "creak", .35f);

            Assert.AreEqual(.35f, loudness, .001f);
        }

        [Test]
        public void PlayFrom_SourceToTheRight_PansRight()
        {
            soundscape.PlayFrom(me.x + 100, me.y, 600, null, StageColor.Rose, "step");

            Assert.Greater(output.Played[0].pan, 0f);
        }

        [Test]
        public void PlayFrom_SourceToTheLeft_PansLeft()
        {
            soundscape.PlayFrom(me.x - 100, me.y, 600, null, StageColor.Rose, "step");

            Assert.Less(output.Played[0].pan, 0f);
        }

        [Test]
        public void PlayFrom_HiddenListener_HearsFurther()
        {
            float openAir = soundscape.PlayFrom(me.x + 330, me.y, 300, null, StageColor.Rose, "step");
            me.hidingSpot = Map.Spots[0];

            float hidden = soundscape.PlayFrom(me.x + 330, me.y, 300, null, StageColor.Rose, "step");

            Assert.AreEqual(0f, openAir, "out of range in the open");
            Assert.Greater(hidden, 0f, "in range while hidden");
        }

        [Test]
        public void PlayFrom_SourceInSightWithALabel_ShowsNoCueOrCaption()
        {
            soundscape.PlayFrom(me.x + 10, me.y, 300, "steps", StageColor.Candle, "step");

            Assert.IsEmpty(output.Cues, "cue");
            Assert.IsEmpty(output.Captions, "caption");
        }

        [Test]
        public void PlayFrom_SourceOutOfSightWithALabel_ShowsACueAndACaption()
        {
            // far enough to be outside the view radius but still audible
            soundscape.PlayFrom(me.x + Rules.HiderViewRange + 60, me.y, 900, "steps", StageColor.Candle, "step");

            Assert.AreEqual(1, output.Cues.Count, "cue");
            StringAssert.StartsWith("steps", output.Captions[0]);
        }

        [Test]
        public void PlayFrom_SourceOutOfSightWithoutALabel_ShowsNothing()
        {
            soundscape.PlayFrom(me.x + Rules.HiderViewRange + 60, me.y, 900, null, StageColor.Candle, "clock");

            Assert.IsEmpty(output.Cues);
        }

        [Test]
        public void PlayFrom_CaughtListener_GetsNoCue()
        {
            me.outOfRound = true;

            soundscape.PlayFrom(me.x + Rules.HiderViewRange + 60, me.y, 900, "steps", StageColor.Candle, "step");

            Assert.IsEmpty(output.Cues);
        }

        // ---------- Tick ----------

        [Test]
        public void Tick_AnotherHiderRunningNearby_PlaysFootstepsOnce()
        {
            var runner = MatchFixture.OtherHider(game, me);
            Place(runner, me.x + 100, me.y); runner.moving = true; runner.loud = true;

            soundscape.Tick(.05f);

            Assert.AreEqual(1, output.Played.Count(p => p.sound == "step"));
        }

        [Test]
        public void Tick_RunnerRightAfterAStep_WaitsForTheNextInterval()
        {
            var runner = MatchFixture.OtherHider(game, me);
            Place(runner, me.x + 100, me.y); runner.moving = true; runner.loud = true;

            soundscape.Tick(.05f);
            soundscape.Tick(.05f);

            Assert.AreEqual(1, output.Played.Count(p => p.sound == "step"));
        }

        [Test]
        public void Tick_SneakingHider_MakesNoSound()
        {
            var sneaker = MatchFixture.OtherHider(game, me);
            Place(sneaker, me.x + 100, me.y); sneaker.moving = true; sneaker.loud = false;

            soundscape.Tick(.05f);

            Assert.AreEqual(0, output.Played.Count(p => p.sound == "step"));
        }

        [Test]
        public void Tick_HiddenHider_MakesNoSound()
        {
            var hidden = MatchFixture.OtherHider(game, me);
            Place(hidden, me.x + 100, me.y); hidden.moving = true; hidden.loud = true; hidden.hidingSpot = Map.Spots[0];

            soundscape.Tick(.05f);

            Assert.AreEqual(0, output.Played.Count(p => p.sound == "step"));
        }

        [Test]
        public void Tick_UniceMoving_TheTeamHearsTheCane()
        {
            var unice = game.unice;
            Place(unice, me.x + 100, me.y); unice.moving = true;

            soundscape.Tick(.05f);

            Assert.AreEqual(1, output.Played.Count(p => p.sound == "cane"));
        }

        [Test]
        public void Tick_PlayerIsTheUniceDuringTheHunt_DoesNotHearTheirOwnCaneButSeesTheRunnersRipple()
        {
            me = game.unice; game.phase = Phase.Hunt; perspective.Update();
            var stage = new FakeStage();
            soundscape = new Soundscape(game, () => me, perspective, stage, output);
            var runner = MatchFixture.PlainHider(game);
            Place(runner, me.x + 100, me.y); runner.moving = true; runner.loud = true;

            soundscape.Tick(.05f);

            Assert.AreEqual(0, output.Played.Count(p => p.sound == "cane"), "cane");
            CollectionAssert.Contains(stage.Ripples, RippleKind.Step);
        }

        [Test]
        public void Tick_PlayerRunning_ShowsTheirOwnRipples()
        {
            var stage = new FakeStage();
            soundscape = new Soundscape(game, () => me, perspective, stage, output);
            me.loud = true;

            soundscape.Tick(.05f);

            CollectionAssert.Contains(stage.Ripples, RippleKind.Own);
        }

        [Test]
        public void Tick_LastSecondsOfTheHunt_TicksOncePerSecond()
        {
            game.phase = Phase.Hunt; game.timeLeft = 5.5f;

            soundscape.Tick(.05f);
            soundscape.Tick(.05f);
            game.timeLeft = 4.5f;
            soundscape.Tick(.05f);

            Assert.AreEqual(2, output.Played.Count(p => p.sound == "tick"));
        }

        [Test]
        public void Tick_PlentyOfTimeLeft_DoesNotTick()
        {
            game.phase = Phase.Hunt; game.timeLeft = 60;

            soundscape.Tick(.05f);

            Assert.AreEqual(0, output.Played.Count(p => p.sound == "tick"));
        }

        [Test]
        public void Tick_CloseToTheClock_HearsItTick()
        {
            var clock = Map.Clock;
            Place(me, clock.CenterX, clock.CenterY + 40);
            perspective.Update();

            soundscape.Tick(.05f);

            Assert.AreEqual(1, output.Played.Count(p => p.sound == "clock"));
        }

        // ---------- heartbeat ----------

        [Test]
        public void Heartbeat_BeatInterval_ShortensAsTheUniceGetsCloser()
        {
            Assert.Less(Heartbeat.BeatInterval(1), Heartbeat.BeatInterval(.2f));
        }

        [Test]
        public void Heartbeat_UniceFarAway_HasNoIntensityAndNoBeat()
        {
            game.phase = Phase.Hunt;
            var stage = new FakeStage();
            var heartbeat = new Heartbeat(game, () => me, stage);

            heartbeat.Tick(.05f);

            Assert.AreEqual(0f, heartbeat.Intensity, "intensity");
            Assert.IsEmpty(stage.Sounds);
        }

        [Test]
        public void Heartbeat_UniceRightNextToTheHider_BeatsAtFullIntensity()
        {
            game.phase = Phase.Hunt;
            Place(game.unice, me.x + 5, me.y);
            var stage = new FakeStage();
            var heartbeat = new Heartbeat(game, () => me, stage);

            heartbeat.Tick(.05f);

            Assert.Greater(heartbeat.Intensity, .9f, "intensity");
            Assert.AreEqual(1f, heartbeat.Pulse, "pulse");
            CollectionAssert.Contains(stage.Sounds, "heart");
        }

        [Test]
        public void Heartbeat_RightAfterABeat_WaitsBeforeTheNext()
        {
            game.phase = Phase.Hunt;
            Place(game.unice, me.x + 5, me.y);
            var stage = new FakeStage();
            var heartbeat = new Heartbeat(game, () => me, stage);

            heartbeat.Tick(.05f);
            heartbeat.Tick(.05f);

            Assert.AreEqual(1, stage.Sounds.Count(sound => sound == "heart"));
        }

        [Test]
        public void Heartbeat_PlayerIsTheUnice_FeelsNothing()
        {
            game.phase = Phase.Hunt; me = game.unice;
            var heartbeat = new Heartbeat(game, () => me, new FakeStage());

            heartbeat.Tick(.05f);

            Assert.AreEqual(0f, heartbeat.Intensity);
        }

        [Test]
        public void Heartbeat_Stop_ClearsTheIntensity()
        {
            game.phase = Phase.Hunt;
            Place(game.unice, me.x + 5, me.y);
            var heartbeat = new Heartbeat(game, () => me, new FakeStage());
            heartbeat.Tick(.05f);

            heartbeat.Stop();

            Assert.AreEqual(0f, heartbeat.Intensity);
        }

        [Test]
        public void Heartbeat_Fade_LowersThePulseDownToZero()
        {
            game.phase = Phase.Hunt;
            Place(game.unice, me.x + 5, me.y);
            var heartbeat = new Heartbeat(game, () => me, new FakeStage());
            heartbeat.Tick(.05f);

            heartbeat.Fade(.2f);
            float afterShortFade = heartbeat.Pulse;
            heartbeat.Fade(10);

            Assert.Less(afterShortFade, 1f, "fading");
            Assert.AreEqual(0f, heartbeat.Pulse, "gone");
        }
    }
}
