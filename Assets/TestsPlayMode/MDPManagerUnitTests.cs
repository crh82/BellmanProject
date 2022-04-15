using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TestsPlayMode
{

    public class MdpManagerUnitTests
    {
        // Todo Test actions on different gridworlds
        //
        // private const string FilePath = "Assets/Resources/CanonicalMDPs/RussellNorvigGridworld-v0.json";
        // static readonly string JsonDataForTests = File.ReadAllText(FilePath);
        // private MDP _testMdp = MdpAdmin.LoadMdp(JsonDataForTests);

        private MDP _testMdp = MdpAdmin.GenerateMdp(
            "RussellNorvigTest",
            MdpRules.RussellAndNorvig,
            new[] {4, 3},
            new[] {5},
            new[] {7},
            new[] {11},
            -0.04f,
            -1f);
        
        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;
        

        // [Test]
        // public void LeftAction()
        // {
        //     // Action performed as intended
        //     _testMdp.ObstacleStates = new[ ]{5};
        //     Assert.That(2, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Left)));
        //     Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 10, Left)));
        //     Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Left)));
        //
        //     // Hitting the left boundary
        //     Assert.That(8, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Left)));
        //     Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Left)));
        //     Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Left)));
        // }
        //
        //
        // [Test]
        // public void DownAction()
        // {
        //     // Action performed as intended
        //     _testMdp.ObstacleStates = new[ ]{5};
        //     Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Down)));
        //     Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 10, Down)));
        //     Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Down)));
        //
        //     // Hitting the bottom boundary
        //     Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Down)));
        //     Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Down)));
        //     Assert.That(3, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Down)));
        // }
    
        //
        // [Test]
        // public void RightAction()
        // {
        //     // Action performed as intended
        //     _testMdp.ObstacleStates = new[ ]{5};
        //     Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 2, Right)));
        //     Assert.That(1,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Right)));
        //     Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Right)));
        //
        //     // Hitting the right boundary
        //     Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 11, Right)));
        //     Assert.That(7,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 7, Right)));
        //     Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 3, Right)));
        // }
        //
        //
        // [Test]
        // public void UpAction()
        // {
        //     // Action performed as intended
        //     _testMdp.ObstacleStates = new[ ]{5};
        //     Assert.That(4,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 0, Up)));
        //     Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 6, Up)));
        //     Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Up)));
        //
        //     // Hitting the top boundary
        //     Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 8, Up)));
        //     Assert.That(9,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Up)));
        //     Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 11, Up)));
        // }
        //
        // [Test]
        // public void HitsObstacleReturnsState()
        // {
        //     _testMdp.ObstacleStates = new[ ]{5};
        //     Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 6, Left)));
        //     Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 9, Down)));
        //     Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 4, Right)));
        //     Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, 1, Up)));
        // }

        [Test]
        public void InverseActionsTests()
        {
            Assert.That(Left,  Is.EqualTo(Right.GetInverseEffectOfAction()));
            Assert.That(Down,  Is.EqualTo(Up.GetInverseEffectOfAction()));
            Assert.That(Right, Is.EqualTo(Left.GetInverseEffectOfAction()));
            Assert.That(Up,    Is.EqualTo(Down.GetInverseEffectOfAction()));
        }

        [Test]
        public void OrthogonalActionsTests()
        {
            GridAction[] vertical   = {Down, Up   };
            GridAction[] horizontal = {Left, Right};
            Assert.That(Left.GetOrthogonalEffects(),  Is.EquivalentTo( vertical ));
            Assert.That(Down.GetOrthogonalEffects(),  Is.EquivalentTo( horizontal ));
            Assert.That(Right.GetOrthogonalEffects(), Is.EquivalentTo( vertical ));
            Assert.That(Up.GetOrthogonalEffects(),    Is.EquivalentTo( horizontal ));
             
        }
    }
    
    public class MdpManagerStateActionTransitionClassUnitTests
    {
        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;

        private MarkovState _s0 = new MarkovState {StateIndex = 0, Reward = 1.2f, TypeOfState = StateType.Standard};
        private MarkovState _s1 = new MarkovState {StateIndex = 1, Reward = 1.2f, TypeOfState = StateType.Standard};
        private MarkovState _s2 = new MarkovState {StateIndex = 2, Reward = 1.2f, TypeOfState = StateType.Goal};
        private MarkovState _s3 = new MarkovState {StateIndex = 3, Reward = 1.2f, TypeOfState = StateType.Terminal};
        private MarkovState _s4 = new MarkovState {StateIndex = 4, Reward = 1.2f, TypeOfState = StateType.Obstacle};
    
        private MarkovTransition _t1 = new MarkovTransition
        {
            State = 0,
            Action = GridAction.Right,
            Probability = 1.0f,
            SuccessorState = 1,
            Reward = 15.75f,
            // IsTerminal = false
        };
        private MarkovTransition _t2 = new MarkovTransition
        {
            State = 1,
            Action = GridAction.Left,
            Probability = 1.0f,
            SuccessorState = 2,
            Reward = 15.75f,
            // IsTerminal = false
        };
        [Test]
        public void BasicStateFunctionality()
        {
            Assert.That(_s0.IsObstacle, Is.False);
            Assert.That(_s1.IsTerminal, Is.False);
            Assert.That(_s2.IsGoal,     Is.True);
            Assert.That(_s3.IsTerminal, Is.True);
        }

        [Test]
        public void SerialisingStates()
        {
            
        }

    }
    
    public class MdpGenerationTests
    {

        private readonly MDP _testMdp = MdpAdmin.GenerateMdp(
            name:"RussellNorvigTest", 
            MdpRules.RussellAndNorvig,
            dimensions: new[] {4, 3},
            new[] {5},
            new[] {7},
            new[] {11},
            -0.04f,
            -1f,
            1f);

        // obstacleStates:
        // terminalStates:
        // goalStates:
        // standard
        [Test]
        public void MdpNotNull()
        {
            
        }

        [Test]
        public void TestGridWorldRules()
        {
            Assert.That(new[]{0.5f, 0.33333f, 0.16667f}, Is.EquivalentTo(MdpRules.SlipperyWalk.GetProbabilityDistributionOfActionOutcomes()));
        }
    
    }
}
