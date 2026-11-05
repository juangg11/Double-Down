using System;
using System.Collections.Generic;
using System.Linq;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using NUnit.Framework;
using UnityEngine;
namespace DoubleDown.Tests
{
    public class RunContentTests
    {
        private static ConsumableDefinition C(string id) => Resources.LoadAll<ConsumableDefinition>("Consumables").Single(x=>x.id==id);
        private static VoucherDefinition V(string id) => Resources.LoadAll<VoucherDefinition>("Vouchers").Single(x=>x.id==id);
        [Test] public void CompleteCatalogHasValidUniqueDefinitionsAndReferences()
        {
            var mods=Resources.LoadAll<CardModificationDefinition>("CardModifications");Assert.AreEqual(12,mods.Length);
            Assert.AreEqual(12,mods.Select(x=>x.id).Distinct().Count());Assert.IsTrue(mods.All(x=>x.icon!=null && x.multiplierFactor>=1));
            var cons=Resources.LoadAll<ConsumableDefinition>("Consumables");Assert.AreEqual(20,cons.Length);
            Assert.AreEqual(20,cons.Select(x=>x.id).Distinct().Count());Assert.IsTrue(cons.All(x=>x.icon!=null && (x.effect!=ConsumableEffect.Modify || x.modification!=null)));
            var vouchers=Resources.LoadAll<VoucherDefinition>("Vouchers");Assert.AreEqual(12,vouchers.Length);
            Assert.AreEqual(12,vouchers.Select(x=>x.id).Distinct().Count());Assert.IsTrue(vouchers.All(x=>x.icon!=null && x.amount>0));
            Assert.IsTrue(vouchers.All(x=>string.IsNullOrEmpty(x.prerequisite) || vouchers.Any(v=>v.id==x.prerequisite)));
            var bosses=Resources.LoadAll<BossDefinition>("Bosses");Assert.AreEqual(10,bosses.Length);Assert.AreEqual(10,bosses.Select(x=>x.id).Distinct().Count());
            foreach(var boss in bosses)Assert.DoesNotThrow(()=>boss.ToRule());
        }
        [Test] public void DuplicatesHaveIndependentIdentityAndLayersAndSurviveSeededShuffle()
        {
            var deck=new RunDeck();Assert.IsTrue(deck.Use(C("C09"),1));Assert.IsTrue(deck.Use(C("C12"),1));
            Assert.IsTrue(deck.Use(C("C18"),1));Assert.IsTrue(deck.Use(C("C02"),1));
            var copy=deck.Cards.Last();Assert.AreEqual(53,copy.Card.InstanceId);
            Assert.AreEqual(deck.Find(1).Card.SpriteName,copy.Card.SpriteName);Assert.AreNotEqual(deck.Find(1).Card,copy.Card);
            Assert.AreSame(deck.Find(1)[CardLayer.Edition],copy[CardLayer.Edition]);
            deck.Use(C("C10"),53);Assert.AreEqual("E01",deck.Find(1)[CardLayer.Edition].id);
            var a=new BlackjackDeck(deck.Snapshot(),4);var b=new BlackjackDeck(deck.Snapshot(),4);
            var ids=new HashSet<int>();while(a.Remaining>0){var card=a.Draw();Assert.AreEqual(card,b.Draw());Assert.IsTrue(ids.Add(card.InstanceId));}
            Assert.AreEqual(53,ids.Count);
        }
        [Test] public void InvalidUseDoesNotSpendItemOrGoldAndCapacityIsEnforced()
        {
            var inv=new RunInventory();var economy=new RunEconomy(Array.Empty<string>(),1);economy.GrantGold(100);
            Assert.IsTrue(inv.Buy(C("C01"),economy,4));var gold=economy.Gold;
            Assert.IsFalse(inv.Use(0,999,true));Assert.IsFalse(inv.Use(0,1,false));Assert.AreEqual(1,inv.Consumables.Count);Assert.AreEqual(gold,economy.Gold);
            Assert.IsTrue(inv.Use(0,1,true));Assert.AreEqual(51,inv.Deck.Cards.Count);Assert.AreEqual(0,inv.Consumables.Count);
            Assert.IsFalse(inv.Use(0,2,true));
            Assert.IsTrue(inv.Buy(C("C02"),economy,4));Assert.IsTrue(inv.Buy(C("C02"),economy,4));gold=economy.Gold;
            Assert.IsFalse(inv.Buy(C("C02"),economy,4));Assert.AreEqual(gold,economy.Gold);
        }
        [Test] public void VoucherPrerequisitesAreAtomicAndResetClearsEveryBenefit()
        {
            var inv=new RunInventory();var economy=new RunEconomy(Array.Empty<string>(),1);economy.GrantGold(100);
            Assert.IsFalse(inv.Buy(V("V04"),economy,8));
            Assert.IsTrue(inv.Buy(V("V03"),economy,8));Assert.AreEqual(5,inv.RelicCapacity);
            Assert.IsTrue(inv.Buy(V("V04"),economy,8));Assert.AreEqual(6,inv.RelicCapacity);
            Assert.IsFalse(inv.Buy(V("V03"),economy,8));
            Assert.IsTrue(inv.Buy(V("V05"),economy,8));Assert.AreEqual(3,inv.ConsumableCapacity);
            Assert.IsTrue(inv.Buy(V("V01"),economy,8));
            var run=new ScoreRun(new ScoreRules(firstGoal:120),r=>r%3==0?2:0,()=>inv.Bonus(VoucherEffect.Hands));
            Assert.AreEqual(5,run.HandLimit);for(var i=0;i<2;i++){run.RecordDeal(new[]{new ScoreBreakdown(BigNumber.From(999),1)});run.NextRound();}
            Assert.AreEqual(2,run.HandLimit);
            inv.Reset();run.Restart();Assert.AreEqual(4,run.HandLimit);Assert.AreEqual(4,inv.RelicCapacity);Assert.AreEqual(2,inv.ConsumableCapacity);Assert.AreEqual(52,inv.Deck.Cards.Count);
        }
        [Test] public void LayersRetriggerBeforeRelicsAndDebuffSuppressesBonuses()
        {
            var deck=new RunDeck();deck.Use(C("C09"),1);deck.Use(C("C12"),1);deck.Use(C("C18"),1);
            var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),1,new[]{deck.Find(1).Card,deck.Find(22).Card,deck.Find(13).Card,deck.Find(20).Card});
            session.StartRound(10);Assert.AreEqual(HandOutcome.Blackjack,session.Hands[0].Outcome);
            var baseline=new ScoreBreakdown(BigNumber.From(36),3.5);
            var result=CardModificationEngine.Apply(baseline,session.Hands[0],deck,-1,out var gold,new HashSet<int>());
            Assert.AreEqual(BigNumber.From(100),result.Chips);Assert.AreEqual(0,gold);
            var debuffed=CardModificationEngine.Apply(baseline,session.Hands[0],deck,(int)CardSuit.Clubs,out gold,null);
            Assert.AreEqual(baseline.Total,debuffed.Total);
            deck.Use(C("C14"),1);Assert.IsTrue(deck.MatchesSuit(deck.Find(1).Card,CardSuit.Hearts));Assert.IsFalse(deck.MatchesSuit(deck.Find(1).Card,CardSuit.Hearts,0));
        }
        [Test] public void DestructionCannotBecomeAnInfiniteFreeModifierAtMinimumDeckSize()
        {
            var deck=new RunDeck();while(deck.Cards.Count>RunDeck.MinimumCards)deck.Use(C("C01"),deck.Cards.Last().Card.InstanceId);
            Assert.IsFalse(deck.Use(C("C01"),1));deck.Use(C("C16"),1);
            Assert.IsFalse(deck.DestroyScoredCard(1));Assert.IsNull(deck.Find(1)[CardLayer.Enhancement]);Assert.AreEqual(24,deck.Cards.Count);
        }
        [Test] public void RanksWrapAndDeckSizeCapsAreEnforced()
        {
            var deck=new RunDeck();deck.Use(C("C04"),1);Assert.AreEqual(CardRank.King,deck.Find(1).Card.Rank);
            deck.Use(C("C03"),1);Assert.AreEqual(CardRank.Ace,deck.Find(1).Card.Rank);
            while(deck.Cards.Count<RunDeck.MaximumCards)Assert.IsTrue(deck.Use(C("C02"),1));Assert.IsFalse(deck.Use(C("C02"),1));
        }
        [Test] public void DeckCannotChangeDuringADealAndExhaustionStillAllowsStanding()
        {
            var deck=new RunDeck();var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),9);
            var four=new[]{new PlayingCard(CardSuit.Clubs,CardRank.Two,1),new PlayingCard(CardSuit.Hearts,CardRank.Two,2),new PlayingCard(CardSuit.Spades,CardRank.Two,3),new PlayingCard(CardSuit.Diamonds,CardRank.Two,4)};
            session.ConfigureDeck(four);session.StartRound(10);
            Assert.IsFalse(session.ConfigureDeck(deck.Snapshot()));Assert.IsFalse(session.CanHit);Assert.IsTrue(session.CanStand);
            session.Stand();session.AdvanceDealerStep();Assert.AreEqual(BlackjackPhase.Settled,session.Phase);
        }
        [Test] public void BossRestrictionsAndGoalFactorsExpireOnNextRound()
        {
            var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),1,new[]{new PlayingCard(CardSuit.Clubs,CardRank.Eight),new PlayingCard(CardSuit.Hearts,CardRank.Seven),new PlayingCard(CardSuit.Spades,CardRank.Eight),new PlayingCard(CardSuit.Diamonds,CardRank.Ten),new PlayingCard(CardSuit.Clubs,CardRank.Two)});
            session.ConfigureTable(false,17,false,false);session.StartRound(10);Assert.IsFalse(session.CanDouble);Assert.IsFalse(session.CanSplit);
            session.Stand();while(session.Phase==BlackjackPhase.DealerTurn)session.AdvanceDealerStep();session.ConfigureTable();
            var run=new ScoreRun(new ScoreRules(firstGoal:100,goalGrowth:1),null,null,r=>r==3?1.5:1);
            for(var i=0;i<2;i++){run.RecordDeal(new[]{new ScoreBreakdown(BigNumber.From(999),1)});run.NextRound();}
            Assert.AreEqual(BigNumber.From(150),run.Goal);run.RecordDeal(new[]{new ScoreBreakdown(BigNumber.From(999),1)});run.NextRound();Assert.AreEqual(BigNumber.From(100),run.Goal);
        }
        [Test] public void EconomyBonusesRewardOnceAndAllowRerollsAfterRelicCollectionIsComplete()
        {
            var e=new RunEconomy(new[]{"00"},2);e.GrantGold(42);e.BonusReward=2;e.InterestCap=5;e.RerollDiscount=1;e.AdditionalOffersAvailable=true;
            e.Open(1,2);Assert.AreEqual(16,e.LastReward);Assert.AreEqual(66,e.Gold);Assert.AreEqual(1,e.RerollPrice);
            e.Open(1,2);Assert.AreEqual(66,e.Gold);Assert.IsTrue(e.Reroll());Assert.AreEqual(65,e.Gold);
        }
    }
}
