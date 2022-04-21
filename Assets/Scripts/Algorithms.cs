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

    public Dictionary<int, GridAction> Policy;
    // public List<GridAction> policy;
    public List<GridAction> previousPolicy;

    // Two array approach. See Sutton & Barto Chp 4.1
    public float[] stateValue;        // 2 Arrays version
    public float[] previousStateValue;// 2 Arrays version
    
    // In-place approach. See Sutton & Barto 4.1. Can converge faster.
    public Dictionary<int, float> ValueOfState; // In-place version

    public float    gamma;

    public float  epsilon;

    public float    theta;

     public int iterations;

    public GridAction Pi(MarkovState state)
    {
        return Policy[state.StateIndex];
    }

    public void PolicyEvaluation()
    {
        iterations         = 0;

        var V = new StateValueFunction();
        ValueOfState = InitializeStateValueDictionary(mdp.States);
        
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
                        // ValueOfState[state.StateIndex] = state.Reward; // In-place version
                          V.SetValue(state, state.Reward); // In-place version
                        break;
                    
                    default:
                    {
                        float v2Arrays = previousStateValue[state.StateIndex]; // 2 arrays version
                        // float vInPlace = ValueOfState[state.StateIndex];       // In-place version
                        float vInPlace = V.Value(state);
                        
                        // Σ P(s'|s,a) [ R(s') + 𝛄 • V(s') ]
                        float valueOfCurrentState2Arrays = 0;
                        float valueOfCurrentStateInPlace = 0;
                        
                        int   currentState = state.StateIndex;

                        foreach (var transition in P(state, Pi(state)))
                        {
                            
                            float     probability = transition.Probability;
                            int         nextState = transition.SuccessorStateIndex;
                            float          reward = transition.Reward;  // Todo Should I make this the reward for arriving in the next state or the reward for the current state?

                            float  vSprime2Arrays = previousStateValue[nextState]; // 2 Arrays version
                            // float  vSprimeInPlace = ValueOfState[nextState];       // In-Place version
                            float vSprimeInPlace = V.Value(mdp.States[nextState]);
                            float  zeroIfTerminal = ZeroIfTerminal(nextState);

                            //                    P(s'| s, π(s) )•[R(s') +   𝛄   •  V(s')              ]
                             valueOfCurrentState2Arrays += probability * (reward + gamma * vSprime2Arrays * zeroIfTerminal); // 2 Arrays version
                             valueOfCurrentStateInPlace += probability * (reward + gamma * vSprimeInPlace * zeroIfTerminal); // In-Place version
                        }

                        delta   = Math.Max(delta  , Math.Abs(v2Arrays - valueOfCurrentState2Arrays));
                        deltaIP = Math.Max(deltaIP, Math.Abs(vInPlace - valueOfCurrentStateInPlace));
                        
                          stateValue[currentState] = valueOfCurrentState2Arrays; // 2 Arrays version
                        // ValueOfState[currentState] = valueOfCurrentStateInPlace; // In-Place version
                        V.SetValue(state, valueOfCurrentStateInPlace);           // In-Place version
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
            
            if (inPlaceVersion & (deltaIP < theta)) 
            {
                Debug.Log($"delta: {delta} deltaIP: {deltaIP}");
                break;
            }
            
            // Two array approach. See Sutton & Barto Chp 4.1
            stateValue.CopyTo(previousStateValue, 0); // 2 Arrays version
            
            iterations++;
        }

        foreach (var stateAndValue in ValueOfState)
        {
            Debug.Log($"V(S{stateAndValue.Key} = {stateAndValue.Value}");
        }
        
        
        Debug.Log(iterations);
    }
    

    public void PolicyImprovement()
    {
        
    }

    private float OneStepLookAhead(float prob, float reward, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + gamma * vSprime * zeroIfTerm);
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
            
            // gamma = 1.0f;
            // theta = 1E-10f;
            
            if (russellMdp)
            {
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
            
            
            PolicyEvaluation();
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
    private readonly Dictionary<string, float> _valueOfQGivenSandA = new Dictionary<string, float>();

    public void SetValue(MarkovState state, MarkovAction action, float qsaValue)
    {
        var stateActionValueKey = $"{state}{action}";
        switch (_valueOfQGivenSandA.ContainsKey(stateActionValueKey))
        {
            case true:
                _valueOfQGivenSandA[stateActionValueKey] = qsaValue;
                break;
            default:
                _valueOfQGivenSandA.Add(stateActionValueKey, qsaValue);
                break;
        }
    }

    public float Value(MarkovState state, MarkovAction action)
    {
        var stateActionValueKey = $"{state}{action}";

        switch (_valueOfQGivenSandA.ContainsKey(stateActionValueKey))
        {
            case true:
                return _valueOfQGivenSandA[stateActionValueKey];
            default:
                SetValue(state, action, 0f);
                return 0;
        }
    }
}

