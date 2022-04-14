using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarkovAction
{
    // public string Action;
    // public int ActionID;
    // public int[] StateAction;
    // public List<MarkovTransition> Transitions;
    [SerializeField] private GridAction action;
    [SerializeField] private int[] stateAction;
    [SerializeField] private List<MarkovTransition> applicableActions;

    public GridAction Action => action;

    public int[] StateAction => stateAction;

    public List<MarkovTransition> ApplicableActions => applicableActions;
    
}
