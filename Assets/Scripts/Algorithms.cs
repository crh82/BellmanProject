using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;
using Object = System.Object;

// using Debug = System.Diagnostics.Debug;
public class Algorithms : MonoBehaviour
{

    [FormerlySerializedAs("mdp")] public MDP globalMdp;

    public Policy currentPolicy;
    
    public string[] editorDisplayPolicy;
    public float[] editorDisplayStateValue;

    // Two array approach. See Sutton & Barto Chp 4.1
    public StateValueFunction stateValue;        // 2 Arrays version
    public StateValueFunction previousStateValue;// 2 Arrays version
    
    // In-place approach. See Sutton & Barto 4.1. Can converge faster.
    public Dictionary<int, float> ValueOfState; // In-place version

    public float discountFactorGamma;

    public double thresholdTheta = 1e-10;

    [FormerlySerializedAs("iterations")] public int evaluationIterations;

    
    // ┌───────────────────┐
    // │ Policy Evaluation │
    // └───────────────────┘
    
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

            stateValueFunctionV.Iterations++;
        }

        // For testing and debugging, updates the Unity Editor stateValue array.
        UpdateGameObjectFields(mdp, stateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, stateValueFunctionV, kIterations);
        // ——— End Unity Editor debug stuff ———
        
        return stateValueFunctionV;
    }

    public IEnumerator<StateValueFunction> PolicyEvaluationCR(
        MDP mdp,
        Policy policy,
        float gamma,
        float theta,
        bool boundIterations = true,
        int maxIterations = 10000,
        bool debugMode = false)
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

            stateValueFunctionV.Iterations++;

            yield return null;
        }

        // For testing and debugging, updates the Unity Editor stateValue array.
        UpdateGameObjectFields(mdp, stateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, stateValueFunctionV, kIterations);
        // ——— End Unity Editor debug stuff ———
        
        yield return stateValueFunctionV;
    }
    
    public StateValueFunction SingleStateSweep(
        MDP                mdp, 
        Policy             policy, 
        float              gamma,
        StateValueFunction stateValueFunctionV)
    {
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            float valueOfState = BellmanBackUpValueOfState(mdp, policy, gamma, state, stateValueFunctionV);
            
            stateValueFunctionV.SetValue(state, valueOfState);
        }
        
        stateValueFunctionV.Iterations++;
        
        return stateValueFunctionV;
    }
    

    public float BellmanBackUpValueOfState(
        MDP                mdp, 
        Policy             policy, 
        float              gamma, 
        MarkovState        state,
        StateValueFunction stateValueFunctionV)
    {
        float valueOfState = 0; // Sutton & Barto
        
        // float valueOfState = state.Reward; // Russell & Norvig
        
        var action = policy.GetAction(state);
        
        foreach (var transition in mdp.TransitionFunction(state, action))
        {
            float          probability = transition.Probability;
            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
            float               reward = transition.Reward;
            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
            float       zeroIfTerminal = ZeroIfTerminal(successorState);

            // Sutton & Barto
            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
            valueOfState += SingleTransitionBackup(probability, reward, gamma,valueOfSuccessor, zeroIfTerminal); 
            
            // Russell & Norvig
            // valueOfState += gamma * probability * valueOfSuccessor;
        }

        return valueOfState;
    }
    
    public float SingleTransitionBackup(float prob, float reward, float gamma, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + gamma * vSprime * zeroIfTerm);
    }
    
    // ┌────────────────────┐
    // │ Policy Improvement │
    // └────────────────────┘
    
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

    
    // ┌──────────────────┐
    // │ Policy Iteration │
    // └──────────────────┘
    
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
    
    // ┌─────────────────┐
    // │ Value Iteration │
    // └─────────────────┘

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

            stateValueFunctionV.Iterations++;
        }

        var policy = GeneratePolicyFromArgMaxActions(mdp, actionValueFunctionQ);

        UpdateGameObjectFields(mdp, stateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, stateValueFunctionV, kIterations);
        
        return (stateValueFunctionV, policy);
    }

    public Policy GeneratePolicyFromArgMaxActions(MDP mdp, ActionValueFunction actionValueFunctionQ)
    {
        var policy = new Policy();

        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            policy.SetAction(state, actionValueFunctionQ.ArgMaxAction(state));
        }

        return policy;
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
    
    // ╔═════════════════════════╗
    // ║ Asynchronous Algorithms ║
    // ╚═════════════════════════╝
    
    public Task<StateValueFunction> SingleStateSweepAsync(
        MDP                mdp, 
        Policy             policy, 
        float              gamma,
        StateValueFunction stateValueFunctionV)
    {
        foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
            // foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            // var valueOfState = 0f;
            // float valueOfState = BellmanBackUpValueOfState(mdp, policy, gamma, state, stateValueFunctionV);
            var action = policy.GetAction(state);
            float valueOfState = (
                from transition in mdp.TransitionFunction(state, action) 
                let probability = transition.Probability 
                let successorState = mdp.States[transition.SuccessorStateIndex] 
                let reward = transition.Reward 
                let valueOfSuccessor = stateValueFunctionV.Value(successorState) 
                let zeroIfTerminal = ZeroIfTerminal(successorState) 
                select probability * (reward + gamma * valueOfSuccessor * zeroIfTerminal)
            ).Sum();
            
            stateValueFunctionV.SetValue(state, valueOfState);
        }
        
        stateValueFunctionV.Iterations++;
        
        return Task.FromResult(stateValueFunctionV);
    }
    
    // public Task<StateValueFunction> SingleStateSweepAsync(
    //     MDP                mdp, 
    //     Policy             policy, 
    //     float              gamma,
    //     StateValueFunction stateValueFunctionV)
    // {
    //     foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
    //     // foreach (var state in mdp.States.Where(state => state.IsStandard()))
    //     {
    //         float valueOfState = BellmanBackUpValueOfState(mdp, policy, gamma, state, stateValueFunctionV);
    //         
    //         stateValueFunctionV.SetValue(state, valueOfState);
    //     }
    //     
    //     stateValueFunctionV.Iterations++;
    //     
    //     return Task.FromResult(stateValueFunctionV);
    // }
    
    public Task<float> BellmanBackUpValueOfStateAsync(
        MDP                mdp, 
        Policy             policy, 
        float              gamma, 
        MarkovState        state,
        StateValueFunction stateValueFunctionV)
    {
        float valueOfState = 0;

        var action = policy.GetAction(state);
        
        foreach (var transition in mdp.TransitionFunction(state, action))
        {
            float          probability = transition.Probability;
            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
            float               reward = transition.Reward;
            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
            float       zeroIfTerminal = ZeroIfTerminal(successorState);

            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
            valueOfState += SingleTransitionBackup(probability, reward, gamma,valueOfSuccessor, zeroIfTerminal);
        }

        return Task.FromResult(valueOfState);
    }
    
    public Task<float> BellmanBackUpAsync(
        MDP                mdp,
        GridAction         action,
        float              gamma, 
        MarkovState        state,
        StateValueFunction stateValueFunctionV)
    {
        var value = 0.0f;

        foreach (var transition in mdp.TransitionFunction(state, action))
        {
            float          probability = transition.Probability;
            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
            float               reward = transition.Reward;
            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
            float       zeroIfTerminal = ZeroIfTerminal(successorState);

            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
            value += SingleTransitionBackup(probability, reward, gamma,valueOfSuccessor, zeroIfTerminal);
        }

        return Task.FromResult(value);
    }
    
    public Task<float> SingleTransitionBackupAsync(MDP mdp, float gamma, MarkovTransition transition, StateValueFunction stateValueFunction)
    {
        float          probability = transition.Probability;
        MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
        float               reward = transition.Reward;
        float     valueOfSuccessor = stateValueFunction.Value(successorState);
        float       zeroIfTerminal = successorState.IsTerminal() ? 0 : 1;
        
        return Task.FromResult(probability * (reward + gamma * valueOfSuccessor * zeroIfTerminal));
    }

    public Task<float> IncrementValueAsync(float currentStateValue, float valueFromSuccessor)
    {
        return Task.FromResult(currentStateValue + valueFromSuccessor);
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
        
    }
    
    // ╔══════════════════════════════════════╗
    // ║ DEPRECATED CODE / CODE FOR LATER USE ║
    // ╚══════════════════════════════════════╝
    
    // CURRENTLY NOT IN USE. KEEPING THIS TO POTENTIALLY SHOW THE DIFFERENCE IN COMPLEXITY TO STUDENTS
    public StateValueFunction PolicyEvaluationTwoArrays(
        MDP    mdp, 
        Policy policy, 
        float  gamma, 
        float  theta,
        bool   boundIterations = true, 
        int    maxIterations   = 10000,
        bool   debugMode       = false)
    {
        var kIterations = 0;

        var previousStateValueFunctionV = new StateValueFunction(mdp);
        
        while (true)
        {
            float delta   = 0;
            
            var stateValueFunctionV = new StateValueFunction(mdp);
            
            stateValue = new StateValueFunction(mdp);
            
            foreach (var state in mdp.States.Where(state => state.IsStandard()))
            {
                
                float v2Arrays = previousStateValueFunctionV.Value(state); // 2 arrays version

                // Σ P(s'|s,a) [ R(s') + 𝛄 • V(s') ]
                float valueOfCurrentState2Arrays = 0;

                int   currentState = state.StateIndex;
                        
                // foreach (var transition in P(state, policy.GetAction(state))) <—— Precomputed transitions.
                foreach (var transition in mdp.TransitionFunction(state, policy.GetAction(state)))
                {
                            
                    float     probability = transition.Probability;
                    int         nextState = transition.SuccessorStateIndex;
                    float          reward = transition.Reward;
                    float         vSprime = previousStateValueFunctionV.Value(nextState); // 2 Arrays version

                    float  zeroIfTerminal = ZeroIfTerminal(mdp.States[nextState]);

                    //                          P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
                    valueOfCurrentState2Arrays += probability * (reward + gamma * vSprime * zeroIfTerminal); // 2 Arrays version
                }

                delta   = Math.Max(delta, Math.Abs(v2Arrays - valueOfCurrentState2Arrays));

                stateValueFunctionV.SetValue(currentState, valueOfCurrentState2Arrays); // 2 Arrays version
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
            
            // Two array approach. See Sutton & Barto Chp 4.1
            previousStateValueFunctionV = stateValueFunctionV;
            
            kIterations++;
        }
        
        // For testing and debugging, updates the Unity Editor stateValue array.
        UpdateGameObjectFields(mdp, previousStateValueFunctionV, kIterations);

        if (debugMode) GenerateDebugInformation(mdp, previousStateValueFunctionV, kIterations);
        // ——— End Unity Editor debug stuff ———
        
        Debug.Log(kIterations);

        previousStateValueFunctionV.Iterations = kIterations;
        
        return previousStateValueFunctionV;
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