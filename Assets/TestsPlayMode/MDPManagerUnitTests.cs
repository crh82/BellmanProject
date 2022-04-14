using System.IO;
using NUnit.Framework;

namespace TestsPlayMode
{
    public class MdpManagerUnitTests
    {
        // Todo Test actions on different gridworlds

        private const string FilePath = "Assets/Resources/CanonicalMDPs/RussellNorvigGridworld-v0.json";
        static readonly string JsonDataForTests = File.ReadAllText(FilePath);
        private MDP _testMdp = MDP.CreateFromJSON(JsonDataForTests);

        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;


        [Test]
        public void LeftAction()
        {
            // Action was applicable
            _testMdp.obstacleStates = new int[ ]{5};
            Assert.That(2, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Left)));
            Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 10, Left)));
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Left)));
        
            // Hitting the left boundary
            Assert.That(8, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Left)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Left)));
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Left)));
        }
    
    
        [Test]
        public void DownAction()
        {
            // Action was applicable
            _testMdp.obstacleStates = new int[ ]{5};
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Down)));
            Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 10, Down)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Down)));
        
            // Hitting the bottom boundary
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Down)));
            Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Down)));
            Assert.That(3, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Down)));
        }
    
    
        [Test]
        public void RightAction()
        {
            // Action was applicable
            _testMdp.obstacleStates = new int[ ]{5};
            Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 2, Right)));
            Assert.That(1,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Right)));
            Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Right)));
        
            // Hitting the right boundary
            Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 11, Right)));
            Assert.That(7,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 7, Right)));
            Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Right)));
        }

    
        [Test]
        public void UpAction()
        {
            // Action was applicable
            _testMdp.obstacleStates = new int[ ]{5};
            Assert.That(4,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Up)));
            Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 6, Up)));
            Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Up)));
        
            // Hitting the top boundary
            Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Up)));
            Assert.That(9,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Up)));
            Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 11, Up)));
        }

        [Test]
        public void HitObstacle()
        {
            _testMdp.obstacleStates = new int[ ]{5};
            Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 6, Left)));
            Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Down)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Right)));
            Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Up)));
        }
    }
}
