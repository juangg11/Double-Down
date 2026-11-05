using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using UnityEditor;
using UnityEngine;
namespace DoubleDown.Editor
{
    public static class PhaseSixSimulator
    {
        [MenuItem("Double Down/Phase 6/Simulate 2000 Runs")]
        public static void Run()
        {
            var report=new StringBuilder("# Simulacion de Fase 6\n\n2000 runs deterministas: 1000 sin compras y 1000 con compras. Limite de observacion: 60 rondas; no es un limite del juego. Politica: pedir hasta 17, doblar en 11, sin seguro ni division. Compras heuristicas, no jugador optimo.\n\n| Politica | Ronda media | Ronda maxima | Runs que llegan a ronda 10 |\n|---|---:|---:|---:|\n");
            foreach(var buy in new[]{false,true})
            {
                var rounds=0;var maximum=0;var ten=0;
                for(var i=0;i<1000;i++)
                {
                    var reached=Simulate((uint)(20261101+i*7919),buy);
                    rounds+=reached;maximum=Math.Max(maximum,reached);if(reached>=10)ten++;
                }
                report.AppendLine($"| {(buy?"Con compras":"Sin compras")} | {rounds/1000.0:F2} | {maximum} | {ten} |");
            }
            report.AppendLine("\nLas compras eligen reliquias de la oferta, vales disponibles y consumibles; editan cartas al azar con la misma semilla. Comprueba ausencia de bloqueos y una aproximacion a progresion, no certifica balance de lanzamiento ni ausencia de builds dominantes. El balance final y las politicas avanzadas corresponden a Fase 9.");
            Directory.CreateDirectory("docs");File.WriteAllText("docs/PHASE_6_SIMULATION.md",report.ToString());Debug.Log(report.ToString());
        }
        private static int Simulate(uint seed,bool purchase)
        {
            var random=new System.Random((int)seed);
            var inventory=new RunInventory();
            var relics=Resources.LoadAll<RelicDefinition>("Relics").OrderBy(r=>r.id,StringComparer.Ordinal).ToArray();
            var cons=Resources.LoadAll<ConsumableDefinition>("Consumables").OrderBy(r=>r.id,StringComparer.Ordinal).ToArray();
            var vouchers=Resources.LoadAll<VoucherDefinition>("Vouchers").OrderBy(r=>r.id,StringComparer.Ordinal).ToArray();
            var economy=new RunEconomy(relics.Select(r=>r.id),(int)seed,Resources.Load<EconomyBalanceConfig>("EconomyBalance").ToRules());
            var rules=AssetDatabase.LoadAssetAtPath<BlackjackRulesConfig>("Assets/_Game/Settings/BlackjackRules.asset").ToRules();
            var scoreRules=AssetDatabase.LoadAssetAtPath<ScoringBalanceConfig>("Assets/_Game/Settings/ScoringBalance.asset").ToRules();
            var session=new BlackjackSession(rules,seed);
            var bosses=new BossSchedule(Resources.LoadAll<BossDefinition>("Bosses").Select(b=>b.ToRule()),seed);
            var score=new ScoreRun(scoreRules,r=>bosses.ForRound(r)?.MaximumHands??0,()=>inventory.Bonus(VoucherEffect.Hands),r=>bosses.ForRound(r)?.GoalFactor??1);
            var loadout=new RelicLoadout();var initial=new[]{0,1,8,14};for(var j=0;j<4;j++)loadout.Equip(j,relics[initial[j]]);
            var streak=0;
            for(var r=1;r<=60;r++)
            {
                var boss=bosses.ForRound(r);session.ConfigureTable(boss?.PlayerAcesLow??false,boss?.DealerStandTotal??17,!(boss?.DisableDouble??false),!(boss?.DisableSplit??false));
                while(score.State==ScoreRunState.Playing)
                {
                    session.ConfigureDeck(inventory.Deck.Snapshot());
                    if(!session.StartRound(rules.MinimumWager)){score.Forfeit();break;}
                    var guard=0;
                    while(session.Phase!=BlackjackPhase.Settled)
                    {
                        if(++guard>200)throw new InvalidOperationException("Simulation stalled");
                        if(session.Phase==BlackjackPhase.Insurance)session.ChooseInsurance(false);
                        else if(session.Phase==BlackjackPhase.DealerTurn)session.AdvanceDealerStep();
                        else if(session.Phase==BlackjackPhase.PlayerTurn)
                        {
                            var total=session.Hands[session.ActiveHandIndex].Value.Total;
                            if(total==11 && session.CanDouble)session.DoubleDown();
                            else if(total<17 && session.CanHit)session.Hit();else session.Stand();
                        }
                    }
                    var points=new List<ScoreBreakdown>();var destroyed=new HashSet<int>();
                    foreach(var hand in session.Hands)
                    {
                        if(hand.Outcome==HandOutcome.Win || hand.Outcome==HandOutcome.Blackjack)streak++;else if(hand.Outcome==HandOutcome.Loss)streak=0;
                        var value=ScoreCalculator.Calculate(hand.Value,hand.Outcome,hand.Cards.Count,hand.Stake,scoreRules);
                        value=CardModificationEngine.Apply(value,hand,inventory.Deck,boss?.DebuffedSuit??-1,out var gold,destroyed);economy.GrantGold(gold);
                        points.Add(RelicEngine.Apply(value,hand,loadout,streak,null,(card,suit)=>inventory.Deck.MatchesSuit(card,suit,boss?.DebuffedSuit??-1)));
                    }
                    foreach(var id in destroyed)inventory.Deck.DestroyScoredCard(id);
                    score.RecordDeal(points);
                }
                if(score.State!=ScoreRunState.Won || r==60)return r;
                economy.RelicDiscount=inventory.Bonus(VoucherEffect.RelicDiscount);economy.BonusReward=inventory.Bonus(VoucherEffect.RoundGold);economy.InterestCap=inventory.Bonus(VoucherEffect.Interest);
                economy.Open(r,score.HandsRemaining);session.GrantBankroll(inventory.Bonus(VoucherEffect.Bankroll));
                if(purchase)
                {
                    var v=vouchers.Where(inventory.CanBuy).ToArray();if(v.Length>0)inventory.Buy(v[random.Next(v.Length)],economy,8);
                    loadout.SetCapacity(inventory.RelicCapacity);
                    var offer=economy.Offers[0];if(economy.Buy(0)){var relic=relics.Single(x=>x.id==offer);loadout.Equip(random.Next(loadout.SlotCount),relic);}
                    if(inventory.Buy(cons[random.Next(cons.Length)],economy,Math.Max(1,4-inventory.Bonus(VoucherEffect.ConsumableDiscount))))
                    {
                        var slot=inventory.Consumables.Count-1;var item=inventory.Consumables[slot];
                        var targets=inventory.Deck.Cards.Where(c=>inventory.Deck.CanUse(item,c.Card.InstanceId)).ToArray();
                        if(targets.Length>0)inventory.Use(slot,targets[random.Next(targets.Length)].Card.InstanceId,true);else inventory.Discard(slot,true);
                    }
                }
                score.NextRound();
            }
            return 60;
        }
    }
}
