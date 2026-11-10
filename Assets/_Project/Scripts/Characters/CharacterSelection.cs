using BallroomLegends.Core;
using UnityEngine;

namespace BallroomLegends.Characters
{
    /// <summary>
    /// Controls the Character Selection screen. Wire the public methods to UI Buttons
    /// in the Inspector (Button component > On Click () > drag this object > pick the method).
    /// </summary>
    public class CharacterSelection : MonoBehaviour
    {
        [Header("Previews shown when a character is highlighted")]
        [SerializeField] private GameObject boyPreview;
        [SerializeField] private GameObject girlPreview;

        private DancerCharacter _highlighted = DancerCharacter.None;

        public DancerCharacter Highlighted => _highlighted;

        public void SelectBoy() => Highlight(DancerCharacter.Boy);

        public void SelectGirl() => Highlight(DancerCharacter.Girl);

        private void Highlight(DancerCharacter character)
        {
            _highlighted = character;

            // TODO (you): show the matching preview and hide the other one
            // (GameObject.SetActive). What should happen if a preview isn't assigned?
        }

        public void ConfirmSelection()
        {
            // TODO (you):
            //  1. Don't allow confirming while nothing is highlighted.
            //  2. Save the choice: GameManager.Instance.SetSelectedCharacter(...).
            //  3. Ask the GameManager to move on: ChangeState(GameState.Dancing).
            //  Debugging exercise: press Play directly in THIS scene. Why is
            //  GameManager.Instance null, and how could you fix it?
        }
    }
}
