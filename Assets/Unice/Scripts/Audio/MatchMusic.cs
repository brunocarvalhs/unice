using System.Collections.Generic;
using static Unice.ToneSynth;

namespace Unice
{
    /// <summary>
    /// The match's soundtrack: an original baião in the spirit of the northeastern pífano bands: two fifes in parallel thirds
    /// (with the trills on long notes), a zabumba and a triangle on the 3+3+2 baião pulse, in the sertão's lydian-mixolydian
    /// mode. Built from the same tones as the sound effects; no audio files.
    /// </summary>
    public static class MatchMusic
    {
        /// <summary>Seconds per sixteenth note (120 bpm), sixteenths per bar (2/4) and bars in the loop.</summary>
        public const float Sixteenth = .125f;
        const int SixteenthsPerBar = 8;
        public const int Bars = 16;
        public const float LoopSeconds = Bars * SixteenthsPerBar * Sixteenth;

        /// <summary>G lydian-mixolydian from G4, in semitones above it, one entry per scale degree.</summary>
        static readonly int[] Scale = { 0, 2, 4, 6, 7, 9, 10 };
        const float RootFrequency = 392;
        const int Rest = -99;
        /// <summary>Notes at least this long (in sixteenths) end in a trill, as the fifes do.</summary>
        const int TrillFrom = 4;

        /// <summary>The fifes' tune, a row per bar: (scale degree, length in sixteenths); each bar adds up to eight.</summary>
        static readonly (int degree, int sixteenths)[][] Tune =
        {
            new[] { (4, 2), (4, 1), (5, 1), (4, 2), (2, 2) },
            new[] { (1, 2), (2, 1), (1, 1), (0, 4) },
            new[] { (4, 1), (4, 1), (4, 2), (5, 2), (4, 1), (2, 1) },
            new[] { (2, 2), (1, 1), (0, 1), (1, 2), (Rest, 2) },
            new[] { (7, 2), (7, 1), (8, 1), (7, 2), (5, 2) },
            new[] { (4, 2), (5, 1), (4, 1), (2, 4) },
            new[] { (7, 1), (7, 1), (7, 2), (8, 2), (7, 1), (5, 1) },
            new[] { (4, 1), (2, 1), (4, 2), (5, 1), (4, 1), (2, 1), (1, 1) },
            new[] { (4, 2), (4, 1), (5, 1), (4, 2), (2, 2) },
            new[] { (1, 2), (2, 1), (1, 1), (0, 4) },
            new[] { (4, 1), (4, 1), (4, 2), (5, 2), (4, 1), (2, 1) },
            new[] { (2, 2), (1, 1), (0, 1), (1, 2), (Rest, 2) },
            new[] { (4, 1), (5, 1), (4, 1), (2, 1), (4, 1), (5, 1), (4, 1), (2, 1) },
            new[] { (1, 1), (2, 1), (1, 1), (0, 1), (1, 1), (2, 1), (1, 1), (0, 1) },
            new[] { (4, 2), (2, 2), (1, 2), (0, 2) },
            new[] { (0, 6), (Rest, 2) },
        };

        /// <summary>Sixteenths of a bar where the triangle plays (the first of each beat is the accent) and where the zabumba hits low.</summary>
        static readonly int[] TriangleHits = { 0, 2, 3, 4, 6, 7 }, ZabumbaLow = { 0, 6 };
        const int ZabumbaHigh = 3;

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
                    if (degree != Rest) AddFifes(tones, degree, sixteenths, barStart + position * Sixteenth);
                    position += sixteenths;
                }
                foreach (int hit in ZabumbaLow) tones.Add(MakeTone(100, .2f, Wave.Sine, .2f, 52, barStart + hit * Sixteenth));
                tones.Add(MakeTone(220, .07f, Wave.Triangle, .06f, 150, barStart + ZabumbaHigh * Sixteenth));
                foreach (int hit in TriangleHits) tones.Add(MakeTone(4200, .04f, Wave.Square, hit % 4 == 0 ? .013f : .007f, 0, barStart + hit * Sixteenth));
            }
            tones.Add(MakeTone(40, .01f, Wave.Triangle, .0002f, 0, LoopSeconds - .04f));   // inaudible: makes the clip exactly one loop long
            return tones.ToArray();
        }

        /// <summary>The lead fife and the second one a third above; long notes finish with a trill (four thirty-second notes).</summary>
        static void AddFifes(List<Tone> tones, int degree, int sixteenths, float at)
        {
            bool trill = sixteenths >= TrillFrom;
            float held = (trill ? sixteenths - 2 : sixteenths) * Sixteenth * .95f;
            AddFifePair(tones, degree, held, at);
            if (!trill) return;
            for (int i = 0; i < 4; i++)
                AddFifePair(tones, i % 2 == 0 ? degree + 1 : degree, Sixteenth / 2 * .9f, at + (sixteenths - 2) * Sixteenth + i * Sixteenth / 2);
        }

        static void AddFifePair(List<Tone> tones, int degree, float duration, float at)
        {
            tones.Add(MakeTone(DegreeFrequency(degree), duration, Wave.Triangle, .05f, 0, at, .02f));
            tones.Add(MakeTone(DegreeFrequency(degree + 2), duration, Wave.Triangle, .035f, 0, at, .014f));
        }
    }
}
