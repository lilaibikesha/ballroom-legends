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
            // TODO (you): handle null, then trim. Hint: string.Trim().
            return rawName ?? string.Empty;
        }

        /// <summary>True if the cleaned name is between MinLength and MaxLength characters.</summary>
        public static bool IsValid(string rawName)
        {
            // TODO (you): clean the name first, then check the length rules.
            // Stretch: should "   " (only spaces) be valid? Should digits be allowed?
            return false;
        }
    }
}
