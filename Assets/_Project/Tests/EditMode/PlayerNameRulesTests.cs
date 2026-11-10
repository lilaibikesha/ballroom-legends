using BallroomLegends.Player;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>
    /// Test-first practice: delete one [Ignore] at a time, watch it fail (red),
    /// write just enough code in PlayerNameRules to make it pass (green), repeat.
    /// </summary>
    public class PlayerNameRulesTests
    {
        private const string Todo = "Implement PlayerNameRules, then delete this [Ignore] line.";

        [Test]
        [Ignore(Todo)]
        public void Clean_TrimsSpaces()
        {
            Assert.AreEqual("Aibike", PlayerNameRules.Clean("  Aibike  "));
        }

        [Test]
        [Ignore(Todo)]
        public void Clean_NullBecomesEmpty()
        {
            Assert.AreEqual(string.Empty, PlayerNameRules.Clean(null));
        }

        [Test]
        [Ignore(Todo)]
        public void IsValid_NormalName_IsTrue()
        {
            Assert.IsTrue(PlayerNameRules.IsValid("Aibike"));
        }

        [Test]
        [Ignore(Todo)]
        public void IsValid_EmptyOrOnlySpaces_IsFalse()
        {
            Assert.IsFalse(PlayerNameRules.IsValid(""));
            Assert.IsFalse(PlayerNameRules.IsValid("   "));
        }

        [Test]
        [Ignore(Todo)]
        public void IsValid_TooLong_IsFalse()
        {
            Assert.IsFalse(PlayerNameRules.IsValid(new string('a', PlayerNameRules.MaxLength + 1)));
        }

        // TODO (you): add boundary tests. Is a name of exactly MinLength valid?
        // Exactly MaxLength? What about "  Al  " (2 letters after trimming)?
    }
}
