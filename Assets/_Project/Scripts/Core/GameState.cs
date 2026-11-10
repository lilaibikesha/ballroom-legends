namespace BallroomLegends.Core
{
    /// <summary>
    /// The high-level states the game can be in. Each state maps to one scene.
    ///
    /// New-game flow:
    /// MainMenu -> CharacterSelection (name + dancer) -> DanceGuide (learn the 10 dances,
    /// pick Standard or Latin) -> TrainingHall (home base) -> Dancing -> Results
    /// </summary>
    public enum GameState
    {
        MainMenu,
        CharacterSelection,
        DanceGuide,
        TrainingHall,
        Dancing,
        Results
    }
}
