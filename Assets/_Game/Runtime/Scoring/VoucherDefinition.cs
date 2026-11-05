using UnityEngine;
namespace DoubleDown.Scoring
{
    public enum VoucherEffect { Hands, RelicSlots, ConsumableSlots, RelicDiscount, ConsumableDiscount, RerollDiscount, Interest, RoundGold, Bankroll }
    [CreateAssetMenu(menuName="Double Down/Voucher")]
    public sealed class VoucherDefinition : ScriptableObject
    {
        public string id, displayName, prerequisite;
        [TextArea] public string description;
        public Sprite icon;
        public VoucherEffect effect;
        public int amount = 1;
    }
}
