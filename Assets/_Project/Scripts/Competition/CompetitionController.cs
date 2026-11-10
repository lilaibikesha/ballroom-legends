using System.Collections.Generic;
using System.Text;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using BallroomLegends.Tournament;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallroomLegends.Competition
{
    /// <summary>
    /// The Blackpool Ballroom scene's director. It connects the two systems and the UI:
    ///
    ///   Intro panel --Dance!--> DanceManager runs the routine (HUD shows beats + feedback)
    ///        ^                         |
    ///        |                   DanceFinished
    ///   Continue                       v
    ///        |       judges mark every couple, TournamentManager ranks them
    ///        +------ Round result panel --(eliminated or Final done)--> Results scene
    ///
    /// DanceManager knows nothing about tournaments and TournamentManager knows nothing
    /// about rhythm. Only this class knows both ("separation of concerns").
    /// </summary>
    public class CompetitionController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private DanceManager danceManager;
        [SerializeField] private TournamentManager tournamentManager;
        [Tooltip("Used to pick the competition dance for the chosen style (Waltz for Standard, Samba for Latin).")]
        [SerializeField] private DanceLibrary library;

        [Header("Field")]
        [SerializeField, Range(0f, 100f)] private float aiMinSkill = 45f;
        [SerializeField, Range(0f, 100f)] private float aiMaxSkill = 88f;
        [Tooltip("How much an AI couple's level changes from round to round (standard deviation, in skill points).")]
        [SerializeField, Min(0f)] private float aiFormVariation = 3f;
        [Tooltip("How much the judges disagree (standard deviation, in marks).")]
        [SerializeField, Min(0f)] private float judgeDisagreement = 0.8f;
        [Tooltip("0 = a different competition every time. Any other number replays the same one (handy for debugging).")]
        [SerializeField] private int randomSeed;

        [Header("Panels (one visible at a time)")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject roundResultPanel;

        [Header("Intro")]
        [SerializeField] private TMP_Text roundTitleText;
        [SerializeField] private TMP_Text introText;

        [Header("HUD")]
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private RectTransform beatPulse;

        [Header("Floor")]
        [Tooltip("Your couple's marker. It travels around the floor (the line of dance) with every good step.")]
        [SerializeField] private RectTransform coupleMarker;
        [SerializeField] private Vector2 floorCenter = new Vector2(0f, -60f);
        [SerializeField] private Vector2 floorRadius = new Vector2(640f, 250f);
        [SerializeField, Min(4)] private int stepsPerLap = 24;

        [Header("Round result")]
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultBodyText;
        [SerializeField] private TMP_Text continueButtonLabel;

        private const string Gold = "#F5CC6B";
        private const string Dim = "#FFFFFF59";
        private const int TopToShow = 6;
        private static readonly string[] CountWords = { "ONE", "two", "three", "four", "five", "six" };

        private System.Random _random;
        private JudgePanel _judges;
        private Couple _player;
        private CompetitionResult _result;
        private bool _competitionOver;
        private int _stepsTravelled;
        private float _pulse;

        private void OnEnable()
        {
            if (danceManager != null)
            {
                danceManager.BeatTicked += OnBeat;
                danceManager.StepJudged += OnStepJudged;
                danceManager.DanceFinished += OnDanceFinished;
            }
        }

        private void OnDisable()
        {
            // Always unsubscribe what you subscribed, or the DanceManager keeps calling a
            // destroyed object.
            if (danceManager != null)
            {
                danceManager.BeatTicked -= OnBeat;
                danceManager.StepJudged -= OnStepJudged;
                danceManager.DanceFinished -= OnDanceFinished;
            }
        }

        private void Start()
        {
            if (danceManager == null || tournamentManager == null)
            {
                Debug.LogError("[Competition] Assign the DanceManager and TournamentManager in the Inspector.");
                enabled = false;
                return;
            }

            _random = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);
            _judges = new JudgePanel(_random, judgeDisagreement);

            DanceInfo dance = PickDance();
            if (dance != null)
            {
                danceManager.Configure(dance);
            }

            GameManager gameManager = GameManager.Instance;
            string playerName = gameManager != null && !string.IsNullOrEmpty(gameManager.PlayerName)
                ? gameManager.PlayerName
                : "You";
            _player = new Couple($"{playerName} (you)", true);

            SetUpField();

            _result = new CompetitionResult
            {
                DanceName = danceManager.DanceName,
                PlayerCoupleName = _player.Name,
                FieldSize = tournamentManager.Couples.Count
            };

            MoveMarker(instant: true);
            ShowIntro();
        }

        private void Update()
        {
            // The pulse circle "pops" on every beat and shrinks back.
            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 4f);
            if (beatPulse != null)
            {
                beatPulse.localScale = Vector3.one * (1f + 0.35f * _pulse);
            }

            MoveMarker(instant: false);
        }

        // ---------- Setup ----------

        /// <summary>The first dance of the chosen style opens the competition: Waltz or Samba.</summary>
        private DanceInfo PickDance()
        {
            if (library == null)
            {
                return null; // DanceManager keeps its own Waltz values.
            }

            GameManager gameManager = GameManager.Instance;
            DanceStyle style = gameManager != null ? gameManager.ChosenStyle : DanceStyle.None;
            if (style == DanceStyle.None)
            {
                style = DanceStyle.Standard;
            }

            List<DanceInfo> dances = library.GetDances(style);
            return dances.Count > 0 ? dances[0] : null;
        }

        private void SetUpField()
        {
            tournamentManager.ResetTournament();

            int aiCount = tournamentManager.FieldSize - 1;
            List<Couple> field = CompetitorField.CreateAiCouples(aiCount, _random, aiMinSkill, Mathf.Max(aiMinSkill, aiMaxSkill));

            // Ties are broken by start number (see TournamentManager.GetRanking). A random
            // start number keeps that fair: always first would win every tie.
            field.Insert(_random.Next(field.Count + 1), _player);

            foreach (Couple couple in field)
            {
                tournamentManager.RegisterCouple(couple);
            }
        }

        // ---------- Intro ----------

        private void ShowIntro()
        {
            RoundType round = tournamentManager.CurrentRound;
            ShowOnly(introPanel);

            SetText(roundTitleText, $"{TournamentRules.DisplayName(round)}: {danceManager.DanceName}");

            string recall = TournamentRules.IsLastRound(round)
                ? "The judges will place all six finalists."
                : $"The best {tournamentManager.CouplesAdvancingFromCurrentRound} are recalled to the " +
                  $"{TournamentRules.DisplayName(TournamentRules.Next(round))}.";

            SetText(introText,
                $"{tournamentManager.Couples.Count} couples on the floor. {recall}\n\n" +
                $"Press <b>SPACE</b> on every beat: {CountPattern()}.\n" +
                $"Listen to the {danceManager.CountInBeats}-beat count-in, then dance {danceManager.StepCount} steps " +
                $"({danceManager.BeatsPerMinute:0} beats per minute).");
        }

        /// <summary>The "Dance!" button.</summary>
        public void BeginRound()
        {
            ShowOnly(hudPanel);
            SetText(feedbackText, string.Empty);
            SetText(countText, "Listen...");
            danceManager.StartDance();
            UpdateScore();
        }

        // ---------- During the dance (DanceManager events) ----------

        private void OnBeat(int songBeat, int beatInBar)
        {
            if (songBeat < danceManager.CountInBeats)
            {
                SetText(countText, $"Get ready... {danceManager.CountInBeats - songBeat}");
            }
            else
            {
                SetText(countText, CountLine(beatInBar));
            }

            _pulse = beatInBar == 0 ? 1.4f : 0.8f;
        }

        private void OnStepJudged(HitResult result, float offset)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    ShowFeedback("Perfect!", UiColors.Gold);
                    break;
                case HitResult.Good:
                    ShowFeedback(offset < 0f ? "Good (a bit early)" : "Good (a bit late)", Color.white);
                    break;
                default:
                    string why = float.IsInfinity(offset) ? "no step" : offset < 0f ? "too early" : "too late";
                    ShowFeedback($"Miss ({why})", UiColors.Miss);
                    break;
            }

            // The couple only travels when the step was danced.
            if (result != HitResult.Miss)
            {
                _stepsTravelled++;
            }

            UpdateScore();
        }

        private void OnDanceFinished(PerformanceTracker tracker)
        {
            RoundType round = tournamentManager.CurrentRound;
            _player.Skill = tracker.Score;

            // Every couple dances at the same time; the judges mark them all.
            int[] playerMarks = new int[0];
            foreach (Couple couple in tournamentManager.Couples)
            {
                float level = couple.IsPlayer
                    ? couple.Skill
                    : couple.Skill + (float)(RandomUtil.NextGaussian(_random) * aiFormVariation);

                int[] marks = _judges.Score(level, tournamentManager.NumberOfJudges);
                tournamentManager.SubmitScores(couple, marks);

                if (couple.IsPlayer)
                {
                    playerMarks = marks;
                }
            }

            List<Couple> ranking = tournamentManager.GetRanking();
            int place = ranking.IndexOf(_player) + 1;
            bool isFinal = TournamentRules.IsLastRound(round);
            bool recalled = !isFinal && place <= tournamentManager.CouplesAdvancingFromCurrentRound;

            var report = new RoundReport
            {
                Round = round,
                CouplesInRound = ranking.Count,
                Place = place,
                Recalled = recalled,
                DanceScore = tracker.Score,
                Perfect = tracker.PerfectCount,
                Good = tracker.GoodCount,
                Miss = tracker.MissCount,
                MaxCombo = tracker.MaxCombo,
                Marks = playerMarks
            };

            _result.Rounds.Add(report);
            _result.LastRoundTop = TopOf(ranking);
            _competitionOver = isFinal || !recalled;

            ShowRoundResult(report);
        }

        // ---------- Round result ----------

        private void ShowRoundResult(RoundReport report)
        {
            ShowOnly(roundResultPanel);

            string title;
            if (report.Round == RoundType.Final)
            {
                title = report.Place == 1 ? "Champions!" : $"{TextFormat.Ordinal(report.Place)} place in the Final";
            }
            else
            {
                title = report.Recalled ? "Recalled!" : "Not recalled this time";
            }

            SetText(resultTitleText, title);

            var body = new StringBuilder();
            body.AppendLine($"Your dance: <b>{report.DanceScore:0}/100</b>   " +
                            $"({report.Perfect} perfect, {report.Good} good, {report.Miss} miss, best combo {report.MaxCombo})");
            body.AppendLine($"Judges' marks: {string.Join("  ", report.Marks)}  =  <b>{report.TotalMarks}</b>");
            body.AppendLine($"Place: <b>{TextFormat.Ordinal(report.Place)}</b> of {report.CouplesInRound}");

            if (report.Recalled)
            {
                body.AppendLine($"You're through to the {TournamentRules.DisplayName(TournamentRules.Next(report.Round))}!");
            }
            else if (report.Round != RoundType.Final)
            {
                body.AppendLine($"The top {tournamentManager.CouplesAdvancingFromCurrentRound} went through. Keep training!");
            }

            body.AppendLine();
            body.AppendLine($"Top of the {TournamentRules.DisplayName(report.Round)}:");
            foreach (RankingEntry entry in _result.LastRoundTop)
            {
                string line = $"{TextFormat.Ordinal(entry.Place)}  {entry.CoupleName}  ({entry.TotalMarks})";
                body.AppendLine(entry.IsPlayer ? $"<color={Gold}>{line}</color>" : line);
            }

            if (report.Place > TopToShow)
            {
                body.AppendLine($"<color={Gold}>...  {TextFormat.Ordinal(report.Place)}  {_player.Name}  ({report.TotalMarks})</color>");
            }

            SetText(resultBodyText, body.ToString());

            string nextLabel = _competitionOver
                ? "See results"
                : $"On to the {TournamentRules.DisplayName(TournamentRules.Next(report.Round))}";
            SetText(continueButtonLabel, nextLabel);
        }

        /// <summary>The Continue button on the round result panel.</summary>
        public void Continue()
        {
            if (_competitionOver)
            {
                FinishCompetition();
                return;
            }

            tournamentManager.AdvanceToNextRound();
            ShowIntro();
        }

        /// <summary>The Withdraw button: leave without a result.</summary>
        public void Withdraw()
        {
            danceManager.StopDance();
            ChangeState(GameState.TrainingHall);
        }

        private void FinishCompetition()
        {
            if (TournamentRules.IsLastRound(tournamentManager.CurrentRound))
            {
                tournamentManager.AdvanceToNextRound(); // marks the tournament finished
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[Competition] No GameManager.");
                return;
            }

            gameManager.SetCompetitionResult(_result);
            gameManager.ChangeState(GameState.Results);
        }

        // ---------- Helpers ----------

        private List<RankingEntry> TopOf(List<Couple> ranking)
        {
            var top = new List<RankingEntry>();
            for (int i = 0; i < ranking.Count && i < TopToShow; i++)
            {
                top.Add(new RankingEntry
                {
                    Place = i + 1,
                    CoupleName = ranking[i].Name,
                    TotalMarks = tournamentManager.GetTotal(ranking[i]),
                    IsPlayer = ranking[i].IsPlayer
                });
            }

            return top;
        }

        /// <summary>"ONE two three" for Waltz, with the current beat highlighted in gold.</summary>
        private string CountLine(int beatInBar)
        {
            var line = new StringBuilder();
            for (int i = 0; i < danceManager.BeatsPerBar; i++)
            {
                string word = CountWords[Mathf.Min(i, CountWords.Length - 1)];
                line.Append(i == beatInBar ? $"<color={Gold}><b>{word}</b></color>" : $"<color={Dim}>{word}</color>");
                line.Append("   ");
            }

            return line.ToString().TrimEnd();
        }

        private string CountPattern()
        {
            var words = new List<string>();
            for (int i = 0; i < danceManager.BeatsPerBar; i++)
            {
                words.Add(CountWords[Mathf.Min(i, CountWords.Length - 1)]);
            }

            return string.Join(" - ", words);
        }

        private void UpdateScore()
        {
            PerformanceTracker tracker = danceManager.Tracker;
            if (tracker == null)
            {
                return;
            }

            SetText(scoreText, $"Score {tracker.Score:0}   Combo {tracker.Combo}\n" +
                               $"Step {tracker.JudgedSteps}/{tracker.TotalSteps}");
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
            feedbackText.color = color;
        }

        /// <summary>Places the marker on an ellipse; it glides there smoothly unless instant.</summary>
        private void MoveMarker(bool instant)
        {
            if (coupleMarker == null)
            {
                return;
            }

            // Ballroom couples travel anticlockwise around the floor ("line of dance").
            // Start at the bottom of the ellipse (-90 degrees) and go anticlockwise.
            float angle = -Mathf.PI / 2f + _stepsTravelled * (2f * Mathf.PI / stepsPerLap);
            Vector2 target = floorCenter + new Vector2(Mathf.Cos(angle) * floorRadius.x, Mathf.Sin(angle) * floorRadius.y);

            coupleMarker.anchoredPosition = instant
                ? target
                : Vector2.Lerp(coupleMarker.anchoredPosition, target, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }

        private void ShowOnly(GameObject panel)
        {
            SetActive(introPanel, panel == introPanel);
            SetActive(hudPanel, panel == hudPanel);
            SetActive(roundResultPanel, panel == roundResultPanel);

            // Forget which button was clicked last. Otherwise SPACE (the UI "submit" key)
            // could press that button again while you're dancing.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(TMP_Text target, string text)
        {
            if (target != null)
            {
                target.text = text;
            }
        }

        private static void ChangeState(GameState state)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(state);
            }
        }

        /// <summary>Colors for feedback text.</summary>
        private static class UiColors
        {
            public static readonly Color Gold = new Color(0.96f, 0.80f, 0.42f);
            public static readonly Color Miss = new Color(1f, 0.45f, 0.45f);
        }
    }
}
