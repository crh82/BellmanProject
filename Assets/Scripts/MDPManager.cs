using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Creates the model of the environment — the underlying MDP data rather than the physical/visual MDP.
/// <include file='include.xml' path='docs/members[@name="mdpmanager"]/MDPManager/*'/>
public class MdpManager : MonoBehaviour
{
    // ┌─────────┐
    // │ Prefabs │
    // └─────────┘
    public              Transform          statePrefab;

    public              Transform          stateActionPrefab;

    public              Transform          obstaclePrefab;

    public              Transform          terminalPrefab;

    public              Transform          goalPrefab;

    public              Transform          stateSpacePrefab;

    public              Transform          gridSquarePrefab;
    
    // ┌─────────────────────────┐
    // │ GridWorld Visualisation │
    // └─────────────────────────┘
    [Range(0, 1)] 
    private const float                    GapBetweenStates = 0.4f;

    public        float                    initialStateValueForTesting; // Todo remove after built

    public              Canvas             currentUi;

    public              UIController       uiController;
    
    private             Vector2            _offsetToCenterVector;

    private readonly    Vector2            _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);

    public bool                            actionSpritesDisplayed;

    public              Vector2            randomStateValueBounds;
    
    private const       BellmanScenes      Title              = BellmanScenes.Title;
    
    private const       BellmanScenes      DynamicProgramming = BellmanScenes.DynamicProgramming;
   
    private const       BellmanScenes      MdpBuilder         = BellmanScenes.MdpBuilder;

    public              GameObject         rabbit;

    
    // ┌───────────────────────────────────────────────────┐
    // │ Algorithm Focus Level and Execution Speed Control │
    // └───────────────────────────────────────────────────┘
    public int                             playSpeed = 1;
    
    public int                             algorithmViewLevel;
    
    public bool                            keepGoing = true;

    public bool                            paused;

    public bool                            stepped;

    private const int                      BySweep = 0;

    private const int                      ByState = 1;

    private const int                      ByAction = 2;

    private const int                      ByTransition = 3;

    private const int                      Paused = -1;

    public bool                            focusAndFollowMode = true;


    // ┌─────────────────────────┐
    // │ MDP data and algorithms │
    // └─────────────────────────┘
    private GridAction[]                   _actions = Enum.GetValues(typeof(GridAction)) as GridAction[];
    
    public  Algorithms                     algorithms;
    
    public  bool                           boundIterations = true;
    
    public  ActionValueFunction            currentActionValueFunction;
    
    private int                            _currentAlgorithmExecutionIterations; // Reset after each alg execution.

    public  Policy                         currentPolicy;

    public  StateValueFunction             currentStateValueFunction;

    public  bool                           debugMode;
    
    public  Vector2                        dimensions = Vector2.one;

    public  float                          gamma = 1f;

    public  int                            maximumIterations = 100_000;
    
    public  MDP                            mdp;
    
    public  TextAsset                      mdpFileToLoad;
 
    public  bool                           mdpLoaded;

    public List<Policy>                    policiesHistory = new List<Policy>();

    private float                          _progressOfAlgorithm;

    private readonly Dictionary<int,State> _stateSpaceVisualStates = new Dictionary<int, State>();
    public Dictionary<int, GameObject>     StateQuads { get; set; } = new Dictionary<int, GameObject>();

    public  float                          theta = 1e-10f;

    public List<StateValueFunction>        valueFunctionsHistory = new List<StateValueFunction>();

    
    // ╔════════════════════════╗
    // ║ NORMAL UNITY FUNCTIONS ║
    // ╚════════════════════════╝
    public void Awake()
    {
        algorithms   = gameObject.AddComponent<Algorithms>();
        currentUi    = GameObject.FindGameObjectWithTag("PolicyEvaluationUI").GetComponent<Canvas>();
        uiController = currentUi.GetComponent<UIController>();

        if (GameManager.instance.sendMdp) LoadMdpFromGameManager();
        
        // MDP grastiens = MdpAdmin.GenerateMdp(
        //     "BloodMoon", 
        //     MdpRules.RussellAndNorvig,
        //     new[] {50, 50},
        //     new int[] {901,801,701},
        //     new int[] {225,775},
        //     new int[] {549,450,550,650,551},
        //     0.001f,
        //     -2f,
        //     5f,
        //     false);
        // grastiens.States[900].Reward = 10f;
        // MdpAdmin.SaveMdpToFile(grastiens, "Assets/Resources/TestMDPs");
    }
    

    // ╔═══════════════════════════════════════════════════╗
    // ║ MDP SERIALIZATION/DESERIALIZATION & INSTANTIATION ║
    // ╚═══════════════════════════════════════════════════╝
    private MDP CreateFromJson(string jsonString) => JsonUtility.FromJson<MDP>(jsonString);

    private async void LoadMdpFromGameManager()
    {
        mdp = await InstantiateMdpVisualisationAsync(GameManager.instance.currentMdp);
        
        mdpLoaded = true;
        
        uiController.SetRunFeaturesActive();
        
        // GameManager instances of MDPs, value functions, and policies get reset to null after transitioning scenes
        // Also sets the send flag false.
        GameManager.instance.currentMdp = null;
        GameManager.instance.sendMdp    = false;
    }
    
    public async void LoadMdpFromFilePath(string filepath)
    {
        string mdpJsonRepresentation = File.ReadAllText(filepath);
        mdp = CreateFromJson(mdpJsonRepresentation);
        mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(mdpJsonRepresentation));

        mdpLoaded = true;
        
        uiController.SetRunFeaturesActive();
    }

    private async Task<MDP> InstantiateMdpVisualisationAsync(MDP mdpFromFile)
    {
        var mdpForCreation = mdpFromFile;
        
        var id = 0;
        
        _offsetToCenterVector = new Vector2((-mdpForCreation.Width / 2f), (-mdpForCreation.Height / 2f));
        
        if (mdpForCreation.Height > 1) {_offsetToCenterVector += _offsetValuesFor2DimensionalGrids;}
        
        float stateCubeDimensions = 1 - GapBetweenStates;

        var stateSpace = Instantiate(
            stateSpacePrefab, 
            transform, 
            true);

        for (var y = 0; y < mdpForCreation.Height; y++)
        {
            for (var x = 0; x < mdpForCreation.Width; x++)
            {
                
                var state = InstantiateIndividualState(mdpForCreation, stateCubeDimensions, x, y, stateSpace, id);

                var gridSquarePosition = new Vector3(_offsetToCenterVector.x + x, 0f, _offsetToCenterVector.y + y);
                
                Instantiate(gridSquarePrefab, gridSquarePosition, Quaternion.identity, stateSpace);

                
                
                id++;
            }
        }

        return await Task.FromResult(mdpForCreation);
    }

    private Transform InstantiateIndividualState(MDP mdp, float stateXandZDimensions, int x, int y, Transform stateSpace, int id)
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
                state           = Instantiate(terminalPrefab, terminalPosition, Quaternion.identity, stateSpace);
                fullStateObject = state.GetComponent<State>();
                fullStateObject.stateIndex = id;
                fullStateObject.stateType = stateType;
                fullStateObject.statePosition = terminalPosition;
                Task.Run(fullStateObject.UpdateStateHeightAsync(terminalGoalScale).RunSynchronously);
                break;
            
            case StateType.Obstacle:
                state           = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, stateSpace);
                fullStateObject = state.GetComponent<State>();
                fullStateObject.stateIndex = id;
                fullStateObject.stateType = stateType;
                fullStateObject.statePosition = obstaclePosition;
                fullStateObject.SetStateScale(obstacleScale);
                break;
            
            case StateType.Goal:
                state           = Instantiate(goalPrefab, goalPosition, Quaternion.identity, stateSpace);
                fullStateObject = state.GetComponent<State>();
                fullStateObject.stateIndex = id;
                fullStateObject.stateType = stateType;
                fullStateObject.statePosition = goalPosition;
                Task.Run(fullStateObject.UpdateStateHeightAsync(terminalGoalScale).RunSynchronously);
                break;
            
            case StateType.Standard:
                state = Instantiate(stateActionPrefab, statePosition, Quaternion.identity, stateSpace);
                fullStateObject = state.GetComponent<State>();
                fullStateObject.stateIndex = id;
                fullStateObject.stateType = stateType;
                fullStateObject.statePosition = statePosition;
                Task.Run(fullStateObject.UpdateStateHeightAsync(scale.y).RunSynchronously);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        state.name      = $"{x}{y}";

        fullStateObject.stateIndex = id;
        
        fullStateObject.DistributeMdpManagerReferenceToComponents(this);

        _stateSpaceVisualStates[id] = fullStateObject;
        
        StateQuads.Add(id, fullStateObject.stateQuad);

        if (mdp.StateCount > 1000)
        {
            fullStateObject.hoverCanvas.enabled = false;
            fullStateObject.hoverCanvas.gameObject.SetActive(false);
        }

        return state;
    }

    
    
    // ╔══════════════════════════════════════╗
    // ║ Main Execution Controlled Algorithms ║
    // ╚══════════════════════════════════════╝

    // ───────────────────
    //  Policy Evaluation 
    // ───────────────────
    /// <summary>
    /// Asynchronous implementation of the policy evaluation algorithm. Runs with control over the execution speed
    /// (delays the execution at specified points to update visuals) It roughly follows Sutton and Barto's
    /// implementation. The <c>await</c> keywords are for the asynchronous execution—the question was raised.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation Token cancels the asynchronous execution of the algorithm.
    /// </param>
    /// <param name="stateValueFunction">
    /// When the method takes in an optional V(s) it sets all the state values (data and visualised heights)
    /// </param>
    /// <param name="policy">
    /// The optional policy here is set to the global <c>currentPolicy</c> field. This enables live editing of the
    /// policy during the execution of the algorithm.
    /// </param>
    /// <returns>
    /// StateValueFunction object. NOTE: It's not always necessary because it's running anytime and directly updating
    /// the global <c>currentStateValueFunction</c> field.
    /// </returns>
    /// <remarks>
    /// Todo Add Russell and Norvig's implementation option.
    /// </remarks>
    public async Task<StateValueFunction> PolicyEvaluationControlAsync(CancellationToken cancellationToken, StateValueFunction stateValueFunction = null, Policy policy = null)
    {

        // Turns off UI features that can crash the system if clicked during algorithm execution.
        uiController.DisableRunFeatures();
        
        _currentAlgorithmExecutionIterations = 0;  

        // Checks which policy to use. If there isn't a policy already displayed (Policy Eval or Policy Iteration
        // running from fresh MDP) it displays the policy.
        currentPolicy = AssignPolicy(policy);

        if (!actionSpritesDisplayed)
        {
            actionSpritesDisplayed = true;
            SetAllActionSprites(currentPolicy);
        }
        
        // Checks which V(s) to use. Either a new one, a continuation of current one, or a specific inputted one (for
        // showing monotonic convergence, for example). Then displays the state heights.
        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);
        
        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        SetKeepGoingTrue();

        float maxDelta;
        
        while (keepGoing)
        {
            // Control flow handles the algorithm's execution speed for displaying how the algorithm functions.
            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);
            
            switch (algorithmViewLevel)
            {
                case BySweep:
                    
                    if (paused) await RunPauseLoop(cancellationToken);
                    
                    stateValueFunctionV =
                        await algorithms.SingleStateSweepAsync(mdp, currentPolicy, Gamma, stateValueFunctionV);

                    await SetAllStateHeightsAsync(stateValueFunctionV);
                    
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    break;
                
                // The next three cases cascade because the control flow for the delays functions more effectively with
                // conditionals.
                case ByState:
                case ByAction:
                case ByTransition:
                    
                    // The rabbit is the orb with a tail that floats above the state. It indicates where the algorithm
                    // is currently in the state sweep.
                    EnableRabbit();

                    if (paused) await RunPauseLoop(cancellationToken);
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        SetRabbitPosition(StateQuads[state.StateIndex].transform.position);
                        
                        // if (focusAndFollowMode) await uiController.FocusCornerCamera(state.StateIndex);

                        if (paused) await RunPauseLoop(cancellationToken);

                        float actualValueOfState =
                            await algorithms.BellmanBackUpValueOfStateAsync(
                                mdp, currentPolicy, Gamma, state, stateValueFunctionV);
                        
                        // Resets the state height to zero to demonstrate that the V(s) <- Bellman equation is an
                        // assignment. It's not incrementing the value. Subtle difference.
                        if (algorithmViewLevel == ByTransition)
                        {
                            await SetIndividualStateHeightAsync(state, actualValueOfState);
                        }
                        
                        var action = currentPolicy.GetAction(state);
                        
                        if (algorithmViewLevel == ByTransition)
                        {
                            foreach (var transition in mdp.TransitionFunction(state, action))
                            {
                                if (paused) await RunPauseLoop(cancellationToken);
                            
                                float valueFromSuccessor = 
                                    await algorithms.CalculateSingleTransitionAsync(mdp, gamma, transition, stateValueFunctionV);

                                // actualValueOfState = await algorithms.IncrementValueAsync(actualValueOfState, valueFromSuccessor);
                            
                                await SetIndividualStateHeightAsync(state, valueFromSuccessor);

                                await Task.Delay(playSpeed, cancellationToken);
                            }  
                        }

                        if (paused) await RunPauseLoop(cancellationToken);
                        
                        stateValueFunctionV.SetValue(state, actualValueOfState);
                        
                        await SetIndividualStateHeightAsync(state, actualValueOfState);

                        if (algorithmViewLevel <= ByAction)
                        {
                            await Task.Delay(playSpeed, cancellationToken);
                        }
                    }
                    DisableRabbit();
                    break;
            }
            
            if (stateValueFunctionV.MaxChangeInValueOfStates() < theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;
            
            _currentAlgorithmExecutionIterations = stateValueFunctionV.Iterations;

            stateValueFunctionV.Iterations++;

            
            // Progress Update to UI
            maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
            await SendProgressToUIForDisplayAsync(maxDelta);
            await uiController.SetMaxDeltaTextAsync(maxDelta);
            
            DisableRabbit();
            
            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);
        }
        
        currentStateValueFunction = stateValueFunctionV;

        // _currentAlgorithmExecutionIterations = 0;
        
        uiController.SetRunFeaturesActive();

        // Progress Update to UI
        maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
        
        await SendProgressToUIForDisplayAsync(maxDelta);
        
        await uiController.SetMaxDeltaTextAsync(maxDelta);
        
        DisableRabbit();
        
        return stateValueFunctionV;
    }

    /// <summary>
    /// As in <see cref="PolicyEvaluationControlAsync"/> except that this runs with almost no delay aside from updating
    /// progress panel in UI at specified points.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation Token cancels the asynchronous execution of the algorithm.
    /// </param>
    /// <param name="stateValueFunction">
    /// When the method takes in an optional V(s) it sets all the state values (data and visualised heights)
    /// </param>
    /// <param name="policy">
    /// The optional policy here is set to the global <code>currentPolicy</code> field. This enables live editing of the policy during the execution of the algorithm.
    /// </param>
    /// <returns>
    /// StateValueFunction object. NOTE: It's not always necessary because it's running anytime and directly updating
    /// the global <c>currentStateValueFunction</c> field.
    /// </returns>
    /// <remarks>
    /// Todo Add Russell and Norvig's implementation option.
    /// </remarks>
    public async Task<StateValueFunction> PolicyEvaluationNoDelay(CancellationToken cancellationToken,
        StateValueFunction stateValueFunction = null, Policy policy = null)
    {
        // Turns off UI features that can crash the system if clicked during algorithm execution.
        uiController.DisableRunFeatures();
        
        _currentAlgorithmExecutionIterations = 0;  

        // Checks which policy to use. If there isn't a policy already displayed (Policy Eval or Policy Iteration
        // running from fresh MDP) it displays the policy.
        currentPolicy = AssignPolicy(policy);
        
        
        if (!actionSpritesDisplayed)
        {
            actionSpritesDisplayed = true;
            SetAllActionSprites(currentPolicy);
        }
        
        // Checks which V(s) to use. Either a new one, a continuation of current one, or a specific inputted one (for
        // showing monotonic convergence, for example). Then displays the state heights.
        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);
        
        SetKeepGoingTrue();

        float maxDelta;

        while (keepGoing)
        {
            stateValueFunctionV = await algorithms.SingleStateSweepAsync(mdp, currentPolicy, Gamma, stateValueFunctionV);
            
            if (stateValueFunctionV.MaxChangeInValueOfStates() < theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;
            
            _currentAlgorithmExecutionIterations = stateValueFunctionV.Iterations;

            stateValueFunctionV.Iterations++;

            maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();

            // uiController.SetMaxDeltaText(maxDelta);

            await uiController.SetMaxDeltaTextAsync(maxDelta);
            await SendProgressToUIForDisplayAsync(maxDelta);

            switch (stateValueFunctionV.Iterations)
            {
                case var i when i < 50:
                    await Task.Yield();
                    break;
                case var i when i >= 50:
                    if (stateValueFunctionV.Iterations % 100 == 0) await Task.Yield();
                    break;
                case var i when i >= 1000:
                    if (stateValueFunctionV.Iterations % 250 == 0) await Task.Yield();
                    break;
            }

            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);
        }
        
        // Sets all the visuals to make sure they're good at the end. NOTE: this is probably only necessary because I'm 
        // new to working with async stuff. Undoubtedly there's a better way of doing this.
        
        maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
        
        await SendProgressToUIForDisplayAsync(maxDelta);
        
        await uiController.SetMaxDeltaTextAsync(maxDelta);
        
        await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);
        
        currentStateValueFunction = stateValueFunctionV;
        
        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        uiController.SetRunFeaturesActive();

        return stateValueFunctionV;
    }
    
    // ────────────────────
    //  Policy Improvement 
    // ────────────────────
    /// <summary>
    /// Execution speed controlled implementation of Policy Improvement. There's no real need to implement a no delay
    /// version of this given that it only does a single sweep of the state space. As with all these algorithms the
    /// necessity to run them in the manager, rather than from the <see cref="Algorithms"/> class is to enable the live
    /// control of the execution. The execution control flow  
    /// </summary>
    /// <param name="cancellationToken">Token cancels the asynchronous execution</param>
    /// <param name="stateValueFunction">Values of the states under the policy to be improved.</param>
    /// <returns>An improved policy</returns>
    public async Task<Policy> PolicyImprovementControlledAsync(CancellationToken cancellationToken, StateValueFunction stateValueFunction = null)
    {
        uiController.DisableRunFeatures();
        
        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);

        var improvedPolicy      = new Policy();
    
        var actionValueFunctionQ = new ActionValueFunction();
        
        if (focusAndFollowMode) EnableRabbit();
        
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            if (focusAndFollowMode) SetRabbitPosition(StateQuads[state.StateIndex].transform.position);
            
            // if (focusAndFollowMode) await uiController.FocusCornerCamera(state.StateIndex);
            
            if (paused) await RunPauseLoop(cancellationToken);
            
            foreach (var action in state.ApplicableActions)
            {
                if (paused) await RunPauseLoop(cancellationToken);
                
                float stateActionValueQsa = await algorithms.CalculateActionValueAsync(
                mdp, state, action.Action, Gamma, stateValueFunctionV);

                if (algorithmViewLevel == ByTransition)
                {
                    foreach (var transition in mdp.TransitionFunction(state, action))
                    {
                        // TODO fire off fading coroutine highlighting the transition
                        // transition.SuccessorStateIndex
                        
                        if (paused) await RunPauseLoop(cancellationToken);
                            
                        float valueFromSuccessor = 
                            await algorithms.CalculateSingleTransitionAsync(mdp, Gamma, transition, stateValueFunctionV);
                        
                        await Task.Delay(playSpeed, cancellationToken);
                    }
                }
                
                actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
    
                await SetIndividualActionHeightAsync(state, action, stateActionValueQsa);
                
                if (algorithmViewLevel == ByAction) await Task.Delay(playSpeed, cancellationToken);
            }
    
            if (paused) await RunPauseLoop(cancellationToken);
            
            var argMaxAction = actionValueFunctionQ.ArgMaxAction(state);
                    
            improvedPolicy.SetAction(state, argMaxAction);
                    
            if (currentPolicy.GetAction(state) != argMaxAction) SetActionImage(state, argMaxAction);
    
            if (algorithmViewLevel == ByState)
            {
                // await SetAllActionHeightsAsync(state, actionValueFunctionQ);
                await Task.Delay(playSpeed, cancellationToken);
            }
        }
        
        currentPolicy = improvedPolicy;
    
        uiController.SetRunFeaturesActive();
        
        DisableRabbit();
        
        return improvedPolicy;
    }
    
    // ──────────────────
    //  Policy Iteration 
    // ──────────────────
    
    public async Task PolicyIterationControlledAsync(
        CancellationToken cancellationToken, StateValueFunction stateValueFunction = null, Policy policy = null)
    {
        ResetPolicyRecord();
        
        ResetStateValueRecord();

        var internalIterations = 0;
        
        var valueOfPolicy = AssignStateValueFunction(stateValueFunction);

        var newPolicy = AssignPolicy(policy);

        await SetAllStateHeightsAsync(valueOfPolicy);

        SetKeepGoingTrue();
        
        while (keepGoing)
        {
            if (paused) await RunPauseLoop(cancellationToken);
            
            // Todo add pseudocode thing
            var oldPolicy = newPolicy.Copy();

            if (focusAndFollowMode)
            {
                valueOfPolicy = await PolicyEvaluationControlAsync(cancellationToken, valueOfPolicy, oldPolicy);
                uiController.PauseAlgorithm();
                await RunPauseLoop(cancellationToken);
            }
            else
            {
                valueOfPolicy = await PolicyEvaluationNoDelay(cancellationToken, valueOfPolicy, oldPolicy);
            }

            newPolicy = await PolicyImprovementControlledAsync(cancellationToken, valueOfPolicy);

            if (focusAndFollowMode)
            {
                uiController.PauseAlgorithm();
                await RunPauseLoop(cancellationToken);
            }
            
            if (oldPolicy.Equals(newPolicy)) break;

            if (boundIterations && internalIterations >= maximumIterations) break;

            valueOfPolicy.Iterations++;
            
            internalIterations++;
            
            valueFunctionsHistory.Add(valueOfPolicy);
            
            policiesHistory.Add(newPolicy);
        }

    }
    
    // ─────────────────
    //  Value Iteration 
    // ─────────────────
    
    public async Task ValueIterationControlledAsync(CancellationToken cancellationToken, StateValueFunction stateValueFunction = null)
    {
        // Disables the run button in the UI so multiple calls can't be made. Multiple calls are a problem because the 
        // algorithm runs asynchronously.
        uiController.DisableRunFeatures();
        
        _currentAlgorithmExecutionIterations = 0;

        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);

        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        SetKeepGoingTrue();
        
        var actionValueFunctionQ = new ActionValueFunction(mdp);

        float maxDelta;
        
        while (keepGoing)
        {
            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

            actionValueFunctionQ = new ActionValueFunction(mdp);
            

            switch (algorithmViewLevel)
            {
                case BySweep:

                    if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

                    await algorithms.SingleSweepValueIteration(mdp, stateValueFunctionV, actionValueFunctionQ, Gamma);
            
                    var tasks = new List<Task>();
                
                    foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
                    {
                        tasks.Add(SetAllActionHeightsAsync(state, actionValueFunctionQ));
                    
                        tasks.Add(SetIndividualStateHeightAsync(state, stateValueFunctionV.Value(state)));
                    }

                    await Task.WhenAll(tasks);
            
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    break;
                
                case ByState:
                case ByAction:
                case ByTransition:
                    
                    foreach (var state in mdp.States.Where(state => state.IsStandard()))
                    {
                        if (focusAndFollowMode)
                        {
                            EnableRabbit();
                            
                            SetRabbitPosition(StateQuads[state.StateIndex].transform.position);
                        }
                        
                        // if (focusAndFollowMode) await uiController.FocusCornerCamera(state.StateIndex);
                        
                        if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);
                        
                        foreach (var action in state.ApplicableActions)
                        {
                            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

                            float stateActionValueQsa = await algorithms.CalculateActionValueAsync(mdp, state, action.Action, Gamma, stateValueFunctionV);
            
                            actionValueFunctionQ.SetValue(state, action, stateActionValueQsa);
            
                            await SetIndividualActionHeightAsync(state, action, stateActionValueQsa);
                    
                            if (algorithmViewLevel == ByAction)
                            {
                                await Task.Delay(playSpeed, cancellationToken);
                            }
                        }
            
                        float valueOfState = actionValueFunctionQ.MaxValue(state);
            
                        if (algorithmViewLevel == ByState)
                        {
                            await SetAllActionHeightsAsync(state, actionValueFunctionQ);
            
                            await Task.Delay(playSpeed, cancellationToken);
                        }
                
                        stateValueFunctionV.SetValue(state, valueOfState);
                
                        await SetIndividualStateHeightAsync(state, valueOfState);
                
                        await Task.Delay(playSpeed, cancellationToken);
                        
                        DisableRabbit();
                    }
                    
                    break;
            }

            currentActionValueFunction = actionValueFunctionQ;

            if (stateValueFunctionV.MaxChangeInValueOfStates() < theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;

            // Progress Update to UI
            maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
            await SendProgressToUIForDisplayAsync(maxDelta);
            await uiController.SetMaxDeltaTextAsync(maxDelta);
            
            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);

            stateValueFunctionV.Iterations++;

        }
        
        currentPolicy = algorithms.GeneratePolicyFromArgMaxActions(mdp, actionValueFunctionQ);
        
        SetAllActionSprites(currentPolicy);
        
        currentStateValueFunction = stateValueFunctionV;
        
        // Progress Update to UI
        maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
        await SendProgressToUIForDisplayAsync(maxDelta);
        await uiController.SetMaxDeltaTextAsync(maxDelta);
        
        uiController.SetRunFeaturesActive();
        
        DisableRabbit();
    }

    // ┌──────────────────────────────────────┐
    // │ HELPER/SPECIFIC GET OR SET FUNCTIONS │
    // └──────────────────────────────────────┘

    public void SetRabbitPosition(Vector3 position) => rabbit.transform.position = position;

    public void EnableRabbit() => rabbit.SetActive(true);
    
    public void DisableRabbit() => rabbit.SetActive(false);
    
    public float ProgressOfAlgorithm
    {
        get => _progressOfAlgorithm;
        set
        {
            double progressValue = ((Math.Log10(value) / Math.Log10(theta)) * 100);
            if (progressValue < 0) progressValue = 0;
            _progressOfAlgorithm = (float) progressValue;
        }
    }

    private void SendProgressToUIForDisplay(float maxDelta)
    {
        var progressValue = (float)((Math.Log10(maxDelta) / Math.Log10(theta)) * 100);
        if (progressValue < 0) progressValue = 0;
        uiController.SetProgressBarPercentage(progressValue);
    }
    
    private Task SendProgressToUIForDisplayAsync(float maxDelta)
    {
        var progressValue = (float)((Math.Log10(maxDelta) / Math.Log10(theta)) * 100);
        if (progressValue < 0) progressValue = 0;
        uiController.SetProgressBarPercentage(progressValue);
        return Task.CompletedTask;
    }

    
    public float Gamma
    {
        get => gamma;
        set => gamma = value;
    }

    public MarkovState GetStateFromCurrentMdp(int stateIndex)
    {
        return mdp.States[stateIndex];
    }

    public Dictionary<string, string> GetStateAndActionInformationForDisplayAndEdit(int stateIndex)
    {
        var state = mdp.States[stateIndex];

        var stateVisualRepresentation = _stateSpaceVisualStates[stateIndex];
        string stateName = StateNameFormatted(stateIndex);
        var stateReward = $"R({stateName}) = {state.Reward}";
        var stateValue = $"V({stateName}) = {stateVisualRepresentation.stateValue}";
        var action = state.IsStandard() ? ActionInStateFormatted(stateIndex) : $"No action because state is {state.TypeOfState.ToString()}";
        // if (CurrentPolicy != null)
        // {
        //     var action = CurrentPolicy.GetAction(stateIndex);
        // }

        var stateInformation = new Dictionary<string, string>
        {
            {"state", stateName},
            {"reward", stateReward},
            {"value", stateValue},
            {"iterations", $"Iteration: T + {_currentAlgorithmExecutionIterations}"},
            {"action", action}
        };

        // var stateInfo = $"{stateName}\n{stateReward}\n{stateValue}";

        return stateInformation;
    }

    public string StateNameFormatted(int stateIndex)
    {
        return $"<b>S</b>{stateIndex}";
    }

    public string ActionInStateFormatted(int stateIndex)
    {
        string action = currentPolicy != null ? currentPolicy.GetAction(stateIndex).ToString() : "n/a";
        return $"π({StateNameFormatted(stateIndex)}) = {action}";
    }

    public void EditRewardOfState(int stateIndex, float newReward)
    {
        mdp.States[stateIndex].Reward = newReward;
    }


    public void SetIndividualStateHeight(MarkovState state, float value)
    {
        SetIndividualStateHeight(state.StateIndex, value);
    }

    public async void SetIndividualStateHeight(int stateIndex, float value)
    {
        await _stateSpaceVisualStates[stateIndex].UpdateStateHeightAsync(value);
    }
    // => _stateSpaceVisualStates[stateIndex].UpdateHeight(value);


    private void SetAllStateHeights(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
            SetIndividualStateHeight(state, valueOfCurrentPolicy.Value(state));
    }

    public Task SetIndividualStateHeightAsync(MarkovState state, float value)
    {
        var setHeight = SetIndividualStateHeightAsync(state.StateIndex, value);
        return setHeight;
    }

    public Task SetIndividualStateHeightAsync(int stateIndex, float value)
    {
        return _stateSpaceVisualStates[stateIndex].UpdateStateHeightAsync(value);
    }

    private async Task SetAllStateHeightsAsync(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in mdp.States.AsParallel().Where(state => state.IsStandard()))
            // foreach (var state in mdp.States.Where(state => state.IsStandard()))
            await SetIndividualStateHeightAsync(state, valueOfCurrentPolicy.Value(state));
    }

    public async Task SetIndividualActionHeightAsync(MarkovState state, MarkovAction action, float stateActionValue)
    {
        await SetIndividualActionHeightAsync(state.StateIndex, action.Action, stateActionValue);
    }

    public async Task SetIndividualActionHeightAsync(int stateIndex, GridAction action, float stateActionValue)
    {
        await _stateSpaceVisualStates[stateIndex].UpdateActionHeightAsync((int) action, stateActionValue);
    }


    public async Task SetAllActionHeightsAsync(MarkovState state, ActionValueFunction actionValueFunction)
    {
        var tasks = (
            from action in state.ApplicableActions
            let actionValue = actionValueFunction.Value(state, action)
            select SetIndividualActionHeightAsync(state, action, actionValue)).ToList();

        await Task.WhenAll(tasks);
    }

    private void ResetPolicyRecord()
    {
        if (policiesHistory.Count > 0) policiesHistory = new List<Policy>();
    }

    private void ResetStateValueRecord()
    {
        if (valueFunctionsHistory.Count > 0) valueFunctionsHistory = new List<StateValueFunction>();
    }

    public void ResetPolicy()
    {
        currentPolicy = null;
    }

    public void ResetCurrentStateValueFunction()
    {
        currentStateValueFunction = null;
    }

    public void ResetStateQuadDictionary()
    {
        StateQuads = new Dictionary<int, GameObject>();
    }

    public void GenerateRandomStateValueFunction()
    {
        currentStateValueFunction = 
            new StateValueFunction(
                mdp, 
                randomStateValueBounds.x, 
                randomStateValueBounds.y);
        
        SetAllStateHeights(currentStateValueFunction);
    }

    public void GenerateRandomPolicy()
    {
        if (mdp == null) return;
        currentPolicy = new Policy(mdp);
        SetAllActionSprites(currentPolicy);
    }

    public void SetKeepGoingFalse()
    {
        keepGoing = false;
    }

    public void SetKeepGoingTrue()
    {
        keepGoing = true;
    }

    public MDP GetCurrentMdp()
    {
        return mdp;
    }

    public void EditCurrentPolicy(MarkovState state, MarkovAction action)
    {
        currentPolicy.SetAction(state, action.Action);
    }

    public void EditCurrentPolicy(MarkovState state, GridAction action)
    {
        currentPolicy.SetAction(state, action);
    }

    public void EditCurrentPolicy(int stateIndex, int action)
    {
        currentPolicy.SetAction(stateIndex, (GridAction) action);
    }


    public void SetActionImage(MarkovState state, MarkovAction action)
    {
        SetActionImage(state.StateIndex, (int) action.Action);
    }

    public void SetActionImage(MarkovState state, GridAction action)
    {
        SetActionImage(state.StateIndex, (int) action);
    }

    public void SetActionImage(int stateIndex, int action)
    {
        _stateSpaceVisualStates[stateIndex].UpdateActionSprite((GridAction) action);
    }

    public void SetAllActionSprites(Policy policy)
    {
        foreach (var stateAction in policy.GetPolicyDictionary())
            SetActionImage(stateAction.Key, (int) stateAction.Value);
    }

    public void ToggleStateHighlight(int stateIndex)
    {
        _stateSpaceVisualStates[stateIndex].StateHighlightToggle();
    }

    public void Toggle(string toToggle)
    {
        foreach (var state in mdp.States.Where(state => state.IsStandard()))
            switch (toToggle)
            {
                case "ActionObjects":
                    _stateSpaceVisualStates[state.StateIndex].ToggleActionObjects();
                    // var actionMeshesContainer = _stateSpaceVisualStates[state.StateIndex].actionMeshesContainer;
                    // actionMeshesContainer.SetActive(!actionMeshesContainer.activeSelf);
                    // _stateSpaceVisualStates[state.StateIndex].ShowActionObjects();
                    break;
                case "ActionSprites":
                    _stateSpaceVisualStates[state.StateIndex].ToggleActionSprites();
                    // var actionSpritesContainer = _stateSpaceVisualStates[state.StateIndex].actionSpritesContainer;
                    // actionSpritesContainer.SetActive(!actionSpritesContainer.activeSelf);
                    // _stateSpaceVisualStates[state.StateIndex].ShowActionSprites();
                    break;
                case "PreviousActionSprites":
                    _stateSpaceVisualStates[state.StateIndex].TogglePreviousActionSprites();
                    // var previousActionSpritesContainer = _stateSpaceVisualStates[state.StateIndex].previousActionSpritesContainer;
                    // previousActionSpritesContainer.SetActive(!previousActionSpritesContainer.activeSelf);
                    // _stateSpaceVisualStates[state.StateIndex].ShowPreviousActionSprites();
                    break;
                default:
                    throw new ArgumentException(
                        "Incorrect string passed. Check that the string is either AS, PAS, or SAO");
            }
    }

    private async Task RunPauseLoop(CancellationToken cancellationToken)
    {
        while (paused && !stepped) await Task.Delay(5, cancellationToken);

        if (stepped) stepped = false;
    }

    public void EnsureMdpAndPolicyAreNotNull()
    {
        if (currentPolicy == null)
        {
            currentPolicy = new Policy(mdp);

            ShowActionSpritesAtopStateValueVisuals();

            Debug.Log("No Policy specified, evaluating a random policy.");
        }

        if (mdp == null) throw new NullReferenceException("No MDP specified.");
    }

    public Task ShowActionSpritesAtopStateValueVisuals()
    {
        if (currentPolicy == null) throw new ArgumentNullException();

        foreach (var state in mdp.States.Where(state => state.IsStandard()))
        {
            var currentAction = currentPolicy.GetAction(state);
            var currentStateVisual = _stateSpaceVisualStates[state.StateIndex];
            currentStateVisual.UpdateActionSprite(currentAction);
        }

        return Task.CompletedTask;
    }

    private StateValueFunction AssignStateValueFunction(StateValueFunction stateValueFunction)
    {
        StateValueFunction stateValueFunctionV;

        if (stateValueFunction != null) stateValueFunctionV = stateValueFunction;

        else if (currentStateValueFunction != null) stateValueFunctionV = currentStateValueFunction;

        else stateValueFunctionV = new StateValueFunction(mdp);

        return stateValueFunctionV;
    }

    private Policy AssignPolicy(Policy policy)
    {
        Policy policyPi;

        if (policy != null)
            policyPi = policy;

        else if (currentPolicy != null)
            policyPi = currentPolicy;

        else
            policyPi = new Policy(mdp);

        return policyPi;
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