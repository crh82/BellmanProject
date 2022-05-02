using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;



// Creates the model of the environment — the underlying MDP data rather than the physical/visual MDP.
/// <include file='include.xml' path='docs/members[@name="mdpmanager"]/MDPManager/*'/>
public class MdpManager : MonoBehaviour
{
    public Transform           statePrefab;

    public Transform           stateSpacePrefab;
    
    public TextAsset           mdpFileToLoad;

    public Vector2             dimensions = Vector2.one;
    
    [Range(0, 1)] public float gapBetweenStates = 0.25f;

    public float               initialStateValueForTesting; // Todo remove after built
    
    // ┌─────────────────────────┐
    // │ MDP data and algorithms │
    // └─────────────────────────┘
    
    public MDP                 mdp;

    public Algorithms          algorithms;

    public Policy              CurrentPolicy;

    public float               gamma = 1f;

    // private double             theta = 1e-10;

    public bool                debugMode;

    public int                 maximumIterations = 10_000;

    public bool                boundIterations = false;
    

    private Vector2            _offsetToCenterVector;

    private readonly Vector2   _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);
    
    private GridAction[]       _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];

    private readonly Dictionary<int,State> _stateSpaceVisualStates = new Dictionary<int, State>();
    
    
    public MDP CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }

    public void LoadMdpFromFilePath(string filepath)
    {
        string mdpJsonRepresentation = File.ReadAllText(filepath);
        mdp = CreateFromJson(mdpJsonRepresentation);
        InstantiateMdpVisualisation();
    }

    public void Awake()
    {
        algorithms = gameObject.AddComponent<Algorithms>();
    }

    private void Start()
    {
        // mdp = CreateFromJson(mdpFileToLoad.text);
        // InstantiateMdpVisualisation();
    }

    public List<MarkovTransition> CalculateTransitions(string transitionProbabilityRules)
    {
        List<MarkovTransition> transitions = new List<MarkovTransition>();
        
        switch (transitionProbabilityRules)
        {
                        
        }

        return transitions;
    }

    public void InstantiateMdpVisualisation()
    {
        var id = 0;
        
        _offsetToCenterVector = new Vector2((-mdp.Width / 2f), (-mdp.Height / 2f));
        
        if (mdp.Height > 1) {_offsetToCenterVector += _offsetValuesFor2DimensionalGrids;}
        
        float stateCubeDimensions = 1 - gapBetweenStates;

        Transform stateSpace = Instantiate(
            stateSpacePrefab, 
            GameObject.FindGameObjectWithTag("MDP GameObject").transform, 
            true);

        for (var y = 0; y < mdp.Height; y++)
        {
            for (var x = 0; x < mdp.Width; x++)
            {
                var state = InstantiateIndividualState(stateCubeDimensions, x, y, stateSpace, id);
                id++;
            }
        }
       
        // StateSpaceVisualStates[0].UpdateHeight(1.34567f);
        // StateSpaceVisualStates[1].UpdateHeight(2.84357f);
        
        Debug.Log("Gridworld Instantiated"); // Todo remove after debug
    }

    public Transform InstantiateIndividualState(float stateXandZDimensions, int x, int y, Transform stateSpace, int id)
    {
        var scale = new Vector3(stateXandZDimensions, initialStateValueForTesting, stateXandZDimensions);

        var statePosition = new Vector3(
            _offsetToCenterVector.x + x,
            (scale.y / 2),
            _offsetToCenterVector.y + y);

        Transform state = Instantiate(
            statePrefab,
            statePosition,
            Quaternion.Euler(Vector3.zero));

        state.Find("StateMesh").localScale = scale;

        state.parent = stateSpace;

        state.name = $"{x}{y}";

        var currentState = GameObject.Find($"{x}{y}");

        var curSt = state.GetComponent<State>();

        curSt.stateIndex = id;

        _stateSpaceVisualStates[id] = curSt;

        if (mdp.States[id].IsObstacle()) currentState.SetActive(false);
        
        return state;
    }

    
    /// <summary>
    /// TODO Improve because Temporary
    /// </summary>
    public void VisualizeStateValues()
    {
        StateValueFunction valueOfCurrentPolicy = algorithms.PolicyEvaluation(
            mdp, 
            CurrentPolicy, 
            gamma, 
            1e-10f,
            boundIterations,
            maximumIterations,
            debugMode);

        foreach (var stateKvp in _stateSpaceVisualStates)
        {
            stateKvp.Value.UpdateHeight(valueOfCurrentPolicy.Value(stateKvp.Key));
        }
    }
    
    // ╔══════════════════════╗
    // ║ Not Currently In Use ║
    // ╚══════════════════════╝
    
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
}