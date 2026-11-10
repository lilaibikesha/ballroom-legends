using BallroomLegends.Core;
using BallroomLegends.Dance;
using TMPro;
using UnityEngine;

namespace BallroomLegends.Training
{
    /// <summary>
    /// The Ballroom Training Hall: the player's home base after the new-game flow.
    /// For now it welcomes the player. Later: practice sessions, coach, outfits,
    /// and the door to the next competition.
    /// </summary>
    public class TrainingHallScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text welcomeText;

        private void Start()
        {
            GameManager gameManager = GameManager.Instance;

            // Sensible fallbacks, so pressing Play directly in this scene still works.
            string playerName = gameManager != null && !string.IsNullOrEmpty(gameManager.PlayerName)
                ? gameManager.PlayerName
                : "dancer";
            DanceStyle style = gameManager != null ? gameManager.ChosenStyle : DanceStyle.None;

            string styleLine = style switch
            {
                DanceStyle.Standard => "Your path: Standard. Waltz, Tango, Viennese Waltz, Slow Foxtrot and Quickstep await.",
                DanceStyle.Latin => "Your path: Latin. Samba, Cha Cha Cha, Rumba, Paso Doble and Jive await.",
                _ => "You haven't chosen a style yet. Visit the Dance Guide."
            };

            if (welcomeText != null)
            {
                welcomeText.text = $"Welcome to the training hall, {playerName}!\n{styleLine}";
            }
        }

        public void OpenDanceGuide() => ChangeState(GameState.DanceGuide);

        public void BackToMainMenu() => ChangeState(GameState.MainMenu);

        /// <summary>The "Go to competition" button. Competitions come in a later milestone.</summary>
        public void GoToCompetition()
        {
            Debug.Log("[TrainingHall] Competitions come later (DanceManager + TournamentManager).");
        }

        private static void ChangeState(GameState state)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(state);
            }
        }
    }
}
