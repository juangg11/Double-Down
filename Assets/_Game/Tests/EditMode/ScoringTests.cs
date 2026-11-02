using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using NUnit.Framework;

namespace DoubleDown.Tests
{
    public sealed class ScoringTests
    {
        [Test]
        public void BigNumberNormalizesAddsAndFormatsBeyondInt64()
        {
            var huge = new BigNumber(1.5, 40);
            Assert.AreEqual(41, (huge * new BigNumber(10)).Exponent);
            Assert.AreEqual(huge, huge + BigNumber.From(1));
            Assert.IsTrue(huge > BigNumber.From(long.MaxValue));
            Assert.IsTrue(BigNumber.Zero < new BigNumber(0.1));
            Assert.AreEqual("1.5e40", huge.ToString());
            Assert.AreEqual("1.2K", new BigNumber(1200).ToString());
        }

        [Test]
        public void HandPipelineUsesChipsTimesOutcomeMultiplier()
        {
            var hand = HandValue.Evaluate(new[]
            {
                new PlayingCard(CardSuit.Clubs, CardRank.Ten),
                new PlayingCard(CardSuit.Spades, CardRank.Queen)
            });
            var rules = new ScoreRules(firstGoal: 140);
            var win = ScoreCalculator.Calculate(hand, HandOutcome.Win, 2, 10, rules);
            Assert.AreEqual("35", win.Chips.ToString());
            Assert.AreEqual(BigNumber.From(2), win.Multiplier);
            Assert.AreEqual("70", win.Total.ToString());
            Assert.AreEqual(BigNumber.Zero,
                ScoreCalculator.Calculate(hand, HandOutcome.Loss, 2, 10, rules).Total);
        }

        [Test]
        public void RoundTracksLimitedDealsAndScalesTheNextGoal()
        {
            var run = new ScoreRun(new ScoreRules(firstGoal: 100, goalGrowth: 2,
                handsPerRound: 2));
            var scored = new[] { new ScoreBreakdown(BigNumber.From(60), 1) };
            Assert.AreEqual(BigNumber.From(60), run.RecordDeal(scored));
            Assert.AreEqual(1, run.HandsRemaining);
            run.RecordDeal(scored);
            Assert.AreEqual(ScoreRunState.Won, run.State);
            Assert.IsTrue(run.NextRound());
            Assert.AreEqual(2, run.Round);
            Assert.AreEqual("200", run.Goal.ToString());
            Assert.AreEqual(BigNumber.From(120), run.RunTotal);
            var blank = new[] { new ScoreBreakdown(BigNumber.From(60), 0) };
            run.RecordDeal(blank);
            run.RecordDeal(blank);
            Assert.AreEqual(ScoreRunState.Lost, run.State);
            Assert.IsFalse(run.NextRound());
            run.Restart();
            Assert.AreEqual(1, run.Round);
            Assert.AreEqual(BigNumber.Zero, run.RunTotal);
        }
    }
}
