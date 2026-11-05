using System;
using System.Collections.Generic;
using DoubleDown.Blackjack;

namespace DoubleDown.Scoring
{
    public sealed class RelicLoadout
    {
        public const int Capacity = 4;
        private readonly RelicDefinition[] slots = new RelicDefinition[6];
        public int SlotCount { get; private set; } = Capacity;
        public void SetCapacity(int count)
        {
            if (count < Capacity || count > 6) throw new ArgumentOutOfRangeException(nameof(count));
            for (var i = count; i < slots.Length; i++) slots[i] = null;
            SlotCount = count;
        }
        public RelicDefinition this[int index] => slots[index];
        public bool Equip(int slot, RelicDefinition relic)
        {
            if (slot < 0 || slot >= SlotCount) return false;
            if (relic != null)
                for (var i = 0; i < SlotCount; i++)
                    if (i != slot && slots[i] != null && slots[i].id == relic.id) return false;
            slots[slot] = relic;
            return true;
        }
        public bool Move(int from, int to)
        {
            if (from < 0 || to < 0 || from >= SlotCount || to >= SlotCount) return false;
            (slots[from], slots[to]) = (slots[to], slots[from]);
            return true;
        }
    }

    public static class RelicEngine
    {
        public static ScoreBreakdown Apply(ScoreBreakdown initial, BlackjackHand hand,
            RelicLoadout loadout, int streak, List<int> activations, Func<PlayingCard, CardSuit, bool> matchesSuit = null)
        {
            if (hand.Outcome == HandOutcome.Loss || hand.Outcome == HandOutcome.Pending) return initial;
            var chips = initial.Chips;
            var multiplier = initial.Multiplier;
            for (var slot = 0; slot < loadout.SlotCount; slot++)
            {
                var relic = loadout[slot];
                if (relic == null) continue;
                var count = Count(relic, hand, streak, matchesSuit);
                if (count <= 0) continue;
                var amount = (double)relic.amount * count;
                switch (relic.operation)
                {
                    case RelicOperation.AddChips: chips += new BigNumber(amount); break;
                    case RelicOperation.AddMultiplier: multiplier += new BigNumber(amount); break;
                    case RelicOperation.MultiplyMultiplier: multiplier *= BigNumber.Pow(relic.amount, count); break;
                    default: throw new ArgumentOutOfRangeException();
                }
                activations?.Add(slot);
            }
            return new ScoreBreakdown(chips, multiplier);
        }

        private static int Count(RelicDefinition relic, BlackjackHand hand, int streak, Func<PlayingCard, CardSuit, bool> matchesSuit)
        {
            matchesSuit ??= (card, suit) => card.Suit == suit;
            var victory = hand.Outcome == HandOutcome.Win || hand.Outcome == HandOutcome.Blackjack;
            switch (relic.trigger)
            {
                case RelicTrigger.Victory: return victory ? 1 : 0;
                case RelicTrigger.Push: return hand.Outcome == HandOutcome.Push ? 1 : 0;
                case RelicTrigger.TwentyOne: return hand.Value.Total == 21 ? 1 : 0;
                case RelicTrigger.StandVictory: return victory && hand.Cards.Count == 2 && !hand.IsDoubled ? 1 : 0;
                case RelicTrigger.DoubleVictory: return victory && hand.IsDoubled ? 1 : 0;
                case RelicTrigger.SplitVictory: return victory && hand.IsSplit ? 1 : 0;
                case RelicTrigger.ExtraCards: return Math.Max(0, hand.Cards.Count - 2);
                case RelicTrigger.Streak: return victory ? Math.Max(0, streak) : 0;
                case RelicTrigger.ScoredHand: return 1;
                case RelicTrigger.Natural: return hand.Outcome == HandOutcome.Blackjack ? 1 : 0;
                case RelicTrigger.FiveCards: return hand.Cards.Count >= 5 ? 1 : 0;
                case RelicTrigger.Soft: return hand.Value.IsSoft ? 1 : 0;
                case RelicTrigger.Hard: return !hand.Value.IsSoft ? 1 : 0;
                case RelicTrigger.OpeningPair: return hand.Cards.Count >= 2 && hand.Cards[0].Rank == hand.Cards[1].Rank ? 1 : 0;
                case RelicTrigger.Nineteen: return hand.Value.Total == 19 ? 1 : 0;
                case RelicTrigger.Twenty: return hand.Value.Total == 20 ? 1 : 0;
                case RelicTrigger.Eighteen: return hand.Value.Total == 18 ? 1 : 0;
                case RelicTrigger.LongHand: return Math.Max(0, hand.Cards.Count - 4);
                case RelicTrigger.FourSuits:
                    for (var s = 0; s < 4; s++) { var found = false; foreach (var c in hand.Cards) found |= matchesSuit(c, (CardSuit)s); if (!found) return 0; }
                    return 1;
                case RelicTrigger.SameSuit:
                    for (var s = 0; s < 4; s++) { var all = true; foreach (var c in hand.Cards) all &= matchesSuit(c, (CardSuit)s); if (all) return 1; }
                    return 0;
                case RelicTrigger.RedHand:
                    foreach (var c in hand.Cards) if (!matchesSuit(c, CardSuit.Hearts) && !matchesSuit(c, CardSuit.Diamonds)) return 0;
                    return 1;
                case RelicTrigger.BlackHand:
                    foreach (var c in hand.Cards) if (!matchesSuit(c, CardSuit.Clubs) && !matchesSuit(c, CardSuit.Spades)) return 0;
                    return 1;
            }
            var count = 0;
            foreach (var card in hand.Cards)
                if ((relic.trigger == RelicTrigger.Aces && card.Rank == CardRank.Ace) ||
                    (relic.trigger == RelicTrigger.Faces && card.Rank >= CardRank.Jack) ||
                    (relic.trigger == RelicTrigger.Suit && matchesSuit(card, relic.suit)) ||
                    (relic.trigger == RelicTrigger.Even && (int)card.Rank >= 2 && (int)card.Rank <= 10 && (int)card.Rank % 2 == 0) ||
                    (relic.trigger == RelicTrigger.Odd && (int)card.Rank >= 3 && (int)card.Rank <= 9 && (int)card.Rank % 2 == 1) ||
                    (relic.trigger == RelicTrigger.Low && (int)card.Rank >= 2 && (int)card.Rank <= 5) ||
                    (relic.trigger == RelicTrigger.High && (int)card.Rank >= 6 && (int)card.Rank <= 10)) count++;
            return count;
        }
    }
}
