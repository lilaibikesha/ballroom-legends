using System;
using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class PerformanceTrackerTests
    {
        [Test]
        public void AllPerfect_Scores100()
        {
            var tracker = new PerformanceTracker(4);
            for (int i = 0; i < 4; i++)
            {
                tracker.Record(HitResult.Perfect);
            }

            Assert.AreEqual(100f, tracker.Score, 1e-4f);
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void GoodIsWorthHalf_MissIsWorthNothing()
        {
            var tracker = new PerformanceTracker(4);
            tracker.Record(HitResult.Perfect); // 1
            tracker.Record(HitResult.Good);    // 0.5
            tracker.Record(HitResult.Miss);    // 0
            tracker.Record(HitResult.Good);    // 0.5  -> 2 of 4 = 50

            Assert.AreEqual(50f, tracker.Score, 1e-4f);
            Assert.AreEqual(1, tracker.PerfectCount);
            Assert.AreEqual(2, tracker.GoodCount);
            Assert.AreEqual(1, tracker.MissCount);
        }

        [Test]
        public void Miss_ResetsTheCombo_ButKeepsMaxCombo()
        {
            var tracker = new PerformanceTracker(6);
            tracker.Record(HitResult.Perfect);
            tracker.Record(HitResult.Good);
            tracker.Record(HitResult.Perfect);
            tracker.Record(HitResult.Miss);
            tracker.Record(HitResult.Perfect);

            Assert.AreEqual(1, tracker.Combo);
            Assert.AreEqual(3, tracker.MaxCombo);
        }

        [Test]
        public void StepsNotDancedYet_CountAsNothing()
        {
            var tracker = new PerformanceTracker(10);
            tracker.Record(HitResult.Perfect);

            Assert.AreEqual(10f, tracker.Score, 1e-4f);
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void RecordingMoreStepsThanTheRoutine_Throws()
        {
            var tracker = new PerformanceTracker(1);
            tracker.Record(HitResult.Good);

            Assert.Throws<InvalidOperationException>(() => tracker.Record(HitResult.Good));
        }

        [Test]
        public void ZeroSteps_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PerformanceTracker(0));
        }
    }
}
