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
    ///
    /// Wire buttons in the Inspector: Button > On Click () > drag this object > pick a method.
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
            ShowStyleOverview();
        }

        // ---------- Navigation (wire these to buttons) ----------

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

            // TODO (you):
            //  1. Set danceListTitle.text to "Standard dances" or "Latin dances".
            //  2. Get the dances: library.GetDances(style).
            //  3. For each button i: if there is a dance i, show the button, set its label
            //     (button.GetComponentInChildren<TMP_Text>().text) and make clicking it call
            //     ShowDanceDetail(thatDance). Hide buttons that have no dance.
            //     - Code clicks: button.onClick.RemoveAllListeners(); then
            //       button.onClick.AddListener(() => ShowDanceDetail(dance));
            //     - Why RemoveAllListeners first? (Open Standard, go back, open it again...)
            //     - Classic C# trap: capturing the loop variable `i` inside the lambda.
            //       Copy the dance into a local variable inside the loop and capture that.
        }

        private void ShowDanceDetail(DanceInfo dance)
        {
            _currentDance = dance;
            ShowOnly(danceDetailPanel);

            // TODO (you): fill the texts from `dance`:
            //  - danceNameText: dance.DisplayName
            //  - originText: "Origin: " + dance.Origin
            //  - historyText: dance.History
            //  - musicText: e.g. "3/4 time, 28-30 bars per minute" (TimeSignature, TempoMin/Max...)
            //    Bonus: also show difficulty as stars, e.g. new string('*', dance.Difficulty).
            //  - characteristicsText / skillsText: one bullet per line.
            //    Hint: string.Join("\n", ...) or a StringBuilder.
            //  - Video: if dance.HasVideo, label "Watch video" and button interactable;
            //    otherwise label "Video coming soon" and videoButton.interactable = false.
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
            // TODO (you):
            //  1. Save it: GameManager.Instance.SetChosenStyle(style).
            //  2. Go to the training hall: GameManager.Instance.ChangeState(GameState.TrainingHall).
            //  Remember the debugging exercise: what if GameManager.Instance is null here?
        }

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
    }
}
