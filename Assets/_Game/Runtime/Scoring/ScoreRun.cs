using System;
using System.Collections.Generic;
using DoubleDown.Blackjack;

namespace DoubleDown.Scoring
{
    public readonly struct ScoreRules
    {
        public int FirstGoal { get; }
        public double GoalGrowth { get; }
        public int HandsPerRound { get; }
        public int ChipsPerCard { get; }
        public int ChipsPerWagerUnit { get; }
        public int WagerUnit { get; }
        public double WinMultiplier { get; }
        public double BlackjackMultiplier { get; }
        public double PushMultiplier { get; }

        public ScoreRules(int firstGoal = 120, double goalGrowth = 1.18,
            int handsPerRound = 4, int chipsPerCard = 5, int chipsPerWagerUnit = 1,
            int wagerUnit = 2, double winMultiplier = 2, double blackjackMultiplier = 3.5,
            double pushMultiplier = 0.5)
        {
            if (firstGoal < 1 || goalGrowth < 1 || handsPerRound < 1 || chipsPerCard < 0 ||
                chipsPerWagerUnit < 0 || wagerUnit < 1 || winMultiplier < 0 ||
                blackjackMultiplier < 0 || pushMultiplier < 0)
                throw new ArgumentOutOfRangeException(nameof(firstGoal));
            FirstGoal = firstGoal;
            GoalGrowth = goalGrowth;
            HandsPerRound = handsPerRound;
            ChipsPerCard = chipsPerCard;
            ChipsPerWagerUnit = chipsPerWagerUnit;
            WagerUnit = wagerUnit;
            WinMultiplier = winMultiplier;
            BlackjackMultiplier = blackjackMultiplier;
            PushMultiplier = pushMultiplier;
        }
    }

    public readonly struct ScoreBreakdown
    {
        public BigNumber Chips { get; }
        public BigNumber Multiplier { get; }
        public BigNumber Total => Chips * Multiplier;
        public ScoreBreakdown(BigNumber chips, double multiplier)
            : this(chips, new BigNumber(multiplier)) { }
        public ScoreBreakdown(BigNumber chips, BigNumber multiplier)
        {
            Chips = chips;
            Multiplier = multiplier;
        }
    }

    public static class ScoreCalculator
    {
        public static ScoreBreakdown Calculate(HandValue hand, HandOutcome outcome,
            int cardCount, long stake, ScoreRules rules)
        {
            if (cardCount < 0 || stake < 0) throw new ArgumentOutOfRangeException();
            var chips = BigNumber.From(hand.Total + (long)cardCount * rules.ChipsPerCard +
                stake / rules.WagerUnit * rules.ChipsPerWagerUnit);
            var multiplier = outcome switch
            {
                HandOutcome.Win => rules.WinMultiplier,
                HandOutcome.Blackjack => rules.BlackjackMultiplier,
                HandOutcome.Push => rules.PushMultiplier,
                _ => 0d
            };
            return new ScoreBreakdown(chips, multiplier);
        }
    }

    public enum ScoreRunState { Playing, Won, Lost }

    public sealed class ScoreRun
    {
        private readonly ScoreRules rules;
        private readonly Func<int, int> handLimitForRound;
        public int Round { get; private set; } = 1;
        public int HandsRemaining { get; private set; }
        public int HandLimit { get; private set; }
        public BigNumber Score { get; private set; }
        public BigNumber Goal { get; private set; }
        public BigNumber RunTotal { get; private set; }
        public ScoreRunState State { get; private set; } = ScoreRunState.Playing;

        public ScoreRun(ScoreRules rules, Func<int, int> handLimitForRound = null)
        {
            this.rules = rules;
            this.handLimitForRound = handLimitForRound;
            ResetRound();
        }

        public BigNumber RecordDeal(IReadOnlyList<ScoreBreakdown> hands)
        {
            if (State != ScoreRunState.Playing || hands == null || hands.Count == 0)
                throw new InvalidOperationException("No active scoring hand.");
            var amount = BigNumber.Zero;
            foreach (var hand in hands) amount += hand.Total;
            HandsRemaining--;
            Score += amount;
            RunTotal += amount;
            if (Score >= Goal) State = ScoreRunState.Won;
            else if (HandsRemaining == 0) State = ScoreRunState.Lost;
            return amount;
        }

        public bool NextRound()
        {
            if (State != ScoreRunState.Won) return false;
            Round++;
            ResetRound();
            return true;
        }

        public void Restart()
        {
            Round = 1;
            RunTotal = BigNumber.Zero;
            ResetRound();
        }

        public void Forfeit() { if (State == ScoreRunState.Playing) State = ScoreRunState.Lost; }

        private void ResetRound()
        {
            var limit = handLimitForRound?.Invoke(Round) ?? 0;
            if (limit < 0) throw new ArgumentOutOfRangeException(nameof(handLimitForRound));
            HandLimit = limit == 0 ? rules.HandsPerRound : Math.Min(rules.HandsPerRound, limit);
            HandsRemaining = HandLimit;
            Goal = BigNumber.From(rules.FirstGoal) * BigNumber.Pow(rules.GoalGrowth, Round - 1);
            Score = BigNumber.Zero;
            State = ScoreRunState.Playing;
        }
    }
}
