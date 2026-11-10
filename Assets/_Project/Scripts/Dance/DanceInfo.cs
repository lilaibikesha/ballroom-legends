using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// Everything the Dance Guide shows about one dance (Waltz, Jive, ...).
    ///
    /// Design note: this is a <see cref="ScriptableObject"/>, a data container saved as an
    /// .asset file. Data lives in assets you edit in the Inspector, and code only reads it.
    /// That's called "data-driven design": adding an 11th dance means creating an asset,
    /// not changing code. Create new ones with Assets > Create > Ballroom Legends > Dance Info.
    /// The 10 dances live in Assets/_Project/Data/Dances.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDance", menuName = "Ballroom Legends/Dance Info")]
    public class DanceInfo : ScriptableObject
    {
        [Header("Basics")]
        [SerializeField] private string displayName;
        [SerializeField] private DanceStyle style;
        [SerializeField] private string origin;
        [SerializeField, TextArea(4, 10)] private string history;

        [Header("Music")]
        [SerializeField] private string timeSignature = "4/4";
        [Tooltip("Official competition tempo, in bars (measures) per minute.")]
        [SerializeField, Min(0)] private int tempoMinBarsPerMinute;
        [SerializeField, Min(0)] private int tempoMaxBarsPerMinute;

        [Header("Characteristics")]
        [SerializeField, Range(1, 5)] private int difficulty = 1;
        [Tooltip("What the dance feels like, e.g. 'Very fast', 'Romantic'.")]
        [SerializeField] private List<string> characteristics = new List<string>();
        [Tooltip("What a dancer needs to be good at, e.g. 'Stamina'.")]
        [SerializeField] private List<string> skills = new List<string>();

        [Header("Media")]
        [Tooltip("Leave empty for now. Until a clip is assigned the video button says 'Coming soon'.")]
        [SerializeField] private VideoClip video;

        public string DisplayName => displayName;
        public DanceStyle Style => style;
        public string Origin => origin;
        public string History => history;
        public string TimeSignature => timeSignature;
        public int TempoMinBarsPerMinute => tempoMinBarsPerMinute;
        public int TempoMaxBarsPerMinute => tempoMaxBarsPerMinute;
        public int Difficulty => difficulty;

        // IReadOnlyList: other classes can read the list but can't add or remove items.
        // That protects the data (encapsulation).
        public IReadOnlyList<string> Characteristics => characteristics;
        public IReadOnlyList<string> Skills => skills;

        public VideoClip Video => video;
        public bool HasVideo => video != null;
    }
}
