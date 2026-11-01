using System.Collections.Generic;
using DoubleDown.Blackjack;
using NUnit.Framework;

namespace DoubleDown.Tests
{
    public sealed class BlackjackRulesTests
    {
        private static PlayingCard Card(CardRank rank, CardSuit suit = CardSuit.Clubs) =>
            new PlayingCard(suit, rank);

        [Test]
        public void AcesUseBestTotalAndNaturalRequiresTwoCards()
        {
            var cards = new[] { Card(CardRank.Ace), Card(CardRank.Ace), Card(CardRank.Nine) };
            var value = HandValue.Evaluate(cards);
            Assert.AreEqual(21, value.Total);
            Assert.IsTrue(value.IsSoft);
            Assert.IsFalse(value.IsBlackjack);
            Assert.AreEqual(12, HandValue.Evaluate(new[]
            {
                Card(CardRank.Ace), Card(CardRank.Ace), Card(CardRank.Ten)
            }).Total);
            Assert.IsTrue(HandValue.Evaluate(new[] { Card(CardRank.Ace), Card(CardRank.King) }).IsBlackjack);
            Assert.IsFalse(HandValue.Evaluate(new[] { Card(CardRank.Ace), Card(CardRank.King) }, false).IsBlackjack);
        }

        [Test]
        public void SeededDeckIsDeterministicAndContainsEveryCardOnce()
        {
            var left = new BlackjackDeck(763u);
            var right = new BlackjackDeck(763u);
            var seen = new HashSet<PlayingCard>();
            for (var i = 0; i < 52; i++)
            {
                var card = left.Draw();
                Assert.AreEqual(card, right.Draw());
                Assert.IsTrue(seen.Add(card));
            }
            Assert.AreEqual(0, left.Remaining);
            Assert.AreEqual(52, seen.Count);
        }

        [Test]
        public void NaturalBlackjackPaysThreeToTwo()
        {
            var game = Session(Card(CardRank.Ace), Card(CardRank.Nine),
                Card(CardRank.King), Card(CardRank.Seven));
            Assert.IsTrue(game.StartRound(10));
            Assert.AreEqual(BlackjackPhase.Settled, game.Phase);
            Assert.AreEqual(HandOutcome.Blackjack, game.Hands[0].Outcome);
            Assert.AreEqual(215, game.Bankroll);
            Assert.AreEqual(15, game.RoundProfit);
        }

        [Test]
        public void InsurancePaysTwoToOneAgainstDealerNatural()
        {
            var game = Session(Card(CardRank.Ten), Card(CardRank.Ace),
                Card(CardRank.Nine), Card(CardRank.King));
            game.StartRound(10);
            Assert.AreEqual(BlackjackPhase.Insurance, game.Phase);
            Assert.IsTrue(game.ChooseInsurance(true));
            Assert.AreEqual(BlackjackPhase.Settled, game.Phase);
            Assert.AreEqual(HandOutcome.Loss, game.Hands[0].Outcome);
            Assert.AreEqual(5, game.InsuranceWager);
            Assert.AreEqual(200, game.Bankroll);
        }

        [Test]
        public void TwoNaturalBlackjacksPushAfterInsuranceDecision()
        {
            var game = Session(Card(CardRank.Ace), Card(CardRank.Ace, CardSuit.Hearts),
                Card(CardRank.King), Card(CardRank.Queen));
            game.StartRound(10);
            Assert.AreEqual(BlackjackPhase.Insurance, game.Phase);
            Assert.IsTrue(game.ChooseInsurance(false));
            Assert.AreEqual(HandOutcome.Push, game.Hands[0].Outcome);
            Assert.AreEqual(200, game.Bankroll);
        }

        [Test]
        public void InsuranceIsOptionalWhenNoChipsRemain()
        {
            var game = new BlackjackSession(new BlackjackRules(10, 10), 1u, new[]
            {
                Card(CardRank.Ten), Card(CardRank.Ace),
                Card(CardRank.Nine), Card(CardRank.Seven)
            });
            game.StartRound(10);
            Assert.IsFalse(game.CanTakeInsurance);
            Assert.IsFalse(game.ChooseInsurance(true));
            Assert.IsTrue(game.ChooseInsurance(false));
            Assert.AreEqual(BlackjackPhase.PlayerTurn, game.Phase);
        }

        [Test]
        public void DealerStandsOnSoftSeventeenByDefault()
        {
            var game = Session(Card(CardRank.Ten), Card(CardRank.Ace),
                Card(CardRank.Eight), Card(CardRank.Six));
            game.StartRound(10);
            game.ChooseInsurance(false);
            Assert.IsTrue(game.Stand());
            Assert.AreEqual(BlackjackPhase.DealerTurn, game.Phase);
            Assert.IsTrue(game.AdvanceDealerStep());
            Assert.AreEqual(2, game.DealerCards.Count);
            Assert.AreEqual(HandOutcome.Win, game.Hands[0].Outcome);
            Assert.AreEqual(210, game.Bankroll);
        }

        [Test]
        public void ConfigurableDealerHitOnSoftSeventeenDrawsAgain()
        {
            var rules = new BlackjackRules(200, 10, dealerHitsSoft17: true);
            var game = new BlackjackSession(rules, 1u, new[]
            {
                Card(CardRank.Ten), Card(CardRank.Ace),
                Card(CardRank.Eight), Card(CardRank.Six), Card(CardRank.Two)
            });
            game.StartRound(10);
            game.ChooseInsurance(false);
            game.Stand();
            game.AdvanceDealerStep();
            Assert.AreEqual(3, game.DealerCards.Count);
            Assert.AreEqual(19, game.DealerValue.Total);
            Assert.AreEqual(HandOutcome.Loss, game.Hands[0].Outcome);
        }

        [Test]
        public void DoubleTakesOneCardAndSettlesTheHand()
        {
            var game = Session(Card(CardRank.Five), Card(CardRank.Nine),
                Card(CardRank.Six), Card(CardRank.Seven),
                Card(CardRank.Ten), Card(CardRank.Ten));
            game.StartRound(10);
            Assert.IsTrue(game.DoubleDown());
            Assert.AreEqual(20, game.Hands[0].Stake);
            Assert.IsTrue(game.Hands[0].IsDoubled);
            Assert.AreEqual(3, game.Hands[0].Cards.Count);
            Assert.IsFalse(game.Hit());
            game.AdvanceDealerStep();
            Assert.AreEqual(220, game.Bankroll);
        }

        [Test]
        public void BustRevealsHoleAndSettlesWithoutDealerDraw()
        {
            var game = Session(Card(CardRank.Ten), Card(CardRank.Nine),
                Card(CardRank.Nine), Card(CardRank.Seven), Card(CardRank.Five));
            game.StartRound(10);
            Assert.IsTrue(game.Hit());
            Assert.AreEqual(BlackjackPhase.Settled, game.Phase);
            Assert.IsTrue(game.DealerHoleRevealed);
            Assert.AreEqual(2, game.DealerCards.Count);
            Assert.AreEqual(HandOutcome.Loss, game.Hands[0].Outcome);
            Assert.AreEqual(190, game.Bankroll);
        }

        [Test]
        public void SplitCreatesTwoHandsAndNeverAwardsSplitBlackjack()
        {
            var game = Session(Card(CardRank.Eight), Card(CardRank.Nine),
                Card(CardRank.Eight, CardSuit.Hearts), Card(CardRank.Seven),
                Card(CardRank.Ten), Card(CardRank.Two),
                Card(CardRank.Ace), Card(CardRank.Ten));
            game.StartRound(10);
            Assert.IsTrue(game.Split());
            Assert.AreEqual(2, game.Hands.Count);
            Assert.AreEqual(180, game.Bankroll);
            Assert.IsTrue(game.Stand());
            Assert.AreEqual(1, game.ActiveHandIndex);
            Assert.IsTrue(game.Hit());
            Assert.AreEqual(BlackjackPhase.DealerTurn, game.Phase);
            game.AdvanceDealerStep();
            Assert.AreEqual(HandOutcome.Win, game.Hands[0].Outcome);
            Assert.AreEqual(HandOutcome.Win, game.Hands[1].Outcome);
            Assert.IsFalse(game.Hands[1].Value.IsBlackjack);
            Assert.AreEqual(220, game.Bankroll);
        }

        [Test]
        public void SplitAcesReceiveOneCardEach()
        {
            var game = Session(Card(CardRank.Ace), Card(CardRank.Nine),
                Card(CardRank.Ace, CardSuit.Hearts), Card(CardRank.Seven),
                Card(CardRank.Ten), Card(CardRank.Nine), Card(CardRank.Ten));
            game.StartRound(10);
            Assert.IsTrue(game.Split());
            Assert.IsFalse(game.CanHit);
            Assert.AreEqual(BlackjackPhase.DealerTurn, game.Phase);
            Assert.AreEqual(2, game.Hands[0].Cards.Count);
            Assert.AreEqual(2, game.Hands[1].Cards.Count);
        }

        private static BlackjackSession Session(params PlayingCard[] drawOrder) =>
            new BlackjackSession(new BlackjackRules(200, 10), 1u, drawOrder);
    }
}
