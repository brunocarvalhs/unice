using System;

namespace Unice
{
    /// <summary>Pitch of a scale degree, for writing tunes as degrees instead of Hz.</summary>
    public static class ScaleMath
    {
        /// <summary>
        /// Frequency of <paramref name="degree"/> (0 = the root) in a scale given as semitones above the root, one per degree.
        /// Degrees past the last one climb an octave; negative degrees go below the root.
        /// </summary>
        public static float DegreeFrequency(float rootFrequency, int[] scale, int degree)
        {
            int octave = degree >= 0 ? degree / scale.Length : -((-degree + scale.Length - 1) / scale.Length);
            int step = degree - octave * scale.Length;
            return rootFrequency * MathF.Pow(2, octave + scale[step] / 12f);
        }
    }
}
