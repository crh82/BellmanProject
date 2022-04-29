using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;



// Creates the model of the environment — the underlying MDP data rather than the physical/visual MDP.
/// <include file='include.xml' path='docs/members[@name="mdpmanager"]/MDPManager/*'/>
public class MdpManager : MonoBehaviour
{
    public Transform statePrefab;
    
    public MDP mdp;

    public TextAsset mdpFileToLoad;

    public Vector2 dimensions = Vector2.one;
    
    [Range(0, 1)] public float gapBetweenStates = 0.25f;

    private float _offsetToCenterGridX;
    
    private float _offsetToCenterGridY;

    private Vector2 _offsetToCenterVector;

    private Vector2 _2Doffset = new Vector2(0.5f, 0.5f);
    
    private GridAction[] _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];
    
    private List<Vector2> _states;
    
    public Transform stateSpace;
    
    public Dictionary<int,State> StateSpaceVisualStates = new Dictionary<int, State>();
    
    
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
        bool outOfBoundsLeft            = origin       % mdp.Width == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = (origin + 1) % mdp.Width == 0 && action == GridAction.Right;
        bool hitObstacle = mdp.ObstacleStates.Contains(destination);

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
            GridAction.Down => -mdp.Width,
            GridAction.Right => 1,
            GridAction.Up => mdp.Width,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
    
    public void InstantiateMdpVisualisation()
    {

        int numberOfStates = mdp.StateCount;

        double extents = Math.Sqrt(mdp.Width * mdp.Height);

        var differenceExtents = extents * 2;

        float cubeLengthWidth = 1 - gapBetweenStates;

        float numStatesToPercent = (1.0f / (numberOfStates - 1));

        var xCenter = 0.0f;
        var yCenter = 0.0f;
        var zCenter = 0.0f;

        var maxDistanceFromCenter = Math.Sqrt(2 * mdp.Width * mdp.Height);
        var reciprocalMaxDistFromCenter = 1.0 / maxDistanceFromCenter;

        var numStatesSquared = numberOfStates * numberOfStates;

        for (int index = 0; index < numberOfStates; index++)
        {
            int i = index / numberOfStates;
            int j = i % numberOfStates;

            var iPercentage = index * numStatesToPercent;
            var y = -extents + iPercentage * differenceExtents;

            var jPercentage = j * numStatesToPercent;
            var x = -extents + jPercentage * differenceExtents;

            var scale = new Vector3(cubeLengthWidth, 1f, cubeLengthWidth);
            float yScale = scale.y;
            float yPositionOffset = yScale / 2;

            var statePosition = new Vector3((float) x, yPositionOffset, (float) y);

            Transform state = Instantiate(
                statePrefab,
                statePosition,
                Quaternion.Euler(Vector3.zero));
        

            state.Find("StateMesh").localScale = scale;
        
            state.parent = stateSpace;
            state.name = $"{i}{j}";
            GameObject currentState = GameObject.Find($"{i}{j}");
            State curSt = currentState.GetComponent<State>();
            
            curSt.stateIndex = index;
                
            StateSpaceVisualStates[index] = curSt;
                
            if (mdp.States[index].IsObstacle()) currentState.SetActive(false);

        }

        Debug.Log("Gridworld Instantiated"); // Todo remove after debug
    }

    private Transform InstantiateIndividualStateVisual(int x, int y)
    {
        var scale = new Vector3(
            (1 - gapBetweenStates),
            0.0f,
            (1 - gapBetweenStates));


        float yScale = scale.y;
        float yPositionOffset = yScale / 2;

        var statePosition = new Vector3(
            _offsetToCenterVector.x + x,
            yPositionOffset,
            _offsetToCenterVector.y + y);

        Transform state = Instantiate(
            statePrefab,
            statePosition,
            Quaternion.Euler(Vector3.zero));
        

        state.Find("StateMesh").localScale = scale;
        
        state.parent = stateSpace;
        state.name = $"{x}{y}";
        
        return state;
    }
}