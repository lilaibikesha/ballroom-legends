using BallroomLegends.Characters;
using BallroomLegends.Core;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>Proves the test pipeline works. If these pass, your setup is good.</summary>
    public class SetupSanityTests
    {
        [Test]
        public void SceneNames_HasFourUniqueScenes()
        {
            Assert.AreEqual(4, SceneNames.All.Length);
            CollectionAssert.AllItemsAreUnique(SceneNames.All);
        }

        [Test]
        public void DancerCharacter_DefaultIsNone()
        {
            Assert.AreEqual(DancerCharacter.None, default(DancerCharacter));
        }
    }
}
