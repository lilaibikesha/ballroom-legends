using System;
using System.Collections.Generic;

namespace BallroomLegends.Tournament
{
    /// <summary>Creates the AI couples you compete against.</summary>
    public static class CompetitorField
    {
        private static readonly string[] Leaders =
        {
            "Oliver", "Luca", "Mateo", "Hugo", "Arjun", "Kenji", "Felix", "Noah",
            "Ivan", "Rafael", "Tomas", "Daniel", "Marco", "Elias", "Leo", "Sami"
        };

        private static readonly string[] Followers =
        {
            "Sofia", "Amelia", "Mia", "Yuna", "Elena", "Chloe", "Isla", "Aria",
            "Nina", "Lucia", "Hana", "Zara", "Maya", "Clara", "Ines", "Alina"
        };

        public static int MaxCouples => Leaders.Length * Followers.Length;

        /// <summary>
        /// <paramref name="count"/> AI couples with unique names ("Luca &amp; Mia") and a skill
        /// spread evenly between <paramref name="minSkill"/> and <paramref name="maxSkill"/>.
        /// </summary>
        public static List<Couple> CreateAiCouples(int count, Random random, float minSkill, float maxSkill)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (count < 0 || count > MaxCouples)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"Between 0 and {MaxCouples} couples.");
            }

            if (minSkill > maxSkill)
            {
                throw new ArgumentException("minSkill must not be bigger than maxSkill.");
            }

            var couples = new List<Couple>(count);
            var usedNames = new HashSet<string>();

            while (couples.Count < count)
            {
                string name = $"{Leaders[random.Next(Leaders.Length)]} & {Followers[random.Next(Followers.Length)]}";

                // HashSet.Add returns false for a duplicate: draw again.
                if (!usedNames.Add(name))
                {
                    continue;
                }

                float skill = (float)(minSkill + random.NextDouble() * (maxSkill - minSkill));
                couples.Add(new Couple(name, false, skill));
            }

            return couples;
        }
    }
}
