using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using PlasticGui.WorkspaceWindow.Items;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
// using Debug = System.Diagnostics.Debug;
using Vector2 = System.Numerics.Vector2;

public class Algorithms : MonoBehaviour
{
    // Cheeky Test stuff Todo Remove
    public bool frozenMdp;
    public bool russellMdp;
    public bool loadMdp;
    public bool runPolicyEvaluation;
    public bool inPlaceVersion;
    
    public MDP mdp;

    public Policy currentPolicy;
    
    
    public Dictionary<int, GridAction> Policy;
    // public List<GridAction> policy;
    public List<GridAction> previousPolicy;

    // Two array approach. See Sutton & Barto Chp 4.1
    public float[] stateValue;        // 2 Arrays version
    public float[] previousStateValue;// 2 Arrays version
    
    // In-place approach. See Sutton & Barto 4.1. Can converge faster.
    public Dictionary<int, float> ValueOfState; // In-place version

    public float discountFactorGamma;

    public float epsilon;

    public float thresholdTheta;

    public int iterations;

    public GridAction Pi(MarkovState state)
    {
        return Policy[state.StateIndex];
    }

    public StateValueFunction PolicyEvaluation(MDP mdp, Policy policy, float gamma, float theta)
    {
        iterations         = 0;

        var V = new StateValueFunction();

        previousStateValue = new float[mdp.StateCount];

        while (true)
        {
            float delta   = 0;
            float deltaIP = 0;
            
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
                          V.SetValue(state, state.Reward); // In-place version
                        break;
                    
                    default:
                    {
                        float v2Arrays = previousStateValue[state.StateIndex]; // 2 arrays version
                        float vInPlace = V.Value(state);
                        
                        // Σ P(s'|s,a) [ R(s') + 𝛄 • V(s') ]
                        float valueOfCurrentState2Arrays = 0;
                        float valueOfCurrentStateInPlace = 0;
                        
                        int   currentState = state.StateIndex;
                        
                        foreach (var transition in P(state, policy.Pi(state)))
                        {
                            
                            float     probability = transition.Probability;
                            int         nextState = transition.SuccessorStateIndex;
                            float          reward = transition.Reward;  // Todo Should I make this the reward for arriving in the next state or the reward for the current state?

                            float  vSprime2Arrays = previousStateValue[nextState]; // 2 Arrays version
                            float  vSprimeInPlace = V.Value(mdp.States[nextState]); // In-Place version
                            
                            float  zeroIfTerminal = ZeroIfTerminal(nextState);

                            //                          P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
                             valueOfCurrentState2Arrays += probability * (reward + gamma * vSprime2Arrays * zeroIfTerminal); // 2 Arrays version
                             valueOfCurrentStateInPlace += probability * (reward + gamma * vSprimeInPlace * zeroIfTerminal); // In-Place version
                        }

                        delta   = Math.Max(delta  , Math.Abs(v2Arrays - valueOfCurrentState2Arrays));
                        deltaIP = Math.Max(deltaIP, Math.Abs(vInPlace - valueOfCurrentStateInPlace));
                        
                        stateValue[currentState] = valueOfCurrentState2Arrays; // 2 Arrays version
                        V.SetValue(state, valueOfCurrentStateInPlace);         // In-Place version
                        
                        break;
                    }
                }
            }

            // if (MaxAbsoluteDifference(previousStateValue, stateValue) < theta) break;
            if (delta < theta)
            {
                Debug.Log($"delta: {delta} deltaIP: {deltaIP}");
                break;
            }
            
            if (inPlaceVersion & (deltaIP < theta)) // Todo Change this as its currently in a hackish form for testing inside the Unity editor
            {
                Debug.Log($"delta: {delta} deltaIP: {deltaIP}");
                break;
            }
            
            // Two array approach. See Sutton & Barto Chp 4.1
            stateValue.CopyTo(previousStateValue, 0); // 2 Arrays version

            if (iterations >= 1000) break;
            
            iterations++;
        }
        
        foreach (var state in mdp.States)
        {
            Debug.Log($"V({state}) = {V.Value(state)}");
        }
        
        Debug.Log(iterations);
        return V;
    }
    

    public Policy PolicyImprovement(StateValueFunction stateValueFunction, Policy policy, float gamma)
    {
        var policyPrime = new Policy();
        
        var actionValueFunctionQ = new ActionValueFunction();
        
        foreach (var state in mdp.States)
        {
            foreach (var action in state.ApplicableActions)
            {
                // LINQ VERSION
                // float stateActionValueQ = (
                //     from transition in action.Transitions 
                //     let probability = transition.Probability 
                //     let nextState = mdp.States[transition.SuccessorStateIndex] 
                //     let reward = transition.Reward 
                //     let valueSprime = stateValueFunction.Value(nextState) 
                //     select probability * (reward + gamma * valueSprime * ZeroIfTerminal(nextState.StateIndex))).Sum();

                var stateActionValueQ = 0.0f;
                
                foreach (var transition in action.Transitions)
                {
                    float probability = transition.Probability;
                    var     nextState = mdp.States[transition.SuccessorStateIndex];
                    float      reward = transition.Reward;
                    float valueSprime = stateValueFunction.Value(nextState);
                    
                    stateActionValueQ +=
                        probability * (reward + gamma * valueSprime * ZeroIfTerminal(nextState.StateIndex));
                }
                
                actionValueFunctionQ.SetValue(state, action.Action, stateActionValueQ);
            }

            GridAction argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
            
            policyPrime.SetAction(state, argMaxAction);
        }
        
        return policyPrime;
    }

    private float OneStepLookAhead(float prob, float reward, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + discountFactorGamma * vSprime * zeroIfTerm);
    }

    public float ZeroIfTerminal(int stateIndex)
    {
        MarkovState state = mdp.States[stateIndex];
        return (state.IsTerminal() || state.IsGoal() || state.IsObstacle()) ? 0 : 1;
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

    public List<MarkovTransition> P(MarkovState state, GridAction action)
    {
        return state.ApplicableActions[(int) action].Transitions;
    }

    // DEPRECATED
    public Dictionary<int, float> InitializeStateValueDictionary(
        List<MarkovState> states, 
        bool randomValues = false,
        float[] specificValues = null)
    {
        // Todo implement the assigning random or specific values feature.
        
        var stateValueDict = new Dictionary<int, float>();
        
        foreach (var state in states)
        {
            stateValueDict[state.StateIndex] = 0f;
        }

        return stateValueDict;
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
                mdp = MdpAdmin.LoadMdp(File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
            }

            if (frozenMdp)
            {
                mdp = MdpAdmin.LoadMdp(
                    File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json"));
            }
            
            
            loadMdp = false;
        }
        
        if (runPolicyEvaluation)
        {
            var Left  = GridAction.Left;
            var Down  = GridAction.Down;
            var Right = GridAction.Right;
            var Up    = GridAction.Up;

            Policy optimalPolicy = new Policy();
            // gamma = 1.0f;
            // theta = 1E-10f;
            
            if (russellMdp)
            {

                optimalPolicy.SetAction(mdp.States[8],  Right);optimalPolicy.SetAction(mdp.States[9],  Right);optimalPolicy.SetAction(mdp.States[10], Right);
                optimalPolicy.SetAction(mdp.States[4],     Up);                                               optimalPolicy.SetAction(mdp.States[6],     Up);
                optimalPolicy.SetAction(mdp.States[0],     Up);optimalPolicy.SetAction(mdp.States[1],   Left);optimalPolicy.SetAction(mdp.States[2],   Left);optimalPolicy.SetAction(mdp.States[3],   Left);
                
                
                
                
                
                
                Policy = new Dictionary<int, GridAction>
                {
                    { 8,  Right},{ 9, Right},{10, Right},
                    { 4,    Up},            { 6,    Up},
                    { 0,    Up},{ 1,  Left},{ 2, Left },{ 3, Left },
                };
            }

            if (frozenMdp)
            {
                Policy = new Dictionary<int, GridAction>
                {
                    {12, Right},{13,  Left},{14,  Down},{15,    Up},
                    { 8,  Left},{ 9, Right},{10, Right},{11,  Down},
                    { 4,    Up},{ 5,  Down},{ 6,    Up},{ 7,  Down},
                    { 0,    Up},{ 1, Right},{ 2, Down },{ 3, Left },
                
                };
            }
            
            
            PolicyEvaluation(this.mdp, optimalPolicy, this.discountFactorGamma, this.thresholdTheta);
            runPolicyEvaluation = false;
        }
    }
}


public class StateValueFunction
{
    private readonly Dictionary<int, float> _valueOfAState = new Dictionary<int, float>();

    public void SetValue(MarkovState state, float valueOfState)
    {
        switch (_valueOfAState.ContainsKey(state.StateIndex))
        {
            case true:
                _valueOfAState[state.StateIndex] = valueOfState;
                break;
            case false:
                _valueOfAState.Add(state.StateIndex, valueOfState);
                break;
        }
    }

    public float Value(MarkovState state)
    {
        switch (_valueOfAState.ContainsKey(state.StateIndex))
        {
            case true:
                return _valueOfAState[state.StateIndex];
            case false:
                SetValue(state, 0);
                return 0;
        }
    }
}

public class ActionValueFunction
{
    // private readonly Dictionary<string, float> _valueOfQGivenSandA = new Dictionary<string, float>();

    private readonly Dictionary<int, Dictionary<GridAction, float>> _valueOfQGivenSandA = new Dictionary<int, Dictionary<GridAction, float>>();
    
    public void SetValue(MarkovState state, GridAction action, float valueOfActionInState)
    {
        if (_valueOfQGivenSandA.ContainsKey(state.StateIndex))
        {
            var stateActions = _valueOfQGivenSandA[state.StateIndex];
            
            if (stateActions.ContainsKey(action))
            {
                _valueOfQGivenSandA[state.StateIndex][action] = valueOfActionInState;
            }
            else
            {
                _valueOfQGivenSandA[state.StateIndex].Add(action, valueOfActionInState);
            }
        }
        else
        {
            var stateActions = new Dictionary<GridAction, float> {{action, valueOfActionInState}};
            _valueOfQGivenSandA.Add(state.StateIndex, stateActions);
        }
        
        // var stateActionValueKey = $"{state}{action}";
        // switch (_valueOfQGivenSandA.ContainsKey(stateActionValueKey))
        // {
        //     case true:
        //         _valueOfQGivenSandA[stateActionValueKey] = valueOfActionInState;
        //         break;
        //     default:
        //         _valueOfQGivenSandA.Add(stateActionValueKey, valueOfActionInState);
        //         break;
        // }
    }

    public float Value(MarkovState state, GridAction action)
    {
        // var stateActionValueKey = $"{state}{action}";
        
        switch (_valueOfQGivenSandA.ContainsKey(state.StateIndex))
        {
            case true:
                switch (_valueOfQGivenSandA[state.StateIndex].ContainsKey(action))
                {
                    case true:
                        return _valueOfQGivenSandA[state.StateIndex][action];
                    default:
                        SetValue(state, action, 0f);
                        Debug.Log($"ActionValueFunction tried to access an uninitialized value. " +
                                  $"{state} was present but {action} was not. " +
                                  $"Stored Q({state},{action}) = {0.0f} instead. " +
                                  $"Check for unintended consequences"); // Todo remove once stable
                        return 0;
                }
            default:
                SetValue(state, action, 0f);
                Debug.Log($"ActionValueFunction tried to access an uninitialized value. " +
                          $"Neither {state} nor {action} were present. " +
                          $"Stored Q({state},{action}) = {0.0f} instead. " +
                          $"Check for unintended consequences"); // Todo remove once stable
                return 0;
        }
    }

    public GridAction ArgMaxAction(MarkovState state)
    {
        float maxStateActionValue = MaxValue(state);
        // var argMaxAction = _valueOfQGivenSandA[state.StateIndex].Where(kvp => kvp.Value == maxStateActionValue).First().Key;
        return _valueOfQGivenSandA[state.StateIndex].First(kvp => Math.Abs(kvp.Value - maxStateActionValue) < 1e-15).Key;
    }

    public float MaxValue(MarkovState state)
    {
        return _valueOfQGivenSandA[state.StateIndex].Max(keyValuePair => keyValuePair.Value);
    }
}


public class Policy
{
    private readonly Dictionary<int, GridAction> _policy = new Dictionary<int, GridAction>();

    // CONSTRUCTORS
    public Policy()
    {
    }
    
    public Policy(MDP mdp)
    {
        Array actions = Enum.GetValues(typeof(GridAction));
        
        foreach (var state in mdp.States)
        {
            if (state.TypeOfState == StateType.Standard)
            {
                SetAction(state, (GridAction) actions.GetValue(Random.Range(0,5)));
            }
        }
    }

    public void SetAction(MarkovState state, GridAction action)
    {
        switch (_policy.ContainsKey(state.StateIndex))
        {
            case true:
                _policy[state.StateIndex] = action;
                break;
            case false:
                _policy.Add(state.StateIndex, action);
                break;
        }
    }

    public GridAction Pi(MarkovState state)
    {
        return _policy.ContainsKey(state.StateIndex) switch
        {
            true  => _policy[state.StateIndex],
            false => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    private sealed class PolicyEqualityComparer : IEqualityComparer<Policy>
    {
        public bool Equals(Policy x, Policy y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return Equals(x._policy, y._policy);
        }

        public int GetHashCode(Policy obj)
        {
            return (obj._policy != null ? obj._policy.GetHashCode() : 0);
        }
    }

    public static IEqualityComparer<Policy> PolicyComparer { get; } = new PolicyEqualityComparer();
}
