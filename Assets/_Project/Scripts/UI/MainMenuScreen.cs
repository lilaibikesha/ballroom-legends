using BallroomLegends.Core;
using UnityEngine;

namespace BallroomLegends.UI
{
    /// <summary>
    /// Main menu. Wire the "Start Game" button's On Click () to <see cref="StartGame"/>.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        public void StartGame()
        {
            // TODO (you): ask the GameManager to move to GameState.CharacterSelection.
        }

        public void QuitGame()
        {
            // Application.Quit does nothing inside the Unity Editor, only in a built game.
            Debug.Log("[MainMenu] Quit");
            Application.Quit();
        }
    }
}
