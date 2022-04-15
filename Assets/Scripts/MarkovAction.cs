using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MarkovAction
{
    // public string Action;
    // public int ActionID;
    // public int[] StateAction;
    // public List<MarkovTransition> Transitions;
    [SerializeField] private GridAction action;
    [SerializeField] private int[] stateAction;
    [SerializeField] private List<MarkovTransition> transitions;

    public GridAction Action
    {
        get => action;
        set => action = value;
    }

    public int[] StateAction
    {
        get => stateAction;
        set => stateAction = value;
    }

    public List<MarkovTransition> Transitions
    {
        get => transitions;
        set => transitions = value;
    }
}
