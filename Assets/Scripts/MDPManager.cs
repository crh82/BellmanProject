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



// Creates the model of the environment — the underlying MDP data rather than the physical/visual MDP.
/// <include file='include.xml' path='docs/members[@name="mdpmanager"]/MDPManager/*'/>
public class MDPManager : MonoBehaviour
{
    public MDP mdp;

    public TextAsset mdpFileToLoad;

    public Vector2 dimensions = Vector2.one;

    private GridAction[] _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];
    
    private List<Vector2> _states;
    
    
    public static MDP CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }

    public void Awake()
    {
        mdp = CreateFromJson(mdpFileToLoad.text);
    }

    public List<MarkovTransition> CalculateTransitions(string transitionProbabilityRules)
    {
        List<MarkovTransition> transitions = new List<MarkovTransition>();
        
        switch (transitionProbabilityRules)
        {
                        
        }

        return transitions;
    }

    // Generates the successor state, given a state and action in the modelling of the environment.
    /// <include file='include.xml' path='docs/members[@name="mdpmanager"]/ExecuteAction/*'/>
    public int ExecuteAction(int state, GridAction action)
    {
        int destination = state + GetEffectOfAction(action);
        return DestinationOutOfBounds(state, destination, action) ? state : destination;
    }

    public bool DestinationOutOfBounds(int origin, int destination, GridAction action)
    {
        bool outOfBoundsTop             = destination > mdp.States.Count - 1;
        bool outOfBoundsBottom          = destination < 0;
        bool outOfBoundsLeft            = origin       % mdp.dimX == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = (origin + 1) % mdp.dimX == 0 && action == GridAction.Right;
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
            GridAction.Down => -mdp.dimX,
            GridAction.Right => 1,
            GridAction.Up => mdp.dimX,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
    
}