using BallroomLegends.Core;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>Checks the state machine rules in GameManager without pressing Play.</summary>
    public class GameStateTransitionTests
    {
        // [TestCase] runs the same test once per line of data.
        [TestCase(GameState.MainMenu, GameState.CharacterSelection)]
        [TestCase(GameState.CharacterSelection, GameState.DanceGuide)]
        [TestCase(GameState.DanceGuide, GameState.TrainingHall)]
        [TestCase(GameState.TrainingHall, GameState.DanceGuide)]
        [TestCase(GameState.TrainingHall, GameState.Dancing)]
        [TestCase(GameState.Dancing, GameState.Results)]
        [TestCase(GameState.Results, GameState.TrainingHall)]
        [TestCase(GameState.DanceGuide, GameState.MainMenu)]
        public void LegalTransitions_AreAllowed(GameState from, GameState to)
        {
            Assert.IsTrue(GameManager.CanTransition(from, to));
        }

        [TestCase(GameState.MainMenu, GameState.Results)]
        [TestCase(GameState.MainMenu, GameState.TrainingHall)]
        [TestCase(GameState.CharacterSelection, GameState.TrainingHall)]
        [TestCase(GameState.DanceGuide, GameState.Dancing)]
        [TestCase(GameState.TrainingHall, GameState.TrainingHall)]
        public void IllegalTransitions_AreRejected(GameState from, GameState to)
        {
            Assert.IsFalse(GameManager.CanTransition(from, to));
        }

        [Test]
        public void EveryState_HasAScene_AndMapsBack()
        {
            foreach (GameState state in System.Enum.GetValues(typeof(GameState)))
            {
                string scene = GameManager.SceneFor(state);
                CollectionAssert.Contains(SceneNames.All, scene);
                Assert.AreEqual(state, GameManager.StateForScene(scene));
            }
        }
    }
}
