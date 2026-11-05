using System;
using System.Collections.Generic;

namespace DoubleDown.Blackjack
{
    public enum BlackjackPhase { Ready, Insurance, PlayerTurn, DealerTurn, Settled }
    public enum HandOutcome { Pending, Loss, Push, Win, Blackjack }
    public enum BlackjackEventType { RoundStarted, CardDealt, HoleRevealed, HandChanged, PhaseChanged, RoundSettled }

    public readonly struct BlackjackEvent
    {
        public BlackjackEventType Type { get; }
        public int HandIndex { get; }
        public PlayingCard Card { get; }
        public bool IsDealer { get; }
        public bool IsHidden { get; }

        public BlackjackEvent(BlackjackEventType type, int handIndex = -1,
            PlayingCard card = default, bool isDealer = false, bool isHidden = false)
        {
            Type = type;
            HandIndex = handIndex;
            Card = card;
            IsDealer = isDealer;
            IsHidden = isHidden;
        }
    }

    public sealed class BlackjackHand
    {
        private readonly List<PlayingCard> cards = new List<PlayingCard>(11);
        private readonly bool acesLow;
        public IReadOnlyList<PlayingCard> Cards => cards;
        public long Stake { get; internal set; }
        public bool IsSplit { get; internal set; }
        public bool IsSplitAces { get; internal set; }
        public bool IsDoubled { get; internal set; }
        public bool IsFinished { get; internal set; }
        public HandOutcome Outcome { get; internal set; }
        public HandValue Value => HandValue.Evaluate(cards, !IsSplit, acesLow);

        internal BlackjackHand(long stake, bool acesLow = false) { Stake = stake; this.acesLow = acesLow; }
        internal void Add(PlayingCard card) => cards.Add(card);
        internal PlayingCard RemoveSecondCard()
        {
            var card = cards[1];
            cards.RemoveAt(1);
            return card;
        }
    }

    public sealed class BlackjackSession
    {
        private readonly BlackjackRules rules;
        private readonly uint seed;
        private readonly IReadOnlyList<PlayingCard> firstRoundDrawOrder;
        private readonly List<BlackjackHand> hands = new List<BlackjackHand>(4);
        private readonly List<PlayingCard> dealer = new List<PlayingCard>(11);
        private BlackjackDeck deck;
        private PlayingCard[] runDeck;
        private bool allowDouble = true, allowSplit = true;
        public bool ConfigureDeck(IReadOnlyList<PlayingCard> cards)
        {
            if (Phase != BlackjackPhase.Ready && Phase != BlackjackPhase.Settled) return false;
            if (cards == null || cards.Count < 4) throw new ArgumentException("A deck needs at least four cards.");
            var ids = new HashSet<int>();
            foreach (var card in cards) if (card.InstanceId <= 0 || !ids.Add(card.InstanceId)) throw new ArgumentException("Deck card IDs must be unique and positive.");
            runDeck = new PlayingCard[cards.Count]; for (var i = 0; i < cards.Count; i++) runDeck[i] = cards[i];
            return true;
        }
        public void GrantBankroll(long amount)
        {
            if (amount < 0 || (Phase != BlackjackPhase.Ready && Phase != BlackjackPhase.Settled)) throw new InvalidOperationException("Bankroll grants occur between hands.");
            Bankroll = checked(Bankroll + amount);
        }
        private long bankrollBeforeRound;
        private bool playerAcesLow;
        private int dealerStandTotal = 17;
        public bool PlayerAcesLow => playerAcesLow;
        public int DealerStandTotal => dealerStandTotal;

        // Table rules change only between deals; they never reset bankroll or the deck seed.
        public bool ConfigureTable(bool acesLow = false, int dealerStand = 17, bool canDouble = true, bool canSplit = true)
        {
            if (dealerStand < 17 || dealerStand > 20) throw new ArgumentOutOfRangeException(nameof(dealerStand));
            if (Phase != BlackjackPhase.Ready && Phase != BlackjackPhase.Settled) return false;
            allowDouble = canDouble; allowSplit = canSplit;
            playerAcesLow = acesLow;
            dealerStandTotal = dealerStand;
            return true;
        }

        public event Action<BlackjackEvent> EventRaised;
        public BlackjackPhase Phase { get; private set; } = BlackjackPhase.Ready;
        public IReadOnlyList<BlackjackHand> Hands => hands;
        public IReadOnlyList<PlayingCard> DealerCards => dealer;
        public long Bankroll { get; private set; }
        public long Wager { get; private set; }
        public long InsuranceWager { get; private set; }
        public long RoundProfit { get; private set; }
        public int ActiveHandIndex { get; private set; }
        public int RoundIndex { get; private set; }
        public bool DealerHoleRevealed { get; private set; }
        public string LastMessage { get; private set; } = "Elige una apuesta para comenzar.";
        public HandValue DealerValue => HandValue.Evaluate(dealer);
        public int DealerVisibleTotal => DealerHoleRevealed
            ? DealerValue.Total
            : dealer.Count == 0 ? 0 : HandValue.Evaluate(new[] { dealer[0] }, false).Total;

        public BlackjackSession(BlackjackRules rules, uint seed,
            IReadOnlyList<PlayingCard> firstRoundDrawOrder = null)
        {
            this.rules = rules;
            this.seed = seed;
            this.firstRoundDrawOrder = firstRoundDrawOrder;
            Bankroll = rules.StartingBankroll;
        }

        public bool StartRound(long wager)
        {
            if ((Phase != BlackjackPhase.Ready && Phase != BlackjackPhase.Settled) ||
                wager < rules.MinimumWager || wager % 2 != 0 || wager > Bankroll)
                return false;

            deck = RoundIndex == 0 && firstRoundDrawOrder != null
                ? new BlackjackDeck(firstRoundDrawOrder)
                : runDeck != null ? new BlackjackDeck(runDeck, seed + (uint)RoundIndex * 0x9E3779B9u)
                : new BlackjackDeck(seed + (uint)RoundIndex * 0x9E3779B9u);
            RoundIndex++;
            bankrollBeforeRound = Bankroll;
            Bankroll -= wager;
            Wager = wager;
            InsuranceWager = 0;
            RoundProfit = 0;
            DealerHoleRevealed = false;
            ActiveHandIndex = 0;
            hands.Clear();
            dealer.Clear();
            hands.Add(new BlackjackHand(wager, playerAcesLow));
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.RoundStarted));

            DealPlayer(0);
            DealDealer(false);
            DealPlayer(0);
            DealDealer(true);

            if (dealer[0].Rank == CardRank.Ace && rules.AllowInsurance)
            {
                LastMessage = "La banca muestra un as. ¿Quieres seguro?";
                SetPhase(BlackjackPhase.Insurance);
            }
            else
            {
                ResolveOpening();
            }
            return true;
        }

        public bool CanTakeInsurance => Phase == BlackjackPhase.Insurance &&
                                        Bankroll >= Wager / 2;
        public bool ChooseInsurance(bool take)
        {
            if (Phase != BlackjackPhase.Insurance || (take && !CanTakeInsurance)) return false;
            if (take)
            {
                InsuranceWager = Wager / 2;
                Bankroll -= InsuranceWager;
            }
            ResolveOpening();
            return true;
        }

        public bool CanStand => Phase == BlackjackPhase.PlayerTurn && !ActiveHand.IsFinished;
        public bool CanHit => CanStand && deck.Remaining > 0;
        public bool CanDouble => allowDouble && CanHit && ActiveHand.Cards.Count == 2 &&
                                 Bankroll >= ActiveHand.Stake &&
                                 (!ActiveHand.IsSplit || rules.AllowDoubleAfterSplit);
        public bool CanSplit => allowSplit && CanHit && deck.Remaining >= 2 && ActiveHand.Cards.Count == 2 &&
                                hands.Count < rules.MaximumSplitHands &&
                                Bankroll >= ActiveHand.Stake &&
                                ActiveHand.Cards[0].BlackjackValue == ActiveHand.Cards[1].BlackjackValue;

        public bool Hit()
        {
            if (!CanHit) return false;
            DealPlayer(ActiveHandIndex);
            if (ActiveHand.Value.IsBust || ActiveHand.Value.Total == 21)
            {
                ActiveHand.IsFinished = true;
                AdvanceHand();
            }
            else
            {
                LastMessage = "Pide otra carta o plántate.";
                EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.HandChanged, ActiveHandIndex));
            }
            return true;
        }

        public bool Stand()
        {
            if (!CanStand) return false;
            ActiveHand.IsFinished = true;
            AdvanceHand();
            return true;
        }

        public bool DoubleDown()
        {
            if (!CanDouble) return false;
            var hand = ActiveHand;
            Bankroll -= hand.Stake;
            hand.Stake *= 2;
            hand.IsDoubled = true;
            DealPlayer(ActiveHandIndex);
            hand.IsFinished = true;
            AdvanceHand();
            return true;
        }

        public bool Split()
        {
            if (!CanSplit) return false;
            var first = ActiveHand;
            Bankroll -= first.Stake;
            var second = new BlackjackHand(first.Stake, playerAcesLow) { IsSplit = true };
            second.Add(first.RemoveSecondCard());
            first.IsSplit = true;
            var splitAces = first.Cards[0].Rank == CardRank.Ace;
            first.IsSplitAces = splitAces;
            second.IsSplitAces = splitAces;
            hands.Insert(ActiveHandIndex + 1, second);
            DealPlayer(ActiveHandIndex);
            DealPlayer(ActiveHandIndex + 1);
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.HandChanged, ActiveHandIndex));
            if (splitAces)
            {
                first.IsFinished = true;
                second.IsFinished = true;
                AdvanceHand();
            }
            else
            {
                LastMessage = "Juega la primera mano dividida.";
            }
            return true;
        }

        public bool AdvanceDealerStep()
        {
            if (Phase != BlackjackPhase.DealerTurn) return false;
            if (ShouldDealerHit()) DealDealer(false);
            if (!ShouldDealerHit()) Settle();
            return true;
        }

        private BlackjackHand ActiveHand => hands[ActiveHandIndex];

        private void DealPlayer(int handIndex)
        {
            var card = deck.Draw();
            hands[handIndex].Add(card);
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.CardDealt, handIndex, card));
        }

        private void DealDealer(bool hidden)
        {
            var card = deck.Draw();
            dealer.Add(card);
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.CardDealt, -1, card, true, hidden));
        }

        private void ResolveOpening()
        {
            if (DealerValue.IsBlackjack || hands[0].Value.IsBlackjack)
            {
                RevealHole();
                Settle();
            }
            else
            {
                LastMessage = "Tu turno: pide, plántate, dobla o divide.";
                SetPhase(BlackjackPhase.PlayerTurn);
            }
        }

        private void AdvanceHand()
        {
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.HandChanged, ActiveHandIndex));
            for (var i = ActiveHandIndex + 1; i < hands.Count; i++)
            {
                if (hands[i].IsFinished) continue;
                ActiveHandIndex = i;
                LastMessage = $"Juega la mano {i + 1}.";
                EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.HandChanged, i));
                return;
            }
            RevealHole();
            if (AllHandsBust()) Settle();
            else
            {
                LastMessage = "Turno de la banca.";
                SetPhase(BlackjackPhase.DealerTurn);
            }
        }

        private bool AllHandsBust()
        {
            foreach (var hand in hands)
                if (!hand.Value.IsBust) return false;
            return true;
        }

        private bool ShouldDealerHit()
        {
            var value = DealerValue;
            return deck.Remaining > 0 && (value.Total < dealerStandTotal ||
                   (value.Total == 17 && value.IsSoft && rules.DealerHitsSoft17));
        }

        private void RevealHole()
        {
            if (DealerHoleRevealed) return;
            DealerHoleRevealed = true;
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.HoleRevealed));
        }

        private void Settle()
        {
            var dealerValue = DealerValue;
            foreach (var hand in hands)
            {
                var value = hand.Value;
                if (dealerValue.IsBlackjack)
                    hand.Outcome = value.IsBlackjack ? HandOutcome.Push : HandOutcome.Loss;
                else if (value.IsBust)
                    hand.Outcome = HandOutcome.Loss;
                else if (value.IsBlackjack)
                    hand.Outcome = HandOutcome.Blackjack;
                else if (dealerValue.IsBust || value.Total > dealerValue.Total)
                    hand.Outcome = HandOutcome.Win;
                else
                    hand.Outcome = value.Total == dealerValue.Total ? HandOutcome.Push : HandOutcome.Loss;

                var payout = hand.Outcome switch
                {
                    HandOutcome.Blackjack => hand.Stake + hand.Stake * rules.BlackjackPayoutNumerator / rules.BlackjackPayoutDenominator,
                    HandOutcome.Win => hand.Stake * 2,
                    HandOutcome.Push => hand.Stake,
                    _ => 0
                };
                Bankroll = checked(Bankroll + payout);
            }

            if (dealerValue.IsBlackjack && InsuranceWager > 0)
                Bankroll = checked(Bankroll + InsuranceWager * 3);
            RoundProfit = Bankroll - bankrollBeforeRound;
            LastMessage = RoundProfit > 0 ? $"Ganaste {RoundProfit} fichas."
                : RoundProfit < 0 ? $"Perdiste {-RoundProfit} fichas."
                : "Empate: recuperas la apuesta.";
            SetPhase(BlackjackPhase.Settled);
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.RoundSettled));
        }

        private void SetPhase(BlackjackPhase phase)
        {
            Phase = phase;
            EventRaised?.Invoke(new BlackjackEvent(BlackjackEventType.PhaseChanged));
        }
    }
}
