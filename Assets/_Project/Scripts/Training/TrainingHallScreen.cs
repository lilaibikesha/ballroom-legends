using BallroomLegends.Core;
using TMPro;
using UnityEngine;

namespace BallroomLegends.Training
{
    /// <summary>
    /// The Ballroom Training Hall: the player's home base after the new-game flow.
    /// For now it just welcomes the player. Later: practice sessions, coach, outfits,
    /// and the door to the next competition.
    /// </summary>
    public class TrainingHallScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text welcomeText;

        private void Start()
        {
            // TODO (you): read PlayerName and ChosenStyle from GameManager.Instance and show
            // something like "Welcome to the training hall, Aibike! Today: Latin practice."
            // What should it say if you pressed Play directly in this scene (no GameManager)?
        }

        /// <summary>Wire to a "Go to competition" button (later).</summary>
        public void GoToCompetition()
        {
            // TODO (later): GameManager.Instance.ChangeState(GameState.Dancing).
            Debug.Log("[TrainingHall] Competitions come later.");
        }
    }
}
