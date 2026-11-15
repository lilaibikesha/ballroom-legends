using System;
using System.Collections.Generic;
using UnityEngine;

namespace BallroomLegends.Floor
{
    /// <summary>
    /// Where couples stand and how they travel. Pure math (no scene needed), unit tested in
    /// Tests/EditMode/FloorLayoutTests.cs.
    ///
    /// Standard couples travel anticlockwise around the room, the "line of dance". We draw
    /// that path as an ellipse: x = cos(angle) * radius.x, y = sin(angle) * radius.y.
    /// Angles are in radians: 0 is the right side, PI/2 the top, -PI/2 the bottom.
    /// </summary>
    public static class FloorLayout
    {
        private const float FullTurn = 2f * Mathf.PI;

        /// <summary>
        /// Big rounds don't fit on the floor at once, so they dance in heats:
        /// 48 couples on a floor for 12 = 4 heats.
        /// </summary>
        public static int HeatCount(int couples, int floorCapacity)
        {
            if (floorCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(floorCapacity));
            }

            if (couples <= 0)
            {
                return 0;
            }

            return (couples + floorCapacity - 1) / floorCapacity; // Rounds up.
        }

        public static Vector2 PointOnEllipse(Vector2 center, Vector2 radius, float angle) =>
            center + new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y);

        /// <summary>
        /// Spreads couples evenly around the floor. Couple 0 (the player) starts at the
        /// bottom, right in front of the judges.
        /// </summary>
        public static float StartAngle(int index, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return -Mathf.PI / 2f + index * FullTurn / count;
        }

        /// <summary>How far (in radians) one step carries a couple around the floor.</summary>
        public static float AnglePerStep(int stepsPerLap)
        {
            if (stepsPerLap < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepsPerLap));
            }

            return FullTurn / stepsPerLap;
        }

        /// <summary>
        /// The rotation (degrees, for a UI element whose "forward" is up) that faces the
        /// direction of travel when moving anticlockwise around the ellipse.
        /// The direction is the derivative of the ellipse: (-sin * radius.x, cos * radius.y).
        /// At the bottom (-PI/2) couples move right, so the answer is -90 (turned clockwise).
        /// </summary>
        public static float TravelFacing(Vector2 radius, float angle)
        {
            float dx = -Mathf.Sin(angle) * radius.x;
            float dy = Mathf.Cos(angle) * radius.y;
            return Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - 90f;
        }

        /// <summary>
        /// Chooses who is on the floor in your heat: you first, then random other couples,
        /// at most <paramref name="capacity"/> in total. The rest dance in the other heats.
        /// </summary>
        public static List<T> PickForFloor<T>(IReadOnlyList<T> all, T mustInclude, int capacity, System.Random random)
            where T : class
        {
            if (all == null)
            {
                throw new ArgumentNullException(nameof(all));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var others = new List<T>();
            bool includeFound = false;
            foreach (T item in all)
            {
                if (item == null)
                {
                    continue;
                }

                if (item == mustInclude)
                {
                    includeFound = true;
                }
                else
                {
                    others.Add(item);
                }
            }

            // Fisher-Yates shuffle: every order is equally likely.
            for (int i = others.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (others[i], others[j]) = (others[j], others[i]);
            }

            var onFloor = new List<T>();
            if (includeFound && capacity > 0)
            {
                onFloor.Add(mustInclude);
            }

            for (int i = 0; i < others.Count && onFloor.Count < capacity; i++)
            {
                onFloor.Add(others[i]);
            }

            return onFloor;
        }
    }
}
