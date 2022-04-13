using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Class to manage the creation of the underlying MDP data and functions. For the handling the generation of the 
/// in game MDP object see: <see cref="MDPGenerator"/>.
/// </summary>
public class MDPManager : MonoBehaviour
{
    public MDP mdp;

    public TextAsset mdpFileToLoad;

    public Vector2 dimensions = Vector2.one;
    
    // private List<Vector2> _actions = new List<Vector2> {Vector2.left, Vector2.down, Vector2.right, Vector2.up};

    private GridAction[] _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];
    
    private List<Vector2> _states;
    
    
    public static MDP CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }

    public void Awake()
    {
        mdp = CreateFromJSON(mdpFileToLoad.text);

        mdp.obstacleStates = new int[] {5};// Todo remove hackish test code
        Debug.Log("Loaded");       // Todo remove hackish test code
    }

    public List<MarkovTransition> CalculateTransitions(string transitionProbabilityRules)
    {
        List<MarkovTransition> transitions = new List<MarkovTransition>();
        
        switch (transitionProbabilityRules)
        {
                        
        }

        return transitions;
    }

    public int ExecuteAction(int state, GridAction action)
    {
        int destination = state + GetEffectOfAction(action);
        return DestinationOutOfBounds(state, destination, action) ? state : destination;
    }

    public bool DestinationOutOfBounds(int origin, int destination, GridAction action)
    {
        bool outOfBoundsTop             = destination > (mdp.States.Count) - 1;
        bool outOfBoundsBottom          = destination < 0;
        bool outOfBoundsLeft            = origin % mdp.dimX == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = origin % mdp.dimX == 1 && action == GridAction.Right;
        bool hitObstacle = mdp.obstacleStates.Contains(destination);

        return (outOfBoundsLeft   | 
                outOfBoundsBottom | 
                outOfBoundsRight  | 
                outOfBoundsTop    |
                hitObstacle) switch
            {
                true => true,
                _ => false
            };
    }
    
    
    public int GetEffectOfAction(GridAction action)
    {
        return action switch
        {
            GridAction.Left => -1,
            GridAction.Down => -mdp.dimY,
            GridAction.Right => 1,
            GridAction.Up => mdp.dimY,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
    
}
