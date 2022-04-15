using System;

public enum GridAction
{
    Left, 
    Down, 
    Right, 
    Up
}

/// <summary>
/// The <c>UncertaintyInEffectsOfActions</c> includes two static methods to make life a bit easier with the generation of MDPs.
/// They are called from the <c>GridAction</c> itself to help generate the effects of actions. Recall that with an MDP
/// there is uncertainty in the effects of the actions. For ease of computation, in a gridworld that ends up looking as
/// if the agent had taken either an orthogonal action or an inverse action. 
/// </summary>
public static class UncertaintyInEffectsOfActions
{
    /// <summary>
    /// Left -> [Up, Down],
    /// Down -> [Left, Right], ..., etc
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static GridAction[] GetOrthogonalEffects(this GridAction action)
    {
        return action switch
        {
            GridAction.Left  => new[] {GridAction.Down, GridAction.Up},
            GridAction.Down  => new[] {GridAction.Left, GridAction.Right},
            GridAction.Right => new[] {GridAction.Down, GridAction.Up},
            GridAction.Up    => new[] {GridAction.Left, GridAction.Right},
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    /// <summary>
    /// <para>
    /// Left -> Right, Down -> Up, ...
    /// </para>
    /// <para>
    /// See the documentation for <see cref="UncertaintyInEffectsOfActions"/> for further explanation.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static GridAction GetInverseEffectOfAction(this GridAction action)
    {
        return action switch
        {
            GridAction.Left  => GridAction.Right,
            GridAction.Down  => GridAction.Up,
            GridAction.Right => GridAction.Left,
            GridAction.Up    => GridAction.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
}