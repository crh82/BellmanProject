using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class MdpAdmin
{
    private const GridAction Left  = GridAction.Left;
    private const GridAction Down  = GridAction.Down;
    private const GridAction Right = GridAction.Right;
    private const GridAction Up    = GridAction.Up;
    
    private const int SeedValue = 5;
    // public static Random RandomValueGenerator = new Random(SeedValue);
    
    public static MDP GenerateMdp(
        string   name,
        MdpRules gridworldMdpRules,
        int[]    dimensions,
        int[]    obstacleStates,
        int[]    terminalStates,
        int[]    goalStates,
        float    standardReward = 0,
        float    terminalReward = 0,
        float    goalReward = 1
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
        
        AssignStateTypesAndRewards(newMdp, standardReward, terminalReward, goalReward);
        
        InitializeTransitions(newMdp);
        
        return newMdp;
    }


    private static void AssignStateTypesAndRewards(MDP newMdp, float standardReward, float terminalReward, float goalReward)
    {
        int numberOfStates = newMdp.Width * newMdp.Height;

        for (var stateIndex = 0; stateIndex < numberOfStates; stateIndex++)
        {
            var stateToAdd = new MarkovState {StateIndex = stateIndex};

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

    private static void InitializeTransitions(MDP newMdp)
    {
        foreach (var state in newMdp.States)
        {
            foreach (GridAction action in Enum.GetValues(typeof(GridAction)))
            {
                int[] stateActionPair = {state.StateIndex, (int) action};

                var actionToAdd = new MarkovAction
                {
                    Action = action,
                    StateAction = stateActionPair
                };

                actionToAdd.Transitions = GenerateTransitions(newMdp, state, actionToAdd);

                state.ApplicableActions.Add(actionToAdd);
            }
        }
    }

    private static List<MarkovTransition> GenerateTransitions(MDP mdp, MarkovState mState, MarkovAction mAction)
    {
        List<MarkovTransition> transitions;
        
        float[] probabilityDistribution = mdp.MdpRules.GetProbabilityDistributionOfActionOutcomes();
        
        switch (mdp.MdpRules)
        {
            case MdpRules.SlipperyWalk:
                
                var intended = new MarkovTransition
                {
                    State = mState.StateIndex,
                    Action = mAction.Action,
                    Probability = probabilityDistribution[0],
                    SuccessorState = GenerateSuccessorStateFromAction(mdp, mState, mAction.Action)
                };

                var noEffect = new MarkovTransition
                {
                    State = mState.StateIndex, 
                    Action = mAction.Action,
                    Probability = probabilityDistribution[1],
                    SuccessorState = mState.StateIndex
                };

                var inverseEffect = new MarkovTransition
                {
                    State = mState.StateIndex,
                    Action = mAction.Action,
                    Probability = probabilityDistribution[2],
                    SuccessorState = GenerateSuccessorStateFromAction(
                        mdp, 
                        mState, 
                        mAction.Action.GetInverseEffectOfAction())
                };

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

            case MdpRules.GrastiensWindFromTheNorth: // Todo Include
            case MdpRules.Deterministic:
            default:
                transitions = FullTransitionsEffects(mdp, mState, probabilityDistribution);
                break;
        }
    
        Assert.IsNotNull(transitions);
        return transitions;
    }

    private static List<MarkovTransition> FullTransitionsEffects(
        MDP mdp, 
        MarkovState mState, 
        IReadOnlyList<float> probabilityDistribution)
    {
        var transitions = new List<MarkovTransition>();
        for (int i = 0; i < 4; i++)
        {
            transitions.Add(new MarkovTransition
            {
                State = mState.StateIndex,
                Action = (GridAction) i,
                Probability = probabilityDistribution[i],
                SuccessorState = GenerateSuccessorStateFromAction(mdp, mState, (GridAction) i)
            });
        }

        return transitions;
    }

    private static List<MarkovTransition> TransitionsWithOrthogonalEffects(
        MDP mdp, 
        MarkovState mState, 
        MarkovAction mAction,
        float[] probabilityDistribution)
    {
        List<MarkovTransition> transitions;
        
        var intended = new MarkovTransition
        {
            State = mState.StateIndex,
            Action = mAction.Action,
            Probability = probabilityDistribution[0],
            SuccessorState = GenerateSuccessorStateFromAction(mdp, mState, mAction.Action)
        };

        var effects = mAction.Action.GetOrthogonalEffects();
        
        var orthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            Action = mAction.Action,
            Probability = probabilityDistribution[1],
            SuccessorState = GenerateSuccessorStateFromAction(
                mdp,
                mState,
                effects[0])
        };

        var otherOrthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            Action = mAction.Action,
            Probability = probabilityDistribution[2],
            SuccessorState = GenerateSuccessorStateFromAction(
                mdp,
                mState,
                effects[1])
        };

        transitions = new List<MarkovTransition> {intended, orthogonalEffect, otherOrthogonalEffect};
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
        
        if (SuccessorStateOutOfBounds(mdp, state.StateIndex, successorIndex, action))
        {
            return state.StateIndex;
        }
        if (mdp.States[successorIndex].IsObstacle())
        {
            return state.StateIndex;
        }
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
    
    public static void SaveMdpToFile(MDP mdp)
    {
        string jsonRepresentationOfMdp = JsonUtility.ToJson(mdp);
        string saveFilePath = Application.persistentDataPath + $"/{mdp.Name}.json";
        File.WriteAllText(saveFilePath, jsonRepresentationOfMdp);
    }
}