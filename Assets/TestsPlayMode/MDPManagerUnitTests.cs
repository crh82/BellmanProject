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
            var vertical   = (Down, Up   );
            var horizontal = (Left, Right);
            Assert.That(Left.GetOrthogonalEffects(),  Is.EqualTo( vertical ));
            Assert.That(Down.GetOrthogonalEffects(),  Is.EqualTo( horizontal ));
            Assert.That(Right.GetOrthogonalEffects(), Is.EqualTo( vertical ));
            Assert.That(Up.GetOrthogonalEffects(),    Is.EqualTo( horizontal ));
             
        }
    }
    
    public class MdpManagerStateActionTransitionClassUnitTests
    {

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
            "RussellNorvigGridworldTest", 
            MdpRules.RussellAndNorvig,
            new[] {4, 3},
            new[] {5},
            new[] {7},
            new[] {11},
            -0.04f,
            -1f,
            1f);
        
        private readonly MDP _frozenLake4 = MdpAdmin.GenerateMdp(
            "FrozenLake4x4Test", 
            MdpRules.FrozenLake,
            new[] {4, 4},
            new int[] {},
            new[] {0,7,9,11},
            new[] {3});
        
        private readonly MDP _drunkBonanza = MdpAdmin.GenerateMdp(
            name:"DrunkBonanza4x4Test", 
            MdpRules.DrunkBonanza,
            dimensions: new[] {4, 4},
            new int[] {},
            new[] {0,7,9,11},
            new[] {3},
            -0.04f,
            -1f,
            10f);

        private readonly MDP _littleTestWorld = MdpAdmin.GenerateMdp(
            "LittleTestWorldTest",
            MdpRules.FrozenLake,
            new [] {3,2},
            new int[] {},
            new [] {1},
            new []{2}
        );

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
            MdpAdmin.SaveMdpToFile(_russellNorvigMdp, "Assets/Resources/TestMDPs");
            MdpAdmin.SaveMdpToFile(_frozenLake4, "Assets/Resources/TestMDPs");
            MdpAdmin.SaveMdpToFile(_drunkBonanza, "Assets/Resources/TestMDPs");
            MdpAdmin.SaveMdpToFile(_littleTestWorld, "Assets/Resources/TestMDPs");
        }

        [Test]
        public void DeserialisationTests()
        {
            string jsonStringFromJsonFile = File.ReadAllText("Assets/Resources/TestMDPs/RussellNorvigGridworldTest.json");
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
            
            jsonStringFromJsonFile = File.ReadAllText("Assets/Resources/TestMDPs/FrozenLake4x4Test.json");
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
        private readonly Algorithms _algorithms = new Algorithms();
        MDP _frozenLake4B4Mdp = MdpAdmin.LoadMdp(
            File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json"));
        MDP _russellNorvigMdp = MdpAdmin.LoadMdp(
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
         public void FrozenLake4X4AdversarialToOptimalGamma1()
        {
            const string nameOfFile = "TestResultsFL4x4";
               
            var frozenLakeAdversarialPolicy = new Policy();
            frozenLakeAdversarialPolicy.SetAction(1,  Left);
            frozenLakeAdversarialPolicy.SetAction(2,  Left);
            frozenLakeAdversarialPolicy.SetAction(4,  Left);
            frozenLakeAdversarialPolicy.SetAction(5,  Left);
            frozenLakeAdversarialPolicy.SetAction(6,  Left);
            frozenLakeAdversarialPolicy.SetAction(8,  Up);
            frozenLakeAdversarialPolicy.SetAction(10, Up);
            frozenLakeAdversarialPolicy.SetAction(12, Up);
            frozenLakeAdversarialPolicy.SetAction(13, Up);
            frozenLakeAdversarialPolicy.SetAction(14, Up);
            frozenLakeAdversarialPolicy.SetAction(15, Up);
            
            StateValueFunction adversarialPolicyValue =
                _algorithms.PolicyEvaluation(_frozenLake4B4Mdp, frozenLakeAdversarialPolicy, 0.99f, 1e-10f, false, 10000, true);

            var (valueOfIteratedPolicy, iteratedPolicy) = _algorithms.PolicyIteration(
                _frozenLake4B4Mdp, 
                frozenLakeAdversarialPolicy, 
                0.99f,
                1e-10f,
                true,
                1000,
                true);
            
            var frozenLakeOptimalBenchmark = new Policy();
            frozenLakeOptimalBenchmark.SetAction(1,  Right);
            frozenLakeOptimalBenchmark.SetAction(2,  Down);
            frozenLakeOptimalBenchmark.SetAction(4,  Up);
            frozenLakeOptimalBenchmark.SetAction(5,  Down);
            frozenLakeOptimalBenchmark.SetAction(6,  Left);
            frozenLakeOptimalBenchmark.SetAction(8,  Left);
            frozenLakeOptimalBenchmark.SetAction(10, Left);
            frozenLakeOptimalBenchmark.SetAction(12, Left);
            frozenLakeOptimalBenchmark.SetAction(13, Up);
            frozenLakeOptimalBenchmark.SetAction(14, Up);
            frozenLakeOptimalBenchmark.SetAction(15, Up);
            
            MdpAdmin.GenerateTestOutputAsCsv(
                _frozenLake4B4Mdp, 
                adversarialPolicyValue, 
                valueOfIteratedPolicy, 
                frozenLakeAdversarialPolicy, 
                iteratedPolicy, 
                nameOfFile);
            
            var saveFilePath = $"Assets/TestResults/{nameOfFile}.csv";
            string[] iteratedPolicyArray =
                iteratedPolicy.PolicyToStringArray(_frozenLake4B4Mdp.States);
            string[] optimalTestArray =
                frozenLakeOptimalBenchmark.PolicyToStringArray(_frozenLake4B4Mdp.States);
            
            Assert.That(iteratedPolicyArray, Is.EquivalentTo(optimalTestArray));

            var outcome = _algorithms.ValueIteration(_frozenLake4B4Mdp);

            iteratedPolicyArray = outcome.Item2.PolicyToStringArray(_frozenLake4B4Mdp.States);
            
            Assert.That(iteratedPolicyArray, Is.EquivalentTo(optimalTestArray));
        }

        [Test]
        public void FrozenLakeOptimalBenchmarkTest()
        {
            var frozenLakeOptimalBenchmark = new Policy();
            frozenLakeOptimalBenchmark.SetAction(1,  Right);
            frozenLakeOptimalBenchmark.SetAction(2,  Down);
            frozenLakeOptimalBenchmark.SetAction(4,  Up);
            frozenLakeOptimalBenchmark.SetAction(5,  Down);
            frozenLakeOptimalBenchmark.SetAction(6,  Left);
            frozenLakeOptimalBenchmark.SetAction(8,  Left);
            frozenLakeOptimalBenchmark.SetAction(10, Left);
            frozenLakeOptimalBenchmark.SetAction(12, Left);
            frozenLakeOptimalBenchmark.SetAction(13, Up);
            frozenLakeOptimalBenchmark.SetAction(14, Up);
            frozenLakeOptimalBenchmark.SetAction(15, Up);
            
            StateValueFunction adversarialPolicyValue =
                _algorithms.PolicyEvaluation(
                    _frozenLake4B4Mdp, 
                    frozenLakeOptimalBenchmark, 
                    0.99f, 
                    1e-10f, 
                    false, 
                    218, 
                    true);

            var twoArraysVersion =
                _algorithms.PolicyEvaluationTwoArrays(_frozenLake4B4Mdp, frozenLakeOptimalBenchmark, 0.99f, 1e-10f);
            
        }
        

        [Test]
        public void RussellNorvigRandomToOptimalTest()
        {
            const string nameOfFile = "TestResultsRNGWRandToOpt";

            var randomPolicy = new Policy(_russellNorvigMdp);

            StateValueFunction randomPolicyValue = _algorithms.PolicyEvaluation(
                _russellNorvigMdp, 
                randomPolicy,
                0.99f,
                1e-10f,
                false,
                1000,
                true);
            
            var (optimalValue, optimalPolicy) = _algorithms.PolicyIteration(
                _russellNorvigMdp, 
                randomPolicy, 
                0.99f,
                1e-10f,
                false,
                1000,
                true);
            
            MdpAdmin.GenerateTestOutputAsCsv(
                _russellNorvigMdp,
                randomPolicyValue,
                optimalValue,
                randomPolicy,
                optimalPolicy,
                nameOfFile);
            
            var saveFilePath = $"Assets/TestResults/{nameOfFile}.csv";

            var vIoutcome = _algorithms.ValueIteration(
                _russellNorvigMdp, 
                0.99f,
                1e-10f,
                false,
                1000,
                true);

            string[] pIactions = optimalPolicy.PolicyToStringArray(_russellNorvigMdp.States);
            string[] vIactions = vIoutcome.Item2.PolicyToStringArray(_russellNorvigMdp.States);
            
            Assert.That(pIactions, Is.EquivalentTo(vIactions));
            
            Assert.That(File.Exists(saveFilePath), Is.True);

        }
        
        
        
        [Test]
        public void PolicyEvaluationTest()
        {
            // _algs.mdp = _frozenLake4B4;
            _algorithms.globalMdp           = _russellNorvigMdp;
            _algorithms.discountFactorGamma = 1.0f;
            _algorithms.thresholdTheta      = 1e-10f;
            
            // A test policy for frozen lake
            

            var myMistakePolicy = new Policy();

            myMistakePolicy.SetAction(_russellNorvigMdp.States[8],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[4],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[0],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[9],  Right);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[1],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[10], Right);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[6],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[2],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[3],   Left);

            StateValueFunction valueFunction = _algorithms.PolicyEvaluation(
                _russellNorvigMdp, 
                myMistakePolicy, 
                1f, 
                1e-10f, 
                true, 
                1000);
            
            Assert.AreEqual(-7.882292, valueFunction.Value(0), 1e-6);
            Assert.AreEqual(-7.920341, valueFunction.Value(1),1e-6);
            Assert.AreEqual(-7.000689, valueFunction.Value(2),1e-6);
            Assert.AreEqual(-6.368815, valueFunction.Value(3),1e-6);
            Assert.AreEqual(-7.842292, valueFunction.Value(4),1e-6);
            Assert.AreEqual(0,         valueFunction.Value(5),1e-6);
            Assert.AreEqual(0.7095891, valueFunction.Value(6),1e-6);
            Assert.AreEqual(-1,        valueFunction.Value(7),1e-6);
            Assert.AreEqual(-7.812047, valueFunction.Value(8),1e-6);
            Assert.AreEqual(0.9232877, valueFunction.Value(9),1e-6);
            Assert.AreEqual(0.9632877, valueFunction.Value(10),1e-6);
            Assert.AreEqual(1,         valueFunction.Value(11),1e-6);
            
        }

        [Test]
        public void TestPolicyEqualityFunction()
        {
            var policy1 = new Policy(4);
            var policy1Copy = policy1.Copy();
            
            Assert.That(policy1.Equals(policy1Copy), Is.True);
            
            var policy2 = new Policy();
            
            Assert.That(policy1.Equals(policy2), Is.False);
            
            policy2.SetAction(0,Left);
            policy2.SetAction(1, Down);
            policy2.SetAction(2, Right);
            policy2.SetAction(3, Up);

            var policy3 = new Policy();
            
            policy3.SetAction(0,Left);
            policy3.SetAction(1, Down);
            policy3.SetAction(2, Right);
            policy3.SetAction(3, Up);
            
            Assert.That(policy2.Equals(policy3), Is.True);
            
            policy3.SetAction(0, Right);
            
            Assert.That(policy2.Equals(policy3), Is.False);

        }

        [Test]
        public void PolicyImprovementTest()
        {
            const float gamma = 1f;
            const float theta = 1e-10f;
            
            var myMistakePolicy = new Policy();

            myMistakePolicy.SetAction(_russellNorvigMdp.States[8],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[4],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[0],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[9],  Right);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[1],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[10], Right);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[6],     Up);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[2],   Left);
            myMistakePolicy.SetAction(_russellNorvigMdp.States[3],   Left);
            
            var optimalPolicy = myMistakePolicy.Copy();
            optimalPolicy.SetAction(8, Right);

            StateValueFunction valueOfMistakePolicy =
                _algorithms.PolicyEvaluation(_russellNorvigMdp, myMistakePolicy, gamma, theta);

            Policy improvedPolicy = _algorithms.PolicyImprovement(_russellNorvigMdp, valueOfMistakePolicy, gamma);

            StateValueFunction valueOfImprovedPolicy =
                _algorithms.PolicyEvaluation(_russellNorvigMdp, improvedPolicy, gamma, theta);

            float sumOfMistakenValues =
                valueOfMistakePolicy.StateValuesToFloatArray(_russellNorvigMdp.StateCount).Sum();
            float sumOfImprovedValues =
                valueOfImprovedPolicy.StateValuesToFloatArray(_russellNorvigMdp.StateCount).Sum();
            
            Assert.That(sumOfMistakenValues, Is.LessThan(sumOfImprovedValues));
            Assert.That(myMistakePolicy.Equals(improvedPolicy), Is.False);
        }

        [Test]
        public void ArgMaxSimple()
        {
            MarkovAction a0 = new MarkovAction {Action = Left};
            MarkovAction a1 = new MarkovAction {Action = Down};
            MarkovAction a2 = new MarkovAction {Action = Right};

            float qs1a0 = 0.0f;
            float qs1a1 = 1.0f;
            float qs1a2 = 2.0f;
            
            MarkovState s1 = new MarkovState
            {
                StateIndex = 0,
                ApplicableActions = new List<MarkovAction>{a0,a1,a2}
            };

            ActionValueFunction q = new ActionValueFunction();
            
            q.SetValue(s1, a0.Action, qs1a0);
            q.SetValue(s1, a2.Action, qs1a2);
            q.SetValue(s1, a1.Action, qs1a1);

            Assert.That(Right, Is.EqualTo(q.ArgMaxAction(s1)));
            Assert.That(Left,  Is.Not.EqualTo(q.ArgMaxAction(s1)));
        }
        
        [Test]
        public void ArgMaxPrecision()
        {
            MarkovAction a0 = new MarkovAction {Action = Left};
            MarkovAction a1 = new MarkovAction {Action = Down};
            MarkovAction a2 = new MarkovAction {Action = Right};

            float qs1a0 = 0.00000000001f;
            float qs1a1 = 0.00000000002f;
            float qs1a2 = 0.00000000003f;
            
            MarkovState s1 = new MarkovState
            {
                StateIndex = 0,
                ApplicableActions = new List<MarkovAction>{a0,a1,a2}
            };

            ActionValueFunction q = new ActionValueFunction();
            
            q.SetValue(s1, a0.Action, qs1a0);
            q.SetValue(s1, a2.Action, qs1a2);
            q.SetValue(s1, a1.Action, qs1a1);

            Assert.That(Right, Is.EqualTo(q.ArgMaxAction(s1)));
            Assert.That(Left,  Is.Not.EqualTo(q.ArgMaxAction(s1)));
        }
    }

    public class LittleWorldTests
    {
        private Algorithms _algorithms = new Algorithms();

        private readonly MDP _littleWorld = MdpAdmin.LoadMdp(
            File.ReadAllText("Assets/Resources/TestMDPs/LittleTestWorldTest.json"));
        
        private const GridAction Left  = GridAction.Left;
        private const GridAction Down  = GridAction.Down;
        private const GridAction Right = GridAction.Right;
        private const GridAction Up    = GridAction.Up;
        private const float TestGamma = 1f;
        private const float TestTheta = 1e-10f;

        private Policy _policyIteratedPolicy;
        
        [Test]
        public void PolicyEvaluationTest()
        {
            var worstPolicy = new Policy();
            worstPolicy.SetAction(0, Down);
            worstPolicy.SetAction(3, Left);
            worstPolicy.SetAction(4, Left);
            worstPolicy.SetAction(5, Up);

            StateValueFunction valueOfWorstPolicy =
                _algorithms.PolicyEvaluation(_littleWorld, worstPolicy, TestGamma, TestTheta);
            
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(0)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(1)));
            Assert.That(1f, Is.EqualTo(valueOfWorstPolicy.Value(2)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(3)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(4)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(5)));

            var terminalStateReachablePolicy = new Policy();
            terminalStateReachablePolicy.SetAction(0, Left);
            terminalStateReachablePolicy.SetAction(3, Up);
            terminalStateReachablePolicy.SetAction(4, Up);
            terminalStateReachablePolicy.SetAction(5, Right);

            StateValueFunction valueOfTerminalStateReachablePolicy =
                _algorithms.PolicyEvaluation(_littleWorld, terminalStateReachablePolicy, TestGamma, TestTheta);
            Assert.That(valueOfTerminalStateReachablePolicy.Value(0), Is.GreaterThan(0));
            Assert.That(0f, Is.EqualTo(valueOfTerminalStateReachablePolicy.Value(1)));
            Assert.That(1f, Is.EqualTo(valueOfTerminalStateReachablePolicy.Value(2)));
            Assert.That(valueOfTerminalStateReachablePolicy.Value(3), Is.GreaterThan(0));
            Assert.That(valueOfTerminalStateReachablePolicy.Value(4), Is.GreaterThan(0));
            Assert.That(valueOfTerminalStateReachablePolicy.Value(5), Is.GreaterThan(0));
            
            
        }

        [Test]
        public void PolicyImprovementTest()
        {

            var worstPolicy = new Policy();
            worstPolicy.SetAction(0, Down);
            worstPolicy.SetAction(3, Left);
            worstPolicy.SetAction(4, Left);
            worstPolicy.SetAction(5, Up);

            StateValueFunction valueOfWorstPolicy =
                _algorithms.PolicyEvaluation(_littleWorld, worstPolicy, TestGamma, TestTheta);
            
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(0)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(1)));
            Assert.That(1f, Is.EqualTo(valueOfWorstPolicy.Value(2)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(3)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(4)));
            Assert.That(0f, Is.EqualTo(valueOfWorstPolicy.Value(5)));

            Policy improvedPolicy = _algorithms.PolicyImprovement(_littleWorld, valueOfWorstPolicy, TestGamma);
            
            Assert.That(worstPolicy.Equals(improvedPolicy), Is.False);
            
            StateValueFunction valueOfImprovedPolicy =
                _algorithms.PolicyEvaluation(_littleWorld, improvedPolicy, TestGamma, TestTheta);
            Assert.That(valueOfImprovedPolicy.Value(5), Is.GreaterThan(0));
        }

        [Test]
        public void PolicyIterationTest()
        {
            const string nameOfFile = "TestLittleWorld_PI";
            
            var worstPolicy = new Policy();
            worstPolicy.SetAction(0, Down);
            worstPolicy.SetAction(3, Left);
            worstPolicy.SetAction(4, Left);
            worstPolicy.SetAction(5, Up);
            
            StateValueFunction valueOfWorstPolicy =
                _algorithms.PolicyEvaluation(_littleWorld, worstPolicy, 0.99f, TestTheta);

            var (stateValueFunction, policy) = _algorithms.PolicyIteration(_littleWorld, worstPolicy, 0.99f);
            
            MdpAdmin.GenerateTestOutputAsCsv(
                _littleWorld, 
                valueOfWorstPolicy, 
                stateValueFunction, 
                worstPolicy, 
                policy, 
                nameOfFile);
            var saveFilePath = $"Assets/TestResults/{nameOfFile}.csv";
            
            Assert.That(File.Exists(saveFilePath), Is.True);
        }

        [Test]
        public void PolicyIterationFromRandomValuesAndRandomPolicy()
        {
            const string nameOfFile2 = "TestLittleWorldFromRandom";
            
            var startPolicy = new Policy(_littleWorld);

            StateValueFunction valueOfStartPolicy = new StateValueFunction(_littleWorld, -2.0f, 3.0f);

            var (valueOfEndPolicy, endPolicy) = _algorithms.PolicyIteration(_littleWorld, startPolicy, 0.99f, debugMode:true);
            
            MdpAdmin.GenerateTestOutputAsCsv(
                _littleWorld, 
                valueOfStartPolicy, 
                valueOfEndPolicy, 
                startPolicy, 
                endPolicy, 
                nameOfFile2);
            
            var saveFilePath2 = $"Assets/TestResults/{nameOfFile2}.csv";

            _policyIteratedPolicy = endPolicy;
            
            Assert.That(File.Exists(saveFilePath2), Is.True);

            
        }
        
        [Test]
        public void ValueIterationTest()
        {
            const string nameOfFile = "TestLittleWorld_VI";
            
            var worstPolicy = new Policy();
            worstPolicy.SetAction(0, Down);
            worstPolicy.SetAction(3, Left);
            worstPolicy.SetAction(4, Left);
            worstPolicy.SetAction(5, Up);
            
            StateValueFunction valueOfWorstPolicy =
                _algorithms.PolicyEvaluation(_littleWorld, worstPolicy, 0.99f, TestTheta);

            
            var (valueIteratedStateValues, policy) = _algorithms.ValueIteration(_littleWorld, 0.99f);
            
            MdpAdmin.GenerateTestOutputAsCsv(
                _littleWorld, 
                valueOfWorstPolicy, 
                valueIteratedStateValues, 
                worstPolicy, 
                policy, 
                nameOfFile);
            
            var saveFilePath = $"Assets/TestResults/{nameOfFile}.csv";

            string[] pI = _policyIteratedPolicy.PolicyToStringArray(_littleWorld.States);
            string[] vI = policy.PolicyToStringArray(_littleWorld.States);
            
            Assert.That(vI, Is.EquivalentTo(pI));
            
            Assert.That(File.Exists(saveFilePath), Is.True);
        }
    }
    
    public class StateValueFunctionTests
    {
        [Test]
        public void StateValueChangeTest()
        {
            StateValueFunction vOfS = new StateValueFunction();
            vOfS.SetValue(0, 2f);
            vOfS.SetValue(0, 2.02f);
            Assert.That(vOfS.StateValueDeltaBetweenTMinusOneAndT(0), Is.EqualTo(0.02f).Within(1e-7f));
        }

        [Test]
        public void MaxDifferenceOfValuesTest()
        {
            StateValueFunction vOfS = new StateValueFunction();
            vOfS.SetValue(0, 2f);
            vOfS.SetValue(0, 2.02f);
            vOfS.SetValue(1,10f);
            vOfS.SetValue(1,20f);
            Assert.That(vOfS.MaxChangeInValueOfStates(), Is.EqualTo(10f).Within(1e-7f));
        }
    }
}
