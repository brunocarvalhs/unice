using System;

namespace Unice
{
    /// <summary>The numbers and words behind a sound placed in the world: how far it carries, how loud and where it is.</summary>
    public static class SoundMath
    {
        /// <summary>Panning: a sound this far to one side (house pixels) plays fully in that ear.</summary>
        public const float FullPanDistance = 300;
        /// <summary>A hidden player hears 30% further.</summary>
        public const float HiddenReachFactor = 1.3f;
        /// <summary>A source closer than this is never muffled by a wall (house pixels).</summary>
        public const float MuffleMinDistance = 40;
        /// <summary>Quieter than this is not played at all (0..1).</summary>
        public const float AudibleLoudness = .01f;
        /// <summary>Loudness above which a sound is "near", and above which "mid" (the rest is "far").</summary>
        const float NearLoudness = .45f, MidLoudness = .15f;
        /// <summary>A source this close on both axes is "right next to you" (house pixels).</summary>
        const float SideNearDistance = 60;
        /// <summary>One axis this many times longer than the other: only that axis is said ("left", not "above, left").</summary>
        const float DominantAxisRatio = 1.6f;

        /// <summary>How far a sound of <paramref name="range"/> carries to this listener.</summary>
        public static float Reach(float range, bool listenerHidden) => range * (listenerHidden ? HiddenReachFactor : 1);

        /// <summary>0..1: 0 beyond <paramref name="reach"/>, rising with the square of how close, never below <paramref name="minLoudness"/>.</summary>
        public static float Loudness(float distance, float reach, float minLoudness = 0)
        {
            float closeness = distance >= reach ? 0 : (1 - distance / reach) * (1 - distance / reach);
            return Math.Max(closeness, minLoudness);
        }

        /// <summary>-1 (left ear) .. 1 (right ear) for a source <paramref name="dx"/> to the right of the listener.</summary>
        public static float Pan(float dx) => Geometry.Clamp(dx / FullPanDistance, -1, 1);

        /// <summary>Whether a wall muffles the sound: only for sources not right next to the listener.</summary>
        public static bool IsMuffled(float distance, bool hasLineOfSight) => distance > MuffleMinDistance && !hasLineOfSight;

        /// <summary>"close" / "medium" / "far" (localized), from the loudness.</summary>
        public static string DescribeDistance(float loudness) =>
            Localization.Get(loudness > NearLoudness ? "app.close.near" : loudness > MidLoudness ? "app.close.mid" : "app.close.far");

        /// <summary>"left", "above, right", "near"…: where a sound is, relative to the player.</summary>
        public static string DescribeSide(float dx, float dy)
        {
            float horizontalDistance = Math.Abs(dx), verticalDistance = Math.Abs(dy);
            if (horizontalDistance < SideNearDistance && verticalDistance < SideNearDistance) return Localization.Get("app.side.near");
            string horizontal = Localization.Get(dx < 0 ? "app.side.left" : "app.side.right");
            string vertical = Localization.Get(dy < 0 ? "app.side.above" : "app.side.below");
            if (horizontalDistance > verticalDistance * DominantAxisRatio) return horizontal;
            if (verticalDistance > horizontalDistance * DominantAxisRatio) return vertical;
            return vertical + ", " + horizontal;
        }
    }
}
