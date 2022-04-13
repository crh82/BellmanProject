using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[System.Serializable]
public class MDP
{
    public string name;
    public int dimX;
    public int dimY;
    public List<MarkovState> States;

    public int[] obstacleStates;

    // public List<Transition> P(int state, int action)
    // {
    //     // return TransitionFunction[state][action];
    // }

    public static MDP CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }
}




