using BallroomLegends.Tournament;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallroomLegends.Floor
{
    /// <summary>
    /// One couple on the floor, seen from above: her skirt (a big circle), his shoulders
    /// (an oval) and two heads. <see cref="BallroomFloor"/> decides where the couple should
    /// be (the Target values); this class glides there smoothly and adds the life: a bounce
    /// on every step, a wobble on a missed step, and a glow on a perfect one.
    /// </summary>
    public class CoupleView : MonoBehaviour
    {
        private const float StandardSeparation = 22f;

        /// <summary>Who this is. The floor uses it to find the player's couple.</summary>
        public Couple Couple { get; private set; }
        public bool IsPlayer => Couple != null && Couple.IsPlayer;

        // Where BallroomFloor wants the couple. Update() moves towards these every frame.
        public Vector2 TargetPosition { get; set; }
        /// <summary>Degrees. 0 = he faces up the screen, she stands in front of him.</summary>
        public float TargetFacing { get; set; }
        /// <summary>Distance between the partners: small in a closed hold, bigger in Latin.</summary>
        public float TargetSeparation { get; set; } = StandardSeparation;

        // Floor bookkeeping (set by BallroomFloor).
        public float HomeAngle { get; set; }
        public float LaneScale { get; set; } = 1f;
        public float HomeFacing { get; set; }
        public int StepsTravelled { get; set; }

        private RectTransform _rect;
        private RectTransform _body;
        private RectTransform _leader;
        private RectTransform _follower;
        private RectTransform _glow;
        private Image _glowImage;
        private Color _glowColor = Color.white;

        private float _facing;
        private float _separation = StandardSeparation;
        private float _bounce;
        private float _wobble;
        private float _glowAmount;

        /// <summary>Builds the couple's shapes under <paramref name="parent"/>.</summary>
        public static CoupleView Create(Transform parent, Couple couple, CoupleLook look, bool latin, string label)
        {
            RectTransform root = FloorShapes.CreateRect(parent, $"Couple: {couple.Name}", Vector2.zero, Vector2.zero);
            var view = root.gameObject.AddComponent<CoupleView>();
            view.Couple = couple;
            view._rect = root;

            FloorShapes.CreateCircle(root, "Shadow", new Vector2(5f, -8f), new Vector2(92f, 74f), new Color(0f, 0f, 0f, 0.28f));

            Image glow = FloorShapes.CreateCircle(root, "Glow", Vector2.zero, new Vector2(120f, 120f), Color.clear);
            view._glow = glow.rectTransform;
            view._glowImage = glow;

            if (couple.IsPlayer)
            {
                FloorShapes.CreateCircle(root, "PlayerRing", Vector2.zero, new Vector2(118f, 118f), new Color(0.96f, 0.80f, 0.42f, 0.9f), ring: true);
            }

            view._body = FloorShapes.CreateRect(root, "Body", Vector2.zero, Vector2.zero);

            // Her: drawn first, so his shoulders overlap her skirt in the hold.
            view._follower = FloorShapes.CreateRect(view._body, "Follower", Vector2.zero, Vector2.zero);
            float skirt = latin ? 46f : 64f;
            FloorShapes.CreateCircle(view._follower, "Skirt", Vector2.zero, new Vector2(skirt, skirt), look.Dress);
            FloorShapes.CreateCircle(view._follower, "Head", new Vector2(0f, 4f), new Vector2(20f, 20f), look.FollowerHair);

            // Him: shoulders are a wide oval, head on top.
            view._leader = FloorShapes.CreateRect(view._body, "Leader", Vector2.zero, Vector2.zero);
            FloorShapes.CreateCircle(view._leader, "Shoulders", Vector2.zero, new Vector2(52f, 28f), look.Suit);
            FloorShapes.CreateCircle(view._leader, "Head", Vector2.zero, new Vector2(21f, 21f), look.LeaderHair);

            if (!string.IsNullOrEmpty(label))
            {
                TMP_Text text = FloorShapes.CreateText(root, "Label", label, 26f, new Vector2(0f, 74f), new Vector2(260f, 36f),
                    new Color(0.96f, 0.80f, 0.42f), bold: true);
                text.outlineWidth = 0.2f;
                text.outlineColor = new Color32(0, 0, 0, 255);
            }

            view.TargetSeparation = latin ? 34f : StandardSeparation;
            return view;
        }

        /// <summary>Jump straight to the targets (when the couple first walks on).</summary>
        public void Snap()
        {
            if (_rect == null)
            {
                return;
            }

            _rect.anchoredPosition = TargetPosition;
            _facing = TargetFacing;
            _separation = TargetSeparation;
            ApplyPose();
        }

        /// <summary>A step: a little bounce. Bigger on "ONE".</summary>
        public void Bounce(float strength = 1f) => _bounce = Mathf.Max(_bounce, strength);

        /// <summary>A missed step: the couple wobbles and flashes red.</summary>
        public void Wobble()
        {
            _wobble = 1f;
            Glow(new Color(1f, 0.35f, 0.35f));
        }

        /// <summary>A short colored glow under the couple (gold for a perfect step).</summary>
        public void Glow(Color color)
        {
            _glowColor = color;
            _glowAmount = 1f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _bounce = Mathf.MoveTowards(_bounce, 0f, dt * 4f);
            _wobble = Mathf.MoveTowards(_wobble, 0f, dt * 2.5f);
            _glowAmount = Mathf.MoveTowards(_glowAmount, 0f, dt * 2f);

            // "Exponential smoothing": cover the same fraction of the remaining distance every
            // second, whatever the frame rate. 1 - e^(-k*dt) is that fraction for this frame.
            _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, TargetPosition, 1f - Mathf.Exp(-8f * dt));
            _facing = Mathf.LerpAngle(_facing, TargetFacing, 1f - Mathf.Exp(-6f * dt));
            _separation = Mathf.Lerp(_separation, TargetSeparation, 1f - Mathf.Exp(-12f * dt));

            ApplyPose();
        }

        private void ApplyPose()
        {
            if (_body == null)
            {
                return;
            }

            _body.localRotation = Quaternion.Euler(0f, 0f, _facing);
            _body.localScale = Vector3.one * (1f + 0.15f * _bounce);
            _body.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 45f) * 8f * _wobble, 0f);

            // She stands in front of him (he faces "up" in the body's own space).
            _leader.anchoredPosition = new Vector2(0f, -_separation / 2f);
            _follower.anchoredPosition = new Vector2(0f, _separation / 2f);

            _glowImage.color = new Color(_glowColor.r, _glowColor.g, _glowColor.b, 0.55f * _glowAmount);
            _glow.localScale = Vector3.one * (0.8f + 0.6f * (1f - _glowAmount));
        }
    }
}
