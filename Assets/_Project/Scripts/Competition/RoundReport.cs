using System;
using System.Collections.Generic;
using BallroomLegends.Tournament;

namespace BallroomLegends.Competition
{
    /// <summary>
    /// How the player did in one round. Plain data, shown on the Results screen.
    /// A round has one or more dances (<see cref="Dances"/>). The totals below (score, counts,
    /// marks) cover the whole round; <see cref="AddDance"/> keeps them up to date.
    /// </summary>
    public class RoundReport
    {
        public RoundType Round;
        public int CouplesInRound;
        public int Place;
        public bool Recalled;

        /// <summary>Average dance score of the round, 0-100.</summary>
        public float DanceScore;
        public int Perfect;
        public int Good;
        public int Miss;
        /// <summary>The best combo of any dance in the round.</summary>
        public int MaxCombo;

        /// <summary>Every judge's mark for every dance of the round.</summary>
        public int[] Marks = new int[0];

        /// <summary>The dances of this round, in the order they were danced.</summary>
        public List<DanceReport> Dances = new List<DanceReport>();

        public int TotalMarks
        {
            get
            {
                int total = 0;
                foreach (int mark in Marks)
                {
                    total += mark;
                }

                return total;
            }
        }

        /// <summary>Adds a finished dance and recalculates the round's totals.</summary>
        public void AddDance(DanceReport dance)
        {
            if (dance == null)
            {
                throw new ArgumentNullException(nameof(dance));
            }

            Dances.Add(dance);

            float scoreSum = 0f;
            var marks = new List<int>();
            Perfect = 0;
            Good = 0;
            Miss = 0;
            MaxCombo = 0;

            foreach (DanceReport danced in Dances)
            {
                scoreSum += danced.Score;
                Perfect += danced.Perfect;
                Good += danced.Good;
                Miss += danced.Miss;
                MaxCombo = Math.Max(MaxCombo, danced.MaxCombo);
                marks.AddRange(danced.Marks);
            }

            DanceScore = scoreSum / Dances.Count;
            Marks = marks.ToArray();
        }
    }
}
