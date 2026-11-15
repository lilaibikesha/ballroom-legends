namespace BallroomLegends.Competition
{
    /// <summary>How the player did in one dance of a round (the Waltz of the Semi-final, ...).</summary>
    public class DanceReport
    {
        public string DanceName = string.Empty;
        public float Score;
        public int Perfect;
        public int Good;
        public int Miss;
        public int MaxCombo;

        /// <summary>One mark per judge for this dance.</summary>
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
