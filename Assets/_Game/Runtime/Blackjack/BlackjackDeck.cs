using System;
using System.Collections.Generic;

namespace DoubleDown.Blackjack
{
    public sealed class BlackjackDeck
    {
        private readonly List<PlayingCard> cards;
        private int next;

        public int Remaining => cards.Count - next;

        public BlackjackDeck(uint seed)
        {
            cards = new List<PlayingCard>(52);
            for (var suit = 0; suit < 4; suit++)
                for (var rank = 1; rank <= 13; rank++)
                    cards.Add(new PlayingCard((CardSuit)suit, (CardRank)rank));

            Shuffle(seed);
        }

        public BlackjackDeck(IEnumerable<PlayingCard> source, uint seed) : this(source) => Shuffle(seed);
        private void Shuffle(uint seed)
        {
            var random = new XorShift32(seed);
            for (var i = cards.Count - 1; i > 0; i--)
            {
                var j = (int)(random.Next() % (uint)(i + 1));
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        // Explicit draw order lets tests cover rare outcomes without relying on luck.
        public BlackjackDeck(IEnumerable<PlayingCard> drawOrder)
        {
            if (drawOrder == null) throw new ArgumentNullException(nameof(drawOrder));
            cards = new List<PlayingCard>(drawOrder);
        }

        public PlayingCard Draw()
        {
            if (next >= cards.Count) throw new InvalidOperationException("The blackjack deck is empty.");
            return cards[next++];
        }

        private struct XorShift32
        {
            private uint state;
            public XorShift32(uint seed) => state = seed == 0 ? 0x6D2B79F5u : seed;
            public uint Next()
            {
                var value = state;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                return state = value;
            }
        }
    }
}
