using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 
/// </summary>
[Serializable]
public class MarkovState
{
    
    // public int State;
    // public List<MarkovAction> ApplicableActions;

    [FormerlySerializedAs("stateIndexIndex")] [SerializeField] private int stateIndex;
    [SerializeField] private List<MarkovAction> applicableActions;
    [SerializeField] private float reward;
    [SerializeField] private StateType typeOfState;

    public int StateIndex
    {
        get => stateIndex;
        set => stateIndex = value;
    }

    public List<MarkovAction> ApplicableActions
    {
        get => applicableActions;
        set => applicableActions = value;
    }

    public float Reward
    {
        get => reward;
        set => reward = value;
    }

    public StateType TypeOfState
    {
        get => typeOfState;
        set => typeOfState = value;
    }

    public bool IsTerminal()
    {
        return typeOfState.Equals(StateType.Terminal);
    }

    public bool IsObstacle()
    {
        return typeOfState.Equals(StateType.Obstacle);
    }

    public bool IsGoal()
    {
        return typeOfState.Equals(StateType.Goal);
    }
}

public enum StateType
{
    Terminal, Obstacle, Goal, Standard
}