using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionValueFunction
{

    private readonly Dictionary<int, Dictionary<GridAction, float>> _valueOfQGivenSandA = new Dictionary<int, Dictionary<GridAction, float>>();

    public ActionValueFunction()
    {
    }

    public ActionValueFunction(MDP mdp)
    {
        foreach (var state in mdp.States)
        {
            switch (state.TypeOfState)
            {
                case StateType.Terminal:
                case StateType.Obstacle:
                case StateType.Goal:
                    break;
                case StateType.Standard:
                    foreach (var action in state.ApplicableActions)
                    {
                        SetValue(state, action.Action, 0f);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }

    public void SetValue(MarkovState state, MarkovAction action, float valueOfActionInState)
    {
        SetValue(state, action.Action, valueOfActionInState);
    }
    
    public void SetValue(MarkovState state, GridAction action, float valueOfActionInState)
    {
        if (_valueOfQGivenSandA.ContainsKey(state.StateIndex))
        {
            var stateActions = _valueOfQGivenSandA[state.StateIndex];
            
            if (stateActions.ContainsKey(action))
            {
                _valueOfQGivenSandA[state.StateIndex][action] = valueOfActionInState;
            }
            else
            {
                _valueOfQGivenSandA[state.StateIndex].Add(action, valueOfActionInState);
            }
        }
        else
        {
            var stateActions = new Dictionary<GridAction, float> {{action, valueOfActionInState}};
            _valueOfQGivenSandA.Add(state.StateIndex, stateActions);
        }
    }

    public float Value(MarkovState state, MarkovAction action)
    {
        return Value(state, action.Action);
    }
    
    public float Value(MarkovState state, GridAction action)
    {

        switch (_valueOfQGivenSandA.ContainsKey(state.StateIndex))
        {
            case true:
                switch (_valueOfQGivenSandA[state.StateIndex].ContainsKey(action))
                {
                    case true:
                        return _valueOfQGivenSandA[state.StateIndex][action];
                    default:
                        SetValue(state, action, 0f);
                        Debug.Log($"ActionValueFunction tried to access an uninitialized value. " +
                                  $"{state} was present but {action} was not. " +
                                  $"Stored Q({state},{action}) = {0.0f} instead. " +
                                  $"Check for unintended consequences"); 
                        return 0;
                }
            default:
                SetValue(state, action, 0f);
                Debug.Log($"ActionValueFunction tried to access an uninitialized value. " +
                          $"Neither {state} nor {action} were present. " +
                          $"Stored Q({state},{action}) = {0.0f} instead. " +
                          $"Check for unintended consequences");
                return 0;
        }
    }

    /// <summary>
    /// The ArgMaxAction method returns the maximally valued action of a given state.
    /// </summary>
    ///
    /// <param name="state"> The state to evaluate</param>
    ///
    /// <returns> Maximally valued action.</returns>
    public GridAction ArgMaxAction(MarkovState state)
    {
        float maxStateActionValue = MaxValue(state);
        return _valueOfQGivenSandA[state.StateIndex].First(kvp => Math.Abs(kvp.Value - maxStateActionValue) < 1e-15).Key;
    }

    public float MaxValue(MarkovState state)
    {
        return _valueOfQGivenSandA[state.StateIndex].Max(keyValuePair => keyValuePair.Value);
    }
}