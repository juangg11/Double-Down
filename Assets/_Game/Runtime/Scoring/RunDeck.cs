using System;
using System.Collections.Generic;
using DoubleDown.Blackjack;
namespace DoubleDown.Scoring
{
    public sealed class RunCard
    {
        public PlayingCard Card { get; internal set; }
        private readonly CardModificationDefinition[] layers = new CardModificationDefinition[3];
        public CardModificationDefinition this[CardLayer layer] => layers[(int)layer];
        internal RunCard(PlayingCard card) { Card = card; }
        internal void RemoveDestructiveLayers()
        { for(var i=0;i<layers.Length;i++) if(layers[i]?.destroyAfterScoring == true) layers[i]=null; }
        internal void Set(CardModificationDefinition modifier) => layers[(int)modifier.layer] = modifier;
        internal RunCard Copy(int id)
        {
            var result = new RunCard(new PlayingCard(Card.Suit, Card.Rank, id));
            Array.Copy(layers, result.layers, layers.Length); return result;
        }
    }
    public sealed class RunDeck
    {
        public const int MinimumCards = 24, MaximumCards = 104;
        private readonly List<RunCard> cards = new List<RunCard>();
        private readonly IReadOnlyList<RunCard> view;
        private int nextId;
        public IReadOnlyList<RunCard> Cards => view;
        public RunDeck() { view = cards.AsReadOnly(); Reset(); }
        public void Reset()
        {
            cards.Clear(); nextId = 1;
            for (var s = 0; s < 4; s++) for (var r = 1; r <= 13; r++)
                cards.Add(new RunCard(new PlayingCard((CardSuit)s, (CardRank)r, nextId++)));
        }
        public RunCard Find(int id) => cards.Find(c => c.Card.InstanceId == id);
        public PlayingCard[] Snapshot() => cards.ConvertAll(c => c.Card).ToArray();
        public bool CanUse(ConsumableDefinition item, int id)
        {
            if (item == null || Find(id) == null) return false;
            return item.effect switch
            {
                ConsumableEffect.Remove => cards.Count > MinimumCards,
                ConsumableEffect.Duplicate => cards.Count < MaximumCards,
                ConsumableEffect.ChangeSuit => Find(id).Card.Suit != item.suit,
                ConsumableEffect.Modify => item.modification != null && Find(id)[item.modification.layer] != item.modification,
                ConsumableEffect.RankUp or ConsumableEffect.RankDown => true,
                _ => false
            };
        }
        public bool Use(ConsumableDefinition item, int id)
        {
            if (!CanUse(item, id)) return false;
            var target = Find(id); var card = target.Card;
            switch(item.effect)
            {
                case ConsumableEffect.Remove: cards.Remove(target); break;
                case ConsumableEffect.Duplicate: cards.Add(target.Copy(nextId++)); break;
                case ConsumableEffect.RankUp: target.Card = new PlayingCard(card.Suit, (CardRank)((int)card.Rank % 13 + 1), id); break;
                case ConsumableEffect.RankDown: target.Card = new PlayingCard(card.Suit, (CardRank)(((int)card.Rank + 11) % 13 + 1), id); break;
                case ConsumableEffect.ChangeSuit: target.Card = new PlayingCard(item.suit, card.Rank, id); break;
                case ConsumableEffect.Modify: target.Set(item.modification); break;
            }
            return true;
        }
        // Destruction is deterministic and stops at the same minimum as manual removal.
        public bool DestroyScoredCard(int id)
        {
            var target=Find(id);if(target==null)return false;
            if(cards.Count>MinimumCards)return cards.Remove(target);
            target.RemoveDestructiveLayers();return false;
        }
        public bool MatchesSuit(PlayingCard card, CardSuit suit, int debuffedSuit = -1)
        {
            if ((int)card.Suit == debuffedSuit) return false;
            return card.Suit == suit || Find(card.InstanceId)?[CardLayer.Enhancement]?.wild == true;
        }
    }
    public static class CardModificationEngine
    {
        public static ScoreBreakdown Apply(ScoreBreakdown score, BlackjackHand hand, RunDeck deck,
            int debuffedSuit, out int gold, ICollection<int> destroyed)
        {
            gold = 0;
            if (hand.Outcome == HandOutcome.Loss || hand.Outcome == HandOutcome.Pending) return score;
            var chips = score.Chips; var mult = score.Multiplier;
            for (var i = 0; i < hand.Cards.Count; i++)
            {
                var played = hand.Cards[i]; var card = deck.Find(played.InstanceId);
                if (card == null || (int)played.Suit == debuffedSuit) continue;
                var repeat = card[CardLayer.Seal]?.repeatBonuses == true ? 2 : 1;
                for (var layer = 0; layer < 3; layer++)
                {
                    var modifier = card[(CardLayer)layer]; if (modifier == null) continue;
                    var count = layer == (int)CardLayer.Seal ? 1 : repeat;
                    for (var n = 0; n < count; n++)
                    {
                        chips += new BigNumber(modifier.chips);
                        mult += new BigNumber(modifier.multiplier + modifier.multiplierPerPosition * (i + 1));
                        mult *= modifier.multiplierFactor;
                        gold += modifier.gold;
                    }
                    if (modifier.destroyAfterScoring) destroyed?.Add(played.InstanceId);
                }
            }
            return new ScoreBreakdown(chips, mult);
        }
    }
}
