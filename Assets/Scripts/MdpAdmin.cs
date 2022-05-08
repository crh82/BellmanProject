using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


/// <summary>
/// The <c>MdpAdmin</c> creates the Canonical MDPs for serialization and persistent storage as JSON files.
/// <remarks>
/// <para>
/// In its current state it is decidedly inefficient and its outrageous complexity is the product of trying to
/// reconcile the constraints of Unity — which doesn't seem to handle serializing and deserializing nested arrays; but
/// also whether visualisation requirements are being taken into account. 
/// </para>
/// <para>
/// I was in a dark place here, a dark dark place. 
/// </para>
/// </remarks>
/// </summary>
public static class MdpAdmin
{
    private const GridAction Left  = GridAction.Left;
    private const GridAction Down  = GridAction.Down;
    private const GridAction Right = GridAction.Right;
    private const GridAction Up    = GridAction.Up;
    
    private const int SeedValue = 5;
    // public static Random RandomValueGenerator = new Random(SeedValue);


    /// <summary>
    /// Todo Deal with the problems that could arise form the obstacle, terminal, and goal arrays
    /// 
    /// 
    /// </summary>
    /// <param name="name"><c>string</c>
    /// </param>
    /// <param name="gridworldMdpRules"></param>
    /// <param name="dimensions"></param>
    /// <param name="obstacleStates"></param>
    /// <param name="terminalStates"></param>
    /// <param name="goalStates"></param>
    /// <param name="standardReward"></param>
    /// <param name="terminalReward"></param>
    /// <param name="goalReward"></param>
    /// <param name="computeTransitions">Sets whether to precompute all transitions in state space</param>
    /// <returns></returns>
    public static MDP GenerateMdp( 
        string   name,
        MdpRules gridworldMdpRules,
        int[]    dimensions,
        int[]    obstacleStates,
        int[]    terminalStates,
        int[]    goalStates,
        float    standardReward = 0,
        float    terminalReward = 0,
        float    goalReward     = 1,
        bool     computeTransitions = true
    )
    {
        
        var newMdp = new MDP
        {
            Name           = name,
            Width          = dimensions[0],
            Height         = dimensions[1],
            States         = new List<MarkovState>(),
            MdpRules       = gridworldMdpRules,
            ObstacleStates = obstacleStates,
            TerminalStates = terminalStates,
            GoalStates     = goalStates
        };
        
        InitializeStateObjects(newMdp, standardReward, terminalReward, goalReward);
        
        InitializeActionsAndTransitions(newMdp, computeTransitions);
        
        return newMdp;
    }


    private static void InitializeStateObjects(MDP newMdp, float standardReward, float terminalReward, float goalReward)
    {
        int numberOfStates = newMdp.Width * newMdp.Height;

        for (var stateIndex = 0; stateIndex < numberOfStates; stateIndex++)
        {
            var stateToAdd = new MarkovState
            {
                StateIndex = stateIndex,
                ApplicableActions = new List<MarkovAction>()
            };

            // Obstacle state
            if (newMdp.ObstacleStates.Contains(stateIndex))
            {
                stateToAdd.TypeOfState = StateType.Obstacle;
                stateToAdd.Reward = 0;
            }

            // Terminal state
            else if (newMdp.TerminalStates.Contains(stateIndex))
            {
                stateToAdd.TypeOfState = StateType.Terminal;
                stateToAdd.Reward = terminalReward;
            }

            // Goal state
            else if (newMdp.GoalStates.Contains(stateIndex))
            {
                stateToAdd.TypeOfState = StateType.Goal;
                stateToAdd.Reward = goalReward;
            }

            // Standard state 
            else
            {
                stateToAdd.TypeOfState = StateType.Standard;
                stateToAdd.Reward = standardReward;
            }

            newMdp.States.Add(stateToAdd);
        }
    }

    private static void InitializeActionsAndTransitions(MDP newMdp, bool computeTransitions)
    {
        foreach (MarkovState markovState in newMdp.States)
        {
            foreach (GridAction action in Enum.GetValues(typeof(GridAction)))
            {
                int[] stateActionPair = {markovState.StateIndex, (int) action};

                Debug.Log($"S_{markovState.StateIndex} A_{action.ToString()}");
                
                var actionToAdd = new MarkovAction
                {
                    Action = action,
                    StateAction = stateActionPair
                };

                if (computeTransitions)
                {
                    actionToAdd.Transitions = GenerateTransitions(newMdp, markovState, actionToAdd);
                }

                markovState.ApplicableActions.Add(actionToAdd);
            }
        }
    }

    public static List<MarkovTransition> GenerateTransitions(MDP mdp, MarkovState mState, MarkovAction mAction)
    {
        List<MarkovTransition> transitions;
        
        float[] probabilityDistribution = mdp.MdpRules.GetProbabilityDistributionOfActionOutcomes();
        
        switch (mdp.MdpRules)
        {
            case MdpRules.SlipperyWalk:
                
                var intended = new MarkovTransition
                {
                    State = mState.StateIndex,
                    ActionTaken = mAction.Action,
                    Probability = probabilityDistribution[0],
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mdp, mState, mAction.Action)
                };
                intended.Reward = mdp.States[intended.SuccessorStateIndex].Reward;

                var noEffect = new MarkovTransition
                {
                    State = mState.StateIndex, 
                    ActionTaken = mAction.Action,
                    Probability = probabilityDistribution[1],
                    SuccessorStateIndex = mState.StateIndex
                };
                noEffect.Reward = mdp.States[noEffect.SuccessorStateIndex].Reward;
                
                var inverseEffect = new MarkovTransition
                {
                    State = mState.StateIndex,
                    ActionTaken = mAction.Action,
                    Probability = probabilityDistribution[2],
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(
                        mdp, 
                        mState, 
                        mAction.Action.GetInverseEffectOfAction())
                };
                inverseEffect.Reward = mdp.States[inverseEffect.SuccessorStateIndex].Reward;

                transitions = new List<MarkovTransition> {intended, noEffect, inverseEffect};
                break;
            
            case MdpRules.RussellAndNorvig:
                transitions = TransitionsWithOrthogonalEffects(mdp, mState, mAction, probabilityDistribution);
                break;
            
            case MdpRules.RandomWalk:
                transitions = FullTransitionsEffects(mdp, mState, probabilityDistribution);
                break;
            
            case MdpRules.FrozenLake:
                transitions = TransitionsWithOrthogonalEffects(mdp, mState, mAction, probabilityDistribution);
                break;
            
            case MdpRules.DrunkBonanza:
                transitions = FullTransitionsEffects(mdp, mState, probabilityDistribution);
                break;

            case MdpRules.GrastiensWindFromTheNorth:
                // transitions = GrastiensRules(mdp, mState, probabilityDistribution);
                // break;
            case MdpRules.Deterministic:
            default:
                transitions = FullTransitionsEffects(mdp, mState, probabilityDistribution);
                break;
        }
    
        Assert.IsNotNull(transitions);
        return transitions;
    }

    // private static List<MarkovTransition> GrastiensRules()
    // {
    //     
    // }

    private static List<MarkovTransition> FullTransitionsEffects(
        MDP                  mdp, 
        MarkovState          mState, 
        IReadOnlyList<float> probabilityDistribution)
    {
        var transitions = new List<MarkovTransition>();
        for (int i = 0; i < 4; i++)
        {
            var newTransition = new MarkovTransition
            {
                State = mState.StateIndex,
                ActionTaken = (GridAction) i,
                Probability = probabilityDistribution[i],
                SuccessorStateIndex = GenerateSuccessorStateFromAction(mdp, mState, (GridAction) i)
            };
            newTransition.Reward = mdp.States[newTransition.SuccessorStateIndex].Reward;
            
            transitions.Add(newTransition);
        }

        return transitions;
    }

    private static List<MarkovTransition> TransitionsWithOrthogonalEffects(
        MDP          mdp, 
        MarkovState  mState, 
        MarkovAction mAction,
        float[]      probabilityDistribution)
    {
        var intended = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = mAction.Action,
            Probability = probabilityDistribution[0],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mdp, mState, mAction.Action)
        };

        intended.Reward = intended.SuccessorStateIndex == intended.State
            ? 0
            : mdp.States[intended.SuccessorStateIndex].Reward;

        var effects = mAction.Action.GetOrthogonalEffects();
        
        var orthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = mAction.Action,
            Probability = probabilityDistribution[1],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(
                mdp,
                mState,
                effects.Item1)
        };
        
        orthogonalEffect.Reward = orthogonalEffect.State == orthogonalEffect.SuccessorStateIndex
            ? 0
            : mdp.States[orthogonalEffect.SuccessorStateIndex].Reward;

        var otherOrthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = mAction.Action,
            Probability = probabilityDistribution[2],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(
                mdp,
                mState,
                effects.Item2)
        };
        
        otherOrthogonalEffect.Reward = otherOrthogonalEffect.State == otherOrthogonalEffect.SuccessorStateIndex
            ? 0
            : mdp.States[otherOrthogonalEffect.SuccessorStateIndex].Reward;

        var transitions = new List<MarkovTransition> {intended, orthogonalEffect, otherOrthogonalEffect};
        return transitions;
    }


    /// <summary>
    /// Uses the index (<c>int</c>) representation of the state and an action to calculate the successor state (again
    /// represented as by its index) during the generation of the model of the environment — in this case the gridworld.
    /// </summary>
    /// <param name="mdp">
    /// <c>MDP</c> object 
    /// </param>
    /// <param name="state">
    /// <c>int</c> representing the state index, rather than the state itself
    /// </param>
    /// <param name="action">
    /// <c>GridAction</c> representing the action taken
    /// </param>
    /// <returns>
    /// <c>int</c> representing the index of the successor state
    /// </returns>
    public static int GenerateSuccessorStateFromAction(MDP mdp, MarkovState state, GridAction action)
    {
        int successorIndex = state.StateIndex + ArithmeticEffectOfAction(mdp, action);
        if (state.IsGoal())     return state.StateIndex;
        if (state.IsTerminal()) return state.StateIndex;
        if (state.IsObstacle()) return state.StateIndex;
        if (SuccessorStateOutOfBounds(mdp, state.StateIndex, successorIndex, action)) return state.StateIndex;
        if (mdp.States[successorIndex].IsObstacle()) return state.StateIndex;
        return successorIndex;
    }
    
    // public static int GenerateSuccessorStateFromAction(int mdpWidth, int state, GridAction action)
    // {
    //     int successorState = state + ArithmeticEffectOfAction(mdpWidth, action);
    //     return SuccessorStateOutOfBounds(mdpWidth, state, successorState, action) ? state : successorState;
    // }

    public static bool SuccessorStateOutOfBounds(
        MDP mdp, 
        int stateIndex, 
        int successorIndex, 
        GridAction action)
    {
        bool outOfBoundsTop             = successorIndex   > mdp.States.Count - 1;
        bool outOfBoundsBottom          = successorIndex   < 0;
        bool outOfBoundsLeft            = stateIndex       % mdp.Width == 0 && action == Left;
        bool outOfBoundsRight           = (stateIndex + 1) % mdp.Width == 0 && action == Right;
        return outOfBoundsLeft | outOfBoundsBottom | outOfBoundsRight | outOfBoundsTop;
    }
    
    // Checks whether taking action ( a ) in state ( s ) goes out of bounds or into an obstacle. 
    // public static bool SuccessorStateOutOfBounds(int mdpWidth, int numberOfStates, MarkovState state, int successorState, GridAction action)
    // {
    //     bool outOfBoundsTop             = successorState > numberOfStates - 1;
    //     bool outOfBoundsBottom          = successorState < 0;
    //     bool outOfBoundsLeft            = state.State      % mdpWidth == 0 && action == GridAction.Left;
    //     bool outOfBoundsRight           = (state.State + 1) % mdpWidth == 0 && action == GridAction.Right;
    //     bool hitObstacle = mdp.obstacleStates.Contains(successorState);
    //
    //     return (outOfBoundsLeft   | 
    //             outOfBoundsBottom | 
    //             outOfBoundsRight  | 
    //             outOfBoundsTop    |
    //             hitObstacle) switch
    //     {
    //         true => true,
    //         _ => false
    //     };
    // }


    // public static float TriggerAction()
    // {
    //     // Todo: This isn't thread safe.
    //     double floatBetweenZeroAndOne = RandomValueGenerator.NextDouble();
    //     return (float) floatBetweenZeroAndOne;
    // }

    // public static int PerformAction(MDP mdp, int state, GridAction action)
    // {
    //     
    // }


    // Given we've enumerated states and actions, we do easy math rather than explicitly defining actions and their
    // effects.  
    public static int ArithmeticEffectOfAction(MDP mdp, GridAction action)
    {
        switch (action)
        {
            case GridAction.Left:
                return -1;
            case GridAction.Down:
                return -mdp.Width;
            case GridAction.Right:
                return 1;
            case GridAction.Up:
                return mdp.Width;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    
    // ╔═══════════════════════════════════════╗
    // ║ MDP SERIALIZATION AND DESERIALIZATION ║
    // ╚═══════════════════════════════════════╝
    
    // Loads an MDP from persistent storage.
    public static MDP LoadMdpFromFile(string mdpName)
    {
        string mdpFilePath = Application.persistentDataPath + $"/{mdpName}.json";
        
        if (!File.Exists(mdpFilePath))
        {
            throw new FileNotFoundException($"No save file named {mdpName}.json");
        }
        
        string jsonStringFromJsonFile = File.ReadAllText(mdpFilePath);
        MDP deserializedMdp = LoadMdp(jsonStringFromJsonFile);
        return deserializedMdp;
    }
    
    // Deserializes and MDP from a JSON string representation.
    public static MDP LoadMdp(string jsonString)
    {
        return JsonUtility.FromJson<MDP>(jsonString);
    }
    
    public static void SaveMdpToFile(MDP mdp, string filePath = null)
    {
        string saveFilePath = Application.persistentDataPath + $"/{mdp.Name}.json";
        if (filePath != null)
        {
            saveFilePath = $"{filePath}/{mdp.Name}.json";
        }
        string jsonRepresentationOfMdp = JsonUtility.ToJson(mdp);
        File.WriteAllText(saveFilePath, jsonRepresentationOfMdp);
    }

    public static void GenerateTestOutputAsCsv(
        MDP mdp,
        StateValueFunction startValueOfStates,
        StateValueFunction endValueOfStates, 
        Policy startPolicy, 
        Policy endPolicy, 
        string fileName)
    {
        var saveFilePath = $"Assets/TestResults/{fileName}.csv";
        var toCsv = new List<string>();
        if (toCsv == null) throw new ArgumentNullException(nameof(toCsv));
        var header = $"StartPolicyValues,EndPolicyValues,StartPolicyActions,EndPolicyActions";
        toCsv.Add(header);
        float[]  startValues        = startValueOfStates.StateValuesToFloatArray(mdp.StateCount);
        float[]  endValues          = endValueOfStates.StateValuesToFloatArray(mdp.StateCount);
        string[] startPolicyActions = startPolicy.PolicyToStringArray(mdp.States);
        string[] endPolicyActions   = endPolicy.PolicyToStringArray(mdp.States);

        for (var i = 0; i < mdp.StateCount; i++)
        {
            var line = $"{startValues[i]},{endValues[i]},{startPolicyActions[i]},{endPolicyActions[i]}";
            toCsv.Add(line);
        }
        
        File.WriteAllLines(saveFilePath, toCsv);
    }
}

/*
 * ┌──────────────────┐
 * │ Russell & Norvig │
 * └──────────────────┘
 * {S     , S     , S     , S     , S     , O   , S     , T  , S     , S     , S     , T}
 * {-0.04 , -0.04 , -0.04 , -0.04 , -0.04 , 0.0 , -0.04 , -1 , -0.04 , -0.04 , -0.04 , 1}
 *
 *
 * ┌───────────────┐
 * │ FrozenLake4x4 │
 * └───────────────┘
 * {T , S , S , T , S , S , S , T , S , T , S , T , S , S , S , S}
 * {0 , 0 , 0 , 1 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0}
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 * 
 */