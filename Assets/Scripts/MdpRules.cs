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
    /// probability of:
    /// Intended  s'   ( 50% )
    /// NoEffect  s'=s ( 33.333% )
    /// 180°      s'   ( 16.667% ) 
    /// </summary>
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