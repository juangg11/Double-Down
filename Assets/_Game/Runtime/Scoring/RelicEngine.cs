using System;
using System.Collections.Generic;
using DoubleDown.Blackjack;

namespace DoubleDown.Scoring
{
    public sealed class RelicLoadout
    {
        public const int Capacity = 4;
        private readonly RelicDefinition[] slots = new RelicDefinition[Capacity];
        public RelicDefinition this[int index] => slots[index];
        public bool Equip(int slot, RelicDefinition relic)
        {
            if (slot < 0 || slot >= Capacity) return false;
            if (relic != null)
                for (var i = 0; i < Capacity; i++)
                    if (i != slot && slots[i] != null && slots[i].id == relic.id) return false;
            slots[slot] = relic;
            return true;
        }
        public bool Move(int from, int to)
        {
            if (from < 0 || to < 0 || from >= Capacity || to >= Capacity) return false;
            (slots[from], slots[to]) = (slots[to], slots[from]);
            return true;
        }
    }

    public static class RelicEngine
    {
        public static ScoreBreakdown Apply(ScoreBreakdown initial, BlackjackHand hand,
            RelicLoadout loadout, int streak, List<int> activations)
        {
            if (hand.Outcome == HandOutcome.Loss || hand.Outcome == HandOutcome.Pending) return initial;
            var chips = initial.Chips;
            var multiplier = initial.Multiplier;
            for (var slot = 0; slot < RelicLoadout.Capacity; slot++)
            {
                var relic = loadout[slot];
                if (relic == null) continue;
                var count = Count(relic, hand, streak);
                if (count <= 0) continue;
                var amount = (double)relic.amount * count;
                switch (relic.operation)
                {
                    case RelicOperation.AddChips: chips += new BigNumber(amount); break;
                    case RelicOperation.AddMultiplier: multiplier += new BigNumber(amount); break;
                    case RelicOperation.MultiplyMultiplier: multiplier *= amount; break;
                    default: throw new ArgumentOutOfRangeException();
                }
                activations?.Add(slot);
            }
            return new ScoreBreakdown(chips, multiplier);
        }

        private static int Count(RelicDefinition relic, BlackjackHand hand, int streak)
        {
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
            }
            var count = 0;
            foreach (var card in hand.Cards)
                if ((relic.trigger == RelicTrigger.Aces && card.Rank == CardRank.Ace) ||
                    (relic.trigger == RelicTrigger.Faces && card.Rank >= CardRank.Jack) ||
                    (relic.trigger == RelicTrigger.Suit && card.Suit == relic.suit)) count++;
            return count;
        }
    }
}
