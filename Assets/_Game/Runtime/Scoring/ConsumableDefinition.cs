using UnityEngine;
using DoubleDown.Blackjack;
namespace DoubleDown.Scoring
{
    public enum ConsumableEffect { Remove, Duplicate, RankUp, RankDown, ChangeSuit, Modify }
    [CreateAssetMenu(menuName="Double Down/Consumable")]
    public sealed class ConsumableDefinition : ScriptableObject
    {
        public string id, displayName;
        [TextArea] public string description;
        public Sprite icon;
        public ConsumableEffect effect;
        public CardSuit suit;
        public CardModificationDefinition modification;
    }
}
