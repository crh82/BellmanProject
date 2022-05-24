using System;
using UnityEngine;

[Serializable]
public class MarkovTransition
{

    [SerializeField] private int        state;
    [SerializeField] private GridAction actionTaken;
    [SerializeField] private float      probability;
    [SerializeField] private int        successorStateIndex;
    [SerializeField] private float      reward;

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
}


