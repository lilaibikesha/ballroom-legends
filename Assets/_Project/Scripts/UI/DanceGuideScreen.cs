using System.Collections.Generic;
using System.Text;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallroomLegends.UI
{
    /// <summary>
    /// The Dance Guide: the "quick preview" the player sees before choosing a style.
    ///
    /// One scene, four panels, only one visible at a time:
    ///   1. Style overview  - two buttons: Standard / Latin (+ "I'm ready to choose")
    ///   2. Dance list      - the 5 dances of that style
    ///   3. Dance detail    - history, characteristics, skills, tempo, video button
    ///   4. Style choice    - "Standard only" or "Latin only" -> Training Hall
    /// </summary>
    public class DanceGuideScreen : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DanceLibrary library;

        [Header("Panels (only one is visible at a time)")]
        [SerializeField] private GameObject styleOverviewPanel;
        [SerializeField] private GameObject danceListPanel;
        [SerializeField] private GameObject danceDetailPanel;
        [SerializeField] private GameObject styleChoicePanel;

        [Header("Dance list panel")]
        [SerializeField] private TMP_Text danceListTitle;
        [Tooltip("5 buttons. Each one shows a dance name and opens its detail panel.")]
        [SerializeField] private Button[] danceButtons = new Button[5];

        [Header("Dance detail panel")]
        [SerializeField] private TMP_Text danceNameText;
        [SerializeField] private TMP_Text originText;
        [SerializeField] private TMP_Text historyText;
        [SerializeField] private TMP_Text musicText;
        [SerializeField] private TMP_Text characteristicsText;
        [SerializeField] private TMP_Text skillsText;
        [SerializeField] private Button videoButton;
        [SerializeField] private TMP_Text videoButtonLabel;

        private DanceStyle _currentStyle = DanceStyle.None;
        private DanceInfo _currentDance;

        private void Start()
        {
            if (library == null)
            {
                Debug.LogError("[DanceGuide] No DanceLibrary assigned. Drag Assets/_Project/Data/DanceLibrary into the 'Library' slot.");
            }

            ShowStyleOverview();
        }

        // ---------- Navigation (buttons call these) ----------

        public void ShowStyleOverview() => ShowOnly(styleOverviewPanel);

        public void ShowStandardDances() => ShowDanceList(DanceStyle.Standard);

        public void ShowLatinDances() => ShowDanceList(DanceStyle.Latin);

        /// <summary>The "Back" button on the detail panel.</summary>
        public void BackToDanceList() => ShowDanceList(_currentStyle);

        /// <summary>The "I'm ready to choose" button.</summary>
        public void ShowStyleChoice() => ShowOnly(styleChoicePanel);

        public void ChooseStandard() => ChooseStyle(DanceStyle.Standard);

        public void ChooseLatin() => ChooseStyle(DanceStyle.Latin);

        // ---------- Screens ----------

        private void ShowDanceList(DanceStyle style)
        {
            _currentStyle = style;
            ShowOnly(danceListPanel);

            SetText(danceListTitle, style == DanceStyle.Latin ? "Latin dances" : "Standard dances");

            List<DanceInfo> dances = library != null ? library.GetDances(style) : new List<DanceInfo>();

            for (int i = 0; i < danceButtons.Length; i++)
            {
                Button button = danceButtons[i];
                if (button == null)
                {
                    continue;
                }

                // Remove the listeners from last time. Without this, opening Standard, going
                // back and opening Latin would leave BOTH dances attached to the same button.
                // (RemoveAllListeners only removes listeners added from code, not Inspector ones.)
                button.onClick.RemoveAllListeners();

                bool hasDance = i < dances.Count;
                button.gameObject.SetActive(hasDance);
                if (!hasDance)
                {
                    continue;
                }

                // Copy into a local variable INSIDE the loop and capture that in the lambda.
                // If the lambda used dances[i] directly, it would capture the variable i, and
                // every button would read i's value at click time (5, past the end of the list).
                DanceInfo dance = dances[i];

                SetText(button.GetComponentInChildren<TMP_Text>(), dance.DisplayName);
                button.onClick.AddListener(() => ShowDanceDetail(dance));
            }
        }

        private void ShowDanceDetail(DanceInfo dance)
        {
            _currentDance = dance;
            ShowOnly(danceDetailPanel);

            SetText(danceNameText, dance.DisplayName);
            SetText(originText, $"Origin: {dance.Origin}");
            SetText(historyText, dance.History);
            SetText(musicText,
                $"<b>Music</b>\n{dance.TimeSignature} time, {dance.TempoMinBarsPerMinute}-{dance.TempoMaxBarsPerMinute} bars per minute\n" +
                $"Difficulty: {dance.Difficulty} / 5");
            SetText(characteristicsText, "<b>What it feels like</b>\n" + ToBulletList(dance.Characteristics));
            SetText(skillsText, "<b>Skills you need</b>\n" + ToBulletList(dance.Skills));

            // The video button is always visible. Until a clip is assigned in the
            // DanceInfo asset it's greyed out (interactable = false).
            if (videoButton != null)
            {
                videoButton.interactable = dance.HasVideo;
            }

            SetText(videoButtonLabel, dance.HasVideo ? "Watch video" : "Video coming soon");
        }

        public void PlayVideo()
        {
            // Placeholder: videos come later. When you add one, look up the VideoPlayer
            // component (it can play _currentDance.Video onto a RawImage via a RenderTexture).
            if (_currentDance == null || !_currentDance.HasVideo)
            {
                Debug.Log("[DanceGuide] No video yet for this dance.");
                return;
            }

            Debug.Log($"[DanceGuide] TODO: play video for {_currentDance.DisplayName}");
        }

        private void ChooseStyle(DanceStyle style)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[DanceGuide] No GameManager.");
                return;
            }

            if (gameManager.SetChosenStyle(style))
            {
                gameManager.ChangeState(GameState.TrainingHall);
            }
        }

        // ---------- Helpers ----------

        /// <summary>Shows one panel and hides the other three.</summary>
        private void ShowOnly(GameObject panelToShow)
        {
            GameObject[] panels = { styleOverviewPanel, danceListPanel, danceDetailPanel, styleChoicePanel };

            foreach (GameObject panel in panels)
            {
                if (panel != null)
                {
                    panel.SetActive(panel == panelToShow);
                }
            }
        }

        /// <summary>
        /// Builds "- item" lines. StringBuilder avoids creating a new string for every "+",
        /// which matters in loops (strings in C# are immutable).
        /// </summary>
        private static string ToBulletList(IReadOnlyList<string> items)
        {
            var builder = new StringBuilder();
            foreach (string item in items)
            {
                builder.Append("- ").AppendLine(item);
            }

            return builder.ToString();
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
