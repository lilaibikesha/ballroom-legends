using BallroomLegends.Characters;
using BallroomLegends.Dance;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BallroomLegends.Core
{
    /// <summary>
    /// The long-lived "brain" of the game. It survives scene loads and owns state that
    /// several scenes need: the current <see cref="GameState"/> and the player's choices
    /// (name, dancer, chosen style).
    ///
    /// Design note: this uses the Singleton pattern (one global instance). It's simple and
    /// common in Unity, but it creates hidden dependencies. Keep it small: if it starts
    /// doing dance or scoring logic, that logic belongs in another class.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // The player's choices from the new-game flow.
        public string PlayerName { get; private set; } = string.Empty;
        public DancerCharacter SelectedCharacter { get; private set; } = DancerCharacter.None;
        public DanceStyle ChosenStyle { get; private set; } = DanceStyle.None;

        private void Awake()
        {
            // Singleton: if another GameManager already exists (e.g. we came back to the
            // MainMenu scene), destroy this duplicate and keep the original.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetPlayerName(string playerName)
        {
            // TODO (you): only accept names where PlayerNameRules.IsValid(...) is true, and
            // store the cleaned version (PlayerNameRules.Clean). Log a warning otherwise.
            PlayerName = playerName;
        }

        public void SetSelectedCharacter(DancerCharacter character)
        {
            // TODO (you): reject DancerCharacter.None with Debug.LogWarning, and log the
            // accepted choice with Debug.Log so you can see it in the Console.
            SelectedCharacter = character;
        }

        public void SetChosenStyle(DanceStyle style)
        {
            // TODO (you): reject DanceStyle.None, log the accepted choice.
            ChosenStyle = style;
        }

        public void ChangeState(GameState newState)
        {
            // TODO (you): implement a small state machine.
            //  1. Decide which transitions are legal. The new-game flow is:
            //     MainMenu -> CharacterSelection -> DanceGuide -> TrainingHall
            //     TrainingHall -> Dancing -> Results -> TrainingHall
            //     Going back to MainMenu is probably always fine. MainMenu -> Results is not.
            //     Reject illegal ones with Debug.LogWarning.
            //  2. Update CurrentState.
            //  3. Load the matching scene with LoadScene(...).
            //     Tip: a C# switch expression mapping GameState -> SceneNames.* keeps this tidy:
            //       string scene = newState switch { GameState.MainMenu => SceneNames.MainMenu, ... };
            Debug.Log($"[GameManager] ChangeState({newState}) is not implemented yet.");
        }

        private void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
