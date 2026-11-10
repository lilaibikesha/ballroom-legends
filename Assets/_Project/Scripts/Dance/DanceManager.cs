using UnityEngine;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// Runs the Waltz rhythm mechanic: keeps time with the music and judges each step.
    /// Waltz is in 3/4 time ("ONE-two-three"). A Slow Waltz is about 28-30 bars per
    /// minute, so 3 beats per bar gives roughly 84-90 beats per minute.
    /// </summary>
    public class DanceManager : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioSource music;
        [SerializeField, Min(1f)] private float beatsPerMinute = 87f;
        [SerializeField, Min(1)] private int beatsPerBar = 3;

        [Header("Timing windows (seconds either side of a beat)")]
        [SerializeField, Min(0f)] private float perfectWindow = 0.05f;
        [SerializeField, Min(0f)] private float goodWindow = 0.12f;

        public float SecondsPerBeat => 60f / beatsPerMinute;
        public int BeatsPerBar => beatsPerBar;
        public bool IsDancing { get; private set; }

        public void StartDance()
        {
            // TODO (you): remember when the dance started and start the music.
            // Research question: why do rhythm games use AudioSettings.dspTime
            // instead of Time.time?
            IsDancing = true;
        }

        private void Update()
        {
            if (!IsDancing)
            {
                return;
            }

            // TODO (you):
            //  1. Compute the song position in seconds, then in beats.
            //  2. Read the player's step. Unity 6 uses the Input System package:
            //       UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame
            //     (add "Unity.InputSystem" to BallroomLegends.asmdef references first).
            //  3. Find how far the press was from the nearest beat, call EvaluateStep,
            //     and react to the result (score, feedback, animation).
        }

        public HitResult EvaluateStep(float secondsFromBeat) =>
            EvaluateStep(secondsFromBeat, perfectWindow, goodWindow);

        /// <summary>
        /// Pure function: rates a step from its distance to the nearest beat.
        /// Negative = early, positive = late. It's static and touches no Unity APIs,
        /// so it can be unit tested without a scene (see Tests/EditMode).
        /// </summary>
        public static HitResult EvaluateStep(float secondsFromBeat, float perfectWindow, float goodWindow)
        {
            // TODO (you): compare the absolute offset against the two windows.
            return HitResult.Miss;
        }
    }
}
