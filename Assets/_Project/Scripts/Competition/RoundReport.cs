using BallroomLegends.Tournament;

namespace BallroomLegends.Competition
{
    /// <summary>How the player did in one round. Plain data, shown on the Results screen.</summary>
    public class RoundReport
    {
        public RoundType Round;
        public int CouplesInRound;
        public int Place;
        public bool Recalled;

        public float DanceScore;
        public int Perfect;
        public int Good;
        public int Miss;
        public int MaxCombo;

        public int[] Marks = new int[0];

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
    }
}
