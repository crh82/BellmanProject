using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Graphs;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Vector2 = System.Numerics.Vector2;

public class Algorithms : MonoBehaviour
{
    public MDP mdp;

    public List<GridAction> policy;
    public List<GridAction> previousPolicy;

    public float[] stateValue;
    public float[] previousStateValue;

    public float gamma;

    public float epsilon;

    public GridAction Pi(int stateIndex)
    {
        
        return policy[stateIndex];
    }

    public void PolicyEvaluation()
    {
        stateValue         = new float[mdp.StateCount];
        previousStateValue = new float[mdp.StateCount];
        
        while (true)
        {
            foreach (var state in mdp.States)
            {
                int si = state.StateIndex;

                foreach (MarkovTransition transition in P(state, Pi(si)))
                {
                    float prob    = transition.Probability;
                    int nextState = transition.SuccessorStateIndex;
                    float reward  = transition.Reward;

                    stateValue[si] += prob * (reward + gamma * previousStateValue[nextState]);
                }
            }
            
        }
    }

    public List<MarkovTransition> P(MarkovState state, GridAction action)
    {
            
        return null; //Todo remove stub
    }
    
    private void Awake()
    {
        mdp = JsonUtility.FromJson<MDP>(File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json"));
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
