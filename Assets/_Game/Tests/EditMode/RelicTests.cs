using System.Linq;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using NUnit.Framework;
using UnityEngine;

namespace DoubleDown.Tests
{
    public class RelicTests
    {
        private static RelicDefinition Relic(string id) => Resources.LoadAll<RelicDefinition>("Relics").Single(r=>r.id==id);
        private static PlayingCard Card(CardRank rank,CardSuit suit)=>new PlayingCard(suit,rank);
        [TestCase("00",46,3.5)] [TestCase("01",44,3.5)] [TestCase("02",42,3.5)]
        [TestCase("03",36,3.5)] [TestCase("04",36,3.5)] [TestCase("05",41,3.5)]
        [TestCase("06",41,3.5)] [TestCase("07",36,3.5)] [TestCase("08",36,4.5)]
        [TestCase("09",36,4)] [TestCase("10",36,3.5)] [TestCase("11",36,3.5)]
        [TestCase("12",36,3.5)] [TestCase("13",36,4)] [TestCase("14",36,4.375)]
        public void EachRelicHonorsItsTriggerOnAKnownHand(string id,int chips,double multiplier)
        {
            var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),1,new[]{
                Card(CardRank.Ace,CardSuit.Clubs),Card(CardRank.Nine,CardSuit.Diamonds),
                Card(CardRank.King,CardSuit.Spades),Card(CardRank.Seven,CardSuit.Hearts)});
            session.StartRound(10);
            var loadout=new RelicLoadout();loadout.Equip(0,Relic(id));
            var result=RelicEngine.Apply(new ScoreBreakdown(BigNumber.From(36),3.5),session.Hands[0],loadout,2,null);
            Assert.AreEqual(BigNumber.From(chips),result.Chips);
            Assert.AreEqual(new BigNumber(multiplier),result.Multiplier);
        }
        [Test]
        public void CatalogContainsFifteenDistinctDefinitionsWithIcons()
        {
            var catalog=Resources.LoadAll<RelicDefinition>("Relics");
            Assert.AreEqual(15,catalog.Length);
            Assert.AreEqual(15,catalog.Select(r=>r.id).Distinct().Count());
            Assert.IsTrue(catalog.All(r=>r.icon!=null && r.amount>0));
        }
        [Test]
        public void ReorderingAdditiveAndMultiplicativeRelicsChangesScore()
        {
            var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),1,new[]{
                Card(CardRank.Ace,CardSuit.Clubs),Card(CardRank.Nine,CardSuit.Diamonds),
                Card(CardRank.King,CardSuit.Spades),Card(CardRank.Seven,CardSuit.Hearts)});
            session.StartRound(10);
            var hand=session.Hands[0];
            Assert.AreEqual(HandOutcome.Blackjack,hand.Outcome);
            var loadout=new RelicLoadout();loadout.Equip(0,Relic("08"));loadout.Equip(1,Relic("14"));
            Assert.IsFalse(loadout.Equip(2,Relic("14")));
            var baseline=new ScoreBreakdown(BigNumber.From(36),3.5);
            var additiveFirst=RelicEngine.Apply(baseline,hand,loadout,1,null);
            loadout.Move(0,1);
            var multiplicativeFirst=RelicEngine.Apply(baseline,hand,loadout,1,null);
            Assert.IsTrue(additiveFirst.Total>multiplicativeFirst.Total);
            Assert.AreEqual(new BigNumber(5.625),additiveFirst.Multiplier);
        }
        [Test]
        public void LossNeverGainsScoreFromRelics()
        {
            var session=new BlackjackSession(new BlackjackRules(startingBankroll:200),1,new[]{
                Card(CardRank.Ten,CardSuit.Clubs),Card(CardRank.Ten,CardSuit.Diamonds),
                Card(CardRank.Eight,CardSuit.Spades),Card(CardRank.Nine,CardSuit.Hearts)});
            session.StartRound(10);session.Stand();session.AdvanceDealerStep();
            Assert.AreEqual(HandOutcome.Loss,session.Hands[0].Outcome);
            var loadout=new RelicLoadout();loadout.Equip(0,Relic("09"));loadout.Equip(1,Relic("14"));
            Assert.AreEqual(BigNumber.Zero,RelicEngine.Apply(new ScoreBreakdown(BigNumber.From(33),0),
                session.Hands[0],loadout,0,null).Total);
        }
    }
}
