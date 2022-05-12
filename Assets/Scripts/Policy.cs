using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class Policy
{
    private readonly Dictionary<int, GridAction> _policy = new Dictionary<int, GridAction>();

    // CONSTRUCTORS
    public Policy()
    {
    }
    
    // Initializes the policy with random actions
    public Policy(MDP mdp)
    {
        Array actions = Enum.GetValues(typeof(GridAction));
        
        foreach (var state in mdp.States)
        {
            if (state.TypeOfState == StateType.Standard)
            {
                SetAction(state, (GridAction) actions.GetValue(Random.Range(0,4)));
            }
        }
    }
    
    
    // Initializes the policy with random actions, ignoring any terminal or obstacle states
    public Policy(int numberOfStates)
    {
        Array actions = Enum.GetValues(typeof(GridAction));

        for (int stateIndex = 0; stateIndex < numberOfStates; stateIndex++)
        {
            SetAction(stateIndex, (GridAction) actions.GetValue(Random.Range(0,4)));
        }
    }

    public void SetAction(MarkovState state, GridAction action)
    {
        switch (_policy.ContainsKey(state.StateIndex))
        {
            case true:
                _policy[state.StateIndex] = action;
                break;
            case false:
                _policy.Add(state.StateIndex, action);
                break;
        }
    }
    
    public void SetAction(int stateIndex, GridAction action)
    {
        switch (_policy.ContainsKey(stateIndex))
        {
            case true:
                _policy[stateIndex] = action;
                break;
            case false:
                _policy.Add(stateIndex, action);
                break;
        }
    }
    
    public void SetAction(int stateIndex, string action)
    {
        string cleanedAction = action.ToLower();
        GridAction parsedAction = cleanedAction switch
        {
            "l"     => GridAction.Left,
            "left"  => GridAction.Left,
            "d"     => GridAction.Down,
            "down"  => GridAction.Down,
            "r"     => GridAction.Right,
            "right" => GridAction.Right,
            "u"     => GridAction.Up,
            "up"    => GridAction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };

        switch (_policy.ContainsKey(stateIndex))
        {
            case true:
                _policy[stateIndex] = parsedAction;
                break;
            case false:
                _policy.Add(stateIndex, parsedAction);
                break;
        }
    }

    /// <summary>
    /// A small method to help with testing and debugging in the Unity Editor. Used to update the Editor visible policy.
    /// </summary>
    /// <param name="setOfStates">Represents the state space</param>
    /// <param name="numberOfStates"><c>int</c></param>
    /// <returns>A string array representation of the policy where the actions are indexed by their corresponding state index</returns>
    public string[] PolicyToStringArray(List<MarkovState> setOfStates)
    {
        
        var stringDisplayOfPolicy = new string[setOfStates.Count];
        for (var stateIndex = 0; stateIndex < setOfStates.Count; stateIndex++)
        {
            stringDisplayOfPolicy[stateIndex] = setOfStates[stateIndex].TypeOfState switch
            {
                StateType.Standard => _policy[stateIndex].ToString(),
                StateType.Terminal => "Term Act N/A",
                StateType.Obstacle => "Obs Act N/A",
                StateType.Goal     => "Goal Act N/A",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        return stringDisplayOfPolicy;
    }

    public GridAction GetAction(int stateIndex)
    {
        return _policy.ContainsKey(stateIndex) switch
        {
            true => _policy[stateIndex],
            false => throw new ArgumentOutOfRangeException($"s{stateIndex} has no assigned action")
        };
    }
    
    public GridAction GetAction(MarkovState state)
    {
        return _policy.ContainsKey(state.StateIndex) switch
        {
            true  => _policy[state.StateIndex],
            false => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    
    public Policy Copy()
    {
        var copyOfPolicy = new Policy();
        foreach (var kvp in _policy)
        {
            copyOfPolicy.SetAction(kvp.Key, kvp.Value);
        }
        return copyOfPolicy;
    }

    // TODO This is currently too strict to function as a check of policy equality—given that it effectively checks the equality of two dictionaries.
    public bool Equals(Policy policyPrime)
    {
        if (null == policyPrime) 
            return _policy == null;
        if (null == _policy) 
            return false;
        if (ReferenceEquals(_policy, policyPrime._policy)) 
            return true;
        if (_policy.Count != policyPrime._policy.Count) 
            return false;

        foreach (int k in _policy.Keys)
        {
            if (!policyPrime._policy.ContainsKey(k))        return false;
            if (!_policy[k].Equals(policyPrime._policy[k])) return false;
        }

        return true;
    }

    public string StringRepresentationOfPolicy()
    {
        StringBuilder policyString = new StringBuilder();
        foreach (var stateAction in _policy)
        {
            policyString.Append($"< s{stateAction.Key} : {stateAction.Value} >");
        }

        return policyString.ToString();
    }
    
    public void PrintPolicyToDebugLog()
    {
        foreach (var kvp in _policy)
        {
            Debug.Log($"s{kvp.Key} : {kvp.Value}");
        }
    }

    public Dictionary<int, GridAction> GetPolicyDictionary()
    {
        return _policy;
    }
}