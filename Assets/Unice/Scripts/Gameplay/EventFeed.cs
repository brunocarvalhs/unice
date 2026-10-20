using System;
using System.Collections.Generic;
using System.Linq;

namespace Unice
{
    /// <summary>
    /// Turns the match's events (<see cref="GameEvent"/>) into what this player sees and hears — banners, toasts, sounds,
    /// ripples, the catch shake — logs the important ones to analytics and reports Play Games achievements and the
    /// "Online wins" leaderboard (rules in <see cref="Achievements"/>). Runs on every device, host included; each event is
    /// played once, so nothing is reported twice.
    /// </summary>
    public sealed class EventFeed
    {
        readonly Game game;
        readonly Func<Character> myCharacter;
        readonly Perspective perspective;
        readonly IStage stage;
        readonly IMatchContext context;
        Character me => myCharacter();
        int lastHandledSeq = -1;
        /// <summary>This round, for <see cref="Achievement.HotPotato"/> and <see cref="Achievement.Decoy"/>. Cleared at round start.</summary>
        bool passedDollThisRound, laughedAsDecoyThisRound;
        /// <summary>When this device saw the match's first round start (seconds, device clock), or -1: the leaderboard's duration filter.</summary>
        float matchStartedAt = -1;
        /// <summary>When this device saw the current round start (seconds, device clock), or -1: "duration_s" of "round_end".</summary>
        float roundStartedAt = -1;
        /// <summary>Most humans in this match at any round start (people can join between rounds): "humans_peak" of "match_end".</summary>
        int humansPeak;

        public EventFeed(Game game, Func<Character> myCharacter, Perspective perspective, IStage stage, IMatchContext context)
        {
            this.game = game; this.myCharacter = myCharacter; this.perspective = perspective; this.stage = stage; this.context = context;
        }

        /// <summary>New room or match: the next <see cref="PlayNewEvents"/> catches up without replaying old events.</summary>
        public void Reset() { lastHandledSeq = -1; matchStartedAt = roundStartedAt = -1; humansPeak = 0; }

        /// <summary>Plays every event newer than the last one handled. The first call after a reset only catches up.</summary>
        public void PlayNewEvents()
        {
            if (lastHandledSeq < 0) { lastHandledSeq = game.eventSeq; return; }
            // by index, no copy: presenting an event never adds or removes events (only the host's rules and snapshots do)
            for (int i = 0, count = game.events.Count; i < count; i++)
            {
                var gameEvent = game.events[i];
                if (gameEvent.sequence <= lastHandledSeq) continue;
                lastHandledSeq = gameEvent.sequence;
                try { Present(gameEvent); }
                catch (Exception error) { stage.Error(error, "event " + gameEvent.type); }   // one broken event must not stop the others
            }
            // a new host restarted the numbering: follow it
            if (game.eventSeq < lastHandledSeq - 50) lastHandledSeq = game.eventSeq;
        }

        void Present(GameEvent gameEvent)
        {
            switch (gameEvent.type)
            {
                case GameEventType.Start: OnRoundStart(gameEvent); break;
                case GameEventType.Enter: OnUniceEnters(); break;
                case GameEventType.Giggle: OnGiggle(gameEvent); break;
                case GameEventType.Pass: OnDollPassed(gameEvent); break;
                case GameEventType.Open: OnSpotOpened(gameEvent); break;
                case GameEventType.Catch: OnCatch(gameEvent); break;
                case GameEventType.End: OnRoundEnd(gameEvent); break;
                case GameEventType.Final: OnMatchEnd(); break;
                case GameEventType.Host: OnNewHost(gameEvent); break;
                case GameEventType.Left: OnPlayerLeft(gameEvent); break;
            }
        }

        string MyRole => me == null ? "spectator" : me.IsUnice ? "unice" : me == game.doll.holder ? "holder" : "hider";
        Character CharacterAt(int index) => game.CharacterAt(index);
        string NameFor(Character character) => perspective.NameForCapitalized(character);
        /// <summary>People still in the match: whoever left became a bot and no longer counts.</summary>
        int Humans => game.characters.Count(character => !character.isBot);
        /// <summary>Whole seconds since a time taken from the device clock; 0 when it wasn't seen (joined midway).</summary>
        int SecondsSince(float startedAt) => startedAt < 0 ? 0 : (int)(context.Now - startedAt);

        /// <summary>Sends what was earned to Play Games (nothing if it isn't signed in) and logs each unlock to analytics.</summary>
        void Report(List<Achievement> earned)
        {
            foreach (var achievement in earned)
            {
                if (!Achievements.IsIncremental(achievement)) stage.Log("achievement_unlock", ("id", achievement.ToString()));
                AccountRegistry.ReportAchievement?.Invoke(achievement);
            }
        }

        void OnRoundStart(GameEvent gameEvent)
        {
            var uniceCharacter = CharacterAt(gameEvent.actor); var dollHolder = CharacterAt(gameEvent.target); int round = gameEvent.value;
            stage.Log("round_start", ("round", round), ("rounds", game.totalRounds), ("role", MyRole), ("online", context.IsOnline), ("map", game.mapId));
            passedDollThisRound = laughedAsDecoyThisRound = false;
            if (matchStartedAt < 0) matchStartedAt = context.Now;
            roundStartedAt = context.Now;
            humansPeak = Math.Max(humansPeak, Humans);
            if (me == null) stage.Banner(Localization.Get("ev.start.watch", round, NameFor(uniceCharacter)), BannerStyle.Info, 3);
            else if (me == uniceCharacter) stage.Banner(Localization.Get("ev.start.you_unice"), BannerStyle.Rose, 3.2f);
            else if (me == dollHolder) stage.Banner(Localization.Get("ev.start.you_holder"), BannerStyle.Rose, 3.2f);
            else stage.Banner(Localization.Get("ev.start.other", NameFor(uniceCharacter)), BannerStyle.Candle, 3.2f);
        }

        void OnUniceEnters()
        {
            stage.Sound("boom");
            stage.SoundFrom(Map.Entrance.x, Map.Entrance.y, 1400, Localization.Get("ev.enter.sound"), StageColor.Rose, "creak", .35f);
            stage.Banner(Localization.Get(game.unice == me ? "ev.enter.unice" : "ev.enter.others"), BannerStyle.Rose, 2.4f);
        }

        /// <summary>The doll giggled or someone laughed: everyone hears it; the Unice is told the room.</summary>
        void OnGiggle(GameEvent gameEvent)
        {
            float x = gameEvent.x, y = gameEvent.y; var laugher = CharacterAt(gameEvent.actor); var room = Map.RoomAt(x, y);
            bool isDoll = laugher != null && laugher == game.doll.holder;
            if (me != null && laugher == me && !isDoll) laughedAsDecoyThisRound = true;
            stage.Ripple(x, y, RippleKind.Giggle);
            stage.WorldText(x, y - 30, Localization.Get("ev.giggle.fx"), StageColor.Rose, 18, 1.8f, true);
            stage.SoundFrom(x, y, 2200, Localization.Get("ev.giggle.sound"), StageColor.Rose, "giggle", .3f);
            if (me != null && me.IsUnice && game.IsLive) stage.Banner(Localization.Get("ev.giggle.unice", Localization.RoomPhrase(room)), BannerStyle.Rose, 2);
            else if (laugher == me) stage.Banner(Localization.Get(isDoll ? "ev.giggle.holder" : "ev.giggle.laughed"), BannerStyle.Rose, 2.4f);
            else if (isDoll) stage.Toast(Localization.Get("ev.giggle.toast_doll", Localization.RoomPhrase(room)));
            else stage.Toast(Localization.Get("ev.giggle.toast_who", NameFor(laugher), Localization.RoomPhrase(room)));
        }

        void OnDollPassed(GameEvent gameEvent)
        {
            var giver = CharacterAt(gameEvent.actor); var receiver = CharacterAt(gameEvent.target);
            if (giver == null || receiver == null) return;
            if (giver == me) passedDollThisRound = true;
            stage.SoundFrom(giver.x, giver.y, 220, Localization.Get("ev.pass.sound"), StageColor.Rose, "swish");
            if (perspective.ShowsTeamInfo || perspective.CanSee(giver.x, giver.y))
                stage.WorldText((giver.x + receiver.x) / 2, (giver.y + receiver.y) / 2 - 26, Localization.Get("ev.pass.fx"), StageColor.Rose, 14, 1);
            if (receiver == me) stage.Banner(Localization.Get("ev.pass.you_got"), BannerStyle.Rose, 2);
            else if (giver == me) stage.Banner(Localization.Get("ev.pass.you_gave", receiver.name), BannerStyle.Rose, 1.5f);
            else if (perspective.ShowsTeamInfo) stage.Toast(Localization.Get("ev.pass.toast", giver.name, receiver.name));
        }

        void OnSpotOpened(GameEvent gameEvent)
        {
            int spotIndex = gameEvent.value;
            if (spotIndex < 0 || spotIndex >= Map.Spots.Length) return;
            var spot = Map.Spots[spotIndex];
            stage.SoundFrom(spot.CenterX, spot.CenterY, 450, Localization.Get("ev.open.sound"), StageColor.Candle, "creak");
            bool wasEmpty = gameEvent.target < 0;
            if (wasEmpty && perspective.CanSee(spot.CenterX, spot.CenterY))
                stage.WorldText(spot.standX, spot.standY - 24, Localization.Get("ev.open.empty"), StageColor.Muted, 14, 1);
        }

        /// <summary>The catch moment: sting, ripple, screen shake and red flash (stronger when the doll was caught).</summary>
        void OnCatch(GameEvent gameEvent)
        {
            var caught = CharacterAt(gameEvent.target); var catcher = CharacterAt(gameEvent.actor);
            bool withDoll = gameEvent.value != 0; float x = gameEvent.x, y = gameEvent.y;
            if (caught == null) return;
            if (caught == me || catcher == me)
                stage.Log("catch", ("side", caught == me ? "caught" : "catcher"), ("with_doll", withDoll), ("time_left", (int)game.timeLeft), ("room", Map.RoomAt(x, y).id));
            Report(Achievements.ForCatch(iCaught: me != null && catcher == me, withDoll));

            stage.Sound("sting");
            stage.WorldText(x, y - 26, Localization.Get(withDoll ? "ev.catch.mine" : "ev.catch.gotcha"), StageColor.Blood, withDoll ? 24 : 20, 1.8f, true);
            stage.Ripple(x, y, RippleKind.Catch);
            stage.ShakeCamera(withDoll ? 16f : 10f);
            stage.FlashScreen(withDoll ? .4f : .22f, withDoll ? .3f : .18f);
            stage.FlashCharacter(caught.index); if (catcher != null) stage.FlashCharacter(catcher.index);

            if (caught == me) stage.Banner(Localization.Get(withDoll ? "ev.catch.you_doll" : "ev.catch.you_out"), BannerStyle.Blood, 2.6f);
            else if (catcher == me && !withDoll) stage.Banner(Localization.Get("ev.catch.not_holder", caught.name), BannerStyle.Candle, 2);
            else if (!withDoll) stage.Toast(Localization.Get("ev.catch.toast", caught.name, Localization.RoomPhrase(Map.RoomAt(x, y))));
        }

        void OnRoundEnd(GameEvent gameEvent)
        {
            bool uniceWon = gameEvent.value != 0;
            stage.Log("round_end", ("round", game.round), ("unice_won", uniceWon), ("role", me == null ? "spectator" : me.IsUnice ? "unice" : "hider"),
                ("won", me != null && uniceWon == me.IsUnice), ("caught", me != null && me.outOfRound), ("online", context.IsOnline), ("duration_s", SecondsSince(roundStartedAt)));
            roundStartedAt = -1;
            if (me != null)
            {
                int dollHolder = game.roundResult?.dollHolder ?? -1;
                Report(Achievements.ForRoundEnd(new RoundFacts
                {
                    uniceWon = uniceWon, iAmUnice = me.IsUnice, iWasCaught = me.outOfRound, iHeldDoll = me.index == dollHolder,
                    iPassedDoll = passedDollThisRound, iLaughedAsDecoy = laughedAsDecoyThisRound,
                    hiders = game.characters.Count(character => !character.IsUnice),
                    hidersFree = game.characters.Count(character => !character.IsUnice && !character.outOfRound && character.index != dollHolder),
                }));
            }
            if (me != null) stage.Sound(uniceWon == me.IsUnice ? "win" : "lose");
            if (!uniceWon) stage.Banner(Localization.Get("ev.end.time"), BannerStyle.Moss, 1.6f);
            stage.StopHeartbeat();
        }

        void OnMatchEnd()
        {
            float seconds = matchStartedAt < 0 ? 0 : context.Now - matchStartedAt;
            int peak = Math.Max(humansPeak, Humans);
            matchStartedAt = -1; humansPeak = 0;
            if (me == null) return;
            int place = 1 + game.characters.Count(character => character.score > me.score);
            stage.Log("match_end", ("rounds", game.totalRounds), ("score", me.score), ("place", place), ("players", game.characters.Count), ("online", context.IsOnline),
                ("duration_s", (int)seconds), ("humans_peak", peak));
            if (context.MarkFirstMatchEnd()) stage.Log("first_match_end", ("online", context.IsOnline), ("place", place), ("duration_s", (int)seconds));
            Report(Achievements.ForMatchEnd(place, Humans, context.IsOnline));
            if (Achievements.CountsForLeaderboard(place, Humans, context.IsOnline, game.totalRounds, seconds))
                AccountRegistry.ReportOnlineWins?.Invoke(context.AddOnlineWin());
        }

        void OnNewHost(GameEvent gameEvent)
        {
            string hostName = TextFormat.CleanName(gameEvent.text);
            bool itsMe = game.isHost && hostName == context.MyName;
            // only the new host logs it: one event per handover, not one per device
            if (itsMe) stage.Log("host_changed", ("phase", game.phase.ToString().ToLowerInvariant()), ("round", game.round), ("humans", Humans));
            stage.Toast(itsMe ? Localization.Get("ev.host.you") : Localization.Get("ev.host.other", hostName));
        }

        void OnPlayerLeft(GameEvent gameEvent)
        {
            var leaver = CharacterAt(gameEvent.actor);
            if (leaver != null) stage.Toast(Localization.Get("ev.left", leaver.name));
        }
    }
}
