using UnityEngine;

namespace DoubleDown.Scoring
{
    [CreateAssetMenu(fileName = "EconomyBalance", menuName = "Double Down/Economy Balance")]
    public sealed class EconomyBalanceConfig : ScriptableObject
    {
        [Min(0)] public int startingGold = 8;
        [Min(1)] public int relicPrice = 6;
        [Min(1)] public int consumablePrice = 4;
        [Min(1)] public int voucherPrice = 8;
        [Min(1)] public int initialRerollPrice = 2;
        [Min(1)] public int rerollIncrease = 1;
        [Min(0)] public int roundReward = 6;
        [Min(0)] public int goldPerRemainingHand = 1;

        public EconomyRules ToRules() => new EconomyRules(startingGold, relicPrice,
            initialRerollPrice, rerollIncrease, roundReward, goldPerRemainingHand);
    }
}
