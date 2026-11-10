using System;
using System.Collections.Generic;
using BallroomLegends.Tournament;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class CompetitorFieldTests
    {
        [Test]
        public void CreatesUniqueAiCouples_WithSkillInRange()
        {
            List<Couple> couples = CompetitorField.CreateAiCouples(47, new Random(3), 45f, 90f);

            Assert.AreEqual(47, couples.Count);
            var names = new HashSet<string>();
            foreach (Couple couple in couples)
            {
                Assert.IsTrue(names.Add(couple.Name), $"Duplicate name {couple.Name}");
                Assert.IsFalse(couple.IsPlayer);
                Assert.That(couple.Skill, Is.InRange(45f, 90f));
            }
        }

        [Test]
        public void TooManyCouples_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CompetitorField.CreateAiCouples(CompetitorField.MaxCouples + 1, new Random(1), 0f, 1f));
        }
    }
}
