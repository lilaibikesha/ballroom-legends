namespace BallroomLegends.Player
{
    /// <summary>
    /// Rules for the dancer name typed on the Character Selection screen.
    ///
    /// Design note: this is a static class with no Unity code at all, so it's easy to
    /// unit test (see Tests/EditMode/PlayerNameRulesTests.cs). Keeping rules like this out
    /// of MonoBehaviours is one of the habits that makes code testable.
    /// </summary>
    public static class PlayerNameRules
    {
        public const int MinLength = 2;
        public const int MaxLength = 16;

        /// <summary>Removes spaces at the start and end. Null becomes an empty string.</summary>
        public static string Clean(string rawName)
        {
            // The null check comes first: calling .Trim() on null would throw a
            // NullReferenceException.
            return rawName == null ? string.Empty : rawName.Trim();
        }

        /// <summary>
        /// Returns a message for the player if the name is not allowed, or null if it's fine.
        /// One method owns all the rules, so IsValid and the UI can never disagree.
        /// </summary>
        public static string GetError(string rawName)
        {
            string name = Clean(rawName);

            if (name.Length == 0)
            {
                return "Please type your name.";
            }

            if (name.Length < MinLength)
            {
                return $"Your name needs at least {MinLength} letters.";
            }

            if (name.Length > MaxLength)
            {
                return $"Your name can have at most {MaxLength} letters.";
            }

            return null;
        }

        /// <summary>True if the cleaned name is between MinLength and MaxLength characters.</summary>
        public static bool IsValid(string rawName) => GetError(rawName) == null;
    }
}
