using System.Linq;
using DoubleDown.Scoring;
using DoubleDown.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace DoubleDown.Tests
{
    public class RunEconomyTests
    {
        [Test]
        public void ShopTransactionsAreDeterministicAndCannotBeRepeated()
        {
            var ids=Enumerable.Range(0,15).Select(i=>i.ToString("00"));
            var a=new RunEconomy(ids,7);var b=new RunEconomy(ids,7);
            a.Open(1);b.Open(1);
            CollectionAssert.AreEqual(a.Offers,b.Offers);
            Assert.AreEqual(15,a.Gold);
            a.Open(1);Assert.AreEqual(15,a.Gold);
            var item=a.Offers[0];Assert.IsTrue(a.Buy(0));Assert.IsTrue(a.Owns(item));
            Assert.AreEqual(9,a.Gold);Assert.IsFalse(a.Buy(0));
            Assert.IsTrue(a.Reroll());Assert.AreEqual(7,a.Gold);
            Assert.IsFalse(a.Offers.Contains(item));
            while(a.Reroll()) { }
            Assert.GreaterOrEqual(a.Gold,0);
            Assert.IsFalse(a.Buy(0));
        }

        [Test]
        public void RewardIncludesUnusedHandsAndRerollsIncreaseUntilNextShop()
        {
            var shop = new RunEconomy(Enumerable.Range(0, 15).Select(i => i.ToString("00")), 9,
                new EconomyRules(startingGold: 10, relicPrice: 4, initialRerollPrice: 3,
                    rerollIncrease: 2, roundReward: 5, goldPerRemainingHand: 2));
            Assert.IsFalse(shop.Reroll());
            shop.Open(1, 3);
            Assert.AreEqual(12, shop.LastReward);
            Assert.AreEqual(22, shop.Gold);
            Assert.IsTrue(shop.Reroll());
            Assert.AreEqual(19, shop.Gold);
            Assert.AreEqual(5, shop.RerollPrice);
            shop.Open(1, 3);
            Assert.AreEqual(19, shop.Gold);
            Assert.AreEqual(5, shop.RerollPrice);
            Assert.IsTrue(shop.Buy(0));
            Assert.AreEqual(15, shop.Gold);
            shop.Open(2, 1);
            Assert.AreEqual(24, shop.Gold);
            Assert.AreEqual(3, shop.RerollPrice);
        }

        [Test]
        public void ExhaustedCatalogCannotChargeRerollsAndResetRestoresSeededOffers()
        {
            var shop = new RunEconomy(new[] { "02", "02" }, 9);
            shop.Open(1);
            CollectionAssert.AreEqual(new[] { "02", null, null }, shop.Offers);
            Assert.IsTrue(shop.Buy(0));
            var gold = shop.Gold;
            Assert.IsFalse(shop.CanReroll);
            Assert.IsFalse(shop.Reroll());
            Assert.AreEqual(gold, shop.Gold);
            shop.Reset();
            Assert.AreEqual(8, shop.Gold);
            Assert.IsFalse(shop.Owns("02"));
            Assert.IsTrue(shop.Offers.All(id => id == null));
            shop.Open(1);
            CollectionAssert.AreEqual(new[] { "02", null, null }, shop.Offers);
        }

        [Test]
        public void StarterRelicIsFreeUniqueAndResetsWithRun()
        {
            var shop=new RunEconomy(new[]{"00","01","02"},9);
            Assert.IsFalse(shop.Owns("00"));
            Assert.IsTrue(shop.ClaimStarter("01"));
            Assert.IsTrue(shop.Owns("01"));
            Assert.AreEqual(8,shop.Gold);
            Assert.IsFalse(shop.ClaimStarter("02"));
            shop.Reset();
            Assert.IsFalse(shop.Owns("01"));
            Assert.IsTrue(shop.ClaimStarter("02"));
        }

        [Test]
        public void AllSupportedHandSizesStayInsideTheirReservedZones()
        {
            for(var count=1;count<=11;count++)
            for(var split=0;split<=1;split++)
            for(var hand=0;hand<(split==1?2:1);hand++)
            for(var i=0;i<count;i++)
            {
                var pos=TableLayout.CardPosition(i,count,false,hand,split==1);
                var size=64f*(split==1?1.15f:1.35f);
                var rect=new Rect(pos.x*64+320-size/2,180-pos.y*64-size/2,size,size);
                Assert.IsFalse(rect.Overlaps(TableLayout.RunRail));
                Assert.IsFalse(rect.Overlaps(TableLayout.Relics));
                Assert.IsFalse(rect.Overlaps(TableLayout.Actions));
                Assert.IsTrue(TableLayout.PlayerCards.Contains(rect.min));
                Assert.IsTrue(TableLayout.PlayerCards.Contains(rect.max));
            }
        }
    }
}
