using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Graphs;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Vector2 = System.Numerics.Vector2;

public class Algorithms : MonoBehaviour
{
    public MDP mdp;

    public Dictionary<int, GridAction> policy;
    // public List<GridAction> policy;
    public List<GridAction> previousPolicy;

    public float[] stateValue;
    public float[] previousStateValue;

    public float gamma;

    public float epsilon;

    public float theta;

    public int iterations; 

    public GridAction Pi(MarkovState state)
    {
        return policy[state.StateIndex];
    }

    public void PolicyEvaluation()
    {
        iterations         = 0;
        stateValue         = new float[mdp.StateCount];
        previousStateValue = new float[mdp.StateCount];
        
        while (true)
        {
            foreach (var state in mdp.States)
            {
                if (state.IsTerminal() | state.IsGoal() | state.IsObstacle()) continue;
                
                foreach (var transition in P(state, Pi(state)))
                {
                    int currentState = state.StateIndex;
                    float       prob = transition.Probability;
                    int    nextState = transition.SuccessorStateIndex;
                    float     reward = transition.Reward;
                        
                    stateValue[currentState] += prob * (reward + gamma * previousStateValue[nextState] * ZeroIfTerminal(nextState));
                }
            }

            if (MaxAbsoluteDifference(previousStateValue, stateValue) < theta) break;
            // previousStateValue = stateValue.Clone() as float[];
            stateValue.CopyTo(previousStateValue, 0);
            iterations++;
        }
    }

    public float ZeroIfTerminal(int stateIndex)
    {
        MarkovState state = mdp.States[stateIndex];
        return state.IsTerminal() ? 0 : 1;
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
        
    }
}
