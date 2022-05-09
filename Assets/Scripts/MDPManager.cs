using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.AppleTV;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = System.Object;


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

    public Canvas                          currentUi;

    public UIController                    uiController;
    
    private Vector2                        _offsetToCenterVector;

    private readonly Vector2               _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);
    
    public int                             playSpeed = 100;

    public int                             cat;
    
    public bool                            keepGoing = true;

    private const int                      BySweep = 0;
    
    private const int                      ByStatePE = 1;
    
    private const int                      ByTransition = 2;

    private const int                      Suspended = 3;

    private const int                      ByStatePI = 0;

    private const int                      ByActionPI = 1;
    
    
    // ┌─────────────────────────┐
    // │ MDP data and algorithms │
    // └─────────────────────────┘
    
    public  MDP                            mdp;

    public  Algorithms                     algorithms;

    public  Policy                         CurrentPolicy;

    public  StateValueFunction             CurrentStateValueFunction;

    public  float                          gamma = 1f;

    public  float                          theta = 1e-10f;

    public  bool                           debugMode;

    public  int                            maximumIterations = 100_000;

    public  bool                           boundIterations = true;

    public  double                         algorithmExecutionSpeed = 0d;

    private int                            _currentAlgorithmExecutionIterations; // Reset after each alg execution.

    

    public  int                             algorithmViewLevel = 2;
    
    private GridAction[]                   _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];

    private readonly Dictionary<int,State> _stateSpaceVisualStates = new Dictionary<int, State>();

    private Stack<Policy>                  _previousPolicyStack = new Stack<Policy>();


    // ╔═══════════════════════╗
    // ║ TODO For Debug REMOVE ║
    // ╚═══════════════════════╝

    public bool suspendForDebug;
    
    
    // ╔════════════════════════╗
    // ║ NORMAL UNITY FUNCTIONS ║
    // ╚════════════════════════╝
    
    public void Awake()
    {
        algorithms   = gameObject.AddComponent<Algorithms>();
        currentUi    = GameObject.FindGameObjectWithTag("PolicyEvaluationUI").GetComponent<Canvas>();
        uiController = currentUi.GetComponent<UIController>();
    }

    private void Start()
    {
        // mdp = CreateFromJson(mdpFileToLoad.text);
        // InstantiateMdpVisualisation();
    }

    private void Update()
    {
        if (suspendForDebug)
        {
            suspendForDebug = false;
        }
    }
    
    // ┌──────────────────────────────────────┐
    // │ HELPER/SPECIFIC GET OR SET FUNCTIONS │
    // └──────────────────────────────────────┘
    
    public float Gamma
    {
        get => gamma;
        set => gamma = value;
    }

    public MarkovState GetStateFromCurrentMdp(int stateIndex) 
        => mdp.States[stateIndex];

    public Dictionary<string, string> GetStateAndActionInformationForDisplayAndEdit(int stateIndex)
    {

        var state = mdp.States[stateIndex];
        
        var    stateVisualRepresentation = _stateSpaceVisualStates[stateIndex];
        string stateName                 = StateNameFormatted(stateIndex);
        var    stateReward               = $"R({stateName}) = {state.Reward}";
        var    stateValue                = $"V({stateName}) = {stateVisualRepresentation.stateValue}";

        // if (CurrentPolicy != null)
        // {
        //     var action = CurrentPolicy.GetAction(stateIndex);
        // }
        
        var stateInformation = new Dictionary<string, string>
        {
            {"state"     , stateName},
            {"reward"    , stateReward},
            {"value"     , stateValue},
            {"iterations", $"Iteration: T + {_currentAlgorithmExecutionIterations}"},
            {"action"    , ActionInStateFormatted(stateIndex)}
        };
        
        // var stateInfo = $"{stateName}\n{stateReward}\n{stateValue}";

        return stateInformation;
    }

    public string StateNameFormatted(int stateIndex) 
        => $"<b>S</b><sub>{stateIndex}</sub>";

    public string ActionInStateFormatted(int stateIndex)
    {
        string action = CurrentPolicy != null ? CurrentPolicy.GetAction(stateIndex).ToString() : "n/a";
        return $"π({StateNameFormatted(stateIndex)}) = {action}";
    }

    public void EditRewardOfState(int stateIndex, float newReward) 
        => mdp.States[stateIndex].Reward = newReward;

    public void SetIndividualStateHeight(MarkovState state, float value) 
        => SetIndividualStateHeight(state.StateIndex, value);

    public void SetIndividualStateHeight(int stateIndex, float value) 
        => _stateSpaceVisualStates[stateIndex].UpdateHeight(value);

    private void SetAllStateHeights(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            SetIndividualStateHeight(state, valueOfCurrentPolicy.Value(state));
        }
    }
    
    public Task SetIndividualStateHeightAsync(MarkovState state, float value)
    {
        var setHeight = SetIndividualStateHeightAsync(state.StateIndex, value);
        return setHeight;
    }
    public Task SetIndividualStateHeightAsync(int stateIndex, float value) 
        => _stateSpaceVisualStates[stateIndex].UpdateHeightAsync(value);

    private async Task SetAllStateHeightsAsync(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            await SetIndividualStateHeightAsync(state, valueOfCurrentPolicy.Value(state));
        }
    }

    public void ResetPolicy() 
        => CurrentPolicy = null;

    public void ResetCurrentStateValueFunction() 
        => CurrentStateValueFunction = null;

    public void GenerateRandomStateValueFunction() 
        => CurrentStateValueFunction = new StateValueFunction(mdp, -3f, 3f);

    public void SetKeepGoingFalse() => keepGoing = false;

    public void SetKeepGoingTrue() => keepGoing = true;

    public MDP GetCurrentMdp() => mdp;

    public void EditCurrentPolicy(MarkovState state, MarkovAction action) 
        => CurrentPolicy.SetAction(state, action.Action);
    public void EditCurrentPolicy(MarkovState state, GridAction action)
        => CurrentPolicy.SetAction(state, action);
    public void EditCurrentPolicy(int stateIndex, int action) 
        => CurrentPolicy.SetAction(stateIndex, (GridAction) action);


    public void SetActionImage(MarkovState state, MarkovAction action) 
        => SetActionImage(state.StateIndex, (int) action.Action);
    public void SetActionImage(MarkovState state, GridAction action) 
        => SetActionImage(state.StateIndex, (int) action);
    public void SetActionImage(int stateIndex, int action) 
        => _stateSpaceVisualStates[stateIndex].UpdateVisibleActionFromPolicy((GridAction) action);
    
 


    // ╔═══════════════════════════════════╗
    // ║ MDP SERIALIZATION/DESERIALIZATION ║
    // ╚═══════════════════════════════════╝
    public MDP CreateFromJson(string jsonString) 
        => JsonUtility.FromJson<MDP>(jsonString);

    public async void LoadMdpFromFilePath(string filepath)
    {
        string mdpJsonRepresentation = File.ReadAllText(filepath);
        mdp = CreateFromJson(mdpJsonRepresentation);
        // StartCoroutine(InstantiateMdpVisualisation());
        mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(mdpJsonRepresentation));
        
        Debug.Log("For Checking");
    }

    public IEnumerator InstantiateMdpVisualisation()
    {
        var id = 0;
        
        _offsetToCenterVector = new Vector2((-mdp.Width / 2f), (-mdp.Height / 2f));
        
        if (mdp.Height > 1) {_offsetToCenterVector += _offsetValuesFor2DimensionalGrids;}
        
        float stateCubeDimensions = 1 - gapBetweenStates;

        Transform stateSpace = Instantiate(
            stateSpacePrefab, 
            this.transform, 
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
    
    public async Task<MDP> InstantiateMdpVisualisationAsync(MDP mdpFromFile)
    {
        var mdpForCreation = mdpFromFile;
        
        var id = 0;
        
        _offsetToCenterVector = new Vector2((-mdpForCreation.Width / 2f), (-mdpForCreation.Height / 2f));
        
        if (mdpForCreation.Height > 1) {_offsetToCenterVector += _offsetValuesFor2DimensionalGrids;}
        
        float stateCubeDimensions = 1 - gapBetweenStates;

        Transform stateSpace = Instantiate(
            stateSpacePrefab, 
            this.transform, 
            true);

        for (var y = 0; y < mdpForCreation.Height; y++)
        {
            for (var x = 0; x < mdpForCreation.Width; x++)
            {
                var state = InstantiateIndividualState(mdpForCreation, stateCubeDimensions, x, y, stateSpace, id);
                id++;
            }
        }

        return await Task.FromResult(mdpForCreation);
    }

    public Transform InstantiateIndividualState(MDP mdp, float stateXandZDimensions, int x, int y, Transform stateSpace, int id)
    {
        var stateType = mdp.States[id].TypeOfState;
        
        var   scale              = new Vector3(stateXandZDimensions, initialStateValueForTesting, stateXandZDimensions);
        var   statePosition      = new Vector3(_offsetToCenterVector.x + x, (scale.y / 2), _offsetToCenterVector.y + y);
          
        // var obstacleScale        = new Vector3(stateXandZDimensions, 1f, stateXandZDimensions);
        // var   obstaclePosition   = new Vector3(_offsetToCenterVector.x + x, (obstacleScale.y / 2), _offsetToCenterVector.y + y);
        var   obstacleScale      = new Vector3(1f, 1f, 1f);
        var   obstaclePosition   = new Vector3(_offsetToCenterVector.x + x, 0, _offsetToCenterVector.y + y);
        
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
                state           = Instantiate(terminalPrefab, terminalPosition, Quaternion.identity);
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(terminalScale);
                fullStateObject.UpdateHeight(terminalGoalScale);
                break;
            case StateType.Obstacle:
                state           = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(obstacleScale);
                break;
            case StateType.Goal:
                state           = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(goalScale);
                fullStateObject.UpdateHeight(terminalGoalScale);
                break;
            case StateType.Standard:
                state           = Instantiate(statePrefab, statePosition, Quaternion.identity);
                state.parent    = stateSpace;
                state.name      = $"{x}{y}";
                fullStateObject = state.GetComponent<State>();
                fullStateObject.SetStateScale(scale);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
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
        
        fullStateObject.DistributeMdpManagerReferenceToComponents(this);

        _stateSpaceVisualStates[id] = fullStateObject;

        // if (mdp.States[id].IsObstacle()) currentStateGameObject.SetActive(false);

        return state;
    }

    
    public void EnsureMdpAndPolicyAreNotNull()
    {
        if (CurrentPolicy == null)
        {
            CurrentPolicy = new Policy(mdp);
            Debug.Log("No Policy specified, evaluating a random policy.");
        }
        
        if (mdp == null) throw new NullReferenceException("No MDP specified.");
    }
    
    public Task ShowActionSpritesAtopStateValueVisuals()
    {
        if (CurrentPolicy == null) throw new ArgumentNullException();
        
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            var currentAction      = CurrentPolicy.GetAction(state);
            var currentStateVisual = _stateSpaceVisualStates[state.StateIndex];
                currentStateVisual.UpdateVisibleActionFromPolicy(currentAction);
        }

        return Task.CompletedTask;
    }

    // ╔══════════════════════════════════════╗
    // ║ Main Execution Controlled Algorithms ║
    // ╚══════════════════════════════════════╝

    // ───────────────────
    //  Policy Evaluation 
    // ───────────────────
    
    /// <summary>
    /// Todo Properly comment this
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="stateValueFunction"></param>
    public async Task PolicyEvaluationControlAsync(CancellationToken cancellationToken, StateValueFunction stateValueFunction = null)
    {

        _currentAlgorithmExecutionIterations = 0;  // TODO problems with the iterations. Needs fixing
        
        // Todo Fix this
        
        var stateValueFunctionV = stateValueFunction ?? new StateValueFunction(mdp);

        if (CurrentStateValueFunction != null && stateValueFunction == null)
        {
            stateValueFunctionV = CurrentStateValueFunction;
        }

        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        SetKeepGoingTrue();

        while (keepGoing)
        {
            switch (algorithmViewLevel)
            {
                case BySweep:

                    stateValueFunctionV =
                        await algorithms.SingleStateSweepAsync(mdp, CurrentPolicy, gamma, stateValueFunctionV);

                    await SetAllStateHeightsAsync(stateValueFunctionV);
                    
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    // Note: Iterations are incremented inside of algorithms.SingleStateSweepAsync
                    
                    break;
                
                case ByStatePE:
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        
                        float valueOfState = await algorithms.BellmanBackUpAsync(
                            mdp, CurrentPolicy.GetAction(state), gamma, state, stateValueFunctionV);
                        
                        // float valueOfState = await algorithms.BellmanBackUpValueOfStateAsync(
                        //     mdp, CurrentPolicy, gamma, state, stateValueFunctionV);
                
                        stateValueFunctionV.SetValue(state, valueOfState);
                
                        await SetIndividualStateHeightAsync(state, valueOfState);
                        
                        await Task.Delay(playSpeed, cancellationToken);
                    }
                    
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    stateValueFunctionV.Iterations++;

                    break;
                
                case ByTransition:
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        // var vS = new Value {TheValue = 0f};
                        
                        float valueOfState = 0;
                        
                        await SetIndividualStateHeightAsync(state, valueOfState);
                        
                        var action = CurrentPolicy.GetAction(state);
                        
                        foreach (var transition in mdp.TransitionFunction(state, action))
                        {
                            
                            float valueFromSuccessor = 
                                await algorithms.SingleTransitionBackupAsync(mdp, gamma, transition, stateValueFunctionV);

                            valueOfState = await algorithms.IncrementValueAsync(valueOfState, valueFromSuccessor);
                            
                            // vS.AdjustValue(valueFromSuccessor);
                            
                            await SetIndividualStateHeightAsync(state, valueOfState);
                            
                            // await SetIndividualStateHeightAsync(state, vS.TheValue);

                            await Task.Delay(playSpeed, cancellationToken);
                        }
                        
                        stateValueFunctionV.SetValue(state, valueOfState);
                        
                        await SetIndividualStateHeightAsync(state, valueOfState);

                        await Task.Delay(playSpeed, cancellationToken);
                        
                    }
                    
                    // await Task.Delay(playSpeed, cancellationToken);
                    
                    stateValueFunctionV.Iterations++;
                    
                    break;
                
                case Suspended:
                    
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    break;
            }
            
            // // FOR DEBUG 
            //
            // if (debugIterations % 10 == 0) Debug.Log(debugIterations);
            //
            // debugIterations++;
            //
            // // END DEBUG
            
            
            if (stateValueFunctionV.MaxChangeInValueOfStates() < theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;

            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);
            
            if (algorithmViewLevel != Suspended) _currentAlgorithmExecutionIterations++;
        }
        
        CurrentStateValueFunction = stateValueFunctionV;

        _currentAlgorithmExecutionIterations = 0;
    }
    
    // ────────────────────
    //  Policy Improvement 
    // ────────────────────

    public async Task<Policy> PolicyImprovementControlledAsync(CancellationToken cancellationToken)
    {
        var stateValueFunction = CurrentStateValueFunction ?? new StateValueFunction(mdp);
        
        var improvedPolicy = new Policy();

        var actionValueFunctionQ = new ActionValueFunction();

        switch (algorithmViewLevel)
        {
            case ByStatePI:
                
                foreach (var state in mdp.States.Where(state => state.IsStandard()))
                {
                    var oldAction = CurrentPolicy.GetAction(state);
                    
                    foreach (var action in state.ApplicableActions)
                    {
                        float stateActionValueQsa = await algorithms.BellmanBackUpAsync(
                            mdp, action.Action, gamma, state, stateValueFunction);
                        
                        actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
                    }
                    
                    var argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
                    
                    improvedPolicy.SetAction(state, argMaxAction);
                    
                    if (oldAction != argMaxAction) SetActionImage(state, argMaxAction);

                    await Task.Delay(playSpeed, cancellationToken);
                }
                
                break;
            
            case ByActionPI:
                
                foreach (var state in mdp.States.Where(state => state.IsStandard()))
                {
                    var oldAction = CurrentPolicy.GetAction(state);
                    
                    foreach (var action in state.ApplicableActions)
                    {
                        float stateActionValueQsa = await algorithms.BellmanBackUpAsync(
                            mdp, action.Action, gamma, state, stateValueFunction);
                        
                        actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
                        
                        // Todo await SetIndividualActionHeightAsync(state, action, stateActionValueQsa);

                        await Task.Delay(playSpeed, cancellationToken);
                    }
                    
                    var argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
                    
                    improvedPolicy.SetAction(state, argMaxAction);
                    
                    if (oldAction != argMaxAction) SetActionImage(state, argMaxAction);
                }
                
                break;
            
            case ByTransition:

                foreach (var state in mdp.States.Where(state => state.IsStandard()))
                {
                    foreach (var action in state.ApplicableActions)
                    {
                        var stateActionValueQsa = 0f;

                        foreach (var transition in mdp.TransitionFunction(state, action))
                        {
                            float actionValueFromSuccessor =
                                await algorithms.SingleTransitionBackupAsync(mdp, Gamma, transition,
                                    stateValueFunction);
                            
                            stateActionValueQsa += actionValueFromSuccessor;
                            
                            // Todo await SetIndividualActionHeightAsync(state, action, stateActionValueQsa);

                            await Task.Delay(playSpeed, cancellationToken);
                        }
                        
                        actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
                    }
                    
                    var argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
                    
                    improvedPolicy.SetAction(state, argMaxAction);
                    
                    if (CurrentPolicy.GetAction(state) != argMaxAction) SetActionImage(state, argMaxAction);
                }
                
                break;
        }
        
        _previousPolicyStack.Push(CurrentPolicy);

        CurrentPolicy = improvedPolicy;

        return improvedPolicy;
    }
    
    
    // ──────────────────
    //  Policy Iteration 
    // ──────────────────
    
    public async Task PolicyIterationControlledAsync()
    {
        
    }
    
    // ─────────────────
    //  Value Iteration 
    // ─────────────────
    
    public async Task ValueIterationControlledAsync()
    {
        
    }
    
    
    // ╔══════════════════════╗
    // ║ Not Currently In Use ║
    // ╚══════════════════════╝
    
    /// <summary>
    /// TODO Improve because Temporary. Break this up into evaluation and visualization
    /// </summary>
    public void EvaluateAndVisualizeStateValues()
    {
        EnsureMdpAndPolicyAreNotNull();
        
        StateValueFunction valueOfCurrentPolicy = algorithms.PolicyEvaluation(
            mdp, 
            CurrentPolicy, 
            gamma, 
            theta,
            boundIterations,
            maximumIterations,
            debugMode);

        SetAllStateHeights(valueOfCurrentPolicy);
    }
    
    public async void PolicyEvaluationFullControl(CancellationToken cancellationToken, StateValueFunction stateValue = null)
    {
        Debug.Log($"Started with speed of: {algorithmExecutionSpeed}");
        
        var i = 0;  // Internal iteration count
        
        var stateValueFunctionV = stateValue ?? new StateValueFunction(mdp);
        
        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        // To reset the keepGoing field to true if the algorithm was previously killed due to an infinite loop
        SetKeepGoingTrue();
        
        while (keepGoing)
        {
            switch (algorithmViewLevel)
            {
                case BySweep:
                    
                    Debug.Log("BySweep started");
                    
                    stateValueFunctionV =
                        algorithms.SingleStateSweep(mdp, CurrentPolicy, gamma, stateValueFunctionV);

                    await SetAllStateHeightsAsync(stateValueFunctionV);
                    
                    if (algorithmExecutionSpeed < 0.0001) await Task.Yield();
                    else await Task.Delay(TimeSpan.FromMilliseconds(algorithmExecutionSpeed), cancellationToken);

                    i++;
                    
                    Debug.Log("BySweep Worked");
                    
                    break;
                
                case ByStatePE:
                    
                    Debug.Log("ByState Started");
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        float valueOfState = algorithms.BellmanBackUpValueOfState(
                            mdp, CurrentPolicy, gamma, state, stateValueFunctionV);
                
                        stateValueFunctionV.SetValue(state, valueOfState);
                
                        SetIndividualStateHeight(state, valueOfState);
                        
                        if (algorithmExecutionSpeed < 0.0001) await Task.Yield();
                        else await Task.Delay(TimeSpan.FromMilliseconds(algorithmExecutionSpeed), cancellationToken);
                    }
                    
                    stateValueFunctionV.Iterations++;

                    i++;
                    
                    Debug.Log("ByState Worked");
                    
                    break;
                
                case ByTransition:
                    
                    Debug.Log("ByTransition Started");
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        float valueOfState = 0;
                        
                        var action = CurrentPolicy.GetAction(state);
                        
                        foreach (var transition in mdp.TransitionFunction(state, action))
                        {
                            float          probability = transition.Probability;
                            MarkovState successorState = mdp.States[transition.SuccessorStateIndex];
                            float               reward = transition.Reward;
                            float     valueOfSuccessor = stateValueFunctionV.Value(successorState);
                            float       zeroIfTerminal = successorState.IsTerminal() ? 0 : 1;

                            valueOfState += algorithms.SingleTransitionBackup(
                                probability, reward, gamma, valueOfSuccessor, zeroIfTerminal);
                            
                            stateValueFunctionV.SetValue(state, valueOfState);
                            
                            SetIndividualStateHeight(state, valueOfState);
                            
                            if (algorithmExecutionSpeed < 0.0001) await Task.Yield();
                            else await Task.Delay(TimeSpan.FromMilliseconds(algorithmExecutionSpeed), cancellationToken);
                        }
                    }
                    
                    stateValueFunctionV.Iterations++;

                    i++;
                    
                    Debug.Log("ByTransition Worked");
                    
                    break;
            }
            
            SetAllStateHeights(stateValueFunctionV);
                    
            if (algorithmExecutionSpeed < 0.0001) await Task.Yield();
            else await Task.Delay(TimeSpan.FromMilliseconds(algorithmExecutionSpeed), cancellationToken);
            
            if (stateValueFunctionV.MaxChangeInValueOfStates() < theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;

            uiController.UpdateNumberOfIterations(stateValueFunctionV.Iterations);
            
            Debug.Log("PolicyEval Full Control Done");
        }

        CurrentStateValueFunction = stateValueFunctionV;
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
        => action switch
        {
            GridAction.Left => -1,
            GridAction.Down => -mdp.Width,
            GridAction.Right => 1,
            GridAction.Up => mdp.Width,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

    // public List<MarkovTransition> CalculateTransitions(string transitionProbabilityRules)
    // {
    //     List<MarkovTransition> transitions = new List<MarkovTransition>();
    //     
    //     switch (transitionProbabilityRules)
    //     {
    //                     
    //     }
    //
    //     return transitions;
    // }
}
// 0.0, 0.001, 0.01, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99, 0.999

public class Value
{
    public float TheValue { get; set; }

    public void AdjustValue(float val) => TheValue += val;
    
}