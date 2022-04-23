using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum MdpRules
{
    SlipperyWalk,
    RussellAndNorvig,
    RandomWalk,
    FrozenLake,
    GrastiensWindFromTheNorth,
    DrunkBonanza,
    Deterministic
}

public static class RuleProbabilityDistributor
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader>
    /// <term><b>Slippery Walk Rules</b></term>
    /// </listheader>
    /// <item>Intended successor state   ( 50% ) </item>
    /// <item>Remain in current state    ( 33.333% )</item>
    /// <item>180° successor state       ( 16.667% ) </item>
    /// </list>
    /// <list type="bullet">
    /// <listheader>
    /// <term><b>Russell and Norvig's Rules</b></term>
    /// </listheader>
    /// <item>Intended successor state ( 80% )</item>
    /// <item> 90° successor state ( 10% )</item>
    /// <item>-90° successor state( 10% )</item>
    /// </list>
    /// <list type="bullet">
    /// <listheader>
    /// <term><b>Frozen Walk Rules</b></term>
    /// </listheader>
    /// <item>Intended successor state ( 33.33333% )</item>
    /// <item> 90° successor state ( 33.33333% )</item>
    /// <item>-90° successor state( 33.33333% )</item>
    /// </list>
    /// <list type="bullet">
    /// <listheader>
    /// <term><b>Drunk Bonanza Rules</b></term>
    /// </listheader>
    /// <item>¯¯\_(ツ)_/¯¯</item>
    /// </list>
    /// </summary>
    /// 
    /// <param name="mdpRuleSet"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "CommentTypo")]
    public static float[] GetProbabilityDistributionOfActionOutcomes(this MdpRules mdpRuleSet)
    {
        float[] transitions;
    
        switch (mdpRuleSet)
        {
            case MdpRules.SlipperyWalk:
                transitions = new[] {0.5f, 0.33333f, 0.16667f};
                break;
            case MdpRules.RussellAndNorvig:
                // Probability of:
                // Intended  s'   ( 80% )
                //  90°      s'   ( 10% )
                // -90°      s'   ( 10% )
                transitions = new[] {0.8f, 0.1f, 0.1f};
                break;
            case MdpRules.RandomWalk:
                // Probability of:
                // All outcomes: 1/NumActions
                transitions = new[] {0.25f, 0.25f, 0.25f, 0.25f};
                break;
            case MdpRules.FrozenLake:
                // Probability of: Intended s' ( 1/3 ); orthogonal s'_1 ( 1/3 ) s'_2 ( 1/3 ) 
                transitions = new[] {0.33334f, 0.33333f, 0.33333f};
                break;
            // case Rules.GrastiensWorld: Todo Include GrastiensWindFromTheNorth
            case MdpRules.DrunkBonanza:
                // ¯¯\_(ツ)_/¯¯
                float bloodAlcoholReading = Random.Range(0f, 1f);
                float effectOne   = 0 + bloodAlcoholReading;
                float effectTwo   = Random.Range(0f, 1f);
                float effectThree = Random.Range(0f, 1f);
                float effectFour  = Random.Range(0f, 1f);
                float[] drunkTransitions = {effectOne, effectTwo, effectThree, effectFour};
                float total = drunkTransitions.Sum();
                transitions = new[] {effectOne / total, effectTwo / total, effectThree / total, effectFour / total};
                break;
            case MdpRules.GrastiensWindFromTheNorth:
                transitions = new[] {1.0f, 1.0f, 1.0f, 1.0f};
                break;
            case MdpRules.Deterministic:
            default:
                // Corresponds to case Rules.Deterministic
                // Agent always arrives in intended s' 
                transitions = new[] {1.0f, 1.0f, 1.0f, 1.0f};
                break;
        }
    
        Assert.IsNotNull(transitions);
        return transitions;
    }
}