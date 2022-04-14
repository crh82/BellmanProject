using System;
using System.Linq;

public static class MdpAdmin
{
    /// <summary>
    /// Uses the index (<c>int</c>) representation of the state and an action to calculate the successor state (again
    /// represented as by its index) during the generation of the model of the environment — in this case the gridworld.
    /// </summary>
    /// <param name="state">
    /// <c>int</c> representing the state index, rather than the state itself
    /// </param>
    /// <param name="action">
    /// <c>GridAction</c> representing the action taken
    /// </param>
    /// <returns>
    /// <c>int</c> representing the index of the successor state
    /// </returns>
    public static int GenerateSuccessorStateFromAction(MDP mdp, int state, GridAction action)
    {
        int successorState = state + ArithmeticEffectOfAction(mdp, action);
        return SuccessorStateOutOfBounds(mdp, state, successorState, action) ? state : successorState;
    }

    // Checks whether taking action ( a ) in state ( s ) goes out of bounds or into an obstacle. 
    public static bool SuccessorStateOutOfBounds(MDP mdp, int state, int successorState, GridAction action)
    {
        bool outOfBoundsTop             = successorState > mdp.States.Count - 1;
        bool outOfBoundsBottom          = successorState < 0;
        bool outOfBoundsLeft            = state       % mdp.dimX == 0 && action == GridAction.Left;
        bool outOfBoundsRight           = (state + 1) % mdp.dimX == 0 && action == GridAction.Right;
        bool hitObstacle = mdp.obstacleStates.Contains(successorState);

        return (outOfBoundsLeft   | 
                outOfBoundsBottom | 
                outOfBoundsRight  | 
                outOfBoundsTop    |
                hitObstacle) switch
        {
            true => true,
            _ => false
        };
    }
    
    // Given we've enumerated states and actions, we do easy math rather than explicitly defining actions and their
    // effects.  
    public static int ArithmeticEffectOfAction(MDP mdp, GridAction action)
    {
        return action switch
        {
            GridAction.Left => -1,
            GridAction.Down => -mdp.dimX,
            GridAction.Right => 1,
            GridAction.Up => mdp.dimX,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
}