using BallroomLegends.Core;
using TMPro;
using UnityEngine;

namespace BallroomLegends.Characters
{
    /// <summary>
    /// "Create your dancer" screen: type a name and pick boy or girl.
    /// Wire the public methods to UI Buttons in the Inspector
    /// (Button component > On Click () > drag this object > pick the method).
    /// </summary>
    public class CharacterSelection : MonoBehaviour
    {
        [Header("Name")]
        [SerializeField] private TMP_InputField nameInput;
        [Tooltip("Shows a message like 'Name must be 2-16 letters'. Optional.")]
        [SerializeField] private TMP_Text errorText;

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
            //  1. Read the name from nameInput.text and check it with PlayerNameRules.IsValid
            //     (namespace BallroomLegends.Player). If it's invalid, show a message in
            //     errorText and stop.
            //  2. Don't allow confirming while no dancer is highlighted.
            //  3. Save both: GameManager.Instance.SetPlayerName(...) and SetSelectedCharacter(...).
            //  4. Move on: GameManager.Instance.ChangeState(GameState.DanceGuide).
            //  Debugging exercise: press Play directly in THIS scene. Why is
            //  GameManager.Instance null, and how could you fix it?
        }
    }
}
