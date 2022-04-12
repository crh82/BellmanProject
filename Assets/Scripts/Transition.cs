using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Transition : MonoBehaviour
{
    public float Probability;
    public int SuccessorState;
    public float Reward;
    public bool IsTerminal;
}
