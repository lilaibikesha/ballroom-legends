using BallroomLegends.Player;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    public class PlayerNameRulesTests
    {
        [Test]
        public void Clean_TrimsSpaces()
        {
            Assert.AreEqual("Aibike", PlayerNameRules.Clean("  Aibike  "));
        }

        [Test]
        public void Clean_NullBecomesEmpty()
        {
            Assert.AreEqual(string.Empty, PlayerNameRules.Clean(null));
        }

        [Test]
        public void IsValid_NormalName_IsTrue()
        {
            Assert.IsTrue(PlayerNameRules.IsValid("Aibike"));
        }

        [Test]
        public void IsValid_EmptyNullOrOnlySpaces_IsFalse()
        {
            Assert.IsFalse(PlayerNameRules.IsValid(""));
            Assert.IsFalse(PlayerNameRules.IsValid("   "));
            Assert.IsFalse(PlayerNameRules.IsValid(null));
        }

        // Boundary tests: bugs love the edges ("off-by-one" errors), so test exactly
        // at the limits and one step past them.
        [Test]
        public void IsValid_ExactlyMinLength_IsTrue()
        {
            Assert.IsTrue(PlayerNameRules.IsValid(new string('a', PlayerNameRules.MinLength)));
        }

        [Test]
        public void IsValid_OneBelowMinLength_IsFalse()
        {
            Assert.IsFalse(PlayerNameRules.IsValid(new string('a', PlayerNameRules.MinLength - 1)));
        }

        [Test]
        public void IsValid_ExactlyMaxLength_IsTrue()
        {
            Assert.IsTrue(PlayerNameRules.IsValid(new string('a', PlayerNameRules.MaxLength)));
        }

        [Test]
        public void IsValid_OneAboveMaxLength_IsFalse()
        {
            Assert.IsFalse(PlayerNameRules.IsValid(new string('a', PlayerNameRules.MaxLength + 1)));
        }

        [Test]
        public void IsValid_LengthIsCountedAfterTrimming()
        {
            Assert.IsTrue(PlayerNameRules.IsValid("  Al  "));
        }

        [Test]
        public void GetError_ValidName_IsNull()
        {
            Assert.IsNull(PlayerNameRules.GetError("Aibike"));
        }

        [Test]
        public void GetError_EmptyName_AsksForAName()
        {
            Assert.AreEqual("Please type your name.", PlayerNameRules.GetError("  "));
        }
    }
}
