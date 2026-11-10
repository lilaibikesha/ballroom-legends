using System;

namespace BallroomLegends.Tournament
{
    /// <summary>Random helpers on top of System.Random (which only gives uniform numbers).</summary>
    public static class RandomUtil
    {
        /// <summary>
        /// A number from the bell curve (mean 0, standard deviation 1), via the Box-Muller
        /// transform. Most results land between -1 and 1, very few beyond -3 or 3. Good for
        /// "a judge is a little more or less generous than average".
        /// </summary>
        public static double NextGaussian(Random random)
        {
            double u1 = 1.0 - random.NextDouble(); // (0, 1], so Log never sees 0
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
    }
}
