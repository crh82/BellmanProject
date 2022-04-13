using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarkovTransition
{
    public float Probability;
    public int SuccessorState;
    public float Reward;
    public bool isTerminal;
}
