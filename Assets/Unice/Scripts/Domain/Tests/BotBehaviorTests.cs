using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>
    /// The bots' decisions one rule at a time: the Unice chases, tires, remembers, reacts to a giggle and keeps searching around a spot;
    /// a hider passes the doll, calms down, leaves a spot the doll gave away and bolts. Each test sets up the one situation the rule needs
    /// (positions, timers, goals) so the outcome does not depend on the dice, except where a test says it forces them.
    /// </summary>
    [TestFixture]
    public class BotBehaviorTests
    {
        const int TestSeed = 1234;
        /// <summary>An open spot of the house floor where two characters see each other.</summary>
        const float OpenX = 900, OpenY = 474;

        [SetUp]
        public void SetUp()
        {
            Map.Load(MapHouse.Build());
            GameRandom.Seed(TestSeed);
        }

        /// <summary>A match in the hunt: the bot is the Unice, the human is a hider standing <paramref name="gap"/> pixels to its right, a second bot holds the doll far away.</summary>
        static (Game game, Character unice, Character hider) UniceHunt(float gap = 40)
        {
            var game = new Game { isHost = true, botCount = 2, totalRounds = 5 };
            game.NewMatch(new List<Game.Human> { new Game.Human { id = "h1", name = "Ana", kit = null, joinTime = 0 } });
            var hider = game.characters.Find(c => !c.isBot);
            var bots = game.characters.Where(c => c.isBot).ToList();
            var unice = bots[0]; var farAway = bots[1];
            foreach (var c in game.characters) { c.role = c == unice ? Role.Unice : Role.Hider; c.hidingSpot = null; c.outOfRound = false; }
            // the doll is with a third character far out of sight, so catching the human is an ordinary catch and the round goes on
            farAway.x = farAway.targetX = 1600; farAway.y = farAway.targetY = 1100;
            game.unice = unice; game.doll.holder = farAway;
            unice.x = unice.targetX = OpenX; unice.y = unice.targetY = OpenY; unice.stunTime = 0; unice.tiredTime = 0;
            unice.brain.Reset();
            hider.x = hider.targetX = OpenX + gap; hider.y = hider.targetY = OpenY;
            game.phase = Phase.Hunt; game.timeLeft = 1000;
            return (game, unice, hider);
        }

        /// <summary>A match in the hunt: the human is the Unice and two bots hide, the first one close to it.</summary>
        static (Game game, Character unice, Character bot, Character mate) HiderHunt(float gap)
        {
            var game = new Game { isHost = true, botCount = 2, totalRounds = 5 };
            game.NewMatch(new List<Game.Human> { new Game.Human { id = "h1", name = "Ana", kit = null, joinTime = 0 } });
            var human = game.characters.Find(c => !c.isBot);
            var bots = game.characters.Where(c => c.isBot).ToList();
            foreach (var c in game.characters) { c.role = c == human ? Role.Unice : Role.Hider; c.hidingSpot = null; c.outOfRound = false; c.brain.Reset(); }
            game.unice = human; game.doll.holder = bots[1];
            human.x = human.targetX = OpenX; human.y = human.targetY = OpenY;
            bots[0].x = bots[0].targetX = OpenX + gap; bots[0].y = bots[0].targetY = OpenY;
            bots[1].x = bots[1].targetX = OpenX + gap + 30; bots[1].y = bots[1].targetY = OpenY;
            game.phase = Phase.Hunt; game.timeLeft = 1000;
            return (game, human, bots[0], bots[1]);
        }

        // ---------- the Unice bot ----------

        [Test]
        public void Unice_SeesAHiderInReach_ChasesAndRemembersWhereItWas()
        {
            var (game, unice, hider) = UniceHunt(gap: 120);

            game.Simulate(0.05f);

            Assert.AreSame(hider, unice.brain.chaseTarget);
            Assert.IsTrue(unice.brain.remembersTarget);
            Assert.AreEqual(hider.x, unice.brain.rememberedX, 0.01f);
            Assert.IsNull(unice.brain.goal, "while chasing it has no search goal");
        }

        [Test]
        public void Unice_ChasingForTooLong_GetsTiredAndChaseTimeStartsOver()
        {
            var (game, unice, _) = UniceHunt(gap: 120);
            unice.brain.chaseTime = 4.99f;

            game.Simulate(0.05f);

            Assert.Greater(unice.tiredTime, 0, "tired after more than five seconds of chase");
            Assert.AreEqual(0, unice.brain.chaseTime, 0.001f);
        }

        [Test]
        public void Unice_LosesTheHider_GoesToWhereItWasLastSeen()
        {
            var (game, unice, hider) = UniceHunt(gap: 120);
            game.Simulate(0.05f);
            var seenAtX = unice.brain.rememberedX; var seenAtY = unice.brain.rememberedY;
            hider.outOfRound = true;   // nobody left to see

            game.Simulate(0.05f);

            Assert.IsFalse(unice.brain.remembersTarget, "the memory is spent once it heads there");
            Assert.IsNotNull(unice.brain.goal);
            Assert.AreEqual(BotGoal.Kind.Position, unice.brain.goal.kind);
            Assert.AreEqual(seenAtX, unice.brain.goal.x, 0.01f);
            Assert.AreEqual(seenAtY, unice.brain.goal.y, 0.01f);
            Assert.GreaterOrEqual(unice.brain.goal.priority, 2, "a sighting beats patrolling");
        }

        [Test]
        public void Unice_HearsAGiggle_ReactsAfterAShortDelayWithAGoalAtTheGiggle()
        {
            var (game, unice, hider) = UniceHunt(gap: 500);
            hider.outOfRound = true;   // keep the Unice from chasing
            var spot = Map.Spots[1];
            game.doll.hasLastGiggle = true; game.doll.lastGiggleTime = 5; game.doll.lastGiggleSpot = spot;
            game.doll.lastGiggleX = spot.standX; game.doll.lastGiggleY = spot.standY;

            game.Simulate(0.016f);
            Assert.IsNotNull(unice.brain.pendingGoal, "it heard it, but takes a moment to react");
            Assert.AreEqual(5, unice.brain.lastGiggleHeardAt, 0.001f);
            Assert.AreSame(spot, unice.brain.pendingGoal.spot);
            Assert.IsTrue(unice.brain.pendingGoal.hasSearchCenter);

            for (int i = 0; i < 20 && unice.brain.pendingGoal != null; i++) game.Simulate(0.05f);

            Assert.IsNull(unice.brain.pendingGoal, "the reaction kicked in within a second");
            Assert.IsNotNull(unice.brain.goal);
            Assert.GreaterOrEqual(unice.brain.goal.priority, 3);
        }

        [Test]
        public void Unice_GigglePositionOnly_ReactsWithAPositionGoal()
        {
            var (game, unice, hider) = UniceHunt(gap: 500);
            hider.outOfRound = true;
            game.doll.hasLastGiggle = true; game.doll.lastGiggleTime = 5; game.doll.lastGiggleSpot = null; game.doll.lastGiggleX = 300; game.doll.lastGiggleY = 300;

            game.Simulate(0.016f);

            Assert.AreEqual(BotGoal.Kind.Position, unice.brain.pendingGoal.kind);
            Assert.AreEqual(300, unice.brain.pendingGoal.x, 0.001f);
        }

        [Test]
        public void Unice_AFinishedCheckWithHopsLeft_MovesOnToAnotherSpotNearby()
        {
            var (game, unice, hider) = UniceHunt(gap: 500);
            hider.outOfRound = true;
            var spot = Map.Spots[0];
            unice.x = spot.standX; unice.y = spot.standY;
            unice.brain.checkingSpot = spot; unice.brain.checkTimer = 0.01f;
            unice.brain.goal = new BotGoal { kind = BotGoal.Kind.Spot, spot = spot, priority = 1, hopsLeft = 1 };
            game.roundClock = 100;   // earlier checks of other spots are long forgotten

            game.Simulate(0.02f);

            Assert.IsNotNull(unice.brain.goal, "it picks the next spot around the one it just opened");
            Assert.AreEqual(BotGoal.Kind.Spot, unice.brain.goal.kind);
            Assert.AreNotSame(spot, unice.brain.goal.spot);
            Assert.AreEqual(0, unice.brain.goal.hopsLeft);
            Assert.IsTrue(unice.brain.goal.hasSearchCenter);
        }

        [Test]
        public void Unice_TouchingAHiderInTheOpen_CatchesIt()
        {
            var (game, unice, hider) = UniceHunt(gap: 5);
            unice.brain.checkTimer = 0;

            game.Simulate(0.02f);

            Assert.IsTrue(hider.outOfRound);
            Assert.AreEqual(1, unice.score);
            Assert.Greater(unice.stunTime, 0, "a catch stuns the Unice for a moment");
        }

        // ---------- the hider bots ----------

        [Test]
        public void Hider_HoldingTheDollWithTheUniceClose_PassesItToTheNearbyMate()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 120);
            game.doll.holder = bot; bot.passCooldown = 0;

            game.Simulate(0.05f);

            Assert.AreSame(mate, game.doll.holder, "cornered, the bot hands the doll over");
        }

        [Test]
        public void Hider_FleeingWithoutSeeingTheUniceForAWhile_GoesBackToHiding()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 1000);
            bot.brain.mode = HiderMode.Flee; bot.brain.lastSawUniceAt = -99;
            game.roundClock = 50;

            game.Simulate(0.05f);

            Assert.AreEqual(HiderMode.Seek, bot.brain.mode);
        }

        [Test]
        public void Hider_WithNoSpotLeft_StandsStill()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 1000);
            var taken = new List<Furniture>();
            foreach (var spot in Map.Spots) if (spot.occupant == null) { spot.occupant = mate; taken.Add(spot); }
            try
            {
                bot.moving = true;
                game.Simulate(0.05f);
                Assert.IsFalse(bot.moving, "every spot is taken: nowhere to go");
            }
            finally { foreach (var spot in taken) spot.occupant = null; }
        }

        [Test]
        public void Hider_DollHolderInASpotTheGiggleGaveAway_RunsToAnotherSpot()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 1000);
            var spot = Map.Spots[0];
            try
            {
                game.Hide(bot, spot); game.doll.holder = bot;
                bot.brain.leaveSpotAt = 1; game.roundClock = 2;

                game.Simulate(0.05f);

                Assert.IsNull(bot.hidingSpot, "it left the spot the doll gave away");
                Assert.AreEqual(0, bot.brain.leaveSpotAt, 0.001f);
                Assert.Greater(bot.brain.dashUntil, game.roundClock, "and it runs instead of sneaking");
                Assert.AreEqual(HiderMode.Seek, bot.brain.mode);
            }
            finally { spot.occupant = null; }
        }

        [Test]
        public void Hider_HiddenWithTheUniceRightOutside_StartlesOnceAndMayBolt()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 60);
            var spot = Map.Spots[0];
            try
            {
                game.Hide(bot, spot);
                unice.x = bot.x + 50; unice.y = bot.y;

                game.Simulate(0.05f);
                Assert.IsTrue(bot.brain.startled, "it only decides once per approach");

                for (int seed = 1; seed < 40 && bot.hidingSpot != null; seed++)
                {
                    GameRandom.Seed(seed); bot.brain.startled = false;
                    game.Simulate(0.05f);
                }
                Assert.IsNull(bot.hidingSpot, "within a handful of dice rolls it bolts (30% each)");
                Assert.AreEqual(HiderMode.Flee, bot.brain.mode);
            }
            finally { spot.occupant = null; }
        }

        [Test]
        public void Hider_HiddenAndTheUniceWalksAway_CalmsDown()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 60);
            var spot = Map.Spots[0];
            try
            {
                game.Hide(bot, spot);
                bot.brain.startled = true;
                unice.x = bot.x + 400; unice.y = bot.y;

                game.Simulate(0.05f);

                Assert.IsFalse(bot.brain.startled, "past the calm distance it can be startled again");
            }
            finally { spot.occupant = null; }
        }

        [Test]
        public void Hider_ArrivingAtASpotItCannotEnter_ReleasesItAndPicksAgain()
        {
            var (game, unice, bot, mate) = HiderHunt(gap: 1000);
            var spot = Map.Spots[0];
            bot.x = spot.standX + 200; bot.y = spot.standY;   // arrives (empty path) but nowhere near the spot's standing point
            bot.brain.targetSpot = spot; spot.reservedBy = bot; bot.brain.path.Clear();
            try
            {
                game.Simulate(0.05f);

                Assert.IsNull(spot.reservedBy, "the reservation is let go");
                Assert.IsNull(bot.brain.targetSpot, "and it will pick another spot next step");
                Assert.IsNull(bot.hidingSpot);
            }
            finally { spot.reservedBy = null; }
        }

        // ---------- small contracts ----------

        [Test]
        public void NetException_CarriesTheMessageForThePlayer()
        {
            Assert.AreEqual("Sala cheia", new NetException("Sala cheia").Message);
        }

        [Test]
        public void MatchHost_Reset_ForgetsTheActionsAlreadyApplied()
        {
            var game = new Game { isHost = true, botCount = 1, totalRounds = 5 };
            var host = new MatchHost(game);
            host.lastActionSeq["peer-1"] = 7;

            host.Reset();

            Assert.IsEmpty(host.lastActionSeq);
        }

        // ---------- walking ----------

        [Test]
        public void Walking_BlockedByAWallForTooLong_RetriesAndThenGivesUpOnThePath()
        {
            var (game, unice, hider) = UniceHunt(gap: 500);
            hider.outOfRound = true;
            // a waypoint straight through the house's outer wall: the bot walks into it and cannot move
            unice.x = 40; unice.y = 40;
            unice.brain.goal = new BotGoal { kind = BotGoal.Kind.Position, x = -300, y = 40, priority = 2, hopsLeft = 0 };
            unice.brain.path = new List<Point> { new Point(-300, 40), new Point(-400, 40) };
            bool gotStuck = false, gaveUp = false;

            for (int i = 0; i < 600 && !gaveUp; i++)
            {
                game.Simulate(0.05f);
                if (unice.brain.stuckCount > 0) gotStuck = true;
                else if (gotStuck) gaveUp = true;   // the counter starts over once it gives the destination up
            }

            Assert.IsTrue(gotStuck, "walking into the wall counts as stuck");
            Assert.IsTrue(gaveUp, "after a few stuck retries it gives the destination up");
        }
    }
}
