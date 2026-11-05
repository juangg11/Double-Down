using UnityEngine;
using DoubleDown.Blackjack;

namespace DoubleDown.Scoring
{
    public enum RelicTrigger { Victory, Aces, Faces, Suit, Push, TwentyOne, StandVictory, DoubleVictory, SplitVictory, ExtraCards, Streak, ScoredHand, Natural, FiveCards, Soft, Hard, FourSuits, SameSuit, OpeningPair, Even, Odd, Low, High, Nineteen, Twenty, Eighteen, RedHand, BlackHand, LongHand }
    public enum RelicOperation { AddChips, AddMultiplier, MultiplyMultiplier }

    public enum RelicRarity { Common, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "Relic", menuName = "Double Down/Relic")]
    public sealed class RelicDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public RelicRarity rarity;
        public RelicTrigger trigger;
        public RelicOperation operation;
        public CardSuit suit;
        [Min(0f)] public float amount;
    }
}
