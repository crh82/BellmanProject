using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Graphs;
using UnityEngine;
// using Debug = System.Diagnostics.Debug;
using Vector2 = System.Numerics.Vector2;

public class Algorithms : MonoBehaviour
{
    // Cheeky Test stuff Todo Remove
    public bool FrozenMdp = false;
    public bool RussellMdp = false;
    public bool loadMdp;
    public bool runPolicyEvaluation = false;
    
    public MDP mdp;

    public Dictionary<int, GridAction> policy;
    // public List<GridAction> policy;
    public List<GridAction> previousPolicy;

    public float[] stateValue;
    public float[] previousStateValue;

    public float    gamma;

    public float  epsilon;

    public float    theta;

    public int iterations; 

    public GridAction Pi(MarkovState state)
    {
        return policy[state.StateIndex];
    }

    public void PolicyEvaluation()
    {
        iterations         = 0;
        
        previousStateValue = new float[mdp.StateCount];
        
        while (true)
        {
            stateValue         = new float[mdp.StateCount];
            
            foreach (var state in mdp.States)
            {
                switch (state.TypeOfState)
                {
                    case StateType.Obstacle:
                        break;
                    
                    case StateType.Terminal:
                        stateValue[state.StateIndex] = state.Reward;
                        break;
                    
                    case StateType.Goal:
                        stateValue[state.StateIndex] = state.Reward;
                        break;
                    
                    default:
                        float  valueOfState = 0;
                        int    currentState = state.StateIndex;
                        foreach (var transition in P(state, Pi(state)))
                        {
                    
                            var     actionTaken = transition.ActionTaken;
                    
                            float          prob = transition.Probability;
                            int       nextState = transition.SuccessorStateIndex;
                            // float        reward = transition.Reward;
                            
                            float        reward = mdp.States[nextState].Reward;

                            float       vSprime = previousStateValue[nextState];
                            float    zeroIfTerm = ZeroIfTerminal(nextState);
                    
                            float bellmanBackup = prob * (reward + gamma * vSprime * zeroIfTerm);

                            valueOfState += bellmanBackup;
                          
                            // stateValue[currentState] += bellmanBackup;
                        }
                        
                        stateValue[currentState] = valueOfState;
                        break;
                }
                
                // // if (state.IsTerminal() || state.IsGoal() || state.IsObstacle()) continue;
                // if (state.IsObstacle()) continue;
                //
                // float  valueOfState = 0;
                // int    currentState = state.StateIndex;
                //
                // foreach (var transition in P(state, Pi(state)))
                // {
                //     
                //     var     actionTaken = transition.ActionTaken;
                //     
                //     float          prob = transition.Probability;
                //     int       nextState = transition.SuccessorStateIndex;
                //     float        reward = transition.Reward;
                //     
                //     float       vSprime = previousStateValue[nextState];
                //     float    zeroIfTerm = ZeroIfTerminal(nextState);
                //     
                //     float bellmanBackup = prob * (reward + gamma * vSprime * zeroIfTerm);
                //
                //           valueOfState += bellmanBackup;
                //           
                //     // stateValue[currentState] += bellmanBackup;
                // }
                //
                // stateValue[currentState] = valueOfState;
            }

            if (MaxAbsoluteDifference(previousStateValue, stateValue) < theta) break;

            if (iterations > 1000) break;  // Todo remove. Just for testing.
            // previousStateValue = stateValue.Clone() as float[];
            stateValue.CopyTo(previousStateValue, 0);
            iterations++;
        }
        
        Debug.Log(iterations);
    }

    private float BellmanBackup(float prob, float reward, float vSprime, float zeroIfTerm)
    {
        return prob * (reward + gamma * vSprime * zeroIfTerm);
    }

    public float ZeroIfTerminal(int stateIndex)
    {
        
        MarkovState state = mdp.States[stateIndex];
        bool terminal = state.IsTerminal();
        bool goal = state.IsGoal();
        float outcome = (state.IsTerminal() || state.IsGoal()) ? 0 : 1;
        return outcome;
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

        if (loadMdp)
        {

            if (RussellMdp)
            {
                mdp = MdpAdmin.LoadMdp(File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
            }

            if (FrozenMdp)
            {
                mdp = MdpAdmin.LoadMdp(
                    File.ReadAllText("Assets/Resources/CanonicalMDPs/FrozenLake4x4.json"));
            }
            
            
            loadMdp = false;
        }
        
        if (runPolicyEvaluation)
        {
            GridAction Left  = GridAction.Left;
            GridAction Down  = GridAction.Down;
            GridAction Right = GridAction.Right;
            GridAction Up    = GridAction.Up;
            
            // gamma = 1.0f;
            // theta = 1E-10f;
            
            if (RussellMdp)
            {
                policy = new Dictionary<int, GridAction>
                {
                    { 8,  Left},{ 9, Right},{10, Right},{11,  Down},
                    { 4,    Up},{ 5,  Down},{ 6,    Up},{ 7,  Down},
                    { 0,    Up},{ 1,  Left},{ 2, Left },{ 3, Left },
                };
            }

            if (FrozenMdp)
            {
                policy = new Dictionary<int, GridAction>
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
