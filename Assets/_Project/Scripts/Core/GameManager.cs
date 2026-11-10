using BallroomLegends.Characters;
using BallroomLegends.Competition;
using BallroomLegends.Dance;
using BallroomLegends.Player;
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

        /// <summary>
        /// The competition that just ended, for the Results screen and the Training Hall.
        /// Null until the first competition. The GameManager only stores it: the scoring
        /// logic lives in the Competition and Tournament classes.
        /// </summary>
        public CompetitionResult LastCompetitionResult { get; private set; }

        /// <summary>
        /// Creates the GameManager automatically when you press Play, in ANY scene.
        ///
        /// This answers the old debugging exercise ("why is GameManager.Instance null when I
        /// press Play in the CharacterSelection scene?"): the GameManager used to live only in
        /// the MainMenu scene, so starting anywhere else meant it never existed.
        /// [RuntimeInitializeOnLoadMethod] makes Unity call this static method on startup.
        /// AfterSceneLoad runs after the first scene's Awake calls but before any Start, so
        /// screens can safely use GameManager.Instance in Start().
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnStartup()
        {
            if (Instance != null)
            {
                return;
            }

            new GameObject(nameof(GameManager)).AddComponent<GameManager>();

            // Start in the state that matches the scene you pressed Play in.
            Instance.CurrentState = StateForScene(SceneManager.GetActiveScene().name);
            Debug.Log($"[GameManager] Created. Starting state: {Instance.CurrentState}");
        }

        private void Awake()
        {
            // Singleton: if another GameManager already exists, destroy this duplicate.
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

        // ---------- Player choices ----------

        /// <summary>Saves the cleaned name. Returns false (and changes nothing) if it's invalid.</summary>
        public bool SetPlayerName(string playerName)
        {
            if (!PlayerNameRules.IsValid(playerName))
            {
                Debug.LogWarning($"[GameManager] Rejected name '{playerName}': {PlayerNameRules.GetError(playerName)}");
                return false;
            }

            PlayerName = PlayerNameRules.Clean(playerName);
            Debug.Log($"[GameManager] Player name: {PlayerName}");
            return true;
        }

        public bool SetSelectedCharacter(DancerCharacter character)
        {
            if (character == DancerCharacter.None)
            {
                Debug.LogWarning("[GameManager] Rejected character: None.");
                return false;
            }

            SelectedCharacter = character;
            Debug.Log($"[GameManager] Character: {SelectedCharacter}");
            return true;
        }

        public bool SetChosenStyle(DanceStyle style)
        {
            if (style == DanceStyle.None)
            {
                Debug.LogWarning("[GameManager] Rejected style: None.");
                return false;
            }

            ChosenStyle = style;
            Debug.Log($"[GameManager] Chosen style: {ChosenStyle}");
            return true;
        }

        public bool SetCompetitionResult(CompetitionResult result)
        {
            if (result == null)
            {
                Debug.LogWarning("[GameManager] Rejected competition result: null.");
                return false;
            }

            LastCompetitionResult = result;
            Debug.Log($"[GameManager] Competition result: {result.Headline()}");
            return true;
        }

        // ---------- State machine ----------

        /// <summary>Moves to a new state and loads its scene, if the move is allowed.</summary>
        public void ChangeState(GameState newState)
        {
            if (!CanTransition(CurrentState, newState))
            {
                Debug.LogWarning($"[GameManager] Illegal transition {CurrentState} -> {newState}. Ignored.");
                return;
            }

            Debug.Log($"[GameManager] {CurrentState} -> {newState}");
            CurrentState = newState;
            LoadScene(SceneFor(newState));
        }

        /// <summary>
        /// The rules of the state machine: which state may follow which.
        /// It's static and pure (no Unity calls), so tests can check every rule
        /// without pressing Play. See Tests/EditMode/GameStateTransitionTests.cs.
        /// </summary>
        public static bool CanTransition(GameState from, GameState to)
        {
            if (from == to)
            {
                return false;
            }

            // Going back to the main menu is always allowed.
            if (to == GameState.MainMenu)
            {
                return true;
            }

            // A tuple switch lists every legal (from, to) pair in one readable table.
            return (from, to) switch
            {
                (GameState.MainMenu, GameState.CharacterSelection) => true,
                (GameState.CharacterSelection, GameState.DanceGuide) => true,
                (GameState.DanceGuide, GameState.TrainingHall) => true,
                (GameState.TrainingHall, GameState.DanceGuide) => true,
                (GameState.TrainingHall, GameState.Dancing) => true,
                (GameState.Dancing, GameState.Results) => true,
                (GameState.Dancing, GameState.TrainingHall) => true, // Withdraw
                (GameState.Results, GameState.TrainingHall) => true,
                _ => false
            };
        }

        /// <summary>Which scene shows each state.</summary>
        public static string SceneFor(GameState state)
        {
            return state switch
            {
                GameState.MainMenu => SceneNames.MainMenu,
                GameState.CharacterSelection => SceneNames.CharacterSelection,
                GameState.DanceGuide => SceneNames.DanceGuide,
                GameState.TrainingHall => SceneNames.TrainingHall,
                GameState.Dancing => SceneNames.BlackpoolBallroom,
                GameState.Results => SceneNames.Results,
                _ => SceneNames.MainMenu
            };
        }

        /// <summary>The reverse of <see cref="SceneFor"/>. Unknown scenes count as MainMenu.</summary>
        public static GameState StateForScene(string sceneName)
        {
            return sceneName switch
            {
                SceneNames.CharacterSelection => GameState.CharacterSelection,
                SceneNames.DanceGuide => GameState.DanceGuide,
                SceneNames.TrainingHall => GameState.TrainingHall,
                SceneNames.BlackpoolBallroom => GameState.Dancing,
                SceneNames.Results => GameState.Results,
                _ => GameState.MainMenu
            };
        }

        private void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
