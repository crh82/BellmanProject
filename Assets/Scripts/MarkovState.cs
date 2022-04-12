using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class MarkovState
{
    public int State;
    public List<MarkovAction> ApplicableActions;
}
