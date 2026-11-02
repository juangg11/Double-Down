using System.IO;
using DoubleDown.Blackjack;
using DoubleDown.Scoring;
using UnityEditor;
using UnityEngine;

namespace DoubleDown.Editor
{
    public static class PhaseFiveSetup
    {
        [MenuItem("Double Down/Phase 5/Build Relic Catalog")]
        public static void Build()
        {
            PhaseFourSetup.Build();
            Directory.CreateDirectory("Assets/Resources/Relics");
            AssetDatabase.Refresh();
            Add(0,"COBRE","+10 fichas al ganar.",RelicTrigger.Victory,RelicOperation.AddChips,10,"token");
            Add(1,"AS DE BOLSILLO","+8 fichas por as.",RelicTrigger.Aces,RelicOperation.AddChips,8,"card");
            Add(2,"CORONA PARTIDA","+6 fichas por J, Q o K.",RelicTrigger.Faces,RelicOperation.AddChips,6,"crown_a");
            Add(3,"CORAZON RUBI","+5 fichas por corazon.",RelicTrigger.Suit,RelicOperation.AddChips,5,"suit_hearts",CardSuit.Hearts);
            Add(4,"DIAMANTE AMBAR","+5 fichas por diamante.",RelicTrigger.Suit,RelicOperation.AddChips,5,"suit_diamonds",CardSuit.Diamonds);
            Add(5,"TREBOL HIERRO","+5 fichas por trebol.",RelicTrigger.Suit,RelicOperation.AddChips,5,"suit_clubs",CardSuit.Clubs);
            Add(6,"PICA OBSIDIANA","+5 fichas por pica.",RelicTrigger.Suit,RelicOperation.AddChips,5,"suit_spades",CardSuit.Spades);
            Add(7,"BALANZA","+0.5 mult al empatar.",RelicTrigger.Push,RelicOperation.AddMultiplier,0.5f,"cards_flip");
            Add(8,"VEINTIUNO","+1 mult con total 21.",RelicTrigger.TwentyOne,RelicOperation.AddMultiplier,1,"card_target");
            Add(9,"MANO FIRME","+0.5 mult al ganar con 2 cartas sin doblar.",RelicTrigger.StandVictory,RelicOperation.AddMultiplier,0.5f,"card_tap");
            Add(10,"RIESGO MEDIDO","+1 mult al ganar doblando.",RelicTrigger.DoubleVictory,RelicOperation.AddMultiplier,1,"dice_2");
            Add(11,"GEMELOS","+8 fichas por mano dividida ganadora.",RelicTrigger.SplitVictory,RelicOperation.AddChips,8,"cards_diagonal");
            Add(12,"ESCALERA CORTA","+6 fichas por carta desde la tercera.",RelicTrigger.ExtraCards,RelicOperation.AddChips,6,"cards_stack");
            Add(13,"RACHA","+0.25 mult por victoria consecutiva. Perder reinicia.",RelicTrigger.Streak,RelicOperation.AddMultiplier,0.25f,"tag_infinite");
            Add(14,"PRENSA DE ORO","Multiplica el mult acumulado por 1.25.",RelicTrigger.ScoredHand,RelicOperation.MultiplyMultiplier,1.25f,"crown_b");
            AssetDatabase.SaveAssets();
            Debug.Log("DOUBLE DOWN Phase 5: 15 relic definitions ready.");
        }

        private static void Add(int index, string name, string description, RelicTrigger trigger,
            RelicOperation operation, float amount, string icon, CardSuit suit = CardSuit.Clubs)
        {
            var path = $"Assets/Resources/Relics/{index:00}.asset";
            if (AssetDatabase.LoadAssetAtPath<RelicDefinition>(path) != null) return;
            var relic = ScriptableObject.CreateInstance<RelicDefinition>();
            relic.id = index.ToString("00"); relic.displayName = name;
            relic.description = description.ToUpperInvariant(); relic.trigger = trigger;
            relic.operation = operation; relic.amount = amount; relic.suit = suit;
            relic.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/TablePresentation/Icons/{icon}.png");
            AssetDatabase.CreateAsset(relic, path);
        }
    }
}
