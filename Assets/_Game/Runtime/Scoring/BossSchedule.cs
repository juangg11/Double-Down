using System;
using System.Collections.Generic;

namespace DoubleDown.Scoring
{
    public readonly struct BossRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool PlayerAcesLow { get; }
        public int DealerStandTotal { get; }
        public int MaximumHands { get; }
        public int FirstTable { get; }

        public BossRule(string id, string name, string description, bool playerAcesLow = false,
            int dealerStandTotal = 17, int maximumHands = 0, int firstTable = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) ||
                dealerStandTotal < 17 || dealerStandTotal > 20 || maximumHands < 0 || firstTable < 1)
                throw new ArgumentException("Invalid boss definition.");
            Id = id;
            Name = name;
            Description = description;
            PlayerAcesLow = playerAcesLow;
            DealerStandTotal = dealerStandTotal;
            MaximumHands = maximumHands;
            FirstTable = firstTable;
        }
    }

    public sealed class BossSchedule
    {
        private readonly List<BossRule> pool;
        private readonly uint seed;

        public BossSchedule(IEnumerable<BossRule> bosses, uint seed)
        {
            pool = new List<BossRule>(bosses);
            pool.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            var ids = new HashSet<string>();
            foreach (var boss in pool)
                if (!ids.Add(boss.Id)) throw new ArgumentException("Duplicate boss ID: " + boss.Id);
            if (!pool.Exists(boss => boss.FirstTable == 1))
                throw new ArgumentException("A boss must be available from table one.");
            this.seed = seed;
        }

        public BossRule? ForRound(int round)
        {
            if (round < 1) throw new ArgumentOutOfRangeException(nameof(round));
            if (round % 3 != 0) return null;
            var table = round / 3;
            var available = pool.FindAll(boss => boss.FirstTable <= table);
            // Stable rotation: reloading or inspecting a boss never consumes random state.
            return available[(int)(((ulong)seed + (ulong)table - 1) % (ulong)available.Count)];
        }
    }
}
