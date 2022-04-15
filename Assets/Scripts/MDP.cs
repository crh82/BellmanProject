using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class MDP
{
    // Todo old code
    // public string name;
    // public int dimX;
    // public int dimY;
    // public List<MarkovState> States;
    //
    // public int[] obstacleStates;
    
    [SerializeField] private string name;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<MarkovState> states;
    [SerializeField] private MdpRules mdpRules;
    [SerializeField] private int[] obstacleStates;
    [SerializeField] private int[] terminalStates;
    [SerializeField] private int[] goalStates;

    // NOTE: Don't know if it's better to use dictionaries here 
    private Dictionary<int[], float> _transitionProbabilities;
    private Dictionary<int, float> _rewards;


    // public List<Transition> P(int state, int action)
    // {
    //     // return TransitionFunction[state][action];
    // }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public int Width
    {
        get => width;
        set => width = value;
    }

    public int Height
    {
        get => height;
        set => height = value;
    }

    public List<MarkovState> States
    {
        get => states;
        set => states = value;
    }

    public int[] ObstacleStates
    {
        get => obstacleStates;
        set => obstacleStates = value;
    }
    
    public int StateCount => States.Count;

    public MdpRules MdpRules
    {
        get => mdpRules;
        set => mdpRules = value;
    }

    public int[] TerminalStates
    {
        get => terminalStates;
        set => terminalStates = value;
    }

    public int[] GoalStates
    {
        get => goalStates;
        set => goalStates = value;
    }

    public Dictionary<int[], float> TransitionProbabilities
    {
        get => _transitionProbabilities;
        set => _transitionProbabilities = value;
    }

    public Dictionary<int, float> Rewards
    {
        get => _rewards;
        set => _rewards = value;
    }
}




