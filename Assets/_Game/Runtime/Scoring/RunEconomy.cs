using System;
using System.Collections.Generic;

namespace DoubleDown.Scoring
{
    public readonly struct EconomyRules
    {
        public int StartingGold { get; }
        public int RelicPrice { get; }
        public int InitialRerollPrice { get; }
        public int RerollIncrease { get; }
        public int RoundReward { get; }
        public int GoldPerRemainingHand { get; }

        public EconomyRules(int startingGold = 8, int relicPrice = 6,
            int initialRerollPrice = 2, int rerollIncrease = 1,
            int roundReward = 6, int goldPerRemainingHand = 1)
        {
            if (startingGold < 0 || relicPrice < 1 || initialRerollPrice < 1 ||
                rerollIncrease < 1 || roundReward < 0 || goldPerRemainingHand < 0)
                throw new ArgumentOutOfRangeException(nameof(startingGold));
            StartingGold = startingGold;
            RelicPrice = relicPrice;
            InitialRerollPrice = initialRerollPrice;
            RerollIncrease = rerollIncrease;
            RoundReward = roundReward;
            GoldPerRemainingHand = goldPerRemainingHand;
        }
    }

    public sealed class RunEconomy
    {
        private readonly List<string> catalog;
        private readonly HashSet<string> owned = new HashSet<string>();
        private readonly string[] offers = new string[3];
        private readonly EconomyRules rules;
        private readonly int seed;
        private Random random;
        public int Gold { get; private set; }
        public int RewardedRound { get; private set; }
        public int LastReward { get; private set; }
        public int RerollPrice { get; private set; }
        public int RelicPrice => rules.RelicPrice;
        public bool CanReroll => RewardedRound > 0 && Gold >= RerollPrice &&
            catalog.Exists(id => !owned.Contains(id));
        public IReadOnlyList<string> Offers => offers;
        public bool Owns(string id) => owned.Contains(id);
        public RunEconomy(IEnumerable<string> ids, int seed, EconomyRules? rules = null)
        {
            catalog = new List<string>(new HashSet<string>(ids));
            catalog.Sort(StringComparer.Ordinal);
            this.rules = rules ?? new EconomyRules(startingGold: 8);
            this.seed = seed;
            Reset();
        }
        public void Reset()
        {
            random = new Random(seed);
            owned.Clear(); foreach(var id in new[]{"00","01","08","14"}) owned.Add(id);
            Gold = rules.StartingGold;
            RewardedRound = 0;
            LastReward = 0;
            RerollPrice = rules.InitialRerollPrice;
            Array.Clear(offers, 0, offers.Length);
        }
        public void Open(int completedRound, int remainingHands = 0)
        {
            if(completedRound<=RewardedRound) return;
            if (remainingHands < 0) throw new ArgumentOutOfRangeException(nameof(remainingHands));
            LastReward = checked(rules.RoundReward + completedRound + remainingHands * rules.GoldPerRemainingHand);
            Gold = checked(Gold + LastReward);
            RewardedRound = completedRound;
            RerollPrice = rules.InitialRerollPrice;
            Roll();
        }
        public bool Reroll()
        {
            if (!CanReroll) return false;
            var nextPrice = checked(RerollPrice + rules.RerollIncrease);
            Gold -= RerollPrice;
            RerollPrice = nextPrice;
            Roll();
            return true;
        }
        public bool Buy(int index)
        {
            if(index<0 || index>=offers.Length || offers[index]==null || Gold<RelicPrice) return false;
            if(!owned.Add(offers[index])) return false;
            Gold-=RelicPrice; offers[index]=null; return true;
        }
        private void Roll()
        {
            var pool=catalog.FindAll(id=>!owned.Contains(id));
            for(var i=0;i<offers.Length;i++)
            {
                if(pool.Count==0) { offers[i]=null; continue; }
                var pick=random.Next(pool.Count); offers[i]=pool[pick]; pool.RemoveAt(pick);
            }
        }
    }
}
