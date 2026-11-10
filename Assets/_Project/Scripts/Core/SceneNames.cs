namespace BallroomLegends.Core
{
    /// <summary>
    /// Scene names in one place, so a typo becomes a compile error instead of a runtime bug.
    /// These must match the .unity file names in Assets/_Project/Scenes.
    /// </summary>
    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string CharacterSelection = "CharacterSelection";
        public const string DanceGuide = "DanceGuide";
        public const string TrainingHall = "TrainingHall";
        public const string BlackpoolBallroom = "BlackpoolBallroom";
        public const string Results = "Results";

        /// <summary>All scenes, in Build Settings order (index 0 loads first).</summary>
        public static readonly string[] All =
        {
            MainMenu,
            CharacterSelection,
            DanceGuide,
            TrainingHall,
            BlackpoolBallroom,
            Results
        };
    }
}
