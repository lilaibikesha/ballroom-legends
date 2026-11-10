using System.Text;
using BallroomLegends.Competition;
using BallroomLegends.Core;
using BallroomLegends.Tournament;
using TMPro;
using UnityEngine;

namespace BallroomLegends.UI
{
    /// <summary>
    /// Results scene: reads the <see cref="CompetitionResult"/> the ballroom saved in the
    /// GameManager and shows it round by round, plus the top of the last round.
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text roundsText;
        [SerializeField] private TMP_Text rankingText;

        private const string Gold = "#F5CC6B";

        private void Start()
        {
            GameManager gameManager = GameManager.Instance;
            CompetitionResult result = gameManager != null ? gameManager.LastCompetitionResult : null;

            if (result == null || result.Rounds.Count == 0)
            {
                SetText(headlineText, "No competition yet");
                SetText(roundsText, "Go to the Training Hall and enter a competition.");
                SetText(rankingText, string.Empty);
                return;
            }

            SetText(headlineText, result.Headline());
            SetText(roundsText, BuildRounds(result));
            SetText(rankingText, BuildRanking(result));
        }

        public void BackToTrainingHall() => ChangeState(GameState.TrainingHall);

        public void BackToMainMenu() => ChangeState(GameState.MainMenu);

        private static string BuildRounds(CompetitionResult result)
        {
            var text = new StringBuilder();
            text.AppendLine($"<b>{result.DanceName}</b>, {result.FieldSize} couples");
            text.AppendLine();

            foreach (RoundReport round in result.Rounds)
            {
                string outcome = round.Round == RoundType.Final ? "final placing"
                    : round.Recalled ? "recalled"
                    : "not recalled";

                text.AppendLine($"<color={Gold}>{TournamentRules.DisplayName(round.Round)}</color>: " +
                                $"{TextFormat.Ordinal(round.Place)} of {round.CouplesInRound} ({outcome})");
                text.AppendLine($"   Dance {round.DanceScore:0}/100, marks {round.TotalMarks}, " +
                                $"{round.Perfect} perfect / {round.Good} good / {round.Miss} miss");
            }

            return text.ToString();
        }

        private static string BuildRanking(CompetitionResult result)
        {
            RoundReport last = result.LastRound;
            var text = new StringBuilder();
            text.AppendLine($"<b>{TournamentRules.DisplayName(last.Round)} ranking</b>");
            text.AppendLine();

            foreach (RankingEntry entry in result.LastRoundTop)
            {
                string line = $"{TextFormat.Ordinal(entry.Place)}   {entry.CoupleName}   ({entry.TotalMarks})";
                text.AppendLine(entry.IsPlayer ? $"<color={Gold}><b>{line}</b></color>" : line);
            }

            if (last.Place > result.LastRoundTop.Count)
            {
                text.AppendLine("...");
                text.AppendLine($"<color={Gold}><b>{TextFormat.Ordinal(last.Place)}   {result.PlayerCoupleName}   ({last.TotalMarks})</b></color>");
            }

            return text.ToString();
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
    }
}
