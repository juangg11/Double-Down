using System.Linq;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using NUnit.Framework;

namespace DoubleDown.Tests
{
    public class BossTests
    {
        private static PlayingCard Card(CardRank rank, CardSuit suit = CardSuit.Clubs) => new PlayingCard(suit, rank);

        [Test]
        public void BossesRotateEveryThirdRoundIndependentOfCatalogOrder()
        {
            var pool = new[] { new BossRule("a", "ACES", "", true), new BossRule("b", "DEALER", "", dealerStandTotal: 18),
                new BossRule("c", "HANDS", "", maximumHands: 2) };
            var a = new BossSchedule(pool, 17);
            var b = new BossSchedule(pool.Reverse(), 17);
            for (var round = 1; round <= 30; round++)
            {
                Assert.AreEqual(round % 3 == 0, a.ForRound(round).HasValue);
                Assert.AreEqual(a.ForRound(round)?.Id, b.ForRound(round)?.Id);
            }
            Assert.AreEqual(3, new[] { a.ForRound(3).Value.Id, a.ForRound(6).Value.Id, a.ForRound(9).Value.Id }.Distinct().Count());
            var gated = new BossSchedule(new[] { pool[0], new BossRule("late", "LATE", "", firstTable: 5) }, 1);
            Assert.AreEqual("a", gated.ForRound(12).Value.Id);
            Assert.AreEqual("late", gated.ForRound(15).Value.Id);
        }

        [Test]
        public void LowAcesAffectPlayerNaturalButDoNotChangeDealerBlackjack()
        {
            var deck = new[] { Card(CardRank.Ace), Card(CardRank.Ace, CardSuit.Hearts),
                Card(CardRank.King), Card(CardRank.King, CardSuit.Hearts) };
            var session = new BlackjackSession(new BlackjackRules(allowInsurance: false), 1, deck);
            Assert.IsTrue(session.ConfigureTable(acesLow: true));
            session.StartRound(10);
            Assert.AreEqual(11, session.Hands[0].Value.Total);
            Assert.IsFalse(session.Hands[0].Value.IsBlackjack);
            Assert.IsTrue(session.DealerValue.IsBlackjack);
            Assert.AreEqual(HandOutcome.Loss, session.Hands[0].Outcome);
            Assert.AreEqual(190, session.Bankroll);
        }

        [Test]
        public void LowAcesRemainLowInBothSplitHands()
        {
            var deck = new[] { Card(CardRank.Ace), Card(CardRank.Nine), Card(CardRank.Ace, CardSuit.Hearts),
                Card(CardRank.Seven), Card(CardRank.King), Card(CardRank.Queen) };
            var session = new BlackjackSession(new BlackjackRules(startingBankroll: 200), 1, deck);
            session.ConfigureTable(acesLow: true);
            session.StartRound(10);
            Assert.IsFalse(session.ConfigureTable());
            Assert.IsTrue(session.Split());
            Assert.AreEqual(11, session.Hands[0].Value.Total);
            Assert.AreEqual(11, session.Hands[1].Value.Total);
            Assert.IsTrue(session.Hands.All(hand => !hand.Value.IsBlackjack && hand.IsFinished));
        }

        [Test]
        public void DealerEighteenDrawsOnHardSeventeenAndStandardRulesCanBeRestored()
        {
            var deck = new[] { Card(CardRank.Ten), Card(CardRank.Ten, CardSuit.Hearts),
                Card(CardRank.Eight), Card(CardRank.Seven), Card(CardRank.Ace) };
            var boss = new BlackjackSession(new BlackjackRules(startingBankroll: 200), 7, deck);
            var normal = new BlackjackSession(new BlackjackRules(startingBankroll: 200), 7, deck);
            boss.ConfigureTable(dealerStand: 18);
            foreach (var session in new[] { boss, normal })
            {
                session.StartRound(10);
                session.Stand();
                while (session.Phase == BlackjackPhase.DealerTurn) session.AdvanceDealerStep();
            }
            Assert.AreEqual(3, boss.DealerCards.Count);
            Assert.AreEqual(18, boss.DealerValue.Total);
            Assert.AreEqual(HandOutcome.Push, boss.Hands[0].Outcome);
            Assert.AreEqual(HandOutcome.Win, normal.Hands[0].Outcome);
            var bankroll = boss.Bankroll;
            var round = boss.RoundIndex;
            Assert.IsTrue(boss.ConfigureTable());
            Assert.AreEqual(17, boss.DealerStandTotal);
            Assert.AreEqual(bankroll, boss.Bankroll);
            Assert.AreEqual(round, boss.RoundIndex);
            boss.StartRound(10); normal.StartRound(10);
            CollectionAssert.AreEqual(normal.Hands[0].Cards, boss.Hands[0].Cards);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BossHandLimitExpiresAfterTheRoundOrRestart(bool win)
        {
            var run = new ScoreRun(new ScoreRules(firstGoal: 120), round => round % 3 == 0 ? 2 : 0);
            var victory = new[] { new ScoreBreakdown(BigNumber.From(999), 1) };
            for (var i = 0; i < 2; i++) { run.RecordDeal(victory); run.NextRound(); }
            Assert.AreEqual(3, run.Round);
            Assert.AreEqual(2, run.HandLimit);
            Assert.AreEqual(2, run.HandsRemaining);
            if (win) { run.RecordDeal(victory); run.NextRound(); Assert.AreEqual(4, run.Round); }
            else
            {
                var loss = new[] { new ScoreBreakdown(BigNumber.Zero, 0) };
                run.RecordDeal(loss); run.RecordDeal(loss);
                Assert.AreEqual(ScoreRunState.Lost, run.State);
                run.Restart();
                Assert.AreEqual(1, run.Round);
            }
            Assert.AreEqual(4, run.HandLimit);
            Assert.AreEqual(4, run.HandsRemaining);
        }
    }
}
