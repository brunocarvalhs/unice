using System.Collections.Generic;
using static Unice.ToneSynth;

namespace Unice
{
    /// <summary>The game's sound effects by name, as tones (the same synthesized sounds as the web version; no audio files).</summary>
    public static class SfxCatalog
    {
        public static readonly IReadOnlyDictionary<string, Tone[]> Sounds = new Dictionary<string, Tone[]>
        {
            ["giggle"] = ToneSequence(new[] { 1250f, 1480, 1180, 1420, 1060, 1300 }, .085f, Wave.Triangle, .07f, .1f, 1.22f),
            ["heart"] = new[] { MakeTone(95, .13f, Wave.Triangle, .22f, 55), MakeTone(85, .12f, Wave.Triangle, .16f, 50, .17f) },
            ["cane"] = new[] { MakeTone(900, .03f, Wave.Square, .05f, 500), MakeTone(240, .07f, Wave.Triangle, .26f, 140) },   // the Unice's cane: toc
            ["creak"] = new[] { MakeTone(220, .32f, Wave.Saw, .03f, 120) },
            ["sting"] = new[] { MakeTone(900, .5f, Wave.Saw, .05f, 90) },
            ["swish"] = new[] { MakeTone(500, .16f, Wave.Triangle, .06f, 1500) },
            ["step"] = new[] { MakeTone(160, .045f, Wave.Square, .03f) },
            ["tick"] = new[] { MakeTone(1500, .03f, Wave.Square, .02f) },
            ["clock"] = new[] { MakeTone(2200, .02f, Wave.Square, .025f) },
            ["boom"] = new[] { MakeTone(120, .8f, Wave.Saw, .06f, 38) },
            ["bad"] = new[] { MakeTone(230, .15f, Wave.Square, .035f, 150) },
            ["win"] = ToneSequence(new[] { 523f, 659, 784, 1046 }, .22f, Wave.Triangle, .06f, .14f, 1),
            ["lose"] = ToneSequence(new[] { 392f, 330, 262, 196 }, .3f, Wave.Triangle, .06f, .22f, 1),
        };
    }
}
