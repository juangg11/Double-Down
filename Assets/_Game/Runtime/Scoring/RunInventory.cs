using System;
using System.Collections.Generic;
namespace DoubleDown.Scoring
{
    public sealed class RunInventory
    {
        public RunDeck Deck { get; } = new RunDeck();
        private readonly List<ConsumableDefinition> consumables = new List<ConsumableDefinition>();
        private readonly List<VoucherDefinition> vouchers = new List<VoucherDefinition>();
        public IReadOnlyList<ConsumableDefinition> Consumables => consumables.AsReadOnly();
        public IReadOnlyList<VoucherDefinition> Vouchers => vouchers.AsReadOnly();
        public int ConsumableCapacity => 2 + Bonus(VoucherEffect.ConsumableSlots);
        public int RelicCapacity => Math.Min(6, 4 + Bonus(VoucherEffect.RelicSlots));
        public int Bonus(VoucherEffect effect)
        { var total = 0; foreach(var voucher in vouchers) if(voucher.effect == effect) total += voucher.amount; return total; }
        public bool OwnsVoucher(string id) => vouchers.Exists(v => v.id == id);
        public bool CanBuy(VoucherDefinition item) => item != null && !OwnsVoucher(item.id) &&
            (string.IsNullOrEmpty(item.prerequisite) || OwnsVoucher(item.prerequisite));
        public bool Buy(ConsumableDefinition item, RunEconomy economy, int price)
        {
            if (item == null || consumables.Count >= ConsumableCapacity || !economy.TrySpend(price)) return false;
            consumables.Add(item); return true;
        }
        public bool Buy(VoucherDefinition item, RunEconomy economy, int price)
        {
            if (!CanBuy(item) || !economy.TrySpend(price)) return false;
            vouchers.Add(item); return true;
        }
        public bool Use(int index, int cardId, bool betweenHands)
        {
            if (!betweenHands || index < 0 || index >= consumables.Count || !Deck.Use(consumables[index], cardId)) return false;
            consumables.RemoveAt(index); return true;
        }
        public bool Discard(int index, bool betweenHands)
        {
            if(!betweenHands || index<0 || index>=consumables.Count)return false;
            consumables.RemoveAt(index);return true;
        }
        public void Reset() { Deck.Reset(); consumables.Clear(); vouchers.Clear(); }
    }
}
