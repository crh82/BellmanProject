using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        

        [Test]
        public void LeftAction()
        {
            // Action performed as intended
            _testMdp.ObstacleStates = new[ ]{5};
            Assert.That(2, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[3], Left)));
            Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[10], Left)));
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[1], Left)));
        
            // Hitting the left boundary
            Assert.That(8, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[8], Left)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[4], Left)));
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[0], Left)));
        }
        
        
        [Test]
        public void DownAction()
        {
            // Action performed as intended
            _testMdp.ObstacleStates = new[ ]{5};
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[4], Down)));
            Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[10], Down)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[8], Down)));
        
            // Hitting the bottom boundary
            Assert.That(0, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[0], Down)));
            Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[1], Down)));
            Assert.That(3, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[3], Down)));
        }
    
        
        [Test]
        public void RightAction()
        {
            // Action performed as intended
            _testMdp.ObstacleStates = new[ ]{5};
            Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[2], Right)));
            Assert.That(1,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[0], Right)));
            Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[9], Right)));
        
            // Hitting the right boundary
            Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[11], Right)));
            Assert.That(7,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[7], Right)));
            Assert.That(3,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[3], Right)));
        }
        
        
        [Test]
        public void UpAction()
        {
            // Action performed as intended
            _testMdp.ObstacleStates = new[ ]{5};
            Assert.That(4,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[0], Up)));
            Assert.That(10, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[6], Up)));
            Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[4], Up)));
        
            // Hitting the top boundary
            Assert.That(8,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[8], Up)));
            Assert.That(9,  Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[9], Up)));
            Assert.That(11, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[11], Up)));
        }
        
        [Test]
        public void HitsObstacleReturnsState()
        {
            _testMdp.ObstacleStates = new[ ]{5};
            Assert.That(6, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[6], Left)));
            Assert.That(9, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[9], Down)));
            Assert.That(4, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[4], Right)));
            Assert.That(1, Is.EqualTo(MdpAdmin.GenerateSuccessorStateFromAction(_testMdp, _testMdp.States[1], Up)));
        }

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
            ActionTaken = GridAction.Right,
            Probability = 1.0f,
            SuccessorStateIndex = 1,
            Reward = 15.75f,
            // IsTerminal = false
        };
        private MarkovTransition _t2 = new MarkovTransition
        {
            State = 1,
            ActionTaken = GridAction.Left,
            Probability = 1.0f,
            SuccessorStateIndex = 2,
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

       

    }
    
    public class MdpGenerationTests
    {
        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;
        
        private readonly MDP _russellNorvigMdp = MdpAdmin.GenerateMdp(
            "RussellNorvigGridworld", 
            MdpRules.RussellAndNorvig,
            new[] {4, 3},
            new[] {5},
            new[] {7},
            new[] {11},
            -0.04f,
            -1f,
            1f);
        
        private readonly MDP _frozenLake4 = MdpAdmin.GenerateMdp(
            "FrozenLake4x4", 
            MdpRules.FrozenLake,
            new[] {4, 4},
            new int[] {},
            new[] {0,7,9,11},
            new[] {3});
        
        private readonly MDP _drunkBonanza = MdpAdmin.GenerateMdp(
            name:"DrunkBonanza4x4", 
            MdpRules.DrunkBonanza,
            dimensions: new[] {4, 4},
            new int[] {},
            new[] {0,7,9,11},
            new[] {3},
            -0.04f,
            -1f,
            10f);

        [Test]
        public void TestGridWorldRules()
        {
            Assert.That(new[]{0.5f, 0.33333f, 0.16667f}, Is.EquivalentTo(MdpRules.SlipperyWalk.GetProbabilityDistributionOfActionOutcomes()));
            Assert.That((MdpRules.DrunkBonanza.GetProbabilityDistributionOfActionOutcomes().Sum()), Is.InRange(0.99f, 1.001f));
        }

        [Test]
        public void MdpInitialization()
        {
            Assert.That(_russellNorvigMdp.MdpRules, Is.EqualTo(MdpRules.RussellAndNorvig));
            Assert.That(_russellNorvigMdp.States.Count, Is.EqualTo(12));
            Assert.That(_russellNorvigMdp.States[2].ApplicableActions.Count, Is.EqualTo(4));
            // Assert.That(_russellNorvigMdp.Rewards[11], Is.EqualTo(1f));
            
        }
        
        [Test]
        public void SerialisingStates()
        {
            MdpAdmin.SaveMdpToFile(_russellNorvigMdp, "Assets/Resources/CanonicalMDPs");
            MdpAdmin.SaveMdpToFile(_frozenLake4, "Assets/Resources/CanonicalMDPs");
            MdpAdmin.SaveMdpToFile(_drunkBonanza, "Assets/Resources/CanonicalMDPs");
        }

        [Test]
        public void DeserialisationTests()
        {
            string jsonStringFromJsonFile = File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json");
            MDP russellAndNorvig = MdpAdmin.LoadMdp(jsonStringFromJsonFile);
            
            Assert.That(russellAndNorvig.ObstacleStates, Has.Member(5));
            Assert.That(russellAndNorvig.GoalStates,     Has.Member(11));
            Assert.That(russellAndNorvig.TerminalStates, Has.Member(7));
            Assert.That(russellAndNorvig
                .States[3]
                .ApplicableActions[(int) Up]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(7));
           
            Assert.That(russellAndNorvig
                .States[3]
                .ApplicableActions[(int) Left]
                .Transitions[1]
                .Probability, Is.InRange(0.09f, 0.11f));
            
            Assert.That(russellAndNorvig
                .States[10]
                .ApplicableActions[(int) Right]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(11));
            
            Assert.That(russellAndNorvig
                .States[8]
                .ApplicableActions[(int) Down]
                .Transitions[2]
                .Probability, Is.InRange(0.09f, 0.11f));
            
            Assert.That(russellAndNorvig
                .States[4]
                .ApplicableActions[(int) Left]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(4));
            
            Assert.That(russellAndNorvig
                .States[10]
                .ApplicableActions[(int) Right]
                .Transitions[0]
                .Reward, Is.InRange(0.9f, 1.1f));
            
            jsonStringFromJsonFile = File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json");
            MDP frozenLake4B4 = MdpAdmin.LoadMdp(jsonStringFromJsonFile);
            
            Assert.That(frozenLake4B4.TerminalStates, Has.Member(11));
            Assert.That(frozenLake4B4.GoalStates,     Has.Member(3));
            Assert.That(frozenLake4B4.TerminalStates, Has.Member(7));
            Assert.That(frozenLake4B4
                .States[3]
                .ApplicableActions[(int) Up]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(3));
           
            Assert.That(frozenLake4B4
                .States[2]
                .ApplicableActions[(int) Right]
                .Transitions[0]
                .Reward, Is.InRange(0.9f, 1.1f));
            
            Assert.That(frozenLake4B4
                .States[10]
                .ApplicableActions[(int) Down]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(6));
            
            Assert.That(frozenLake4B4
                .States[8]
                .ApplicableActions[(int) Down]
                .Transitions[0]
                .Probability, Is.InRange(0.32f, 0.34f));
            
            Assert.That(frozenLake4B4
                .States[9]
                .ApplicableActions[(int) Left]
                .Transitions[0]
                .SuccessorStateIndex, Is.EqualTo(9));
            
            Assert.That(frozenLake4B4
                .States[14]
                .ApplicableActions[(int) Right]
                .Transitions[0]
                .Reward, Is.InRange(0.0f, 0.01f));
            
        }

    }

    public class AlgorithmsTests
    {
        // private GameObject _testGameObject = new GameObject().AddComponent<Algorithms>();
        // private readonly Algorithms _algs = new GameObject().AddComponent<Algorithms>();
        private readonly Algorithms _algs = new Algorithms();
        MDP _frozenLake4B4 = MdpAdmin.LoadMdp(
            File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json"));
        MDP _russellNorvig = MdpAdmin.LoadMdp(
            File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;
        
        [Test]
        public void TestMaxAbsoluteDifference()
        {
            float[] arrayOne = {1.0f, 3.0f, 5.0f};
            float[] arrayTwo = {1.5f, 2.0f, 8.0f};

            float testDiff = Algorithms.MaxAbsoluteDifference(arrayOne, arrayTwo);
            
            Assert.That(testDiff, Is.InRange(2.99f, 3.01f));
        }

        [Test]
        public void PolicyEvaluationTest()
        {
            // _algs.mdp = _frozenLake4B4;
            _algs.mdp = _russellNorvig;
            _algs.gamma = 1.0f;
            _algs.theta = 1E-10f;
            
            // A test policy for frozen lake
            
            // _algs.policy = new Dictionary<int, GridAction>
            // {
            //     {12, Right},{13,  Left},{14,  Down},{15,    Up},
            //     { 8,  Left},{ 9, Right},{10, Right},{11,  Down},
            //     { 4,    Up},{ 5,  Down},{ 6,    Up},{ 7,  Down},
            //     { 0,    Up},{ 1, Right},{ 2, Down },{ 3, Left },
            //     
            // };
            //
            // // Test policy for Russell Norvig Gridworld
            //
            // _algs.policy = new Dictionary<int, GridAction>
            // {
            //     { 8,  Left},{ 9, Right},{10, Right},{11,  Down},
            //     { 4,    Up},{ 5,  Down},{ 6,    Up},{ 7,  Down},
            //     { 0,    Up},{ 1,  Left},{ 2, Left },{ 3, Left },
            // };
            
            _algs.Policy = new Dictionary<int, GridAction>
            {
                { 8, Right},{ 9, Right},{10, Right},
                { 4,    Up},            { 6,    Up},
                { 0,    Up},{ 1,  Left},{ 2, Left },{ 3, Left },
            };
            
            _algs.PolicyEvaluation();
                
        }
    }
}
