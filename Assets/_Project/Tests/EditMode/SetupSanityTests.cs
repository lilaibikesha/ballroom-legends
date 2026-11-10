using BallroomLegends.Characters;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using NUnit.Framework;

namespace BallroomLegends.Tests
{
    /// <summary>Proves the test pipeline works. If these pass, your setup is good.</summary>
    public class SetupSanityTests
    {
        [Test]
        public void SceneNames_HasSixUniqueScenes()
        {
            Assert.AreEqual(6, SceneNames.All.Length);
            CollectionAssert.AllItemsAreUnique(SceneNames.All);
        }

        [Test]
        public void SceneNames_MainMenuLoadsFirst()
        {
            Assert.AreEqual(SceneNames.MainMenu, SceneNames.All[0]);
        }

        [Test]
        public void DancerCharacter_DefaultIsNone()
        {
            Assert.AreEqual(DancerCharacter.None, default(DancerCharacter));
        }

        [Test]
        public void DanceStyle_DefaultIsNone()
        {
            Assert.AreEqual(DanceStyle.None, default(DanceStyle));
        }
    }
}
