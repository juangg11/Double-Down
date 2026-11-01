using System;
using System.IO;
using System.Text;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using UnityEditor;
using UnityEngine;

namespace DoubleDown.Editor
{
    public static class ScoringBalanceSimulator
    {
        private const int Simulations = 1000;
        private const int RoundLimit = 5;

        [MenuItem("Double Down/Phase 3/Simulate Balance")]
        public static void Run()
        {
            var blackjackAsset = AssetDatabase.LoadAssetAtPath<BlackjackRulesConfig>(
                "Assets/_Game/Settings/BlackjackRules.asset");
            var scoreAsset = AssetDatabase.LoadAssetAtPath<ScoringBalanceConfig>(
                "Assets/_Game/Settings/ScoringBalance.asset");
            if (blackjackAsset == null || scoreAsset == null)
                throw new InvalidOperationException("Build the blackjack table before simulating balance.");
            var blackjackRules = blackjackAsset.ToRules();
            var scoreRules = scoreAsset.ToRules();
            var reached = new int[RoundLimit];
            var cleared = new int[RoundLimit];
            var lastScore = new double[RoundLimit];
            var outcomes = new int[5];

            for (var simulation = 0; simulation < Simulations; simulation++)
            {
                var blackjack = new BlackjackSession(blackjackRules, (uint)(20261101 + simulation * 7919));
                var run = new ScoreRun(scoreRules);
                for (var round = 0; round < RoundLimit; round++)
                {
                    reached[round]++;
                    while (run.State == ScoreRunState.Playing)
                    {
                        if (!blackjack.StartRound(blackjackRules.MinimumWager))
                        {
                            run.Forfeit();
                            break;
                        }
                        var guard = 0;
                        while (blackjack.Phase != BlackjackPhase.Settled && guard++ < 100)
                        {
                            if (blackjack.Phase == BlackjackPhase.Insurance) blackjack.ChooseInsurance(false);
                            else if (blackjack.Phase == BlackjackPhase.PlayerTurn)
                            {
                                var value = blackjack.Hands[blackjack.ActiveHandIndex].Value.Total;
                                if (value == 11 && blackjack.CanDouble) blackjack.DoubleDown();
                                else if (value < 17) blackjack.Hit();
                                else blackjack.Stand();
                            }
                            else if (blackjack.Phase == BlackjackPhase.DealerTurn) blackjack.AdvanceDealerStep();
                        }
                        if (guard >= 100) throw new InvalidOperationException("Blackjack simulation did not settle.");
                        var hands = new ScoreBreakdown[blackjack.Hands.Count];
                        for (var i = 0; i < hands.Length; i++)
                        {
                            var hand = blackjack.Hands[i];
                            outcomes[(int)hand.Outcome]++;
                            hands[i] = ScoreCalculator.Calculate(hand.Value, hand.Outcome,
                                hand.Cards.Count, hand.Stake, scoreRules);
                        }
                        run.RecordDeal(hands);
                    }
                    lastScore[round] += run.Score.Mantissa * Math.Pow(10, run.Score.Exponent);
                    if (run.State != ScoreRunState.Won) break;
                    cleared[round]++;
                    if (round + 1 < RoundLimit) run.NextRound();
                }
            }

            var report = new StringBuilder();
            report.AppendLine("# Simulación de balance — Fase 3");
            report.AppendLine();
            report.AppendLine($"{Simulations:N0} runs deterministas, apuesta mínima, política simple: doblar en 11, pedir hasta 17, plantarse desde 17, sin seguro ni división.");
            report.AppendLine($"Configuración: meta inicial {scoreRules.FirstGoal}, crecimiento {scoreRules.GoalGrowth:0.##}, {scoreRules.HandsPerRound} manos por ronda.");
            report.AppendLine();
            report.AppendLine("| Ronda | Runs que llegan | Runs que superan meta | Éxito condicional | Puntuación media al salir |");
            report.AppendLine("| --- | ---: | ---: | ---: | ---: |");
            for (var i = 0; i < RoundLimit; i++)
                report.AppendLine($"| {i + 1} | {reached[i]} | {cleared[i]} | {(reached[i] == 0 ? 0 : cleared[i] * 100d / reached[i]):0.0}% | {(reached[i] == 0 ? 0 : lastScore[i] / reached[i]):0.0} |");
            report.AppendLine();
            report.AppendLine("La simulación sirve para calibrar metas; no representa una estrategia óptima ni reliquias futuras.");
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../docs/BalanceSimulation.md"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            Debug.Log($"DOUBLE DOWN Phase 3: {Simulations} balance runs written to {path}");
        }
    }
}
