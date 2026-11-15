using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// Runs the rhythm mechanic: plays the track, keeps time, and judges each step.
    ///
    /// The routine: a count-in (just listen), then <see cref="StepCount"/> steps.
    /// Press SPACE (or the gamepad's south button) on every step.
    /// Waltz is 3/4 ("ONE-two-three") at 28-30 bars per minute, about 87 beats per minute,
    /// with a step on every beat. Fast dances step on every 2nd or 3rd beat instead
    /// (see <see cref="RhythmMath.StepInterval"/>): Quickstep is 204 beats a minute!
    ///
    /// Other scripts don't poll this class; they subscribe to its events
    /// (<see cref="BeatTicked"/>, <see cref="StepJudged"/>, <see cref="DanceFinished"/>).
    /// That's the Observer pattern: DanceManager doesn't know who is listening, so the UI
    /// can change without touching the rhythm code.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class DanceManager : MonoBehaviour
    {
        [Header("Dance (optional)")]
        [Tooltip("If set, tempo and time signature come from this dance's data. Otherwise the Music values below are used.")]
        [SerializeField] private DanceInfo dance;

        [Header("Music")]
        [SerializeField] private AudioSource music;
        [Tooltip("When no clip is assigned, generate a click track (accent on beat 1).")]
        [SerializeField] private bool useGeneratedClickTrack = true;
        [SerializeField, Min(1f)] private float beatsPerMinute = 87f;
        [SerializeField, Min(1)] private int beatsPerBar = 3;

        [Header("Routine")]
        [SerializeField, Min(0)] private int countInBars = 1;
        [Tooltip("Roughly how long each dance lasts. Fast dances get more bars, slow ones fewer.")]
        [SerializeField, Min(4f)] private float routineSeconds = 24f;
        [Tooltip("Faster than this, you step on every 2nd (or 3rd) beat instead of every beat.")]
        [SerializeField, Min(30f)] private float maxStepsPerMinute = 140f;
        [Tooltip("Seconds between pressing Dance! and the first count-in click.")]
        [SerializeField, Min(0.1f)] private float leadInSeconds = 0.5f;

        [Header("Timing windows (seconds either side of a beat)")]
        [SerializeField, Min(0f)] private float perfectWindow = 0.05f;
        [SerializeField, Min(0f)] private float goodWindow = 0.12f;
        [Tooltip("Calibration. If your presses feel on time but are judged late (e.g. Bluetooth headphones), raise this.")]
        [SerializeField] private float inputOffsetSeconds;

        /// <summary>Every click of the track: (beat index since the start, beat in bar where 0 = "ONE").</summary>
        public event Action<int, int> BeatTicked;

        /// <summary>
        /// A step was judged: (result, seconds from the beat). Negative = early, positive = late.
        /// For a step you never pressed the offset is <see cref="float.PositiveInfinity"/>.
        /// </summary>
        public event Action<HitResult, float> StepJudged;

        /// <summary>The routine is over. The tracker holds the final counts and score.</summary>
        public event Action<PerformanceTracker> DanceFinished;

        private AudioClip _generatedClip;
        private double _songStartDsp;
        private bool[] _judged;
        private int _nextBeatToAnnounce;
        private int _nextStepToCheck;

        public double SecondsPerBeat => RhythmMath.SecondsPerBeat(beatsPerMinute);
        public float BeatsPerMinute => beatsPerMinute;
        public int BeatsPerBar => beatsPerBar;
        /// <summary>
        /// Listening beats before the first step: whole bars, at least 3 beats and 2 seconds,
        /// so a 2/4 dance like Samba gets two bars instead of a too-short single bar.
        /// </summary>
        public int CountInBeats => RhythmMath.CountInBeats(countInBars, beatsPerBar, beatsPerMinute);
        /// <summary>Beats between two steps: 1 = every beat, 2 = every other beat, ...</summary>
        public int StepInterval => RhythmMath.StepInterval(beatsPerMinute, beatsPerBar, maxStepsPerMinute);
        public int RoutineBars => RhythmMath.RoutineBars(routineSeconds, beatsPerMinute / beatsPerBar);
        /// <summary>Count-in plus routine: every click of the track.</summary>
        public int TotalBeats => CountInBeats + RoutineBars * beatsPerBar;
        public int StepCount => RoutineBars * beatsPerBar / StepInterval;
        public string DanceName => dance != null ? dance.DisplayName : "Waltz";

        /// <summary>True for the beats you should step on (never during the count-in).</summary>
        public bool IsStepBeat(int songBeat) =>
            songBeat < TotalBeats && RhythmMath.IsStepBeat(songBeat, CountInBeats, StepInterval);
        public bool IsDancing { get; private set; }
        public PerformanceTracker Tracker { get; private set; }

        /// <summary>Seconds since the first click. Negative during the lead-in.</summary>
        public double SongTime => AudioSettings.dspTime - _songStartDsp;

        private void Awake()
        {
            if (music == null)
            {
                music = GetComponent<AudioSource>();
            }

            // RequireComponent only applies when the component is added, so a DanceManager
            // from an older scene may not have an AudioSource yet.
            if (music == null)
            {
                music = gameObject.AddComponent<AudioSource>();
            }

            music.playOnAwake = false;
            music.loop = false;
            ApplyDanceData();
        }

        private void OnDestroy()
        {
            if (_generatedClip != null)
            {
                Destroy(_generatedClip);
            }
        }

        /// <summary>Switches to another dance (tempo + time signature). Not allowed mid-dance.</summary>
        public void Configure(DanceInfo newDance)
        {
            if (IsDancing)
            {
                Debug.LogWarning("[DanceManager] Can't change the dance while dancing.");
                return;
            }

            dance = newDance;
            ApplyDanceData();
        }

        private void ApplyDanceData()
        {
            if (dance == null)
            {
                return;
            }

            beatsPerBar = RhythmMath.BeatsPerBar(dance.TimeSignature);

            // Use the middle of the official tempo range.
            float barsPerMinute = (dance.TempoMinBarsPerMinute + dance.TempoMaxBarsPerMinute) / 2f;
            if (barsPerMinute > 0f)
            {
                beatsPerMinute = (float)RhythmMath.BeatsPerMinute(barsPerMinute, beatsPerBar);
            }
        }

        public void StartDance()
        {
            if (IsDancing)
            {
                return;
            }

            Tracker = new PerformanceTracker(StepCount);
            _judged = new bool[StepCount];
            _nextBeatToAnnounce = 0;
            _nextStepToCheck = 0;

            // Build the click track unless you've assigned a real music clip.
            bool needsClickTrack = music.clip == null || music.clip == _generatedClip;
            if (useGeneratedClickTrack && needsClickTrack)
            {
                if (_generatedClip != null)
                {
                    Destroy(_generatedClip);
                }

                _generatedClip = MetronomeClip.Create(DanceName, beatsPerMinute, beatsPerBar,
                    CountInBeats, TotalBeats, AudioSettings.outputSampleRate, StepInterval);
                music.clip = _generatedClip;
            }

            // Why AudioSettings.dspTime and not Time.time? dspTime is the audio hardware's
            // clock, the same clock the music plays on. Time.time is the frame clock: it
            // drifts from the audio and jumps when a frame is slow. PlayScheduled starts the
            // music at an exact dspTime, so "beat 0" and the first click line up perfectly.
            _songStartDsp = AudioSettings.dspTime + leadInSeconds;
            if (music.clip != null)
            {
                music.PlayScheduled(_songStartDsp);
            }

            IsDancing = true;
            Debug.Log($"[DanceManager] {DanceName}: {beatsPerMinute:0} BPM, {beatsPerBar}/4, " +
                      $"{StepCount} steps (one every {StepInterval} beat(s)).");
        }

        /// <summary>Stops immediately without raising <see cref="DanceFinished"/> (e.g. Withdraw).</summary>
        public void StopDance()
        {
            IsDancing = false;
            if (music != null)
            {
                music.Stop();
            }
        }

        private void Update()
        {
            if (!IsDancing)
            {
                return;
            }

            double songTime = SongTime;
            AnnounceBeats(songTime);

            // The player's press is shifted by the calibration offset before judging.
            double stepTime = songTime - inputOffsetSeconds;
            if (StepPressedThisFrame())
            {
                JudgePress(stepTime);
            }

            MarkMissedSteps(stepTime);

            if (_nextStepToCheck >= StepCount)
            {
                IsDancing = false;
                DanceFinished?.Invoke(Tracker);
            }
        }

        private static bool StepPressedThisFrame()
        {
            // Unity 6 uses the Input System package. Keyboard.current is null when no
            // keyboard is connected, hence the null checks.
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        }

        /// <summary>Raises <see cref="BeatTicked"/> once for every click that has played.</summary>
        private void AnnounceBeats(double songTime)
        {
            int lastBeat = TotalBeats - 1;

            // "while", not "if": after a slow frame, several beats may have passed.
            while (_nextBeatToAnnounce <= lastBeat &&
                   songTime >= RhythmMath.BeatTime(_nextBeatToAnnounce, SecondsPerBeat))
            {
                BeatTicked?.Invoke(_nextBeatToAnnounce, RhythmMath.BeatInBar(_nextBeatToAnnounce, beatsPerBar));
                _nextBeatToAnnounce++;
            }
        }

        private void JudgePress(double stepTime)
        {
            int step = RhythmMath.NearestStep(stepTime, SecondsPerBeat, CountInBeats, StepInterval);

            // Ignore presses during the count-in, after the routine, or a second press on
            // a step that's already judged (so mashing the key can't earn points).
            if (step < 0 || step >= StepCount || _judged[step])
            {
                return;
            }

            int beat = RhythmMath.StepBeat(step, CountInBeats, StepInterval);
            float offset = (float)RhythmMath.OffsetFromBeat(stepTime, beat, SecondsPerBeat);
            Record(step, EvaluateStep(offset), offset);
        }

        /// <summary>A step whose Good window has closed without a press is a Miss.</summary>
        private void MarkMissedSteps(double stepTime)
        {
            while (_nextStepToCheck < StepCount)
            {
                if (_judged[_nextStepToCheck])
                {
                    _nextStepToCheck++;
                    continue;
                }

                int beat = RhythmMath.StepBeat(_nextStepToCheck, CountInBeats, StepInterval);
                double beatTime = RhythmMath.BeatTime(beat, SecondsPerBeat);
                if (stepTime <= beatTime + goodWindow)
                {
                    break; // Still time to press this one.
                }

                Record(_nextStepToCheck, HitResult.Miss, float.PositiveInfinity);
                _nextStepToCheck++;
            }
        }

        private void Record(int step, HitResult result, float offset)
        {
            _judged[step] = true;
            Tracker.Record(result);
            StepJudged?.Invoke(result, offset);
        }

        public HitResult EvaluateStep(float secondsFromBeat) =>
            EvaluateStep(secondsFromBeat, perfectWindow, goodWindow);

        /// <summary>
        /// Pure function: rates a step from its distance to the nearest beat.
        /// Negative = early, positive = late; both count the same.
        /// The windows are inclusive: exactly 0.05 s off is still Perfect (see the tests).
        /// </summary>
        public static HitResult EvaluateStep(float secondsFromBeat, float perfectWindow, float goodWindow)
        {
            float distance = Math.Abs(secondsFromBeat);

            if (distance <= perfectWindow)
            {
                return HitResult.Perfect;
            }

            return distance <= goodWindow ? HitResult.Good : HitResult.Miss;
        }
    }
}
