using System.Collections.Generic;
using BallroomLegends.Floor;
using NUnit.Framework;
using UnityEngine;

namespace BallroomLegends.Tests
{
    /// <summary>The geometry of the ballroom floor.</summary>
    public class FloorLayoutTests
    {
        private const float Tolerance = 0.01f;

        [TestCase(48, 12, 4)]
        [TestCase(24, 12, 2)]
        [TestCase(13, 12, 2)]
        [TestCase(6, 12, 1)]
        [TestCase(0, 12, 0)]
        public void HeatCount_RoundsUp(int couples, int capacity, int expected)
        {
            Assert.AreEqual(expected, FloorLayout.HeatCount(couples, capacity));
        }

        [Test]
        public void PointOnEllipse_RightTopAndBottom()
        {
            var center = new Vector2(0f, -60f);
            var radius = new Vector2(640f, 250f);

            AssertClose(new Vector2(640f, -60f), FloorLayout.PointOnEllipse(center, radius, 0f));
            AssertClose(new Vector2(0f, 190f), FloorLayout.PointOnEllipse(center, radius, Mathf.PI / 2f));
            AssertClose(new Vector2(0f, -310f), FloorLayout.PointOnEllipse(center, radius, -Mathf.PI / 2f));
        }

        [Test]
        public void StartAngle_PlayerStartsAtTheBottom_OthersSpreadEvenly()
        {
            Assert.AreEqual(-Mathf.PI / 2f, FloorLayout.StartAngle(0, 4), Tolerance);
            Assert.AreEqual(0f, FloorLayout.StartAngle(1, 4), Tolerance);
            Assert.AreEqual(Mathf.PI / 2f, FloorLayout.StartAngle(2, 4), Tolerance);
        }

        [Test]
        public void TravelFacing_MovingAnticlockwise()
        {
            var radius = new Vector2(640f, 250f);

            // Bottom of the floor: moving right, so turned 90 degrees clockwise (-90).
            Assert.AreEqual(-90f, Mathf.DeltaAngle(0f, FloorLayout.TravelFacing(radius, -Mathf.PI / 2f)), Tolerance);
            // Right side: moving up, so not turned at all.
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, FloorLayout.TravelFacing(radius, 0f)), Tolerance);
            // Top: moving left (+90).
            Assert.AreEqual(90f, Mathf.DeltaAngle(0f, FloorLayout.TravelFacing(radius, Mathf.PI / 2f)), Tolerance);
        }

        [Test]
        public void PickForFloor_YouAreAlwaysOnTheFloor_NoOneTwice()
        {
            var all = new List<string>();
            for (int i = 0; i < 48; i++)
            {
                all.Add($"Couple {i}");
            }

            string you = all[37];
            List<string> heat = FloorLayout.PickForFloor(all, you, 12, new System.Random(3));

            Assert.AreEqual(12, heat.Count);
            Assert.AreEqual(you, heat[0]);
            CollectionAssert.AllItemsAreUnique(heat);
        }

        [Test]
        public void PickForFloor_SmallRound_EveryoneDances()
        {
            var final = new List<string> { "A", "B", "C", "D", "E", "You" };

            List<string> onFloor = FloorLayout.PickForFloor(final, "You", 12, new System.Random(1));

            Assert.AreEqual(6, onFloor.Count);
            CollectionAssert.AreEquivalent(final, onFloor);
        }

        [Test]
        public void ShapeAlpha_SolidInside_ClearOutside_HollowRing()
        {
            Assert.AreEqual(1f, FloorShapes.ShapeAlpha(64, 64, 128, 0f), Tolerance, "center of a circle");
            Assert.AreEqual(0f, FloorShapes.ShapeAlpha(0, 0, 128, 0f), Tolerance, "corner");
            Assert.AreEqual(0f, FloorShapes.ShapeAlpha(64, 64, 128, 10f), Tolerance, "center of a ring is empty");
            Assert.AreEqual(1f, FloorShapes.ShapeAlpha(64, 4, 128, 10f), Tolerance, "the ring's band is solid");
        }

        private static void AssertClose(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
