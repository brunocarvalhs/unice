using System.Collections.Generic;
using static Unice.ToneSynth;

namespace Unice
{
    /// <summary>
    /// The menu's theme: an original forró xote in the style of the pé-de-serra trios: an accordion (two slightly detuned
    /// reeds, an octave below as well) singing over its own bass-and-chord left hand, a zabumba and a triangle; D mixolydian.
    /// Same synthesized tones as the rest of the sound; no audio files.
    /// </summary>
    public static class MenuMusic
    {
        /// <summary>Seconds per sixteenth note (96 bpm), sixteenths per bar (2/4) and bars in the loop.</summary>
        public const float Sixteenth = 60f / 96 / 4;
        const int SixteenthsPerBar = 8;
        public const int Bars = 16;
        public const float LoopSeconds = Bars * SixteenthsPerBar * Sixteenth;

        /// <summary>D mixolydian from D4, in semitones above it, one entry per scale degree.</summary>
        static readonly int[] Scale = { 0, 2, 4, 5, 7, 9, 10 };
        const float RootFrequency = 293.66f;
        const int Rest = -99;
        /// <summary>How much the second reed is out of tune (the accordion's beating).</summary>
        const float ReedDetune = 1.007f;

        /// <summary>The accordion's tune, a row per bar: (scale degree, length in sixteenths); each bar adds up to eight.</summary>
        static readonly (int degree, int sixteenths)[][] Tune =
        {
            new[] { (4, 3), (5, 1), (4, 2), (2, 2) },
            new[] { (0, 2), (2, 2), (4, 4) },
            new[] { (6, 3), (5, 1), (4, 2), (2, 2) },
            new[] { (1, 4), (0, 2), (-1, 2) },
            new[] { (3, 2), (4, 2), (5, 2), (4, 1), (3, 1) },
            new[] { (2, 3), (1, 1), (2, 4) },
            new[] { (0, 2), (1, 1), (2, 1), (4, 4) },
            new[] { (2, 2), (1, 1), (0, 1), (0, 4) },
            new[] { (7, 3), (8, 1), (7, 2), (5, 2) },
            new[] { (4, 2), (5, 2), (7, 4) },
            new[] { (6, 3), (5, 1), (4, 2), (3, 2) },
            new[] { (4, 4), (3, 2), (2, 2) },
            new[] { (4, 2), (6, 2), (8, 2), (6, 2) },
            new[] { (5, 3), (6, 1), (4, 4) },
            new[] { (2, 2), (4, 2), (2, 2), (1, 2) },
            new[] { (0, 6), (Rest, 2) },
        };

        /// <summary>The root of each bar's chord: D D C C G G D D / D D G G A A D D.</summary>
        static readonly int[] ChordRoots = { 0, 0, -1, -1, 3, 3, 0, 0, 0, 0, 3, 3, 4, 4, 0, 0 };

        public static float DegreeFrequency(int degree) => ScaleMath.DegreeFrequency(RootFrequency, Scale, degree);

        /// <summary>The whole loop as tones, ready to be rendered with <see cref="ToneSynth.Render"/>.</summary>
        public static Tone[] Loop()
        {
            var tones = new List<Tone>();
            for (int bar = 0; bar < Bars; bar++)
            {
                float barStart = bar * SixteenthsPerBar * Sixteenth;
                int position = 0;
                foreach (var (degree, sixteenths) in Tune[bar])
                {
                    if (degree != Rest) AddReeds(tones, DegreeFrequency(degree), sixteenths * Sixteenth * .95f, .02f, barStart + position * Sixteenth);
                    position += sixteenths;
                }
                AddLeftHand(tones, ChordRoots[bar], barStart);
                tones.Add(MakeTone(95, .22f, Wave.Sine, .2f, 52, barStart));                          // zabumba: low on beat 1 and 2, the tak in between
                tones.Add(MakeTone(95, .22f, Wave.Sine, .16f, 52, barStart + 4 * Sixteenth));
                tones.Add(MakeTone(220, .07f, Wave.Triangle, .06f, 150, barStart + 3 * Sixteenth));
                tones.Add(MakeTone(220, .07f, Wave.Triangle, .06f, 150, barStart + 7 * Sixteenth));
                for (int hit = 0; hit < SixteenthsPerBar; hit += 2)                                    // triangle on the eighths, stronger off the beat
                    tones.Add(MakeTone(4200, .04f, Wave.Square, hit % 4 == 2 ? .011f : .006f, 0, barStart + hit * Sixteenth));
            }
            tones.Add(MakeTone(40, .01f, Wave.Triangle, .0002f, 0, LoopSeconds - .04f));   // inaudible: makes the clip exactly one loop long
            return tones.ToArray();
        }

        /// <summary>One accordion note: two reeds a hair apart plus one an octave below, sustained like a reed instead of dying away.</summary>
        static void AddReeds(List<Tone> tones, float frequency, float duration, float volume, float at)
        {
            tones.Add(MakeTone(frequency, duration, Wave.Square, volume, 0, at, volume * .6f));
            tones.Add(MakeTone(frequency * ReedDetune, duration, Wave.Saw, volume * .7f, 0, at, volume * .45f));
            tones.Add(MakeTone(frequency / 2, duration, Wave.Triangle, volume * 1.4f, 0, at, volume * .9f));
        }

        /// <summary>The accordion's left hand: the bass (root, then fifth) on the beats and the chord on the off-beats.</summary>
        static void AddLeftHand(List<Tone> tones, int root, float barStart)
        {
            tones.Add(MakeTone(DegreeFrequency(root - 7), .3f, Wave.Triangle, .09f, 0, barStart));
            tones.Add(MakeTone(DegreeFrequency(root - 3), .3f, Wave.Triangle, .08f, 0, barStart + 4 * Sixteenth));
            foreach (int offBeat in new[] { 2, 6 })
                foreach (int third in new[] { 0, 2, 4 })
                    tones.Add(MakeTone(DegreeFrequency(root + third - 7), .14f, Wave.Square, .008f, 0, barStart + offBeat * Sixteenth, .004f));
        }
    }
}
