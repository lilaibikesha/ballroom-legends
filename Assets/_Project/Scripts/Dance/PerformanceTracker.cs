using System;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// Counts how one dance went: Perfect / Good / Miss, the combo, and a 0-100 score.
    /// A plain C# class (not a MonoBehaviour): it holds data and rules only, so tests can
    /// create one with "new" and check it without a scene.
    /// </summary>
    public class PerformanceTracker
    {
        public PerformanceTracker(int totalSteps)
        {
            if (totalSteps < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalSteps), "A dance needs at least one step.");
            }

            TotalSteps = totalSteps;
        }

        /// <summary>How many steps the routine has (one per beat after the count-in).</summary>
        public int TotalSteps { get; }

        public int PerfectCount { get; private set; }
        public int GoodCount { get; private set; }
        public int MissCount { get; private set; }
        public int JudgedSteps => PerfectCount + GoodCount + MissCount;
        public bool IsComplete => JudgedSteps == TotalSteps;

        /// <summary>Steps in a row without a Miss.</summary>
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }

        /// <summary>
        /// 0-100. A Perfect is worth 1 point, a Good half a point, a Miss nothing, divided by
        /// all the steps in the routine. Steps not danced yet count as nothing, so the score
        /// only goes up as you dance.
        /// </summary>
        public float Score => 100f * (PerfectCount + 0.5f * GoodCount) / TotalSteps;

        public void Record(HitResult result)
        {
            if (IsComplete)
            {
                throw new InvalidOperationException($"All {TotalSteps} steps were already judged.");
            }

            switch (result)
            {
                case HitResult.Perfect:
                    PerfectCount++;
                    Combo++;
                    break;
                case HitResult.Good:
                    GoodCount++;
                    Combo++;
                    break;
                default:
                    MissCount++;
                    Combo = 0;
                    break;
            }

            if (Combo > MaxCombo)
            {
                MaxCombo = Combo;
            }
        }
    }
}
