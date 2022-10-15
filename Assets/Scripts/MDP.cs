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
    
    // These are just for readability
    private const GridAction                   Left  = GridAction.Left;
    private const GridAction                   Down  = GridAction.Down;
    private const GridAction                   Right = GridAction.Right;
    private const GridAction                   Up    = GridAction.Up;

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

    /// <summary>See <see cref="TransitionFunction(MarkovState,GridAction)"/> below for documentation.</summary>
    public List<MarkovTransition> TransitionFunction(MarkovState state, MarkovAction action) => TransitionFunction(state, action.Action);

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
                transitions = GrastiensWindFromTheNorth(state, action);
                break;
            
            case MdpRules.Deterministic:
            default:
                var deterministicEffect = new MarkovTransition
                {
                    State               = state.StateIndex,
                    ActionTaken         = action,
                    Probability         = 1,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(state, action)
                };
                deterministicEffect.Reward = RewardFunction(deterministicEffect.SuccessorStateIndex);
                transitions = new List<MarkovTransition> {deterministicEffect};
                break;
        }
    
        Assert.IsNotNull(transitions);
        return transitions;
    }

    /// <summary>
    /// <para>
    /// Used to dynamically calculate transitions. We use this instead of precalculated transitions (State Machine) so
    /// that we will be able to change parameters such as the environment's dynamics and other features. Uses the index
    /// (<c>int</c>) representation of the state and an action to calculate the successor state (represented by its index).
    /// </para>
    /// </summary>
    /// <param name="mdp">
    /// <c>MDP</c> object 
    /// </param>d
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
        
        intended.Reward = RewardFunction(intended.SuccessorStateIndex);

        var effects = action.GetOrthogonalEffects();
        
        var orthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = action,
            Probability = probabilityDistribution[1],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, effects.Item1)
        };

        orthogonalEffect.Reward = RewardFunction(orthogonalEffect.SuccessorStateIndex);

        var otherOrthogonalEffect = new MarkovTransition
        {
            State = mState.StateIndex,
            ActionTaken = action,
            Probability = probabilityDistribution[2],
            SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, effects.Item2)
        };

        otherOrthogonalEffect.Reward = RewardFunction(otherOrthogonalEffect.SuccessorStateIndex);
        
        return new List<MarkovTransition> {intended, orthogonalEffect, otherOrthogonalEffect};
    }

    /// <summary>
    /// <para>
    /// Simulates a wind blowing from the north. If the agent takes the <c>Up</c> action there's a 50% chance of moving
    /// to the state above (if it's reachable) a 33.33...% chance of staying in the current state and a 16.66667% chance
    /// of slipping down to the state below (if it's reachable) 
    /// </para>
    /// </summary>
    /// <param name="mState"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private List<MarkovTransition> GrastiensWindFromTheNorth(MarkovState mState, GridAction action)
    {
        var transitions = new List<MarkovTransition>();

        switch (action)
        {
            case GridAction.Left:
                
                var leftTransition = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.5f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, action)
                };

                leftTransition.Reward = RewardFunction(leftTransition.SuccessorStateIndex);
                
                transitions.Add(leftTransition);
               
                var downEffectFromLeft = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.5f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, Down)
                };

                downEffectFromLeft.Reward = RewardFunction(downEffectFromLeft.SuccessorStateIndex);
                
                transitions.Add(downEffectFromLeft);
                
                break;
            
            case GridAction.Right:
                
                var rightTransition = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.5f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, action)
                };

                rightTransition.Reward = RewardFunction(rightTransition.SuccessorStateIndex);
                
                transitions.Add(rightTransition);
               
                var downEffectFromRight = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.5f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, Down)
                };

                downEffectFromRight.Reward = RewardFunction(downEffectFromRight.SuccessorStateIndex);
                
                transitions.Add(downEffectFromRight);
                
                break;
            
            case GridAction.Down:
                var intendedDown = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 1f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, action)
                };
                intendedDown.Reward = RewardFunction(intendedDown.SuccessorStateIndex);
                transitions.Add(intendedDown);
                break;
            
            case GridAction.Up:
                
                var intendedUp = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.5f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, action)
                };

                intendedUp.Reward = RewardFunction(intendedUp.SuccessorStateIndex);
                
                transitions.Add(intendedUp);
                
                var stayInPlace = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.33333f,
                    SuccessorStateIndex = mState.StateIndex
                };

                stayInPlace.Reward = RewardFunction(stayInPlace.SuccessorStateIndex);
                
                transitions.Add(stayInPlace);
                
                var slipBackwards = new MarkovTransition
                {
                    State               = mState.StateIndex,
                    ActionTaken         = action,
                    Probability         = 0.16667f,
                    SuccessorStateIndex = GenerateSuccessorStateFromAction(mState, Down)
                };

                slipBackwards.Reward = RewardFunction(slipBackwards.SuccessorStateIndex);
                
                transitions.Add(slipBackwards);
                
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
        
        return transitions;
    } 
    
    private bool SuccessorStateOutOfBounds(int stateIndex, int successorIndex, GridAction action)
    {
        bool outOfBoundsTop             = successorIndex   > States.Count - 1;
        bool outOfBoundsBottom          = successorIndex   < 0;
        bool outOfBoundsLeft            = stateIndex       % Width == 0 && action == Left;
        bool outOfBoundsRight           = (stateIndex + 1) % Width == 0 && action == Right;
        return outOfBoundsLeft | outOfBoundsBottom | outOfBoundsRight | outOfBoundsTop;
    }
    
    // Given we've enumerated states and actions, we do easy math rather than explicitly defining actions and their
    // effects. At some future point we'll need to transition to an adjacency matrix if we want to move beyond grid worlds.
    private int ArithmeticEffectOfAction(GridAction action)
    {
        return action switch
        {
            Left  => -1,
            Down  => -Width,
            Right =>  1,
            Up    =>  Width,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    /// <summary>
    /// For now we use R(s, a, s'), but it's simply encoded as the reward for reaching the successor state. In
    /// retrospect I should have created either a dictionary or a RewardFunction object to handle each version of the
    /// reward function ( R(s) , R(s,a) , and R(s,a,s') ).  
    /// </summary>
    /// <param name="state"></param>
    /// <returns><c>float</c> representing the reward for arriving in the given state.</returns>
    /// <exception cref="Exception"></exception>
    public float RewardFunction(MarkovState state) => state.Reward;

    public float RewardFunction(int stateIndex) => States[stateIndex].Reward;


    public void EditRewardForState(MarkovState state, float newReward)
    {
        state.Reward = newReward;
    }
}




