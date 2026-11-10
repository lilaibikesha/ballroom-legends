using BallroomLegends.Core;
using UnityEngine;

namespace BallroomLegends.UI
{
    /// <summary>
    /// Main menu. The "Start Game" button's On Click () calls <see cref="StartGame"/>.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        public void StartGame()
        {
            // Note: we compare with null explicitly instead of using "?.".
            // Unity overrides == for its objects (a destroyed object == null), and "?."
            // skips that override, so explicit checks are the safe habit in Unity.
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenu] No GameManager.");
                return;
            }

            GameManager.Instance.ChangeState(GameState.CharacterSelection);
        }

        public void QuitGame()
        {
            // Application.Quit does nothing inside the Unity Editor, only in a built game.
            Debug.Log("[MainMenu] Quit");
            Application.Quit();
        }
    }
}
