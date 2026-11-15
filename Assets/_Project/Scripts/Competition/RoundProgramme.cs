using System;
using System.Collections.Generic;

namespace BallroomLegends.Competition
{
    /// <summary>
    /// Which dances a round has. At the Blackpool Dance Festival every round of the
    /// Standard event has all five Standard dances, always in the same order
    /// (Waltz, Tango, Viennese Waltz, Slow Foxtrot, Quickstep), and Latin has its five
    /// (Samba, Cha Cha Cha, Rumba, Paso Doble, Jive). Pure functions, unit tested.
    /// </summary>
    public static class RoundProgramme
    {
        /// <summary>
        /// The first <paramref name="dancesPerRound"/> dances of the style, in competition order.
        /// Asking for more than there are gives all of them; asking for fewer than 1 gives 1.
        /// </summary>
        public static List<T> Pick<T>(IReadOnlyList<T> styleDances, int dancesPerRound)
        {
            if (styleDances == null)
            {
                throw new ArgumentNullException(nameof(styleDances));
            }

            int count = Math.Min(styleDances.Count, Math.Max(1, dancesPerRound));
            var programme = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                programme.Add(styleDances[i]);
            }

            return programme;
        }

        /// <summary>"Waltz", "Waltz and Tango", "Waltz, Tango and Quickstep".</summary>
        public static string Describe(IReadOnlyList<string> danceNames)
        {
            if (danceNames == null || danceNames.Count == 0)
            {
                return string.Empty;
            }

            if (danceNames.Count == 1)
            {
                return danceNames[0];
            }

            var allButLast = new List<string>();
            for (int i = 0; i < danceNames.Count - 1; i++)
            {
                allButLast.Add(danceNames[i]);
            }

            return $"{string.Join(", ", allButLast)} and {danceNames[danceNames.Count - 1]}";
        }
    }
}
