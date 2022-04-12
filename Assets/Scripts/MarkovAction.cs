using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarkovAction
{
    public string Action;
    public int ActionID;
    public int[] StateAction;
    public List<Transition> Transitions;

}
