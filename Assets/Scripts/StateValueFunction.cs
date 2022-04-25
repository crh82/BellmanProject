using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class StateValueFunction
{
    private readonly Dictionary<int, float> _valueOfAState = new Dictionary<int, float>();

    public StateValueFunction()
    {
    }

    public StateValueFunction(MDP mdp)
    {
        foreach (var state in mdp.States)
        {
            switch (state.TypeOfState)
            {
                case StateType.Terminal:
                case StateType.Obstacle:
                case StateType.Goal:
                    SetValue(state, state.Reward);
                    break;
                case StateType.Standard:
                    SetValue(state, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // if (state.IsObstacle() || state.IsGoal() || state.IsTerminal()) SetValue(state, state.Reward);
            // else SetValue(state, Random.Range(lowerBoundOfValues, upperBoundOfValues));
            
        }
    }

    public StateValueFunction(MDP mdp, float lowerBoundOfValues, float upperBoundOfValues)
    {
        Assert.IsTrue(lowerBoundOfValues < upperBoundOfValues);
        foreach (var state in mdp.States)
        {
            switch (state.TypeOfState)
            {
                case StateType.Terminal:
                case StateType.Obstacle:
                case StateType.Goal:
                    SetValue(state, state.Reward);
                    break;
                case StateType.Standard:
                    SetValue(state, Random.Range(lowerBoundOfValues, upperBoundOfValues));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // if (state.IsObstacle() || state.IsGoal() || state.IsTerminal()) SetValue(state, state.Reward);
            // else SetValue(state, Random.Range(lowerBoundOfValues, upperBoundOfValues));
            
        }
    }
    
    public void SetValue(MarkovState state, float valueOfState)
    {
        switch (_valueOfAState.ContainsKey(state.StateIndex))
        {
            case true:
                _valueOfAState[state.StateIndex] = valueOfState;
                break;
            case false:
                _valueOfAState.Add(state.StateIndex, valueOfState);
                break;
        }
    }
    
    public void SetValue(int stateIndex, float valueOfState)
    {
        switch (_valueOfAState.ContainsKey(stateIndex))
        {
            case true:
                _valueOfAState[stateIndex] = valueOfState;
                break;
            case false:
                _valueOfAState.Add(stateIndex, valueOfState);
                break;
        }
    }

    public float Value(MarkovState state)
    {
        switch (_valueOfAState.ContainsKey(state.StateIndex))
        {
            case true:
                return _valueOfAState[state.StateIndex];
            case false:
                SetValue(state, 0);
                return 0;
        }
    }
    
    public float Value(int stateIndex)
    {
        switch (_valueOfAState.ContainsKey(stateIndex))
        {
            case true:
                return _valueOfAState[stateIndex];
            case false:
                SetValue(stateIndex, 0);
                return 0;
        }
    }

    /// <summary>
    /// A small method to help with testing and debugging in the Unity Editor. Used to update the Editor visible list
    /// of state values.
    /// </summary>
    /// <param name="stateValueArrayLength"><c>int</c>: Representing the number of states</param>
    /// <returns><c>float[]</c>Updating the Editor visible state value array.</returns>
    public float[] StateValuesToFloatArray(int stateValueArrayLength)
    {
        var newStateValues = new float[stateValueArrayLength];
        
        foreach (var stateValue in _valueOfAState)
        {
            newStateValues[stateValue.Key] = stateValue.Value;
        }
        
        return newStateValues;
    }
}