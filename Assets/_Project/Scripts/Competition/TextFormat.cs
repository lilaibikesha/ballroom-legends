namespace BallroomLegends.Competition
{
    /// <summary>Small text helpers shared by the competition and results screens.</summary>
    public static class TextFormat
    {
        /// <summary>1 -> "1st", 2 -> "2nd", 3 -> "3rd", 11 -> "11th", 22 -> "22nd".</summary>
        public static string Ordinal(int number)
        {
            int lastTwo = number % 100;
            if (lastTwo >= 11 && lastTwo <= 13)
            {
                return $"{number}th";
            }

            return (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };
        }
    }
}
