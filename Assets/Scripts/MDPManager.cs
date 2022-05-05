using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codice.CM.Common;
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
    public Transform                       statePrefab;

    public Transform                       obstaclePrefab;

    public Transform                       terminalPrefab;

    public Transform                       goalPrefab;

    public Transform                       stateSpacePrefab;
    
    public TextAsset                       mdpFileToLoad;

    public Vector2                         dimensions = Vector2.one;
    
    [Range(0, 1)] public float             gapBetweenStates = 0.25f;

    public float                           initialStateValueForTesting; // Todo remove after built
    
    // ┌─────────────────────────┐
    // │ MDP data and algorithms │
    // └─────────────────────────┘
    
    public MDP                             mdp;

    public Algorithms                      algorithms;

    public Policy                          CurrentPolicy;

    public StateValueFunction              CurrentStateValueFunction;

    public float                           gamma = 1f;

    public float                           theta = 1e-10f;

    public bool                            debugMode;

    public int                             maximumIterations = 10_000;

    public bool                            boundIterations = false;
    

    private Vector2                        _offsetToCenterVector;

    private readonly Vector2               _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);
    
    private GridAction[]                   _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];

    private readonly Dictionary<int,State> _stateSpaceVisualStates = new Dictionary<int, State>();
    
    
    public MDP CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }

    public void LoadMdpFromFilePath(string filepath)
    {
        string mdpJsonRepresentation = File.ReadAllText(filepath);
        mdp = CreateFromJson(mdpJsonRepresentation);
        StartCoroutine(InstantiateMdpVisualisation());
        // InstantiateMdpVisualisation();
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

    public IEnumerator InstantiateMdpVisualisation()
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
                var state = InstantiateIndividualState(mdp, stateCubeDimensions, x, y, stateSpace, id);
                id++;
                yield return null;
            }
        }
    }

    public Transform InstantiateIndividualState(MDP mdp, float stateXandZDimensions, int x, int y, Transform stateSpace, int id)
    {
        var stateType = mdp.States[id].TypeOfState;
        
        var   scale              = new Vector3(stateXandZDimensions, initialStateValueForTesting, stateXandZDimensions);
        var   statePosition      = new Vector3(_offsetToCenterVector.x + x, (scale.y / 2), _offsetToCenterVector.y + y);
          
        var   obstacleScale      = new Vector3(stateXandZDimensions, 1f, stateXandZDimensions);
        var   obstaclePosition   = new Vector3(_offsetToCenterVector.x + x, (obstacleScale.y / 2), _offsetToCenterVector.y + y);
        
        float terminalGoalScale  = mdp.States[id].Reward;
        var   terminalScale      = new Vector3(stateXandZDimensions, terminalGoalScale, stateXandZDimensions);
        var   terminalPosition   = new Vector3(_offsetToCenterVector.x + x, (terminalGoalScale / 2), _offsetToCenterVector.y + y);
        
        var   goalScale          = new Vector3(stateXandZDimensions, terminalGoalScale, stateXandZDimensions);
        var   goalPosition       = new Vector3(_offsetToCenterVector.x + x, (terminalGoalScale / 2), _offsetToCenterVector.y + y);
        
        Transform state;
        State fullStateObject;
        
        switch (stateType)
        {
            case StateType.Terminal:
                state           = Instantiate(terminalPrefab, terminalPosition, Quaternion.Euler(Vector3.zero));
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(terminalScale);
                fullStateObject.UpdateHeight(terminalGoalScale);
                break;
            case StateType.Obstacle:
                state           = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.Euler(Vector3.zero));
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(obstacleScale);
                break;
            case StateType.Goal:
                state           = Instantiate(goalPrefab, goalPosition, Quaternion.Euler(Vector3.zero));
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(goalScale);
                fullStateObject.UpdateHeight(terminalGoalScale);
                break;
            case StateType.Standard:
                state           = Instantiate(statePrefab, statePosition, Quaternion.Euler(Vector3.zero));
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(scale);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // var state = stateType switch
        // {
        //     StateType.Terminal => Instantiate(terminalPrefab, statePosition, Quaternion.Euler(Vector3.zero)),
        //     StateType.Obstacle => Instantiate(obstaclePrefab, obstaclePosition, Quaternion.Euler(Vector3.zero)),
        //     StateType.Goal     => Instantiate(goalPrefab,     statePosition, Quaternion.Euler(Vector3.zero)),
        //     StateType.Standard => Instantiate(statePrefab,    statePosition, Quaternion.Euler(Vector3.zero)),
        //     _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
        // };

        // ┌──────────┐
        // │ SAFE ONE │
        // └──────────┘
        // var state = Instantiate(statePrefab, statePosition, Quaternion.Euler(Vector3.zero));
        // var fullStateObject = state.GetComponent<State>();
        // fullStateObject.SetStateScale(scale);
        state.parent = stateSpace;
        
        //
        // state.name = $"{x}{y}";
        //
        // var curSt = state.GetComponent<State>();
        //
        // curSt.SetStateScale(scale);
        //
        // curSt.stateIndex = id;
        //
        // _stateSpaceVisualStates[id] = curSt;

        fullStateObject.stateIndex = id;

        _stateSpaceVisualStates[id] = fullStateObject;

        // if (mdp.States[id].IsObstacle()) currentStateGameObject.SetActive(false);

        return state;
    }

    
    /// <summary>
    /// TODO Improve because Temporary. Break this up into evaluation and visualization
    /// </summary>
    public void EvaluateAndVisualizeStateValues()
    {
        NullValuesCheck();
        
        StateValueFunction valueOfCurrentPolicy = algorithms.PolicyEvaluation(
            mdp, 
            CurrentPolicy, 
            gamma, 
            theta,
            boundIterations,
            maximumIterations,
            debugMode);

        foreach (var stateKvp in _stateSpaceVisualStates.Where(
                     stateKvp => mdp.States[stateKvp.Key].IsStandard()))
        {
            stateKvp.Value.UpdateHeight(valueOfCurrentPolicy.Value(stateKvp.Key));
        }
    }

    private void NullValuesCheck()
    {
        if (CurrentPolicy == null)
        {
            CurrentPolicy = new Policy(mdp);
            Debug.Log("No Policy specified, evaluating a random policy.");
        }
        
        if (mdp == null) throw new NullReferenceException("No MDP specified.");
    }
    
    public IEnumerator ShowActionSpritesAtopStateValueVisuals()
    {
        if (CurrentPolicy == null) throw new ArgumentNullException();
        
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            var currentAction      = CurrentPolicy.GetAction(state);
            var currentStateVisual = _stateSpaceVisualStates[state.StateIndex];
                currentStateVisual.UpdateVisibleActionFromPolicy(currentAction);
                yield return null;
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