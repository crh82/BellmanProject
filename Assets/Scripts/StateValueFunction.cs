using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class StateValueFunction
{
    private readonly Dictionary<int, float> _valueOfAState = new Dictionary<int, float>();
    private readonly Dictionary<int, float> _changeInStateValue = new Dictionary<int, float>();
    
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
                    _changeInStateValue[state.StateIndex] = float.PositiveInfinity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                    _changeInStateValue[state.StateIndex] = float.PositiveInfinity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public int Iterations { get; set; } = 0;

    public void SetValue(MarkovState state, float valueOfState)
    {
        SetValue(state.StateIndex, valueOfState);
    }
    
    public void SetValue(int stateIndex, float valueOfState)
    {
        switch (_valueOfAState.ContainsKey(stateIndex))
        {
            case true:
                float oldValueOfState = Value(stateIndex);
                _changeInStateValue[stateIndex] = Math.Abs(oldValueOfState - valueOfState); 
                _valueOfAState[stateIndex] = valueOfState;
                break;
            case false:
                _valueOfAState.Add(stateIndex, valueOfState);
                break;
        }
    }

    public float Value(MarkovState state)
    {
        return Value(state.StateIndex);
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

    public float MaxChangeInValueOfStates()
    {
        return _changeInStateValue.Values.Max();
    }

    public float StateValueDeltaBetweenTMinusOneAndT(MarkovState state)
    {
        return StateValueDeltaBetweenTMinusOneAndT(state.StateIndex);
    }

    public float StateValueDeltaBetweenTMinusOneAndT(int stateIndex)
    {
        if (_changeInStateValue.ContainsKey(stateIndex))
        {
            return _changeInStateValue[stateIndex];
        }
        throw new ArgumentNullException(nameof(stateIndex), $"No state with index {stateIndex} in Dictionary.");
    }
}