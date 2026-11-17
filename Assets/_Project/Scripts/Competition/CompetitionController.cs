using System.Collections.Generic;
using System.Text;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using BallroomLegends.Floor;
using BallroomLegends.Tournament;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallroomLegends.Competition
{
    /// <summary>
    /// The Blackpool Ballroom scene's director. It connects the systems and the UI.
    ///
    /// Like the real festival, every round has all five dances of your style:
    ///
    ///   Intro panel --Dance!--> dance 1 (HUD) --> dance break --Next--> dance 2 ... dance 5
    ///        ^                                                                 |
    ///        |                                       judges' totals rank every couple
    ///   Continue                                                               v
    ///        +----------- Round result panel --(eliminated or Final done)--> Results scene
    ///
    /// After every dance the judges mark every couple for that dance; the round's ranking
    /// is the total of all the dances. DanceManager knows nothing about tournaments,
    /// TournamentManager knows nothing about rhythm and BallroomFloor only draws. Only this
    /// class knows them all ("separation of concerns").
    /// </summary>
    public class CompetitionController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private DanceManager danceManager;
        [SerializeField] private TournamentManager tournamentManager;
        [Tooltip("Where the dances of your style come from (Waltz, Tango, ... or Samba, Cha Cha Cha, ...).")]
        [SerializeField] private DanceLibrary library;
        [Tooltip("Draws the couples, judges and audience. Optional: the competition works without it.")]
        [SerializeField] private BallroomFloor floor;

        [Header("Programme")]
        [Tooltip("Dances in every round. 5 = the real thing. Lower it to test faster.")]
        [SerializeField, Range(1, 5)] private int dancesPerRound = 5;

        [Header("Field")]
        [SerializeField, Range(0f, 100f)] private float aiMinSkill = 45f;
        [SerializeField, Range(0f, 100f)] private float aiMaxSkill = 88f;
        [Tooltip("How much an AI couple's level changes from dance to dance (standard deviation, in skill points).")]
        [SerializeField, Min(0f)] private float aiFormVariation = 3f;
        [Tooltip("How much the judges disagree (standard deviation, in marks).")]
        [SerializeField, Min(0f)] private float judgeDisagreement = 0.8f;
        [Tooltip("0 = a different competition every time. Any other number replays the same one (handy for debugging).")]
        [SerializeField] private int randomSeed;

        [Header("Panels (one visible at a time)")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject danceBreakPanel;
        [SerializeField] private GameObject roundResultPanel;

        [Header("Intro")]
        [SerializeField] private TMP_Text roundTitleText;
        [SerializeField] private TMP_Text introText;

        [Header("HUD")]
        [SerializeField] private TMP_Text danceTitleText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private RectTransform beatPulse;

        [Header("Dance break (between two dances)")]
        [SerializeField] private TMP_Text breakTitleText;
        [SerializeField] private TMP_Text breakBodyText;
        [SerializeField] private TMP_Text breakButtonLabel;

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
        private DanceStyle _style;
        private CompetitionResult _result;
        private bool _competitionOver;
        private float _pulse;

        // The dances of every round. A null entry means "the DanceManager's own settings"
        // (only when no DanceLibrary is assigned).
        private List<DanceInfo> _programme = new List<DanceInfo>();
        private int _danceIndex;
        private RoundReport _round;

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

            GameManager gameManager = GameManager.Instance;
            _style = gameManager != null ? gameManager.ChosenStyle : DanceStyle.None;
            if (_style == DanceStyle.None)
            {
                _style = DanceStyle.Standard; // Pressing Play straight in this scene.
            }

            _programme = BuildProgramme();

            string playerName = gameManager != null && !string.IsNullOrEmpty(gameManager.PlayerName)
                ? gameManager.PlayerName
                : "You";
            _player = new Couple($"{playerName} (you)", true);

            SetUpField();

            List<string> danceNames = ProgrammeNames();
            _result = new CompetitionResult
            {
                StyleName = _style.ToString(),
                DanceNames = danceNames,
                DanceName = $"{_style}: {RoundProgramme.Describe(danceNames)}",
                PlayerCoupleName = _player.Name,
                FieldSize = tournamentManager.Couples.Count
            };

            ShowIntro();
        }

        private void Update()
        {
            // The pulse circle "pops" on every step and shrinks back.
            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.deltaTime * 4f);
            if (beatPulse != null)
            {
                beatPulse.localScale = Vector3.one * (1f + 0.35f * _pulse);
            }
        }

        // ---------- Setup ----------

        /// <summary>All five dances of the chosen style, in competition order.</summary>
        private List<DanceInfo> BuildProgramme()
        {
            List<DanceInfo> styleDances = library != null ? library.GetDances(_style) : new List<DanceInfo>();
            if (styleDances.Count == 0)
            {
                Debug.LogWarning("[Competition] No dances found for the style. Using the DanceManager's own Waltz.");
                return new List<DanceInfo> { null };
            }

            return RoundProgramme.Pick(styleDances, dancesPerRound);
        }

        private List<string> ProgrammeNames()
        {
            var names = new List<string>();
            foreach (DanceInfo dance in _programme)
            {
                names.Add(dance != null ? dance.DisplayName : danceManager.DanceName);
            }

            return names;
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

        // ---------- Intro (before each round) ----------

        private void ShowIntro()
        {
            RoundType round = tournamentManager.CurrentRound;
            ShowOnly(introPanel);

            int couples = tournamentManager.Couples.Count;
            if (floor != null)
            {
                floor.SetUpRound(tournamentManager.Couples, _player, _style, tournamentManager.NumberOfJudges, _random);
            }

            SetText(roundTitleText, $"{TournamentRules.DisplayName(round)}: {_style}");

            string recall = TournamentRules.IsLastRound(round)
                ? "The judges will place all six finalists."
                : $"The best {tournamentManager.CouplesAdvancingFromCurrentRound} are recalled to the " +
                  $"{TournamentRules.DisplayName(TournamentRules.Next(round))}.";

            int capacity = floor != null ? floor.FloorCapacity : couples;
            int heats = FloorLayout.HeatCount(couples, capacity);
            string heatLine = heats > 1
                ? $"{couples} couples, dancing in {heats} heats of up to {capacity}."
                : $"{couples} couples on the floor.";

            string dances = _programme.Count == 1
                ? $"One dance: {RoundProgramme.Describe(ProgrammeNames())}."
                : $"{_programme.Count} dances: {RoundProgramme.Describe(ProgrammeNames())}. Your marks from all of them are added up.";

            SetText(introText,
                $"{heatLine} {recall}\n\n" +
                $"{dances}\n" +
                "Press an <b>ARROW</b> key on every lit beat. Fast dances step on every 2nd or 3rd beat: listen for the loud clicks.");
        }

        /// <summary>The "Dance!" button: the first dance of the round.</summary>
        public void BeginRound()
        {
            _round = new RoundReport
            {
                Round = tournamentManager.CurrentRound,
                CouplesInRound = tournamentManager.Couples.Count
            };
            _danceIndex = 0;
            StartCurrentDance();
        }

        /// <summary>The "Next: Tango" button on the dance break panel.</summary>
        public void NextDance()
        {
            if (_round == null || _danceIndex >= _programme.Count - 1)
            {
                return;
            }

            _danceIndex++;
            StartCurrentDance();
        }

        private void StartCurrentDance()
        {
            DanceInfo dance = _programme[_danceIndex];
            if (dance != null)
            {
                danceManager.Configure(dance);
            }

            ShowOnly(hudPanel);
            SetText(danceTitleText,
                $"{TournamentRules.DisplayName(tournamentManager.CurrentRound)}  -  {danceManager.DanceName}  " +
                $"<size=70%>(dance {_danceIndex + 1} of {_programme.Count})</size>");
            SetText(feedbackText, string.Empty);
            SetText(countText, "Listen...");

            if (floor != null)
            {
                floor.SetDancing(true);
            }

            danceManager.StartDance();
            UpdateScore();
        }

        // ---------- During the dance (DanceManager events) ----------

        private void OnBeat(int songBeat, int beatInBar)
        {
            bool countIn = songBeat < danceManager.CountInBeats;
            bool isStep = danceManager.IsStepBeat(songBeat);

            SetText(countText, countIn
                ? $"Get ready... {danceManager.CountInBeats - songBeat}"
                : CountLine(beatInBar));

            if (countIn || isStep)
            {
                _pulse = beatInBar == 0 ? 1.4f : 0.8f;
            }

            if (floor != null)
            {
                floor.OnBeat(isStep, beatInBar == 0);
            }
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

            if (floor != null)
            {
                floor.OnPlayerStep(result);
            }

            UpdateScore();
        }

        private void OnDanceFinished(PerformanceTracker tracker)
        {
            if (floor != null)
            {
                floor.SetDancing(false);
            }

            // Your dance score is your level for this dance.
            _player.Skill = tracker.Score;

            // Every couple dances at the same time; the judges mark them all for this dance.
            int[] playerMarks = new int[0];
            foreach (Couple couple in tournamentManager.Couples)
            {
                float level = couple.IsPlayer
                    ? couple.Skill
                    : couple.Skill + (float)(RandomUtil.NextGaussian(_random) * aiFormVariation);

                int[] marks = _judges.Score(level, tournamentManager.NumberOfJudges);
                tournamentManager.AddDanceScores(couple, marks);

                if (couple.IsPlayer)
                {
                    playerMarks = marks;
                }
            }

            var danceReport = new DanceReport
            {
                DanceName = danceManager.DanceName,
                Score = tracker.Score,
                Perfect = tracker.PerfectCount,
                Good = tracker.GoodCount,
                Miss = tracker.MissCount,
                MaxCombo = tracker.MaxCombo,
                Marks = playerMarks
            };
            _round.AddDance(danceReport);

            if (floor != null)
            {
                floor.ShowJudgeMarks(playerMarks);
            }

            if (_danceIndex < _programme.Count - 1)
            {
                ShowDanceBreak(danceReport);
            }
            else
            {
                FinishRound();
            }
        }

        // ---------- Between two dances ----------

        private void ShowDanceBreak(DanceReport dance)
        {
            ShowOnly(danceBreakPanel);

            SetText(breakTitleText, $"{dance.DanceName}: {dance.Score:0}/100");

            int place = tournamentManager.GetPlace(_player);
            var body = new StringBuilder();
            body.AppendLine($"{dance.Perfect} perfect, {dance.Good} good, {dance.Miss} miss, best combo {dance.MaxCombo}");
            body.AppendLine($"Judges' marks: {string.Join("  ", dance.Marks)}  =  <b>{dance.TotalMarks}</b>");
            body.AppendLine();
            body.AppendLine($"After {_danceIndex + 1} of {_programme.Count} dances you're " +
                            $"<color={Gold}><b>{TextFormat.Ordinal(place)}</b></color> of {tournamentManager.Couples.Count}.");

            if (!TournamentRules.IsLastRound(tournamentManager.CurrentRound))
            {
                int recalled = tournamentManager.CouplesAdvancingFromCurrentRound;
                body.AppendLine(place <= recalled
                    ? $"That's inside the top {recalled}: keep it up!"
                    : $"The top {recalled} are recalled. Time to push!");
            }

            SetText(breakBodyText, body.ToString());

            DanceInfo next = _programme[_danceIndex + 1];
            string nextName = next != null ? next.DisplayName : "next dance";
            SetText(breakButtonLabel, $"Next: {nextName}");
        }

        // ---------- Round result ----------

        private void FinishRound()
        {
            RoundType round = tournamentManager.CurrentRound;
            List<Couple> ranking = tournamentManager.GetRanking();
            int place = ranking.IndexOf(_player) + 1;
            bool isFinal = TournamentRules.IsLastRound(round);
            bool recalled = !isFinal && place <= tournamentManager.CouplesAdvancingFromCurrentRound;

            _round.CouplesInRound = ranking.Count;
            _round.Place = place;
            _round.Recalled = recalled;

            _result.Rounds.Add(_round);
            _result.LastRoundTop = TopOf(ranking);
            _competitionOver = isFinal || !recalled;

            if (floor != null && (recalled || (isFinal && place <= 3)))
            {
                floor.Cheer();
            }

            ShowRoundResult(_round);
        }

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
            foreach (DanceReport dance in report.Dances)
            {
                body.AppendLine($"{dance.DanceName}: {dance.Score:0}/100, marks {string.Join(" ", dance.Marks)} = {dance.TotalMarks}");
            }

            body.AppendLine($"Total: <b>{report.TotalMarks}</b> marks   (average dance {report.DanceScore:0}/100, best combo {report.MaxCombo})");
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
            if (floor != null)
            {
                floor.SetDancing(false);
            }

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

        /// <summary>
        /// "ONE two three" for Waltz, with the current beat in gold. Beats you don't step on
        /// (fast dances) show as a dot: Quickstep is "ONE · three ·".
        /// </summary>
        private string CountLine(int beatInBar)
        {
            var line = new StringBuilder();
            for (int i = 0; i < danceManager.BeatsPerBar; i++)
            {
                string word = IsStepInBar(i) ? CountWords[Mathf.Min(i, CountWords.Length - 1)] : "·";
                line.Append(i == beatInBar ? $"<color={Gold}><b>{word}</b></color>" : $"<color={Dim}>{word}</color>");
                line.Append("   ");
            }

            return line.ToString().TrimEnd();
        }

        /// <summary>The count-in is whole bars, so "step beats" are the same in every bar.</summary>
        private bool IsStepInBar(int beatInBar) => beatInBar % danceManager.StepInterval == 0;

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

        private void ShowOnly(GameObject panel)
        {
            SetActive(introPanel, panel == introPanel);
            SetActive(hudPanel, panel == hudPanel);
            SetActive(danceBreakPanel, panel == danceBreakPanel);
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
