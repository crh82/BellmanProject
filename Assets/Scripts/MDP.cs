using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class MDP
{

    [SerializeField] private string            name;
    [SerializeField] private int               width;
    [SerializeField] private int               height;
    [SerializeField] private List<MarkovState> states;
    [SerializeField] private MdpRules          mdpRules;
    [SerializeField] private int[]             obstacleStates;
    [SerializeField] private int[]             terminalStates;
    [SerializeField] private int[]             goalStates;

    // NOTE: Don't know if it's better to use dictionaries here...
    // noting that Unity can't serialize and deserialize dictionaries.
    private Dictionary<int[], float> _transitionProbabilities;  // CURRENTLY DOESN'T DO ANYTHING
    private Dictionary<int, float> _rewards;                    // CURRENTLY DOESN'T DO ANYTHING
    
    
    // Getters & Setters
    
    public string Name
    {
        get => name;
        set => name = value;
    }

    public int Width
    {
        get => width;
        set => width = value;
    }

    public int Height
    {
        get => height;
        set => height = value;
    }

    public List<MarkovState> States
    {
        get => states;
        set => states = value;
    }

    public int[] ObstacleStates
    {
        get => obstacleStates;
        set => obstacleStates = value;
    }
    
    public int StateCount => States.Count;

    public MdpRules MdpRules
    {
        get => mdpRules;
        set => mdpRules = value;
    }

    public int[] TerminalStates
    {
        get => terminalStates;
        set => terminalStates = value;
    }

    public int[] GoalStates
    {
        get => goalStates;
        set => goalStates = value;
    }

    public Dictionary<int[], float> TransitionProbabilities
    {
        get => _transitionProbabilities;
        set => _transitionProbabilities = value;
    }

    public Dictionary<int, float> Rewards
    {
        get => _rewards;
        set => _rewards = value;
    }

    /// <summary>See <see cref="TransitionFunction(MarkovState,GridAction)"/> below for documentation.</summary>
    public List<MarkovTransition> TransitionFunction(MarkovState state, MarkovAction action)
    {
        return TransitionFunction(state, action.Action);
    }

    /// <summary>
    /// <para>
    /// Computes the transition objects from the given state and action according to the specified 
    /// MDP rule set ( <see cref="MdpRules"/> ).
    /// </para>
    /// <para>
    /// Used to dynamically calculate transitions. We use this instead of precalculated transitions (State Machine) so
    /// that we will be able to change parameters such as the rule sets, the discount factor (gamma), convergence
    /// threshold (theta), and so forth.
    /// </para>
    /// </summary>
    /// <param name="state">Markov state object representing the current state</param>
    /// <param name="action">GridAction object representing the action taken in the current state</param>
    /// <returns>A List of transition objects with which the Bellman Equations are computed</returns>
    public List<MarkovTransition> TransitionFunction(MarkovState state, GridAction action)
    {
        List<MarkovTransition> transitions;
        
        float[] probabilityDistribution = MdpRules.GetProbabilityDistributionOfActionOutcomes();
        
        switch (MdpRules)
        {
            case MdpRules.SlipperyWalk:
                
                var intended = new MarkovTransition
                {
                    State               = state.StateIndex,
                    ActionTaken         = action,
                    Probability         = probabilityDistribution[0],
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(state, action)
                };
                intended.Reward         = States[intended.SuccessorStateIndex].Reward;

                var noEffect = new MarkovTransition
                {
                    State               = state.StateIndex, 
                    ActionTaken         = action,
                    Probability         = probabilityDistribution[1],
                    SuccessorStateIndex = state.StateIndex
                };
                noEffect.Reward         = States[noEffect.SuccessorStateIndex].Reward;
                
                var inverseEffect = new MarkovTransition
                {
                    State               = state.StateIndex,
                    ActionTaken         = action,
                    Probability         = probabilityDistribution[2],
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(state, action.GetInverseEffectOfAction())
                };
                inverseEffect.Reward    = States[inverseEffect.SuccessorStateIndex].Reward;

                transitions = new List<MarkovTransition> {intended, noEffect, inverseEffect};
                break;
            
            case MdpRules.RussellAndNorvig:
                transitions = TransitionsWithOrthogonalEffects(state, action, probabilityDistribution);
                break;
            
            case MdpRules.RandomWalk:
                transitions = FullTransitionsEffects(state, probabilityDistribution);
                break;
            
            case MdpRules.FrozenLake:
                transitions = TransitionsWithOrthogonalEffects(state, action, probabilityDistribution);
                break;
            
            case MdpRules.DrunkBonanza:
                transitions = FullTransitionsEffects(state, probabilityDistribution);
                break;

            case MdpRules.GrastiensWindFromTheNorth:
            // transitions = GrastiensRules(mdp, mState, probabilityDistribution);
            // break;
            case MdpRules.Deterministic:
            default:
                transitions = FullTransitionsEffects(state, probabilityDistribution);
                break;
        }
    
        Assert.IsNotNull(transitions);
        return transitions;
    }

    /// <summary>
    /// Used to dynamically calculate transitions. We use this instead of precalculated transitions (State Machine) so
    /// that we will be able to change parameters such as the rule sets, the discount factor (gamma), convergence
    /// threshold (theta), and so forth. Uses the index (<c>int</c>) representation of the state and an action to
    /// calculate the successor state (represented by its index). 
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
    private int GenerateSuccessorStateFromAction(MarkovState state, GridAction action)
    {
        int successorIndex = state.StateIndex + ArithmeticEffectOfAction(action);
        if (state.IsGoal())     return state.StateIndex;
        if (state.IsTerminal()) return state.StateIndex;
        if (state.IsObstacle()) return state.StateIndex;
        if (SuccessorStateOutOfBounds(state.StateIndex, successorIndex, action)) return state.StateIndex;
        if (States[successorIndex].IsObstacle()) return state.StateIndex;
        return successorIndex;
    }

    private List<MarkovTransition> FullTransitionsEffects(MarkovState mState, IReadOnlyList<float> probabilityDistribution)
    {
        var transitions = new List<MarkovTransition>();
        
        for (var i = 0; i < 4; i++)
        {
            var newTransition = new MarkovTransition
            {
                State               = mState.StateIndex,
                ActionTaken         = (GridAction) i,
                Probability         = probabilityDistribution[i],
                SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, (GridAction) i)
            };
            newTransition.Reward    = States[newTransition.SuccessorStateIndex].Reward;
            
            transitions.Add(newTransition);
        }

        return transitions;
    }

    private List<MarkovTransition> TransitionsWithOrthogonalEffects(
        MarkovState mState, 
        GridAction action, 
        [NotNull] IReadOnlyList<float> probabilityDistribution)
    {
        var intended = new MarkovTransition
        {
            State               = mState.StateIndex,
            ActionTaken         = action,
            Probability         = probabilityDistribution[0],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, action)
        };

        intended.Reward = intended.SuccessorStateIndex == intended.State
            ? 0
            : States[intended.SuccessorStateIndex].Reward;

        var effects = action.GetOrthogonalEffects();
        
        var orthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = action,
            Probability = probabilityDistribution[1],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, effects.Item1)
        };
        
        orthogonalEffect.Reward = 
            orthogonalEffect.State == orthogonalEffect.SuccessorStateIndex
            ? 0
            : States[orthogonalEffect.SuccessorStateIndex].Reward;

        var otherOrthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = action,
            Probability = probabilityDistribution[2],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, effects.Item2)
        };
        
        otherOrthogonalEffect.Reward = 
            otherOrthogonalEffect.State == otherOrthogonalEffect.SuccessorStateIndex
            ? 0
            : States[otherOrthogonalEffect.SuccessorStateIndex].Reward;
        
        return new List<MarkovTransition> {intended, orthogonalEffect, otherOrthogonalEffect};
    }
    
    private bool SuccessorStateOutOfBounds(int stateIndex, int successorIndex, GridAction action)
    {
        bool outOfBoundsTop             = successorIndex   > States.Count - 1;
        bool outOfBoundsBottom          = successorIndex   < 0;
        bool outOfBoundsLeft            = stateIndex       % Width == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = (stateIndex + 1) % Width == 0 && action == GridAction.Right;
        return outOfBoundsLeft | outOfBoundsBottom | outOfBoundsRight | outOfBoundsTop;
    }
    
    // Given we've enumerated states and actions, we do easy math rather than explicitly defining actions and their
    // effects.  
    private int ArithmeticEffectOfAction(GridAction action)
    {
        return action switch
        {
            GridAction.Left  => -1,
            GridAction.Down  => -Width,
            GridAction.Right =>  1,
            GridAction.Up    =>  Width,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    /// <summary>
    /// Todo implement dynamic reward function
    /// </summary>
    /// <param name="state"></param>
    /// <returns><c>float</c> representing the reward for arriving in the given state.</returns>
    /// <exception cref="Exception"></exception>
    public float RewardFunction(MarkovState state)
    {
        throw new Exception();
    }
}




