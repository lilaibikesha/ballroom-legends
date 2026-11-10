using System.Collections.Generic;
using UnityEngine;

namespace BallroomLegends.Dance
{
    /// <summary>
    /// One asset that lists every <see cref="DanceInfo"/> in the game
    /// (Assets/_Project/Data/DanceLibrary.asset). Screens ask the library for dances
    /// instead of each screen keeping its own list, so there's one source of truth.
    /// </summary>
    [CreateAssetMenu(fileName = "DanceLibrary", menuName = "Ballroom Legends/Dance Library")]
    public class DanceLibrary : ScriptableObject
    {
        [Tooltip("All dances, in competition order: Standard first, then Latin.")]
        [SerializeField] private List<DanceInfo> dances = new List<DanceInfo>();

        public IReadOnlyList<DanceInfo> All => dances;

        /// <summary>Returns the dances of one style, in the order they appear in the list.</summary>
        public List<DanceInfo> GetDances(DanceStyle style)
        {
            var result = new List<DanceInfo>();

            // A plain loop keeps the list order, which is the competition order.
            // LINQ version, same result: dances.Where(d => d != null && d.Style == style).ToList()
            foreach (DanceInfo dance in dances)
            {
                // An empty slot in the Inspector list is null: skip it instead of crashing.
                if (dance != null && dance.Style == style)
                {
                    result.Add(dance);
                }
            }

            return result;
        }
    }
}
