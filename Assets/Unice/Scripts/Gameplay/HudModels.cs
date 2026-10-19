using System;

namespace Unice
{
    /// <summary>What the match card shows: the clock, the round, the phase.</summary>
    public struct MatchCardData
    {
        public string round, clock, phase;
        /// <summary>The clock turns red.</summary>
        public bool clockLow;
    }

    /// <summary>How the role card is tinted (the UI maps each to its own style).</summary>
    public enum RoleStyle { None, Unice, Out, Holder }

    /// <summary>What the role card shows: your role and goal, who has the doll, the giggle meter and note, and your noise.</summary>
    public struct RoleCardData
    {
        public string role, goal;
        public RoleStyle style;
        public string dollHolder;
        public bool showGiggleMeter;
        /// <summary>0..1, how close the doll is to its next giggle.</summary>
        public float giggleProgress;
        /// <summary>Only what is news (a countdown, a cooldown); empty = hide the line.</summary>
        public string note;
        public bool showNoise;
        /// <summary>0..1.</summary>
        public float noiseProgress;
        public string noiseText;
    }

    /// <summary>The numbers and words behind the HUD's top cards, with no UI in them.</summary>
    public static class HudModels
    {
        /// <summary>The clock turns red in the last seconds of the hunt.</summary>
        public const float LowTimeSeconds = 15;
        /// <summary>Below this the noise meter stays hidden (0..1, same scale as <see cref="Character.noiseLevel"/>).</summary>
        public const float MinNoiseShown = .05f;
        /// <summary>Noise above this reads as "loud" (0..1).</summary>
        const float LoudNoise = .5f;

        public static MatchCardData MatchCard(Game game, Character me)
        {
            string phaseKey = game.phase switch
            {
                Phase.Hide => "hud.phase_hide",
                Phase.Hunt => game.unice == me ? "hud.phase_hunt_you" : "hud.phase_hunt_other",
                Phase.Result => "hud.phase_result",
                _ => null,
            };
            return new MatchCardData
            {
                round = $"{game.round}/{game.totalRounds}",
                clock = TextFormat.FormatMinutesSeconds(game.IsLive ? game.timeLeft : 0),
                clockLow = game.phase == Phase.Hunt && game.timeLeft <= LowTimeSeconds,
                phase = phaseKey != null ? Localization.Get(phaseKey) : "",
            };
        }

        public static RoleCardData RoleCard(Game game, Character me, Perspective perspective)
        {
            var data = new RoleCardData { style = RoleStyle.None };
            FillRole(ref data, game, me);
            FillDoll(ref data, game, me, perspective);
            FillNoise(ref data, me);
            return data;
        }

        /// <summary>The localization key for what a character is doing, in the player list; the Unice's player only gets "somewhere" for the hiders.</summary>
        public static string PlayerStatusKey(Character character, bool teamInfo)
        {
            if (character.IsUnice) return "hud.st_unice";
            if (character.outOfRound) return "hud.st_out";
            if (!teamInfo) return "hud.st_somewhere";
            if (character.hidingSpot != null) return "hud.st_hidden";
            if (!character.moving) return "hud.st_still";
            return character.sneaking ? "hud.st_sneaking" : "hud.st_running";
        }

        static void FillRole(ref RoleCardData data, Game game, Character me)
        {
            if (me == null) { data.role = Localization.Get("hud.role.watch"); data.goal = Localization.Get("hud.goal.watch"); }
            else if (me.IsUnice) { data.role = Localization.Get("hud.role.unice"); data.style = RoleStyle.Unice; data.goal = Localization.Get(game.phase == Phase.Hide ? "hud.goal.unice_hide" : "hud.goal.unice_hunt"); }
            else if (me.outOfRound) { data.role = Localization.Get("hud.role.caught"); data.style = RoleStyle.Out; data.goal = Localization.Get("hud.goal.caught"); }
            else if (game.doll.holder == me) { data.role = Localization.Get("hud.role.holder"); data.style = RoleStyle.Holder; data.goal = Localization.Get("hud.goal.holder", game.giggleInterval.ToString("0")); }
            else { data.role = Localization.Get("hud.role.hiding"); data.goal = Localization.Get(me.hidingSpot != null ? "hud.goal.hiding_spot" : "hud.goal.hiding_out"); }
        }

        /// <summary>The team sees who has the doll and a meter until the next giggle; the Unice only sees where it last giggled.</summary>
        static void FillDoll(ref RoleCardData data, Game game, Character me, Perspective perspective)
        {
            var doll = game.doll; var holder = doll.holder;
            if (holder != null && perspective.ShowsTeamInfo)
            {
                data.dollHolder = holder == me ? Localization.Get("hud.doll_with_you") : Localization.Get("hud.doll_with", holder.name.ToUpperInvariant());
                data.giggleProgress = game.phase == Phase.Hunt ? Geometry.Clamp(1 - doll.giggleTimer / game.giggleInterval, 0, 1) : 0;
                // the standing rules ("it stays quiet out of hiding") are in the how-to, not here: only the countdown and the pass cooldown are news
                string note = game.phase == Phase.Hunt && holder.hidingSpot != null ? Localization.Get("hud.giggle_in", Localization.SpotPhrase(holder.hidingSpot), (int)Math.Ceiling(doll.giggleTimer)) : "";
                if (holder == me && me.passCooldown > 0) note += Localization.Get("hud.repass_in", (int)Math.Ceiling(me.passCooldown));
                data.note = note.TrimStart();
                data.showGiggleMeter = true;
                return;
            }
            data.dollHolder = holder != null ? Localization.Get("hud.doll_hidden") : "";
            if (doll.hasLastGiggle)
            {
                var room = Map.RoomAt(doll.lastGiggleX, doll.lastGiggleY);
                int secondsAgo = Math.Max(0, (int)Math.Floor(game.roundClock - doll.lastGiggleTime));
                data.note = Localization.Get("hud.giggle_last", Localization.RoomPhrase(room), secondsAgo);
            }
            else data.note = "";
        }

        static void FillNoise(ref RoleCardData data, Character me)
        {
            // hidden or standing still there is nothing to watch; the meter only appears while you are making noise
            data.showNoise = me != null && !me.outOfRound && !me.IsUnice && me.hidingSpot == null && (me.moving || me.noiseLevel > MinNoiseShown);
            if (me == null) return;
            data.noiseProgress = Geometry.Clamp(me.noiseLevel, 0, 1);
            string noiseKey = me.hidingSpot != null ? "hud.noise_hidden" : me.noiseLevel > LoudNoise ? "hud.noise_loud" : me.moving ? "hud.noise_quiet" : "hud.noise_stopped";
            data.noiseText = Localization.Get(noiseKey);
        }
    }
}
