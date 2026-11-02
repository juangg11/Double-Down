using UnityEngine;

namespace DoubleDown.Scoring
{
    [CreateAssetMenu(fileName = "Boss", menuName = "Double Down/Boss")]
    public sealed class BossDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public bool playerAcesLow;
        [Range(17, 20)] public int dealerStandTotal = 17;
        [Min(0)] public int maximumHands;
        [Min(1)] public int firstTable = 1;
        public BossRule ToRule() => new BossRule(id, displayName, description,
            playerAcesLow, dealerStandTotal, maximumHands, firstTable);
    }
}
