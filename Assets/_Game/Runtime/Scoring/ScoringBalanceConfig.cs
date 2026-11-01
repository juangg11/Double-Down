using UnityEngine;

namespace DoubleDown.Scoring
{
    [CreateAssetMenu(fileName = "ScoringBalance", menuName = "Double Down/Scoring Balance")]
    public sealed class ScoringBalanceConfig : ScriptableObject
    {
        [Min(1)] public int firstGoal = 120;
        [Min(1f)] public float goalGrowth = 1.18f;
        [Range(1, 10)] public int handsPerRound = 4;
        [Min(0)] public int chipsPerCard = 5;
        [Min(0)] public int chipsPerWagerUnit = 1;
        [Min(1)] public int wagerUnit = 2;
        [Min(0f)] public float winMultiplier = 2f;
        [Min(0f)] public float blackjackMultiplier = 3.5f;
        [Min(0f)] public float pushMultiplier = 0.5f;

        public ScoreRules ToRules() => new ScoreRules(firstGoal, goalGrowth, handsPerRound,
            chipsPerCard, chipsPerWagerUnit, wagerUnit, winMultiplier,
            blackjackMultiplier, pushMultiplier);
    }
}
