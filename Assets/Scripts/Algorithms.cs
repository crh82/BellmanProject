using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;
using Object = System.Object;

// using Debug = System.Diagnostics.Debug;
public class Algorithms : MonoBehaviour
{
    // Cheeky Test stuff Todo Remove
    public bool frozenMdp;
    public bool russellMdp;
    public bool myLittleMdp;
    public bool loadMdp;
    public bool runPolicyEvaluation;
    public bool runPolicyImprovement;
    public bool inPlaceVersion = true;
    
    [FormerlySerializedAs("mdp")] public MDP globalMdp;

    public Policy currentPolicy;
    
    
    public Dictionary<int, GridAction> PolicyDictDeprecated;
    // public List<GridAction> policy;
    public string[] editorDisplayPolicy;
    public float[] editorDisplayStateValue;

    // Two array approach. See Sutton & Barto Chp 4.1
    public float[] stateValue;        // 2 Arrays version
    public float[] previousStateValue;// 2 Arrays version
    
    // In-place approach. See Sutton & Barto 4.1. Can converge faster.
    public Dictionary<int, float> ValueOfState; // In-place version

    public float discountFactorGamma;

    public float thresholdTheta;

    [FormerlySerializedAs("iterations")] public int evaluationIterations;

    public StateValueFunction PolicyEvaluation(
        MDP    mdp, 
        Policy policy, 
        float  gamma, 
        float  theta, 
        bool   boundIterations = true, 
        int    maxIterations   = 10000,
        bool   debugMode       = false)
    {
        var kIterations = 0;

        var stateValueFunctionV = new StateValueFunction(mdp);

        while (true)
        {
            float delta = 0;

            foreach (var state in mdp.States.Where(state => state.IsStandard()))
            {
                
                float valueBeforeUpdate = stateValueFunctionV.Value(state);     
                        
                        // Σ P(s'|s,a) [ R(s') + 𝛄 • V(s') ]
                        float valueOfState = 0;

                        // LINQ VERSION
                        //
                        // float valueOfState = (
                        //     from transition in mdp.TransitionFunction(state, policy.GetAction(state)) 
                        //     let probability = transition.Probability 
                        //     let successorState = mdp.States[transition.SuccessorStateIndex] 
                        //     let reward = transition.Reward 
                        //     let valueOfSuccessor = stateValueFunctionV.Value(successorState) 
                        //     let zeroIfTerminal = ZeroIfTerminal(successorState) 
                        //     select probability * (reward + gamma * valueOfSuccessor * zeroIfTerminal)
                        // ).Sum();
                            
                        // foreach (var transition in P(state, policy.Pi(state))) <—— Precomputed transitions.
                        foreach (var transition in mdp.TransitionFunction(state, policy.GetAction(state))) 
                        {
                            float          probability = transition.Probability;
                            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
                            float               reward = transition.Reward;
                            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
                            float       zeroIfTerminal = ZeroIfTerminal(successorState);

                            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
                            valueOfState += probability * (reward + gamma * valueOfSuccessor * zeroIfTerminal);
                        }

                        stateValueFunctionV.SetValue(state, valueOfState);
                        
                        // Rather than running the L-inf norm on the full state set, this checks the change incrementally
                        delta = Math.Max(delta, Math.Abs(valueBeforeUpdate - valueOfState));
                        
            }
            
            if (delta < theta) 
            {
                if (debugMode) Debug.Log($"delta: {delta} theta: {theta}");
                break;
            }
            
            if (boundIterations & (kIterations >= maxIterations))
            {
                if (debugMode) Debug.Log($"delta: {delta} theta: {theta}");
                break;
            }

            kIterations++;
        }

        // For testing and debugging, updates the Unity Editor stateValue array.
        UpdateGameObjectFields(mdp, stateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, stateValueFunctionV, kIterations);
        // ——— End Unity Editor debug stuff ———
        
        return stateValueFunctionV;
    }

    public Policy PolicyImprovement(
        MDP                mdp, 
        StateValueFunction stateValueFunction, 
        float              gamma,
        bool               debugMode = false)
    {
        var policyPrime = new Policy();
        
        var actionValueFunctionQ = new ActionValueFunction();
        
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            foreach (var action in state.ApplicableActions)
            {
                // LINQ VERSION
                // var stateActionValueQsa = (
                //     from transition in mdp.TransitionFunction(state, action.Action) 
                //     let probability = transition.Probability 
                //     let nextState = mdp.States[transition.SuccessorStateIndex]
                //     let reward = transition.Reward
                //     let valueSprime = stateValueFunction.Value(nextState)
                //     select probability * (reward + gamma * valueSprime * ZeroIfTerminal(nextState))
                //     ).Sum();
                
                
                var stateActionValueQsa = 0.0f;

                // foreach (var transition in action.Transitions) <—— Precomputed transitions.
                foreach (var transition in mdp.TransitionFunction(state, action.Action))
                {
                    float probability = transition.Probability;
                    var     nextState = mdp.States[transition.SuccessorStateIndex];
                    float      reward = transition.Reward;
                    float valueSprime = stateValueFunction.Value(nextState);
                    
                    stateActionValueQsa +=
                        probability * (reward + gamma * valueSprime * ZeroIfTerminal(nextState));
                }
                
                actionValueFunctionQ.SetValue(state, action.Action, stateActionValueQsa);
            }

            GridAction argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
            
            policyPrime.SetAction(state, argMaxAction);
        }

        // todo remove after debug
        editorDisplayPolicy = policyPrime.PolicyToStringArray(mdp.States);
        
        if (debugMode) GenerateDebugInformation(mdp, policyPrime);

        return policyPrime;
    }

    public (StateValueFunction, Policy) PolicyIteration(
        MDP    mdp,
        Policy policy          = null, 
        float  gamma           = 1f, 
        float  theta           = 1e-10f,
        bool   boundIterations = true, 
        int    maxIterations   = 10000,
        bool   debugMode       = false)
    {
        var kIterations = 0;
        
        StateValueFunction valueOfPolicy;

        var newPolicy = policy ?? new Policy(mdp.StateCount);
        
        while (true)
        {
            var oldPolicy = newPolicy.Copy();
            valueOfPolicy = PolicyEvaluation(mdp, newPolicy, gamma, theta, boundIterations, maxIterations, debugMode);
            newPolicy     = PolicyImprovement(mdp, valueOfPolicy, gamma, debugMode);
            if (oldPolicy.Equals(newPolicy)) break;
            if (boundIterations && (kIterations >= maxIterations)) break;
            kIterations++;
        }
        
        if (debugMode) Debug.Log($"Num Iterations: {kIterations}");
        
        return (valueOfPolicy, newPolicy);
    }

    public (StateValueFunction, Policy) ValueIteration(
        MDP    mdp,
        float  gamma           = 1f, 
        float  theta           = 1e-10f,
        bool   boundIterations = true, 
        int    maxIterations   = 10000,
        bool   debugMode       = false)
    {
        var kIterations = 0;

        var stateValueFunctionV = new StateValueFunction(mdp);
        
        ActionValueFunction actionValueFunctionQ;
        
        while (true)
        {
            float delta = 0;
            
            actionValueFunctionQ = new ActionValueFunction(mdp);
            
            foreach (var state in mdp.States.Where(state => state.IsStandard()))
            {

                float valueBeforeUpdate = stateValueFunctionV.Value(state); 
                
                foreach (var action in state.ApplicableActions)
                {
                    var stateActionValueQsa = 0.0f;

                    // foreach (var transition in action.Transitions) <—— Precomputed transitions.
                    foreach (var transition in mdp.TransitionFunction(state, action))
                    {
                        float probability = transition.Probability;
                        var     nextState = mdp.States[transition.SuccessorStateIndex];
                        float      reward = transition.Reward;
                        float valueSprime = stateValueFunctionV.Value(nextState);
                    
                        stateActionValueQsa +=
                            probability * (reward + gamma * valueSprime * ZeroIfTerminal(nextState));
                    }
                    actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
                }

                float valueOfState = actionValueFunctionQ.MaxValue(state);
                        
                stateValueFunctionV.SetValue(state, valueOfState);

                delta = Math.Max(delta, Math.Abs(valueBeforeUpdate - valueOfState));

            }
            
            if (delta < theta) 
            {
                if (debugMode) Debug.Log($"delta: {delta} theta: {theta}");
                break;
            }
            
            if (boundIterations & (kIterations >= maxIterations))
            {
                if (debugMode) Debug.Log($"delta: {delta} theta: {theta}");
                break;
            }

            kIterations++;
        }

        var policy = new Policy();

        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            policy.SetAction(state, actionValueFunctionQ.ArgMaxAction(state));
        }
        
        UpdateGameObjectFields(mdp, stateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, stateValueFunctionV, kIterations);
        
        return (stateValueFunctionV, policy);
    }

    public float ZeroIfTerminal(MarkovState state)
    {
        return (state.IsTerminal() || state.IsGoal() || state.IsObstacle()) ? 0 : 1;
    }

    private static void GenerateDebugInformation(MDP mdp, StateValueFunction stateValueFunctionV, int kIterations)
    {
        Debug.Log("——————————————————————————");
        foreach (var state in mdp.States)
        {
            Debug.Log($"V({state}) = {stateValueFunctionV.Value(state)}");
        }

        Debug.Log(kIterations);
        Debug.Log("——————————————————————————");
    }

    private static void GenerateDebugInformation(MDP mdp, Policy policy)
    {
        string[] stringRepresentationOfPolicy = policy.PolicyToStringArray(mdp.States);
            
        Debug.Log("——————————————————————————"); 
        foreach (var state in mdp.States)
        {
            Debug.Log($"π({state}) = {stringRepresentationOfPolicy[state.StateIndex]}");
        }
        Debug.Log("——————————————————————————");
    }

    private void UpdateGameObjectFields(MDP mdp, StateValueFunction stateValueFunctionV, int kIterations)
    {
        editorDisplayStateValue = stateValueFunctionV.StateValuesToFloatArray(mdp.StateCount);

        evaluationIterations = kIterations;
    }

    private void Awake()
    {
        // mdp = JsonUtility.FromJson<MDP>(File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Cheeky Test stuff Todo Remove
        if (loadMdp)
        {

            if (russellMdp)
            {
                globalMdp = MdpAdmin.LoadMdp(File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
            }

            if (frozenMdp)
            {
                globalMdp = MdpAdmin.LoadMdp(
                    File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json"));
            }

            if (myLittleMdp)
            {
                globalMdp = MdpAdmin.LoadMdp(
                    File.ReadAllText("Assets/Resources/TestMDPs/LittleTestWorldTest.json"));
            }
            
            
            loadMdp = false;
        }
        
        var optimalPolicy = new Policy();
        var myMistakePolicy = new Policy();
        var mistakePolEvaluated = new StateValueFunction();
        
        if (runPolicyEvaluation)
        {
            var Left  = GridAction.Left;
            var Down  = GridAction.Down;
            var Right = GridAction.Right;
            var Up    = GridAction.Up;

            
            // gamma = 1.0f;
            // theta = 1E-10f;
            
            if (russellMdp)
            {

                optimalPolicy.SetAction(globalMdp.States[8],  Right);optimalPolicy.SetAction(globalMdp.States[9],  Right);optimalPolicy.SetAction(globalMdp.States[10], Right);
                optimalPolicy.SetAction(globalMdp.States[4],     Up);                                               optimalPolicy.SetAction(globalMdp.States[6],     Up);
                optimalPolicy.SetAction(globalMdp.States[0],     Up);optimalPolicy.SetAction(globalMdp.States[1],   Left);optimalPolicy.SetAction(globalMdp.States[2],   Left);optimalPolicy.SetAction(globalMdp.States[3],   Left);
                
                myMistakePolicy.SetAction(globalMdp.States[8],  Left);myMistakePolicy.SetAction(globalMdp.States[9],  Right);myMistakePolicy.SetAction(globalMdp.States[10], Right);
                myMistakePolicy.SetAction(globalMdp.States[4],     Up);                                                 myMistakePolicy.SetAction(globalMdp.States[6],     Up);
                myMistakePolicy.SetAction(globalMdp.States[0],     Up);myMistakePolicy.SetAction(globalMdp.States[1],   Left);myMistakePolicy.SetAction(globalMdp.States[2],   Left);myMistakePolicy.SetAction(globalMdp.States[3],   Left);
            }

            if (frozenMdp)
            {
                // PolicyDictDeprecated = new Dictionary<int, GridAction>
                // {
                //     {12, Right},{13,  Left},{14,  Down},{15,    Up},
                //     { 8,  Left},{ 9, Right},{10, Right},{11,  Down},
                //     { 4,    Up},{ 5,  Down},{ 6,    Up},{ 7,  Down},
                //     { 0,    Up},{ 1, Right},{ 2, Down },{ 3, Left },
                //
                // };
            }

            if (myLittleMdp)
            {
                myMistakePolicy = new Policy();
                myMistakePolicy.SetAction(0, Down);
                myMistakePolicy.SetAction(3, Left);
                myMistakePolicy.SetAction(4, Left);
                myMistakePolicy.SetAction(5, Up);
            }
            
            
            mistakePolEvaluated = PolicyEvaluation(this.globalMdp, myMistakePolicy, this.discountFactorGamma, this.thresholdTheta);
            runPolicyEvaluation = false;
        }

        if (runPolicyImprovement)
        {
            var improved = PolicyImprovement(globalMdp, mistakePolEvaluated, discountFactorGamma);
            runPolicyImprovement = false;
            var valueImproved = PolicyEvaluation(globalMdp, improved, discountFactorGamma, thresholdTheta);

            stateValue = valueImproved.StateValuesToFloatArray(globalMdp.StateCount);
        }
        
    }
    
    // ╔══════════════════════════════════════╗
    // ║ DEPRECATED CODE / CODE FOR LATER USE ║
    // ╚══════════════════════════════════════╝
    
    // CURRENTLY NOT IN USE. KEEPING THIS TO POTENTIALLY SHOW THE DIFFERENCE IN COMPLEXITY TO STUDENTS
    public float[] PolicyEvaluationTwoArrays(MDP mdp, Policy policy, float gamma, float theta)
    {
        evaluationIterations = 0;

        previousStateValue = new float[mdp.StateCount];

        while (true)
        {
            float delta   = 0;

            stateValue = new float[mdp.StateCount];
            
            foreach (var state in mdp.States)
            {
                switch (state.TypeOfState)
                {
                    case StateType.Obstacle:
                        break;
                    
                    case StateType.Terminal:
                    case StateType.Goal:
                          stateValue[state.StateIndex] = state.Reward; // 2 arrays version
                          break;
                    
                    default:
                    {
                        float v2Arrays = previousStateValue[state.StateIndex]; // 2 arrays version

                        // Σ P(s'|s,a) [ R(s') + 𝛄 • V(s') ]
                        float valueOfCurrentState2Arrays = 0;

                        int   currentState = state.StateIndex;
                        
                        // foreach (var transition in P(state, policy.GetAction(state))) <—— Precomputed transitions.
                        foreach (var transition in mdp.TransitionFunction(state, policy.GetAction(state)))
                        {
                            
                            float     probability = transition.Probability;
                            int         nextState = transition.SuccessorStateIndex;
                            float          reward = transition.Reward;
                            float         vSprime = previousStateValue[nextState]; // 2 Arrays version

                            float  zeroIfTerminal = ZeroIfTerminal(mdp.States[nextState]);

                            //                          P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
                             valueOfCurrentState2Arrays += probability * (reward + gamma * vSprime * zeroIfTerminal); // 2 Arrays version
                        }

                        delta   = Math.Max(delta  , Math.Abs(v2Arrays - valueOfCurrentState2Arrays));

                        stateValue[currentState] = valueOfCurrentState2Arrays; // 2 Arrays version

                        break;
                    }
                }
            }
            
            if (delta <= theta) 
            {
                Debug.Log($"delta: {delta} theta: {theta}");
                break;
            }
            
            // Two array approach. See Sutton & Barto Chp 4.1
            stateValue.CopyTo(previousStateValue, 0); // 2 Arrays version

            if (evaluationIterations >= 1000) break;  // 
            
            evaluationIterations++;
        }
        
        
        Debug.Log(evaluationIterations);
        return stateValue;
    }
    
    private float OneStepUpdate(float prob, float reward, float gamma, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + gamma * vSprime * zeroIfTerm);
    }
    
    public static float MaxAbsoluteDifference(float[] prevValue, float[] value)
    {
        var absoluteDifferences = new float[value.Length];
        
        for (var i = 0; i < value.Length; i++)
        {
            absoluteDifferences[i] = Math.Abs(prevValue[i] - value[i]);
        }
        return absoluteDifferences.Max();
    }
}