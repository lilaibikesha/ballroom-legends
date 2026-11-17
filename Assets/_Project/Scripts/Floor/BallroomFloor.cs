using System.Collections.Generic;
using BallroomLegends.Dance;
using BallroomLegends.Tournament;
using TMPro;
using UnityEngine;

namespace BallroomLegends.Floor
{
    /// <summary>
    /// Everything you see on and around the ballroom floor: the couples of your heat, the
    /// five judges along the front, and the audience at the sides.
    ///
    /// It only shows things. CompetitionController tells it what happened ("a beat",
    /// "the player hit a step", "here are the judges' marks") and it animates that.
    /// The floor builds its pieces in code at runtime (see <see cref="FloorShapes"/>), so the
    /// scene only needs this one component on a full-screen UI object.
    ///
    ///  - Standard: couples travel anticlockwise around the floor (the line of dance).
    ///    Your couple only moves forward when you dance the step.
    ///  - Latin: couples dance on the spot, opening out and closing in on the beat.
    /// </summary>
    public class BallroomFloor : MonoBehaviour
    {
        [Header("Floor (canvas units, measured from the screen center)")]
        [SerializeField] private Vector2 laneCenter = new Vector2(0f, -60f);
        [SerializeField] private Vector2 laneRadius = new Vector2(640f, 250f);
        [Tooltip("How many couples fit on the floor at once. Bigger rounds dance in heats.")]
        [SerializeField, Min(1)] private int floorCapacity = 12;
        [Tooltip("Standard: steps for one lap of the floor.")]
        [SerializeField, Min(4)] private int stepsPerLap = 32;
        [Tooltip("Chance per step that an AI couple wobbles, for a couple of skill 0. Better couples wobble less.")]
        [SerializeField, Range(0f, 1f)] private float aiWobbleChance = 0.25f;

        [Header("Judges")]
        [SerializeField] private float judgesY = -470f;
        [SerializeField] private float judgesSpacing = 300f;

        [Header("Audience (the side galleries)")]
        [SerializeField] private float audienceX = 880f;
        [SerializeField, Min(0)] private int audienceRows = 10;

        private static readonly string[] JudgeLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        private static readonly Color Gold = new Color(0.96f, 0.80f, 0.42f);

        private readonly List<CoupleView> _couples = new List<CoupleView>();
        private readonly List<GameObject> _judgePaddles = new List<GameObject>();
        private readonly List<TMP_Text> _judgeMarkTexts = new List<TMP_Text>();
        private readonly List<RectTransform> _audience = new List<RectTransform>();
        private readonly List<Vector2> _audienceHome = new List<Vector2>();

        private RectTransform _couplesLayer;
        private CoupleView _player;
        private DanceStyle _style = DanceStyle.Standard;
        private System.Random _random = new System.Random();
        private bool _dancing;
        private float _crowdBounce;
        private float _cheerSeconds;

        public int FloorCapacity => floorCapacity;
        public int CouplesOnFloor => _couples.Count;
        private bool IsLatin => _style == DanceStyle.Latin;

        private void Awake()
        {
            // Order of creation = drawing order: audience and judges first, couples on top.
            BuildAudience();
            BuildJudges(5);
            _couplesLayer = FloorShapes.CreateRect(transform, "Couples", Vector2.zero, Vector2.zero);
        }

        // ---------- Called by CompetitionController ----------

        /// <summary>
        /// Puts your heat on the floor: you plus random couples of this round, up to the
        /// floor's capacity. Call it at the start of every round.
        /// </summary>
        public void SetUpRound(IReadOnlyList<Couple> couplesInRound, Couple player, DanceStyle style,
            int numberOfJudges, System.Random random)
        {
            _random = random ?? new System.Random();
            _style = style == DanceStyle.None ? DanceStyle.Standard : style;
            ClearCouples();

            if (_judgePaddles.Count != numberOfJudges)
            {
                BuildJudges(numberOfJudges);
            }

            HideJudgeMarks();

            List<Couple> onFloor = FloorLayout.PickForFloor(couplesInRound, player, floorCapacity, _random);
            for (int i = 0; i < onFloor.Count; i++)
            {
                Couple couple = onFloor[i];
                CoupleLook look = couple.IsPlayer ? CoupleLook.Player(IsLatin) : CoupleLook.Random(_random, IsLatin);
                string label = couple.IsPlayer ? "You" : null;

                CoupleView view = CoupleView.Create(_couplesLayer, couple, look, IsLatin, label);
                view.HomeAngle = FloorLayout.StartAngle(i, onFloor.Count);

                // Not everyone dances on the same lane: some take a smaller circle.
                view.LaneScale = couple.IsPlayer ? 1f : 0.78f + 0.22f * (float)_random.NextDouble();
                view.HomeFacing = IsLatin ? (float)(_random.NextDouble() * 60.0 - 30.0) : 0f;
                view.StepsTravelled = 0;
                PlaceAtStep(view);
                view.Snap();

                _couples.Add(view);
                if (couple.IsPlayer)
                {
                    _player = view;
                }
            }

            // The player is drawn last, so you're never hidden under another couple.
            if (_player != null)
            {
                _player.transform.SetAsLastSibling();
            }
        }

        /// <summary>The music starts (true) or stops (false). AI couples only move while dancing.</summary>
        public void SetDancing(bool dancing)
        {
            _dancing = dancing;
            if (dancing)
            {
                HideJudgeMarks();
            }
        }

        /// <summary>
        /// Every click of the track. On a step beat the AI couples take their step (the
        /// player's couple waits for your step press, see <see cref="OnPlayerStep"/>).
        /// </summary>
        public void OnBeat(bool isStepBeat, bool isDownbeat)
        {
            // The audience nods along to "ONE" even during the count-in.
            if (isDownbeat)
            {
                _crowdBounce = 1f;
            }

            if (!_dancing || !isStepBeat)
            {
                return;
            }

            foreach (CoupleView view in _couples)
            {
                if (view == _player)
                {
                    continue;
                }

                // Weaker couples wobble more often: chance shrinks as skill grows.
                float wobbleChance = aiWobbleChance * (1f - Mathf.Clamp01(view.Couple.Skill / 100f));
                if (_random.NextDouble() < wobbleChance)
                {
                    view.Wobble();
                }
                else
                {
                    TakeStep(view, isDownbeat ? 1f : 0.6f);
                }
            }
        }

        /// <summary>Your step press was judged: step forward, or wobble on a miss.</summary>
        public void OnPlayerStep(HitResult result)
        {
            if (_player == null)
            {
                return;
            }

            switch (result)
            {
                case HitResult.Perfect:
                    TakeStep(_player, 1f);
                    _player.Glow(Gold);
                    break;
                case HitResult.Good:
                    TakeStep(_player, 0.7f);
                    break;
                default:
                    _player.Wobble();
                    break;
            }
        }

        /// <summary>The judges hold up their marks for your couple (one paddle each).</summary>
        public void ShowJudgeMarks(IReadOnlyList<int> marks)
        {
            for (int i = 0; i < _judgePaddles.Count; i++)
            {
                bool hasMark = marks != null && i < marks.Count;
                _judgePaddles[i].SetActive(hasMark);
                if (hasMark)
                {
                    _judgeMarkTexts[i].text = marks[i].ToString();
                }
            }
        }

        public void HideJudgeMarks()
        {
            foreach (GameObject paddle in _judgePaddles)
            {
                paddle.SetActive(false);
            }
        }

        /// <summary>The audience cheers (jumps up and down) for a few seconds.</summary>
        public void Cheer(float seconds = 2.5f) => _cheerSeconds = seconds;

        // ---------- Movement ----------

        private void TakeStep(CoupleView view, float bounce)
        {
            view.StepsTravelled++;
            PlaceAtStep(view);
            view.Bounce(bounce);
        }

        /// <summary>Sets where a couple should be after its number of steps.</summary>
        private void PlaceAtStep(CoupleView view)
        {
            Vector2 radius = laneRadius * view.LaneScale;
            bool evenStep = view.StepsTravelled % 2 == 0;

            if (IsLatin)
            {
                // Spot dance: stay home, open out and close in, swaying with the rhythm.
                view.TargetPosition = FloorLayout.PointOnEllipse(laneCenter, radius, view.HomeAngle);
                view.TargetSeparation = evenStep ? 26f : 46f;
                view.TargetFacing = view.HomeFacing + (evenStep ? -18f : 18f);
                return;
            }

            // Standard: travel along the line of dance, with a gentle sway from side to side.
            float angle = view.HomeAngle + view.StepsTravelled * FloorLayout.AnglePerStep(stepsPerLap);
            view.TargetPosition = FloorLayout.PointOnEllipse(laneCenter, radius, angle);
            view.TargetFacing = FloorLayout.TravelFacing(radius, angle) + (evenStep ? 8f : -8f);
        }

        private void ClearCouples()
        {
            foreach (CoupleView view in _couples)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _couples.Clear();
            _player = null;
        }

        // ---------- Judges and audience ----------

        private void BuildJudges(int count)
        {
            Transform old = transform.Find("Judges");
            if (old != null)
            {
                // Destroy happens at the end of the frame, so rename it now to avoid
                // finding the old one again.
                old.name = "Judges (old)";
                Destroy(old.gameObject);
            }

            _judgePaddles.Clear();
            _judgeMarkTexts.Clear();

            RectTransform judges = FloorShapes.CreateRect(transform, "Judges", Vector2.zero, Vector2.zero);
            float firstX = -(count - 1) * judgesSpacing / 2f;

            for (int i = 0; i < count; i++)
            {
                string letter = JudgeLetters[Mathf.Min(i, JudgeLetters.Length - 1)];
                var position = new Vector2(firstX + i * judgesSpacing, judgesY);
                RectTransform judge = FloorShapes.CreateRect(judges, $"Judge {letter}", position, Vector2.zero);

                FloorShapes.CreateCircle(judge, "Body", Vector2.zero, new Vector2(50f, 34f), new Color(0.16f, 0.16f, 0.22f));
                FloorShapes.CreateCircle(judge, "Head", new Vector2(0f, 2f), new Vector2(22f, 22f), new Color(0.75f, 0.75f, 0.78f));
                FloorShapes.CreateText(judge, "Letter", $"Judge {letter}", 20f, new Vector2(0f, -38f), new Vector2(160f, 30f),
                    new Color(1f, 1f, 1f, 0.75f));

                // The paddle: a white card held up with the mark on it.
                RectTransform paddle = FloorShapes.CreateCircle(judge, "Paddle", new Vector2(0f, 58f), new Vector2(58f, 58f), Color.white).rectTransform;
                TMP_Text mark = FloorShapes.CreateText(paddle, "Mark", "", 32f, Vector2.zero, new Vector2(58f, 58f),
                    new Color(0.12f, 0.08f, 0.16f), bold: true);
                paddle.gameObject.SetActive(false);

                _judgePaddles.Add(paddle.gameObject);
                _judgeMarkTexts.Add(mark);
            }

            // Judges sit in front of the audience but behind the couples.
            judges.SetSiblingIndex(Mathf.Min(1, transform.childCount - 1));
        }

        private void BuildAudience()
        {
            RectTransform audience = FloorShapes.CreateRect(transform, "Audience", Vector2.zero, Vector2.zero);
            audience.SetAsFirstSibling();
            var random = new System.Random(7); // The same crowd every time.

            for (int side = -1; side <= 1; side += 2)
            {
                for (int column = 0; column < 2; column++)
                {
                    for (int row = 0; row < audienceRows; row++)
                    {
                        float x = side * (audienceX + column * 52f);
                        float y = 260f - row * 70f + (column == 1 ? 35f : 0f);
                        var home = new Vector2(x, y);

                        RectTransform person = FloorShapes.CreateRect(audience, "Spectator", home, Vector2.zero);
                        var clothes = Color.HSVToRGB((float)random.NextDouble(), 0.45f, 0.75f);
                        FloorShapes.CreateCircle(person, "Body", Vector2.zero, new Vector2(36f, 30f), clothes);
                        FloorShapes.CreateCircle(person, "Head", new Vector2(0f, 4f), new Vector2(17f, 17f),
                            new Color(0.25f + 0.5f * (float)random.NextDouble(), 0.18f, 0.10f));

                        _audience.Add(person);
                        _audienceHome.Add(home);
                    }
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _crowdBounce = Mathf.MoveTowards(_crowdBounce, 0f, dt * 3f);
            _cheerSeconds = Mathf.Max(0f, _cheerSeconds - dt);

            for (int i = 0; i < _audience.Count; i++)
            {
                float lift = 5f * _crowdBounce * (i % 3 == 0 ? 1f : 0.5f);
                if (_cheerSeconds > 0f)
                {
                    // Cheering: everyone jumps, each at their own moment (i shifts the phase).
                    lift += 14f * Mathf.Abs(Mathf.Sin(Time.time * 9f + i * 0.7f));
                }

                _audience[i].anchoredPosition = _audienceHome[i] + new Vector2(0f, lift);
            }
        }
    }
}
