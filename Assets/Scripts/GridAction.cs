using System;

public enum GridAction
{
    Left, Down, Right, Up
}

/// <summary>
/// The <c>UncertaintyActions</c> includes two static methods to make life a bit easier with the generation of MDPs.
/// They can be called from the <c>GridAction</c> itself to generate the actions that are either orthogonal to the action or the
/// inverse of the action. 
/// </summary>
public static class UncertaintyActions
{
    /// <summary>
    /// Left -> [Up, Down],
    /// Down -> [Left, Right], ..., etc
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static GridAction[] GetOrthongonalActions(this GridAction action)
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
    /// See the documentation for <see cref="UncertaintyActions"/> for further explanation.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static GridAction GetInverseAction(this GridAction action)
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