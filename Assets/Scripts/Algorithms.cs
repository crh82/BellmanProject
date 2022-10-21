using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Originally this Algorithms class handled all algorithm related computation. However, during development that became
/// unworkable given that to control the execution speed I needed to be able to have the inner works of the algorithms
/// accessing the user interface and global variables. As a result, aside from the Bellman backups themselves, I transitioned
/// the main algorithms to the MdpManager class. See <see cref="MdpManager"/>
/// </summary>
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
    
    /// <summary>
    /// Original implementation of policy evaluation used for early testing. Now migrated to the MdpManager.
    /// </summary>
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

   

    // ┌────────────────────┐
    // │ Policy Improvement │
    // └────────────────────┘
    /// <summary>
    /// Original implementation of policy improvement used for early testing. Now migrated to the MdpManager.
    /// </summary>
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
    /// <summary>
    /// Original implementation of policy iteration used for early testing. Now migrated to the MdpManager.
    /// </summary>
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
    /// <summary>
    /// Original implementation of value iteration used for early testing. Now migrated to the MdpManager.
    /// </summary>
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
    
    public StateValueFunction SingleStateSweep(
        MDP                mdp, 
        Policy             policy, 
        float              gamma,
        StateValueFunction stateValueFunctionV)
    {
        foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
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
    /// <summary>
    /// <c>Policy Evaluation</c> in <see cref="MdpManager"/> calls this to asynchronously update a every state in the
    /// state space in a single sweep.
    /// </summary>
    public Task<StateValueFunction> SingleStateSweepAsync(
        MDP                mdp, 
        Policy             policy, 
        float              gamma,
        StateValueFunction stateValueFunctionV)
    {
        foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
        {
            // This LINQ implementation replaces the BellmanBackUpValueOfState method to leverage the performance gains
            // that come from PLINQ (parallel) the on larger state spaces.
            
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
        
        // I've left this commented out to be able to show the differences between the various r(s), r(s,a), and r(s,a,s') formulations.
        // foreach (var state in mdp.States.Where(state => !state.IsObstacle()))
        // {
        //     float valueOfState = state.Reward;
        //     
        //     if (state.IsStandard())
        //     {
        //         var action = policy.GetAction(state);
        //         valueOfState += (
        //             from transition in mdp.TransitionFunction(state, action) 
        //             let p = transition.Probability 
        //             let sPrime = transition.SuccessorStateIndex 
        //             let vsPrime = stateValueFunctionV.Value(sPrime) 
        //             select p * gamma * vsPrime
        //             ).Sum();
        //     }
        //     
        //     stateValueFunctionV.SetValue(state, valueOfState);
        // }
        
        return Task.FromResult(stateValueFunctionV);
    }
    

    /// <summary>
    /// The SingleSweepValueIteration function performs a single sweep of value iteration on the given MDP,
    /// using the given state and action value functions.  The function iterates over all states in the MDP,
    /// performing a Bellman backup on each state to update its value.  The function returns when every standard
    /// state has been updated.
    /// </summary>
    ///
    /// <param name="mdp"> The mdp to solve.</param>
    /// <param name="stateValueFunction">A state value function V(s)</param>
    /// <param name="actionValueFunction">Action Value Function Q(s,a)</param>
    /// <param name="gamma"> </param>
    /// <returns> A task.</returns>
    public async Task SingleSweepValueIteration(
        MDP mdp, 
        StateValueFunction stateValueFunction,
        ActionValueFunction actionValueFunction, 
        float gamma)
    {
        foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
        {
            float valueOfState = await BellmanBackupMaxActionValue(
                mdp, stateValueFunction, actionValueFunction, state, gamma);
            
            stateValueFunction.SetValue(state, valueOfState);
        }
    }
    
    /// <summary>
    /// The BellmanBackupMaxActionValue function calculates the maximum action value for a given state.
    /// </summary>
    public async Task<float> BellmanBackupMaxActionValue(
        MDP                 mdp,                 
        StateValueFunction  stateValueFunction,  
        ActionValueFunction actionValueFunction, 
        MarkovState         state,               
        float               gamma)

    {
        
        foreach (var action in state.ApplicableActions)
        {
            float stateActionValueQsa =
                await CalculateActionValueAsync(mdp, state, action.Action, gamma, stateValueFunction);
            
            actionValueFunction.SetValue(state, action, stateActionValueQsa);
        }

        return actionValueFunction.MaxValue(state);
    }
    
    /// <summary>
    ///  The BellmanBackUpValueOfStateAsync function computes the value of a state, given a policy and an MDP.
    /// </summary>
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

            //           P(s'| s, π(s) )•[  R( s, a, s') +   𝛄   •  V(s') ]
            valueOfState += SingleTransitionBackup(probability, reward, gamma,valueOfSuccessor, zeroIfTerminal);
        }

        return Task.FromResult(valueOfState);
    }
    

    /// <summary>
    /// The CalculateActionValueAsync function calculates the value of a state-action pair. <see cref="MdpManager"/>
    /// calls this during value iteration and policy improvement.
    /// </summary>
    public Task<float> CalculateActionValueAsync(
        MDP                mdp,
        MarkovState        state,
        GridAction         action,
        float              gamma, 
        StateValueFunction stateValueFunction)
    {
        var value = 0.0f;

        foreach (var transition in mdp.TransitionFunction(state, action))
        {
            float          probability = transition.Probability;
            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
            float               reward = transition.Reward;
            float     valueOfSuccessor = stateValueFunction.Value(successorState);
            float       zeroIfTerminal = ZeroIfTerminal(successorState);

            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
            value += SingleTransitionBackup(probability, reward, gamma,valueOfSuccessor, zeroIfTerminal);
        }

        return Task.FromResult(value);
    }
    
    public Task<float> CalculateSingleTransitionAsync(
        MDP mdp, 
        float gamma, 
        MarkovTransition transition, 
        StateValueFunction stateValueFunction)
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