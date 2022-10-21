using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

/// <summary>
/// <para>
/// The <c>MdpManager</c> class managers most of the behaviour of the general solver. The <c>UIController</c> and it
/// communicate back and forth to handle most behaviour.
/// </para>
/// <para>
/// Where this can get a little confusing is the <c>MdpManager</c>'s interactions with the <c>Algorithms</c> class.
/// Originally I had intended that the <c>Algorithms</c> class would run all the algorithms (it wasn't supposed to hold
/// any state), however, in wanting to run all the major algorithms "anytime" and also be able to dynamically control
/// the execution of them, it became apparent that the manager (which holds state) would need to have each unravelled
/// part of the algorithms running in the manager itself so that it has access to all the <c>MdpManager</c>'s fields and
/// properties.
/// </para>
/// <para>
/// <b>Author:</b> <i>Christopher Howell</i>
/// </para>
/// </summary>
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

    public              Transform          updateTrail;
    
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

    public        bool                     actionSpritesDisplayed;

    public              Vector2            randomStateValueBounds;
    
    private const       BellmanScenes      Title              = BellmanScenes.Title;
    
    private const       BellmanScenes      DynamicProgramming = BellmanScenes.MdpSolver;
   
    private const       BellmanScenes      MdpBuilder         = BellmanScenes.MdpBuilder;

    public              GameObject         rabbit;

    public        bool                     stateValueObjectsVisible  = true;
    public        bool                     stateValueValuesVisible   = true;
    public        bool                     actionValueObjectsVisible = true;

    
    // ┌───────────────────────────────────────────────────┐
    // │ Algorithm Focus Level and Execution Speed Control │
    // └───────────────────────────────────────────────────┘
    public int                             playSpeed = 1;
    
    public int                             algorithmViewLevel;
    
    public bool                            maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK = true;

    public bool                            paused;

    public bool                            stepped;

    private const int                      BySweep      = 0;

    private const int                      ByState      = 1;

    private const int                      ByAction     = 2;

    private const int                      ByTransition = 3;

    private const int                      Paused       = -1;

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

    public  TextAsset                      mdpFileToLoad;
 
    public  bool                           mdpLoaded;

    private string                         _mdpForReset;

    public List<Policy>                    policiesHistory = new List<Policy>();

    private float                          _progressOfAlgorithm;

    private readonly Dictionary<int,State> _stateSpaceVisualStates = new Dictionary<int, State>();

    private Dictionary<int,GridSquareData> _stateSpaceGridSquares = new Dictionary<int, GridSquareData>();
    public Dictionary<int, GameObject>     StateQuads { get; set; } = new Dictionary<int, GameObject>();

    public  float                          theta = 1e-10f;

    public List<StateValueFunction>        valueFunctionsHistory = new List<StateValueFunction>();

    
    // ┌────────────┐
    // │ Properties │
    // └────────────┘
    public float Gamma
    {
        get => gamma;
        set => gamma = value;
    }

    public MDP Mdp { get; set; }

    public float Theta
    {
        get => theta;
        set => theta = value;
    }

    // ╔════════════════════════╗
    // ║ NORMAL UNITY FUNCTIONS ║
    // ╚════════════════════════╝
    public void Awake()
    {
        algorithms   = gameObject.AddComponent<Algorithms>();
        currentUi    = GameObject.FindGameObjectWithTag("PolicyEvaluationUI").GetComponent<Canvas>();
        uiController = currentUi.GetComponent<UIController>();

        GameManager.instance.SetMdpManager(this);

        if (GameManager.instance.sendMdp)
        {
            uiController.uiMdpSelector.index = 9;
            uiController.uiMdpSelector.UpdateUI();
            LoadMdpFromGameManager();
        }
    }
    

    // ╔═══════════════════════════════════════════════════╗
    // ║ MDP SERIALIZATION/DESERIALIZATION & INSTANTIATION ║
    // ╚═══════════════════════════════════════════════════╝
    
    
    private MDP CreateFromJson(string jsonString) => JsonUtility.FromJson<MDP>(jsonString);

    /// <summary>
    /// <para>
    /// The GameManager controls the flow between scenes carrying over the necessary data. For example, if we've created
    /// a grid world in the grid world builder and we want to run the algorithms on it, GameManager handles the
    /// transition from the builder to the solver handing off the built gridworld then deleting it from its state. I do
    /// this so it will throw a null exception error if I inadvertently try access the data from the GameManger when I
    /// shouldn't.
    /// </para>
    /// </summary>
    private async void LoadMdpFromGameManager()
    {
        Mdp = await InstantiateMdpVisualisationAsync(GameManager.instance.currentMdp);

        _mdpForReset = GameManager.instance.backupMDP;
        
        mdpLoaded = true;
        
        uiController.SetRunFeaturesActive();
        uiController.uiMdpSelector.index = 9;
        uiController.uiMdpSelector.UpdateUI();
        uiController.SetEnvironmentDynamicsVisuals(GetRulesString(GetCurrentMDPDynamics()));
        
        if (uiController.PolicyEvaluationMode()) SwitchOffActionValues();
        else SwitchOnActionValues();
        
        // GameManager instances of MDPs, value functions, and policies get reset to null after transitioning scenes
        // Also sets the send flag false.
        GameManager.instance.currentMdp = null;
        GameManager.instance.backupMDP = null;
        GameManager.instance.sendMdp    = false;
    }
    
    /// <summary>
    /// Loads a gridworld from persistant storage into the solver.
    /// </summary>
    /// <param name="filepath">String representation of the filepath</param>
    public async void LoadMdpFromFilePath(string filepath)
    {
        TextAsset mdpFromFile = Resources.Load<TextAsset>(filepath);
        
        string mdpJsonRepresentation = mdpFromFile.text;
        
        Mdp = CreateFromJson(mdpJsonRepresentation);

        _mdpForReset = mdpJsonRepresentation;
        
        Mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(mdpJsonRepresentation));
        
        mdpLoaded = true;
        
        uiController.SetRunFeaturesActive();
    }

    public async void LoadNonPersistentCustomMDP()
    {
        
        _mdpForReset = string.Copy(GameManager.instance.currentCustomMDP);
        
        Mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(GameManager.instance.currentCustomMDP));
        
        mdpLoaded = true;
    }

    public async void ResetMDP()
    {
        Assert.IsNotNull(_mdpForReset);
        
        Mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(_mdpForReset));
        
        mdpLoaded = true;
        
        uiController.SetRunFeaturesActive();
    }

    /// <summary>
    /// <para>
    /// Generates all the visuals and sets up the solver for whichever MDP gridworld we are wanting to run the
    /// algorithms on.
    /// </para>
    /// </summary>
    /// <param name="mdpToBuild"></param>
    /// <returns></returns>
    private async Task<MDP> InstantiateMdpVisualisationAsync(MDP mdpToBuild)
    {
        // Resets the grid square holder (this allows access to the toggle visibility the white boxes with the indices
        // and coordinates). 
        ResetGridSquareDictionary();
        
        var mdpForCreation = mdpToBuild; // Inevitably this is unnecessary I did it for some reason at some point. 
        
        // This block does all the offsets and so forth so the grid actually forms.
        _offsetToCenterVector = new Vector2((-mdpForCreation.Width / 2f), (-mdpForCreation.Height / 2f));
        
        if (mdpForCreation.Height > 1) {_offsetToCenterVector += _offsetValuesFor2DimensionalGrids;}
        
        float stateCubeDimensions = 1 - GapBetweenStates;
        
        // Holds everything so that when we reset, change MDP, etc, we can just delete all the current stuff in one go.
        var stateSpace = Instantiate(
            stateSpacePrefab, 
            transform, 
            true);

        // Main loops to handle instantiating the actual grid.
        var id = 0;
        
        for (var y = 0; y < mdpForCreation.Height; y++)
        {
            for (var x = 0; x < mdpForCreation.Width; x++)
            {
                // Calls the method below to deal with the individual states.
                var state = InstantiateIndividualState(mdpForCreation, stateCubeDimensions, x, y, stateSpace, id);
                
                // All of this is to create and label the grid squares.
                var gridSquarePosition = new Vector3(_offsetToCenterVector.x + x, 0f, _offsetToCenterVector.y + y);
                
                var gridSquare = Instantiate(gridSquarePrefab, gridSquarePosition, Quaternion.identity, stateSpace);

                var gridSquareData = gridSquare.GetComponent<GridSquareData>();
                
                gridSquareData.SetGridSquareVisualInformation(x, y, id);
                
                _stateSpaceGridSquares.Add(id, gridSquareData);
                
                id++;
            }
        }

        return await Task.FromResult(mdpForCreation);
    }

    /// <summary>
    /// This monstrosity is what creates the visual representations of the both the V(s) and Q(s,a) also assigns its
    /// necessary information. It's a complex little beast because the hardest thing about all this is figuring out how
    /// to visually express each part of the algorithm/equation/model/or anything else. Whether it's comprehensible or
    /// incoherent depends entirely on how exhausted I was trying to solve what ever annoying UI related problem at the
    /// time.
    /// </summary>
    /// <param name="mdp"></param>
    /// <param name="stateXandZDimensions"></param>
    /// <param name="x">X-Coordinate of the state</param>
    /// <param name="y">Y-Coordinate of the state</param>
    /// <param name="stateSpace">State space GameObject of which the state is a child</param>
    /// <param name="id">Index of the state</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private Transform InstantiateIndividualState(MDP mdp, float stateXandZDimensions, int x, int y, Transform stateSpace, int id)
    {
        // These set all the dimensions and positions of the visuals. 
        var stateType = mdp.States[id].TypeOfState;
        
        var   scale              = new Vector3(stateXandZDimensions, initialStateValueForTesting, stateXandZDimensions);
        var   statePosition      = new Vector3(_offsetToCenterVector.x + x, (scale.y / 2), _offsetToCenterVector.y + y);
          
        // var obstacleScale        = new Vector3(stateXandZDimensions, 1f, stateXandZDimensions);
        // var   obstaclePosition   = new Vector3(_offsetToCenterVector.x + x, (obstacleScale.y / 2), _offsetToCenterVector.y + y);
        var   obstacleScale      = new Vector3(1f, 0.2f, 1f);
        var   obstaclePosition   = new Vector3(_offsetToCenterVector.x + x, 0, _offsetToCenterVector.y + y);
        
        float terminalGoalScale  = mdp.States[id].Reward;
        var   terminalScale      = new Vector3(stateXandZDimensions, terminalGoalScale, stateXandZDimensions);
        var   terminalPosition   = new Vector3(_offsetToCenterVector.x + x, (terminalGoalScale / 2), _offsetToCenterVector.y + y);
        
        var   goalScale          = new Vector3(stateXandZDimensions, terminalGoalScale, stateXandZDimensions);
        var   goalPosition       = new Vector3(_offsetToCenterVector.x + x, (terminalGoalScale / 2), _offsetToCenterVector.y + y);
        
        Transform state;
        
        State fullStateObject;

        // This switch block instantiates whatever version of the state, the state is (obstacle is the black boxes,
        // terminals are red, goals are green, standard is, well, standard)
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

        // This is to save framerate with a HUGE state space. Rendering text in large volumes really slows things down. 
        if (mdp.StateCount <= 1000) return state;
        fullStateObject.hoverCanvas.enabled = false;
        fullStateObject.hoverCanvas.gameObject.SetActive(false);

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
    public async Task<StateValueFunction> PolicyEvaluationControlAsync(
        CancellationToken cancellationToken, 
        StateValueFunction stateValueFunction = null, 
        Policy policy = null)
    {

        // Turns off UI features that can crash the system if clicked during algorithm execution.
        uiController.DisableRunFeatures();
        
        _currentAlgorithmExecutionIterations = 0;  

        // Checks which policy to use. If there isn't a policy already displayed (Policy Eval or Policy Iteration
        // running from fresh MDP) it displays the policy. Note: that the default policy input parameter is null.
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
        
        ResetMainLoopBoolConditionToTrue();

        float maxDelta;
        
        while (maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK)
        {
            // Control flow handles the algorithm's execution speed for displaying how the algorithm functions.
            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);
            
            switch (algorithmViewLevel)
            {
                case BySweep:
                    
                    if (paused) await RunPauseLoop(cancellationToken);
                    
                    // Calculates the state values over a single sweep of the state space.
                    stateValueFunctionV =
                        await algorithms.SingleStateSweepAsync(Mdp, currentPolicy, Gamma, stateValueFunctionV);

                    // Sets the heights of the state values to correspond to the values calculated.
                    await SetAllStateHeightsAsync(stateValueFunctionV);
                    
                    await Task.Delay(playSpeed, cancellationToken);
                    
                    break;
                
                // The next three cases cascade because the control flow for the delays functions more effectively with
                // conditionals. It's like this currently because I haven't yet had a chance to adjust it. Moreover, I'd
                // rather wait until I can brainstorm with someone else to figure out whether there might be a better solution.
                case ByState:
                case ByAction:
                case ByTransition:
                    
                    // The rabbit is the orb with a tail that floats above the state. It indicates where the algorithm
                    // is currently in the state sweep.
                    EnableRabbit();

                    if (paused) await RunPauseLoop(cancellationToken);
                    
                    foreach (var state in Mdp.States.Where(state => state.IsStandard()))
                    {
                        SetRabbitPosition(StateQuads[state.StateIndex].transform.position);

                        if (paused) await RunPauseLoop(cancellationToken);

                        // Here it runs the actual back up first—computing it here is a relic of a previous way I was
                        // trying to visualise the individual transitions.
                        // That is it computes V(s) = Σ_s' P(s'|s,a)[r(s')+ 𝛄V(s')]
                        float actualValueOfState =
                            await algorithms.BellmanBackUpValueOfStateAsync(
                                Mdp, currentPolicy, Gamma, state, stateValueFunctionV);

                        var action = currentPolicy.GetAction(state);
                        
                        // Descends down if the user has selected to visualize the individual transitions
                        if (algorithmViewLevel == ByTransition)
                        {
                            foreach (var transition in Mdp.TransitionFunction(state, action))
                            {
                                if (paused) await RunPauseLoop(cancellationToken);

                                // This creates the little visual trails that shoot from the successor states (s') back
                                // to the state.  
                                await SendTransitionTrailFromSuccessorToVSorQSA(transition, state, action, false);
                                                                                    
                                await Task.Delay(playSpeed, cancellationToken);
                            }  
                        }

                        if (paused) await RunPauseLoop(cancellationToken);
                        
                        // Finally, here it sets the value of the state
                        stateValueFunctionV.SetValue(state, actualValueOfState);
                        
                        // This sets the state value heights to correspond to the values—think, makes state boxes be
                        // height of value.
                        await SetIndividualStateHeightAsync(state, actualValueOfState);

                        if (algorithmViewLevel <= ByAction)
                        {
                            await Task.Delay(playSpeed, cancellationToken);
                        }
                    }
                    
                    DisableRabbit();
                    
                    break;
            }
            
            // Standard P.Eval. check of whether the max absolute difference is within the convergence threshold ( Θ )
            // we have set in the parameters.
            if (stateValueFunctionV.MaxChangeInValueOfStates() < Theta) break;
            
            // k-Iterations check if we've set it so that it's running finitely, IOW we've set a max K.
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;
            
            _currentAlgorithmExecutionIterations = stateValueFunctionV.Iterations;

            stateValueFunctionV.Iterations++;

            
            // Progress Update to UI, that is, sends the iteration count and the max absolute difference to the UI for 
            // displaying.
            maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
            
            await SendProgressToUIForDisplayAsync(maxDelta);
            
            await uiController.SetMaxDeltaTextAsync(maxDelta);
            
            DisableRabbit();
            
            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);
        }
        
        currentStateValueFunction = stateValueFunctionV;
        
        uiController.SetRunFeaturesActive();

        // Progress Update to UI...Yes I've put this in again...why, you ask...I can't quite remember, but I do remember
        // something being weird at some point and this fixing it. I haven't taken the time to go find out why.
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
    /// The optional policy here is set to the global <code>currentPolicy</code> field. This enables live editing of the
    /// policy during the execution of the algorithm.
    /// </param>
    /// <returns>
    /// StateValueFunction object. NOTE: It's not always necessary because it's running anytime and directly updating
    /// the global <c>currentStateValueFunction</c> field.
    /// </returns>
    /// <remarks>
    /// Todo Add Russell and Norvig's implementation option.
    /// </remarks>
    public async Task<StateValueFunction> PolicyEvaluationNoDelay(
        CancellationToken cancellationToken,
        StateValueFunction stateValueFunction = null, 
        Policy policy = null)
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
        
        ResetMainLoopBoolConditionToTrue();

        float maxDelta;

        while (maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK)
        {
            stateValueFunctionV = await algorithms.SingleStateSweepAsync(Mdp, currentPolicy, Gamma, stateValueFunctionV);
            
            if (stateValueFunctionV.MaxChangeInValueOfStates() < Theta) break;
            
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
    public async Task<Policy> PolicyImprovementControlledAsync(
        CancellationToken cancellationToken, 
        StateValueFunction stateValueFunction = null)
    {
        
        // ┌──────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        // │ I have removed most comments from the internals of the remaining algs (this, Policy Iteration, and Value │
        // │ Iteration) because it is effectively the same as expressed in Policy Evaluation above. Moreover, each of │
        // │ the methods are commented.                                                                               │
        // └──────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        uiController.DisableRunFeatures();
        
        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);

        var improvedPolicy      = new Policy();
    
        var actionValueFunctionQ = new ActionValueFunction();
        
        if (focusAndFollowMode) EnableRabbit();
        
        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
        {
            if (focusAndFollowMode) SetRabbitPosition(StateQuads[state.StateIndex].transform.position);

            if (paused) await RunPauseLoop(cancellationToken);
            
            foreach (var action in state.ApplicableActions)
            {
                if (paused) await RunPauseLoop(cancellationToken);
                
                float stateActionValueQsa = await algorithms.CalculateActionValueAsync(
                    Mdp, state, action.Action, Gamma, stateValueFunctionV);

                if (algorithmViewLevel == ByTransition)
                {
                    foreach (var transition in Mdp.TransitionFunction(state, action))
                    {
                        if (paused) await RunPauseLoop(cancellationToken);
                        
                        await SendTransitionTrailFromSuccessorToVSorQSA(transition, state, action.Action);
                        
                        float valueFromSuccessor = 
                            await algorithms.CalculateSingleTransitionAsync(Mdp, Gamma, transition, stateValueFunctionV);
                        
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
    
            if (algorithmViewLevel == ByState) await Task.Delay(playSpeed, cancellationToken);
         
        }
        
        currentPolicy = improvedPolicy;
    
        uiController.SetRunFeaturesActive();
        
        DisableRabbit();
        
        return improvedPolicy;
    }

    

    // ──────────────────
    //  Policy Iteration 
    // ──────────────────
    /// <summary>
    /// Asynchronous Execution speed controlled version of policy iteration. 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="stateValueFunction"></param>
    /// <param name="policy"></param>
    public async Task PolicyIterationControlledAsync(
        CancellationToken cancellationToken, 
        StateValueFunction stateValueFunction = null, 
        Policy policy = null)
    {
        ResetPolicyRecord();
        
        ResetStateValueRecord();

        var internalIterations = 0;
        
        var valueOfPolicy = AssignStateValueFunction(stateValueFunction);

        var newPolicy = AssignPolicy(policy);

        await SetAllStateHeightsAsync(valueOfPolicy);

        ResetMainLoopBoolConditionToTrue();
        
        while (maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK)
        {
            if (paused) await RunPauseLoop(cancellationToken);
            
            var oldPolicy = newPolicy.Copy();
            
            SwitchOffActionValues();
            
            if (focusAndFollowMode)
            {
                valueOfPolicy = await PolicyEvaluationControlAsync(cancellationToken, valueOfPolicy, oldPolicy);
                // uiController.PauseAlgorithm();
                // await RunPauseLoop(cancellationToken);
            }
            else
            {
                valueOfPolicy = await PolicyEvaluationNoDelay(cancellationToken, valueOfPolicy, oldPolicy);
            }
            
            SwitchOnActionValues();

            if (focusAndFollowMode && playSpeed < 100)
            {
                algorithmViewLevel = ByAction;
                playSpeed = 50;
            }
            
            await Task.Delay(playSpeed, cancellationToken);
            
            newPolicy = await PolicyImprovementControlledAsync(cancellationToken, valueOfPolicy);

            if (focusAndFollowMode)
            {
                uiController.UpdateAlgorithmExecutionSpeedValue();
                uiController.SendLevelInformationToMdp();
                // uiController.PauseAlgorithm();
                // await RunPauseLoop(cancellationToken);
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
    

    
    /// <summary>
    /// Asynchronous Execution speed controlled version of value iteration.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="stateValueFunction"></param>
    public async Task ValueIterationControlledAsync(
        CancellationToken cancellationToken, 
        StateValueFunction stateValueFunction = null)
    {
        uiController.DisableRunFeatures();
        
        _currentAlgorithmExecutionIterations = 0;

        var stateValueFunctionV = AssignStateValueFunction(stateValueFunction);

        await SetAllStateHeightsAsync(stateValueFunctionV);
        
        ResetMainLoopBoolConditionToTrue();
        
        var actionValueFunctionQ = new ActionValueFunction(Mdp);

        float maxDelta;
        
        while (maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK)
        {
            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

            actionValueFunctionQ = new ActionValueFunction(Mdp);
            

            switch (algorithmViewLevel)
            {
                case BySweep:

                    if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

                    await algorithms.SingleSweepValueIteration(Mdp, stateValueFunctionV, actionValueFunctionQ, Gamma);
            
                    var tasks = new List<Task>();
                
                    foreach (var state in Mdp.States.AsParallel().Where(state => state.IsStandard()))
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
                    
                    foreach (var state in Mdp.States.Where(state => state.IsStandard()))
                    {
                        if (focusAndFollowMode)
                        {
                            EnableRabbit();
                            
                            SetRabbitPosition(StateQuads[state.StateIndex].transform.position);
                        }

                        if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);
                        
                        foreach (var action in state.ApplicableActions)
                        {
                            if (algorithmViewLevel == Paused) await RunPauseLoop(cancellationToken);

                            float stateActionValueQsa = await algorithms.CalculateActionValueAsync(Mdp, state, action.Action, Gamma, stateValueFunctionV);
                            
                            if (algorithmViewLevel == ByTransition)
                            {
                                foreach (var transition in Mdp.TransitionFunction(state, action))
                                {
                                    if (paused) await RunPauseLoop(cancellationToken);
                                    
                                    await SendTransitionTrailFromSuccessorToVSorQSA(transition, state, action.Action);
                                    
                                    await Task.Delay(playSpeed, cancellationToken);
                                }
                            }
            
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

            if (stateValueFunctionV.MaxChangeInValueOfStates() < Theta) break;
            
            if (boundIterations && stateValueFunctionV.Iterations >= maximumIterations) break;

            // Progress Update to UI
            maxDelta = stateValueFunctionV.MaxChangeInValueOfStates();
            await SendProgressToUIForDisplayAsync(maxDelta);
            await uiController.SetMaxDeltaTextAsync(maxDelta);
            
            await uiController.UpdateNumberOfIterationsAsync(stateValueFunctionV.Iterations);

            stateValueFunctionV.Iterations++;

        }
        
        currentPolicy = algorithms.GeneratePolicyFromArgMaxActions(Mdp, actionValueFunctionQ);
        
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
    
    
    private string GetRulesString(MdpRules dynamics)
    {
        switch (dynamics)
        {
            case MdpRules.SlipperyWalk:
                return "SW";
            case MdpRules.RussellAndNorvig:
                return "RN";
            case MdpRules.RandomWalk:
                return "RW";
            case MdpRules.FrozenLake:
                return "FL";
            case MdpRules.GrastiensWindFromTheNorth:
                return "NW";
            case MdpRules.DrunkBonanza:
            case MdpRules.Deterministic:
                return "D";
            default:
                throw new ArgumentOutOfRangeException(nameof(dynamics), dynamics, null);
        }
    }
    
    // ────────
    //  Rabbit 
    // ────────
    public void SetRabbitPosition(Vector3 position) => rabbit.transform.position = position;

    public void EnableRabbit() => rabbit.SetActive(true);
    
    public void DisableRabbit() => rabbit.SetActive(false);
    
    
    // ─────────────────────────
    //  Transition Update Trail  <- These methods handle everything on the Manager side to shoot the little transition 
    // ─────────────────────────    trails off from each transition.
    private Task SendTransitionTrailFromSuccessorToVSorQSA(MarkovTransition transition, MarkovState state, GridAction action, bool destinationActionValue = true)
    {
        var valueFromPosition = GetOriginatingPosition(transition);

        Transform target, valueToPosition;
        
        if (destinationActionValue)
        {
            target = GetActionValueTargetPosition(state, action);
            
            valueToPosition = GetActionValueTargetPosition(state, action);
        }
        else
        {
            target = GetStateValueTargetPosition(state);
            
            valueToPosition = GetStateValueTargetPosition(state);
        }

        var trl = Instantiate(updateTrail, valueFromPosition, Quaternion.Euler(Vector3.zero));

        var transTail = trl.GetComponent<TransitionTrail>();
        
        transTail.target = target.gameObject;

        // This actually fires off the transition trail. The TransitionTrail class linearly interpolates the position of 
        // the trail at each frame before destroying itself once it's reached its destination. 
        transTail.releaseTrail = true;

        Debug.Log(
            $"Transition from s{state.StateIndex} at {valueFromPosition} to s{transition.SuccessorStateIndex} at {valueToPosition}");
        return Task.CompletedTask;
    }

    private Transform GetStateValueTargetPosition(MarkovState state) => _stateSpaceVisualStates[state.StateIndex].hoveringText.transform;
    
    private Transform GetActionValueTargetPosition(MarkovState state, GridAction action) => _stateSpaceVisualStates[state.StateIndex].actionTargetGameObjects[(int) action].transform;

    private Vector3 GetOriginatingPosition(MarkovTransition transition) => _stateSpaceVisualStates[transition.SuccessorStateIndex].hoveringText.transform.position;
    
    
    // ─────────────────────── 
    //  Progress of Algorithm  <- Deals with the data related to expressing: iterations, max difference, etc. 
    // ───────────────────────    Communicates with uiController.
    public float ProgressOfAlgorithm
    {
        get => _progressOfAlgorithm;
        set
        {
            double progressValue = ((Math.Log10(value) / Math.Log10(Theta)) * 100);
            if (progressValue < 0) progressValue = 0;
            _progressOfAlgorithm = (float) progressValue;
        }
    }

    private void SendProgressToUIForDisplay(float maxDelta)
    {
        var progressValue = (float)((Math.Log10(maxDelta) / Math.Log10(Theta)) * 100);
        if (progressValue < 0) progressValue = 0;
        uiController.SetProgressBarPercentage(progressValue);
    }
    
    private Task SendProgressToUIForDisplayAsync(float maxDelta)
    {
        var progressValue = (float)((Math.Log10(maxDelta) / Math.Log10(Theta)) * 100);
        if (progressValue < 0) progressValue = 0;
        uiController.SetProgressBarPercentage(progressValue);
        return Task.CompletedTask;
    }

    
    
    // ───────────────────────── 
    //  State Information Panel  <- This is the panel that comes up when you click on an individual visual state.
    // ───────────────────────── 
    public MarkovState GetStateFromCurrentMdp(int stateIndex) => Mdp.States[stateIndex];

    public Dictionary<string, string> GetStateAndActionInformationForDisplayAndEdit(int stateIndex)
    {
        var state = Mdp.States[stateIndex];

        var stateVisualRepresentation = _stateSpaceVisualStates[stateIndex];
        string stateName = StateNameFormatted(stateIndex);
        var stateReward = $"R(s, a, {stateName}) = {state.Reward}";
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

    public string StateNameFormatted(int stateIndex) => $"<b>S<sub> {stateIndex}</sub></b>";

    public string ActionInStateFormatted(int stateIndex)
    {
        string action = currentPolicy != null ? currentPolicy.GetAction(stateIndex).ToString() : "n/a";
        return $"π({StateNameFormatted(stateIndex)}) = {action}";
    }

    public void EditRewardOfState(int stateIndex, float newReward) => Mdp.States[stateIndex].Reward = newReward;
    
    public void SetIndividualStateHeight(MarkovState state, float value) => SetIndividualStateHeight(state.StateIndex, value);

    public async void SetIndividualStateHeight(int stateIndex, float value) => await _stateSpaceVisualStates[stateIndex].UpdateStateHeightAsync(value);
    
    public void EditCurrentPolicy(MarkovState state, MarkovAction action) => currentPolicy.SetAction(state, action.Action);

    public void EditCurrentPolicy(MarkovState state, GridAction action) => currentPolicy.SetAction(state, action);

    public void EditCurrentPolicy(int stateIndex, int action) => currentPolicy.SetAction(stateIndex, (GridAction) action);


    // ────────────────────────── 
    //  State and Action Heights  <- Methods (and overloads) to set all heights (and positions of invisible target 
    // ──────────────────────────    objects). There are both the regular synchronous and asynchronous versions in here.
    private void SetAllStateHeights(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
            SetIndividualStateHeight(state, valueOfCurrentPolicy.Value(state));
    }

    /// <summary>
    /// Sets the visual representations of the state values. Note, it's currently set to leverage PLINQ (run in
    /// parallel) for a, hopefully, speed up on very large state spaces—that said, prolly does squat. 
    /// </summary>
    /// <param name="valueOfCurrentPolicy"></param>
    private async Task SetAllStateHeightsAsync(StateValueFunction valueOfCurrentPolicy)
    {
        foreach (var state in Mdp.States.AsParallel().Where(state => state.IsStandard()))
            // foreach (var state in mdp.States.Where(state => state.IsStandard()))
            await SetIndividualStateHeightAsync(state, valueOfCurrentPolicy.Value(state));
    }
    public Task SetIndividualStateHeightAsync(MarkovState state, float value)
    {
        var setHeight = SetIndividualStateHeightAsync(state.StateIndex, value);
        return setHeight;
    }

    public Task SetIndividualStateHeightAsync(int stateIndex, float value) => _stateSpaceVisualStates[stateIndex].UpdateStateHeightAsync(value);


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

    
    // ─────────────────
    //  Reset functions 
    // ─────────────────
    private void ResetPolicyRecord()
    {
        if (policiesHistory.Count > 0) policiesHistory = new List<Policy>();
    }

    private void ResetStateValueRecord()
    {
        if (valueFunctionsHistory.Count > 0) valueFunctionsHistory = new List<StateValueFunction>();
    }

    public void ResetPolicy() => currentPolicy = null;

    public void ResetCurrentStateValueFunction() => currentStateValueFunction = null;

    public void ResetStateQuadDictionary() => StateQuads = new Dictionary<int, GameObject>();

    /// <summary>
    /// <para>
    /// Resets the grid square holder (this allows access to the toggle visibility the white boxes with the indices and
    /// coordinates).
    /// </para>
    /// </summary>
    public void ResetGridSquareDictionary() => _stateSpaceGridSquares = new Dictionary<int, GridSquareData>();
    
    public void GenerateRandomStateValueFunction()
    {
        currentStateValueFunction = 
            new StateValueFunction(
                Mdp, 
                randomStateValueBounds.x, 
                randomStateValueBounds.y);
        
        SetAllStateHeights(currentStateValueFunction);
    }

    public void GenerateRandomPolicy()
    {
        if (Mdp == null) return;
        currentPolicy = new Policy(Mdp);
        
        SetAllActionSprites(currentPolicy);
    }
    
    public MDP GetCurrentMdp() => Mdp;

    
    // ───────── 
    //  Visuals  <- Methods and overloads handling the action sprites and the visibility of each element (Coordinates 
    // ─────────    and indices on the grid squares, visual state value heights, visual state value values, visual
    //              action value heights, current policy action sprites, and previous policy action sprites.
    
    public void SetActionImage(MarkovState state, MarkovAction action) => SetActionImage(state.StateIndex, (int) action.Action);

    public void SetActionImage(MarkovState state, GridAction action) => SetActionImage(state.StateIndex, (int) action);

    public void SetActionImage(int stateIndex, int action) => _stateSpaceVisualStates[stateIndex].UpdateActionSprite((GridAction) action);

    public void SetAllActionSprites(Policy policy)
    {
        foreach (var stateAction in policy.GetPolicyDictionary())
            SetActionImage(stateAction.Key, (int) stateAction.Value);
    }

    public Task ShowActionSpritesAtopStateValueVisuals()
    {
        if (currentPolicy == null) throw new ArgumentNullException();

        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
        {
            var currentAction = currentPolicy.GetAction(state);
            var currentStateVisual = _stateSpaceVisualStates[state.StateIndex];
            currentStateVisual.UpdateActionSprite(currentAction);
        }

        return Task.CompletedTask;
    }
    
    // The instruction for this comes from the UI controller from the state interactions object in the visual state.
    public void ToggleStateHighlight(int stateIndex) => _stateSpaceVisualStates[stateIndex].StateHighlightToggle();

    // UI Calls this to toggle the visibility.
    public void Toggle(string toToggle)
    {
        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
        {
            switch (toToggle)
            {
                case "GridSquare":
                    _stateSpaceGridSquares[state.StateIndex].ToggleVisibility();
                    break;
                case "StateValue":
                    _stateSpaceVisualStates[state.StateIndex].ToggleStateValue();
                    break;
                case "StateValueText":
                    _stateSpaceVisualStates[state.StateIndex].ToggleStateValueText();
                    break;
                case "ActionObjects":
                    _stateSpaceVisualStates[state.StateIndex].ToggleActionObjects();
                    break;
                case "ActionSprites":
                    _stateSpaceVisualStates[state.StateIndex].ToggleActionSprites();
                    break;
                case "PreviousActionSprites":
                    _stateSpaceVisualStates[state.StateIndex].TogglePreviousActionSprites();
                    break;
                default:
                    throw new ArgumentException("Incorrect string passed.");
            }
        }
    }

    public void SwitchOnActionValues()
    {
        if (!mdpLoaded) return;
        
        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
        {
            _stateSpaceVisualStates[state.StateIndex].TogglOnActionObjects();
        }
    }
    
    public void SwitchOffActionValues()
    {
        if (!mdpLoaded) return;
        
        foreach (var state in Mdp.States.Where(state => state.IsStandard()))
        {
            _stateSpaceVisualStates[state.StateIndex].TogglOffActionObjects();
        }
    }

    /// <summary>
    /// <para>
    /// Runs an infinite loop until the pause button in the UI sends the signal to release it. There is a
    /// (semi-functional) step button. It's buggy as hell at the moment and I haven't had the time to fix it. 
    /// </para>
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task RunPauseLoop(CancellationToken cancellationToken)
    {
        while (paused && !stepped) await Task.Delay(5, cancellationToken);

        if (stepped) stepped = false;
    }

    public void SetMainLoopBoolConditionFalse() => maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK = false;

    public void ResetMainLoopBoolConditionToTrue() => maxDeltaGreaterThanThetaOrNumberOfIterationsLessThanK = true;
    
    public void EnsureMdpAndPolicyAreNotNull()
    {
        if (currentPolicy == null)
        {
            currentPolicy = new Policy(Mdp);

            ShowActionSpritesAtopStateValueVisuals();

            Debug.Log("No Policy specified, evaluating a random policy.");
        }

        if (Mdp == null) throw new NullReferenceException("No MDP specified.");
    }
    

    /// <summary>
    /// <para>
    /// Assigns the relevant value function object. Either a new one, a continuation of current one, or a specific
    /// inputted one (for example, assigning random values, for showing monotonic convergence).
    /// </para>
    /// </summary>
    /// <param name="stateValueFunction">StateValueFunction object containing the state values (<c>float</c>)</param>
    /// <returns>The actual state value function</returns>
    private StateValueFunction AssignStateValueFunction(StateValueFunction stateValueFunction)
    {
        StateValueFunction stateValueFunctionV;

        if (stateValueFunction != null) stateValueFunctionV = stateValueFunction;

        else if (currentStateValueFunction != null) stateValueFunctionV = currentStateValueFunction;

        else stateValueFunctionV = new StateValueFunction(Mdp);

        return stateValueFunctionV;
    }

    /// <summary>
    /// <para>
    /// Checks which policy to use. If there isn't a policy already displayed (Policy Eval or Policy Iteration
    /// running from fresh MDP) it displays the policy.
    /// </para>
    /// </summary>
    /// <param name="policy"></param>
    /// <returns></returns>
    private Policy AssignPolicy(Policy policy)
    {
        Policy policyPi;

        if (policy != null)
            policyPi = policy;

        else if (currentPolicy != null)
            policyPi = currentPolicy;

        else
            policyPi = new Policy(Mdp);

        return policyPi;
    }

    public MdpRules GetCurrentMDPDynamics() => Mdp.MdpRules;

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
        bool outOfBoundsTop             = destination > Mdp.States.Count - 1;
        bool outOfBoundsBottom          = destination < 0;
        bool outOfBoundsLeft            = origin       % Mdp.Width == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = (origin + 1) % Mdp.Width == 0 && action == GridAction.Right;
        bool hitObstacle = Mdp.ObstacleStates.Contains(destination);

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
            GridAction.Down => -Mdp.Width,
            GridAction.Right => 1,
            GridAction.Up => Mdp.Width,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
}
