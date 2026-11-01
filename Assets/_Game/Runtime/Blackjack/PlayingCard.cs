using System;
using System.Collections.Generic;

namespace DoubleDown.Blackjack
{
    public enum CardSuit { Clubs, Diamonds, Hearts, Spades }
    public enum CardRank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public readonly struct PlayingCard : IEquatable<PlayingCard>
    {
        public CardSuit Suit { get; }
        public CardRank Rank { get; }
        public int BlackjackValue => Rank == CardRank.Ace ? 1 : Math.Min((int)Rank, 10);

        public PlayingCard(CardSuit suit, CardRank rank)
        {
            if (!Enum.IsDefined(typeof(CardSuit), suit) || !Enum.IsDefined(typeof(CardRank), rank))
                throw new ArgumentOutOfRangeException(nameof(rank), "Invalid card suit or rank.");
            Suit = suit;
            Rank = rank;
        }

        public string SpriteName
        {
            get
            {
                var suit = Suit.ToString().ToLowerInvariant();
                var rank = Rank switch
                {
                    CardRank.Ace => "A",
                    CardRank.Jack => "J",
                    CardRank.Queen => "Q",
                    CardRank.King => "K",
                    _ => ((int)Rank).ToString("00")
                };
                return $"card_{suit}_{rank}";
            }
        }

        public bool Equals(PlayingCard other) => Suit == other.Suit && Rank == other.Rank;
        public override bool Equals(object obj) => obj is PlayingCard other && Equals(other);
        public override int GetHashCode() => ((int)Suit * 17) ^ (int)Rank;
        public override string ToString() => $"{Rank} of {Suit}";
    }

    public readonly struct HandValue
    {
        public int Total { get; }
        public bool IsSoft { get; }
        public bool IsBust => Total > 21;
        public bool IsBlackjack { get; }

        private HandValue(int total, bool isSoft, bool isBlackjack)
        {
            Total = total;
            IsSoft = isSoft;
            IsBlackjack = isBlackjack;
        }

        public static HandValue Evaluate(IReadOnlyList<PlayingCard> cards, bool naturalEligible = true)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            var total = 0;
            var aces = 0;
            foreach (var card in cards)
            {
                total += card.BlackjackValue;
                if (card.Rank == CardRank.Ace) aces++;
            }
            var soft = aces > 0 && total + 10 <= 21;
            if (soft) total += 10;
            return new HandValue(total, soft, naturalEligible && cards.Count == 2 && total == 21);
        }
    }
}
