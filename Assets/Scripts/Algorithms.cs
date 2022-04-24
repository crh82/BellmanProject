using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.WSA;
using Object = System.Object;
using Random = UnityEngine.Random;
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

    public int iterations;

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

        var stateValueFunctionV = new StateValueFunction();

        while (true)
        {
            float delta = 0;

            foreach (var state in mdp.States)
            {
                switch (state.TypeOfState)
                {
                    case StateType.Obstacle:
                        break;
                    
                    case StateType.Terminal:
                    case StateType.Goal:
                        stateValueFunctionV.SetValue(state, state.Reward); 
                        break;
                    
                    default:
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
                            float               reward = transition.Reward;  // TODO Should I make this the reward for arriving in the next state or the reward for having been in the current state?
                            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
                            float       zeroIfTerminal = ZeroIfTerminal(successorState);

                            //           P(s'| s, π(s) )•[  R(s') +   𝛄   •  V(s') ]
                            valueOfState += probability * (reward + gamma * valueOfSuccessor * zeroIfTerminal);
                        }

                        stateValueFunctionV.SetValue(state, valueOfState);
                        
                        // Rather than running the L-inf norm
                        delta = Math.Max(delta, Math.Abs(valueBeforeUpdate - valueOfState));
                        
                        break;
                    }
                }
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
        editorDisplayStateValue = stateValueFunctionV.StateValuesToFloatArray(mdp.StateCount);

        iterations = kIterations;
        
        if (debugMode)
        {
            Debug.Log("——————————————————————————");
            foreach (var state in mdp.States)
            {
                Debug.Log($"V({state}) = {stateValueFunctionV.Value(state)}");
            }
            
            Debug.Log(kIterations);
            Debug.Log("——————————————————————————");
        }
        // ——— End Unity Editor debug stuff ———
        
        return stateValueFunctionV;
    }
    
    public float[] PolicyEvaluationTwoArrays(MDP mdp, Policy policy, float gamma, float theta)
    {
        iterations = 0;

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
                            float  vSprime = previousStateValue[nextState]; // 2 Arrays version

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

            if (iterations >= 1000) break;  // 
            
            iterations++;
        }
        
        
        Debug.Log(iterations);
        return stateValue;
    }

    public Policy PolicyImprovement(
        MDP                mdp, 
        StateValueFunction stateValueFunction, 
        float              gamma,
        bool               debugMode = false)
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

                var stateActionValueQsa = 0.0f;
                
                foreach (var transition in action.Transitions)
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
        editorDisplayPolicy = policyPrime.PolicyToStringArray(mdp.States, mdp.StateCount);
        
        if (debugMode)
        {
            Debug.Log("——————————————————————————"); 
            foreach (var state in mdp.States)
            {
                Debug.Log($"π({state}) = {editorDisplayPolicy[state.StateIndex]}");
            }
            Debug.Log("——————————————————————————");
        }
        
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
        
        Debug.Log($"Num Iterations: {kIterations}");
        
        return (valueOfPolicy, newPolicy);
    }
    private float OneStepLookAhead(float prob, float reward, float gamma, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + gamma * vSprime * zeroIfTerm);
    }

    public float ZeroIfTerminal(MarkovState state)
    {
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
}


public class StateValueFunction
{
    private readonly Dictionary<int, float> _valueOfAState = new Dictionary<int, float>();

    public StateValueFunction()
    {
    }

    public StateValueFunction(MDP mdp, float lowerBoundOfValues, float upperBoundOfValues)
    {
        Assert.IsTrue(lowerBoundOfValues < upperBoundOfValues);
        foreach (var state in mdp.States)
        {
            switch (state.TypeOfState)
            {
                case StateType.Terminal:
                case StateType.Obstacle:
                case StateType.Goal:
                    SetValue(state, state.Reward);
                    break;
                case StateType.Standard:
                    SetValue(state, Random.Range(lowerBoundOfValues, upperBoundOfValues));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // if (state.IsObstacle() || state.IsGoal() || state.IsTerminal()) SetValue(state, state.Reward);
            // else SetValue(state, Random.Range(lowerBoundOfValues, upperBoundOfValues));
            
        }
    }
    
    

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
    
    
    public void SetValue(int stateIndex, float valueOfState)
    {
        switch (_valueOfAState.ContainsKey(stateIndex))
        {
            case true:
                _valueOfAState[stateIndex] = valueOfState;
                break;
            case false:
                _valueOfAState.Add(stateIndex, valueOfState);
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
    
    public float Value(int stateIndex)
    {
        switch (_valueOfAState.ContainsKey(stateIndex))
        {
            case true:
                return _valueOfAState[stateIndex];
            case false:
                SetValue(stateIndex, 0);
                return 0;
        }
    }

    /// <summary>
    /// A small method to help with testing and debugging in the Unity Editor. Used to update the Editor visible list
    /// of state values.
    /// </summary>
    /// <param name="stateValueArrayLength"><c>int</c>: Representing the number of states</param>
    /// <returns><c>float[]</c>Updating the Editor visible state value array.</returns>
    public float[] StateValuesToFloatArray(int stateValueArrayLength)
    {
        var newStateValues = new float[stateValueArrayLength];
        
        foreach (var stateValue in _valueOfAState)
        {
            newStateValues[stateValue.Key] = stateValue.Value;
        }
        
        return newStateValues;
    }
}

public class ActionValueFunction
{

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
    }

    public float Value(MarkovState state, GridAction action)
    {

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
    
    // Initializes the policy with random actions
    public Policy(MDP mdp)
    {
        Array actions = Enum.GetValues(typeof(GridAction));
        
        foreach (var state in mdp.States)
        {
            if (state.TypeOfState == StateType.Standard)
            {
                SetAction(state, (GridAction) actions.GetValue(Random.Range(0,4)));
            }
        }
    }
    
    
    // Initializes the policy with random actions, ignoring any terminal or obstacle states
    public Policy(int numberOfStates)
    {
        Array actions = Enum.GetValues(typeof(GridAction));

        for (int stateIndex = 0; stateIndex < numberOfStates; stateIndex++)
        {
            SetAction(stateIndex, (GridAction) actions.GetValue(Random.Range(0,4)));
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
    
    public void SetAction(int stateIndex, GridAction action)
    {
        switch (_policy.ContainsKey(stateIndex))
        {
            case true:
                _policy[stateIndex] = action;
                break;
            case false:
                _policy.Add(stateIndex, action);
                break;
        }
    }
    
    public void SetAction(int stateIndex, string action)
    {
        string cleanedAction = action.ToLower();
        GridAction parsedAction = cleanedAction switch
        {
            "l"     => GridAction.Left,
            "left"  => GridAction.Left,
            "d"     => GridAction.Down,
            "down"  => GridAction.Down,
            "r"     => GridAction.Right,
            "right" => GridAction.Right,
            "u"     => GridAction.Up,
            "up"    => GridAction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };

        switch (_policy.ContainsKey(stateIndex))
        {
            case true:
                _policy[stateIndex] = parsedAction;
                break;
            case false:
                _policy.Add(stateIndex, parsedAction);
                break;
        }
    }

    /// <summary>
    /// A small method to help with testing and debugging in the Unity Editor. Used to update the Editor visible policy.
    /// </summary>
    /// <param name="setOfStates">Represents the state space</param>
    /// <param name="numberOfStates"><c>int</c></param>
    /// <returns>A string array representation of the policy where the actions are indexed by their corresponding state index</returns>
    public string[] PolicyToStringArray(List<MarkovState> setOfStates, int numberOfStates)
    {
        var stringDisplayOfPolicy = new string[numberOfStates];
        for (var stateIndex = 0; stateIndex < numberOfStates; stateIndex++)
        {
            stringDisplayOfPolicy[stateIndex] = setOfStates[stateIndex].TypeOfState switch
            {
                StateType.Standard => _policy[stateIndex].ToString(),
                StateType.Terminal => "Term Act N/A",
                StateType.Obstacle => "Obs Act N/A",
                StateType.Goal     => "Goal Act N/A",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        return stringDisplayOfPolicy;
    }
    
    public GridAction GetAction(MarkovState state)
    {
        return _policy.ContainsKey(state.StateIndex) switch
        {
            true  => _policy[state.StateIndex],
            false => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public Policy Copy()
    {
        var copyOfPolicy = new Policy();
        foreach (var kvp in _policy)
        {
            copyOfPolicy.SetAction(kvp.Key, kvp.Value);
        }
        return copyOfPolicy;
    }


    public bool Equals(Policy policyPrime)
    {
        if (null == policyPrime) 
            return _policy == null;
        if (null == _policy) 
            return false;
        if (ReferenceEquals(_policy, policyPrime._policy)) 
            return true;
        if (_policy.Count != policyPrime._policy.Count) 
            return false;

        foreach (int k in _policy.Keys)
        {
            if (!policyPrime._policy.ContainsKey(k))        return false;
            if (!_policy[k].Equals(policyPrime._policy[k])) return false;
        }

        return true;
    }
}
