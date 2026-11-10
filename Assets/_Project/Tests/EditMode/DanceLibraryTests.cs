using System.Collections.Generic;
using BallroomLegends.Dance;
using NUnit.Framework;
using UnityEditor;

namespace BallroomLegends.Tests
{
    /// <summary>Checks the real DanceLibrary asset: right dances, right order, no blanks.</summary>
    public class DanceLibraryTests
    {
        private const string LibraryPath = "Assets/_Project/Data/DanceLibrary.asset";

        private DanceLibrary _library;

        [SetUp]
        public void LoadLibrary()
        {
            _library = AssetDatabase.LoadAssetAtPath<DanceLibrary>(LibraryPath);
            Assert.IsNotNull(_library, $"Missing {LibraryPath}");
        }

        [Test]
        public void Standard_HasFiveDances_InCompetitionOrder()
        {
            AssertNames(_library.GetDances(DanceStyle.Standard),
                "Waltz", "Tango", "Viennese Waltz", "Slow Foxtrot", "Quickstep");
        }

        [Test]
        public void Latin_HasFiveDances_InCompetitionOrder()
        {
            AssertNames(_library.GetDances(DanceStyle.Latin),
                "Samba", "Cha Cha Cha", "Rumba", "Paso Doble", "Jive");
        }

        [Test]
        public void None_ReturnsNoDances()
        {
            Assert.AreEqual(0, _library.GetDances(DanceStyle.None).Count);
        }

        [Test]
        public void EveryDance_HasContent()
        {
            foreach (DanceInfo dance in _library.All)
            {
                Assert.IsNotNull(dance, "Empty slot in the DanceLibrary list");
                Assert.IsNotEmpty(dance.History, $"{dance.DisplayName} has no history");
                Assert.IsNotEmpty(dance.Origin, $"{dance.DisplayName} has no origin");
                Assert.Greater(dance.Characteristics.Count, 0, $"{dance.DisplayName} has no characteristics");
                Assert.Greater(dance.Skills.Count, 0, $"{dance.DisplayName} has no skills");
                Assert.LessOrEqual(dance.TempoMinBarsPerMinute, dance.TempoMaxBarsPerMinute, dance.DisplayName);
            }
        }

        private static void AssertNames(List<DanceInfo> dances, params string[] expected)
        {
            Assert.AreEqual(expected.Length, dances.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], dances[i].DisplayName);
            }
        }
    }
}
