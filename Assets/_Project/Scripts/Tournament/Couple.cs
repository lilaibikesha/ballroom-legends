using System;

namespace BallroomLegends.Tournament
{
    /// <summary>A couple competing on the floor: the player's couple or an AI couple.</summary>
    [Serializable]
    public class Couple
    {
        public string Name;
        public bool IsPlayer;

        /// <summary>
        /// How well this couple dances, 0-100. AI couples get a fixed level; the player's
        /// level is the score of the dance they just performed.
        /// </summary>
        public float Skill;

        public Couple(string name, bool isPlayer, float skill = 0f)
        {
            Name = name;
            IsPlayer = isPlayer;
            Skill = skill;
        }

        public override string ToString() => Name;
    }
}
