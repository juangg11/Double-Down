using System;
using UnityEngine;

namespace DoubleDown.Blackjack
{
    [CreateAssetMenu(fileName = "BlackjackRules", menuName = "Double Down/Blackjack Rules")]
    public sealed class BlackjackRulesConfig : ScriptableObject
    {
        [Min(2)] public int startingBankroll = 200;
        [Min(2)] public int minimumWager = 10;
        public bool dealerHitsSoft17;
        public bool allowInsurance = true;
        public bool allowDoubleAfterSplit = true;
        [Range(1, 2)] public int maximumSplitHands = 2;
        [Min(1)] public int blackjackPayoutNumerator = 3;
        [Min(1)] public int blackjackPayoutDenominator = 2;

        public BlackjackRules ToRules() => new BlackjackRules(startingBankroll, minimumWager,
            dealerHitsSoft17, allowInsurance, allowDoubleAfterSplit, maximumSplitHands,
            blackjackPayoutNumerator, blackjackPayoutDenominator);
    }

    public readonly struct BlackjackRules
    {
        public long StartingBankroll { get; }
        public long MinimumWager { get; }
        public bool DealerHitsSoft17 { get; }
        public bool AllowInsurance { get; }
        public bool AllowDoubleAfterSplit { get; }
        public int MaximumSplitHands { get; }
        public int BlackjackPayoutNumerator { get; }
        public int BlackjackPayoutDenominator { get; }

        public BlackjackRules(long startingBankroll = 200, long minimumWager = 10,
            bool dealerHitsSoft17 = false, bool allowInsurance = true,
            bool allowDoubleAfterSplit = true, int maximumSplitHands = 2,
            int blackjackPayoutNumerator = 3, int blackjackPayoutDenominator = 2)
        {
            if (startingBankroll < 0 || minimumWager < 2 || minimumWager % 2 != 0 ||
                maximumSplitHands < 1 || blackjackPayoutNumerator < 1 ||
                blackjackPayoutDenominator < 1)
                throw new ArgumentOutOfRangeException(nameof(minimumWager), "Invalid blackjack rules.");
            StartingBankroll = startingBankroll;
            MinimumWager = minimumWager;
            DealerHitsSoft17 = dealerHitsSoft17;
            AllowInsurance = allowInsurance;
            AllowDoubleAfterSplit = allowDoubleAfterSplit;
            MaximumSplitHands = maximumSplitHands;
            BlackjackPayoutNumerator = blackjackPayoutNumerator;
            BlackjackPayoutDenominator = blackjackPayoutDenominator;
        }
    }
}
