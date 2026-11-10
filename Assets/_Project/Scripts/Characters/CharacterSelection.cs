using BallroomLegends.Core;
using BallroomLegends.Player;
using TMPro;
using UnityEngine;

namespace BallroomLegends.Characters
{
    /// <summary>
    /// "Create your dancer" screen: type a name and pick boy or girl.
    /// Buttons call the public methods (Button > On Click () in the Inspector).
    /// </summary>
    public class CharacterSelection : MonoBehaviour
    {
        [Header("Name")]
        [SerializeField] private TMP_InputField nameInput;
        [Tooltip("Shows a message like 'Please type your name'. Optional.")]
        [SerializeField] private TMP_Text errorText;

        [Header("Shown when a character is highlighted (e.g. a gold frame)")]
        [SerializeField] private GameObject boyPreview;
        [SerializeField] private GameObject girlPreview;

        private DancerCharacter _highlighted = DancerCharacter.None;

        public DancerCharacter Highlighted => _highlighted;

        private void Start()
        {
            ShowError(string.Empty);

            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // Coming back to this screen? Pre-fill the previous choices.
                if (nameInput != null && !string.IsNullOrEmpty(gameManager.PlayerName))
                {
                    nameInput.text = gameManager.PlayerName;
                }

                Highlight(gameManager.SelectedCharacter);
            }
            else
            {
                Highlight(DancerCharacter.None);
            }
        }

        public void SelectBoy() => Highlight(DancerCharacter.Boy);

        public void SelectGirl() => Highlight(DancerCharacter.Girl);

        private void Highlight(DancerCharacter character)
        {
            _highlighted = character;

            // SetActive(bool) with a comparison: shows exactly one preview (or none).
            // The null checks mean a missing preview doesn't crash the screen.
            if (boyPreview != null)
            {
                boyPreview.SetActive(character == DancerCharacter.Boy);
            }

            if (girlPreview != null)
            {
                girlPreview.SetActive(character == DancerCharacter.Girl);
            }
        }

        public void ConfirmSelection()
        {
            string rawName = nameInput != null ? nameInput.text : string.Empty;

            // 1. Validate everything first ("guard clauses"), then act. Each check exits
            //    early, so the happy path at the bottom stays flat and easy to read.
            string nameError = PlayerNameRules.GetError(rawName);
            if (nameError != null)
            {
                ShowError(nameError);
                return;
            }

            if (_highlighted == DancerCharacter.None)
            {
                ShowError("Pick your dancer: boy or girl.");
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[CharacterSelection] No GameManager.");
                return;
            }

            // 2. Save the choices, 3. move on.
            gameManager.SetPlayerName(rawName);
            gameManager.SetSelectedCharacter(_highlighted);
            gameManager.ChangeState(GameState.DanceGuide);
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
            }
        }
    }
}
