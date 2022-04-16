using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MarkovTransition
{
     
    // Old code, currently kept as a touchstone.
    // public float Probability;
    // public int SuccessorState;
    // public float Reward;
    // public bool isTerminal;
    //
    // {
    //     "state": 0,
    //     "probability": 0.3,
    //     "successorState": 1,
    //     "reward":
    // }
    
    [SerializeField] private int        state;
    [SerializeField] private GridAction actionTaken;
    [SerializeField] private float      probability;
    [SerializeField] private int        successorStateIndex;
    [SerializeField] private float      reward;
    // [SerializeField] private bool       isTerminal;

    public int State
    {
        get => state;
        set => state = value;
    }

    public GridAction ActionTaken
    {
        get => actionTaken;
        set => actionTaken = value;
    }

    public float Probability
    {
        get => probability;
        set => probability = value;
    }

    public int SuccessorStateIndex
    {
        get => successorStateIndex;
        set => successorStateIndex = value;
    }

    public float Reward
    {
        get => reward;
        set => reward = value;
    }

    // public bool IsTerminal
    // {
    //     get => isTerminal;
    //     set => isTerminal = value;
    // }
}


