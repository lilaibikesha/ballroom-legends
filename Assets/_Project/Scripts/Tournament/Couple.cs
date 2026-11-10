using System;

namespace BallroomLegends.Tournament
{
    /// <summary>A couple competing on the floor: the player's couple or an AI couple.</summary>
    [Serializable]
    public class Couple
    {
        public string Name;
        public bool IsPlayer;

        public Couple(string name, bool isPlayer)
        {
            Name = name;
            IsPlayer = isPlayer;
        }

        public override string ToString() => Name;
    }
}
