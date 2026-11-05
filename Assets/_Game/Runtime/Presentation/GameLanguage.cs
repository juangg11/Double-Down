using DoubleDown.Scoring;
using UnityEngine;

namespace DoubleDown.Presentation
{
    public static class GameLanguage
    {
        private const string Preference = "ui.language";
        public static bool English => PlayerPrefs.GetInt(Preference, 0) == 1;
        public static string Pick(string spanish, string english) => English ? english : spanish;
        public static string RelicEffect(RelicDefinition relic)
        {
            if (!English) return relic.description;
            var number=relic.amount.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);
            var amount=relic.operation switch
            {
                RelicOperation.AddChips=>"+"+number+" CHIPS",
                RelicOperation.AddMultiplier=>"+"+number+" MULT",
                _=>"x"+number+" MULT"
            };
            var when=relic.trigger switch
            {
                RelicTrigger.Victory=>"ON WIN", RelicTrigger.Aces=>"PER ACE", RelicTrigger.Faces=>"PER FACE CARD",
                RelicTrigger.Suit=>"PER "+relic.suit.ToString().ToUpperInvariant(),
                RelicTrigger.Push=>"ON PUSH", RelicTrigger.TwentyOne=>"AT 21",
                RelicTrigger.StandVictory=>"ON 2-CARD WIN WITHOUT DOUBLE",
                RelicTrigger.DoubleVictory=>"ON DOUBLED WIN", RelicTrigger.SplitVictory=>"ON SPLIT WIN",
                RelicTrigger.ExtraCards=>"PER CARD AFTER SECOND", RelicTrigger.Streak=>"PER WIN STREAK",
                RelicTrigger.ScoredHand=>"ON SCORED HAND", RelicTrigger.Natural=>"ON NATURAL BLACKJACK",
                RelicTrigger.FiveCards=>"WITH 5+ CARDS", RelicTrigger.Soft=>"ON SOFT HAND",
                RelicTrigger.Hard=>"ON HARD HAND", RelicTrigger.FourSuits=>"WITH ALL 4 SUITS",
                RelicTrigger.SameSuit=>"WITH SAME SUIT", RelicTrigger.OpeningPair=>"WITH OPENING PAIR",
                RelicTrigger.Even=>"PER EVEN CARD", RelicTrigger.Odd=>"PER ODD CARD",
                RelicTrigger.Low=>"PER CARD RANK 2-5", RelicTrigger.High=>"PER CARD RANK 6-10",
                RelicTrigger.Nineteen=>"AT 19", RelicTrigger.Twenty=>"AT 20",
                RelicTrigger.Eighteen=>"AT 18", RelicTrigger.RedHand=>"WITH RED CARDS",
                RelicTrigger.BlackHand=>"WITH BLACK CARDS", RelicTrigger.LongHand=>"PER CARD AFTER FOURTH",
                _=>""
            };
            return amount+" "+when;
        }
        public static string ConsumableEffect(ConsumableDefinition item)
        {
            if(!English)return item.description;
            return item.effect switch
            {
                DoubleDown.Scoring.ConsumableEffect.Remove=>"REMOVE A CARD (MINIMUM DECK: 24)",
                DoubleDown.Scoring.ConsumableEffect.Duplicate=>"DUPLICATE THE SELECTED CARD",
                DoubleDown.Scoring.ConsumableEffect.RankUp=>"RAISE THE SELECTED CARD'S RANK",
                DoubleDown.Scoring.ConsumableEffect.RankDown=>"LOWER THE SELECTED CARD'S RANK",
                DoubleDown.Scoring.ConsumableEffect.ChangeSuit=>"CHANGE SUIT TO "+item.suit.ToString().ToUpperInvariant(),
                DoubleDown.Scoring.ConsumableEffect.Modify=>"APPLY "+(item.modification?.displayName??"CARD MOD")+": "+CardModificationEffect(item.modification),
                _=>item.description
            };
        }
        private static string CardModificationEffect(CardModificationDefinition mod)
        {
            if(mod==null)return "MODIFY A CARD";
            var parts=new System.Collections.Generic.List<string>();
            if(mod.chips>0)parts.Add("+"+mod.chips.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+" CHIPS");
            if(mod.multiplier>0)parts.Add("+"+mod.multiplier.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+" MULT");
            if(mod.multiplierFactor>1)parts.Add("x"+mod.multiplierFactor.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+" MULT");
            if(mod.multiplierPerPosition>0)parts.Add("+"+mod.multiplierPerPosition.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+" MULT PER POSITION");
            if(mod.gold>0)parts.Add("+"+mod.gold+" GOLD");
            if(mod.wild)parts.Add("COUNTS AS ANY SUIT");
            if(mod.destroyAfterScoring)parts.Add("DESTROYS AFTER SCORING");
            if(mod.repeatBonuses)parts.Add("REPEATS BONUSES");
            return string.Join(", ",parts);
        }
        public static string VoucherEffect(VoucherDefinition voucher)
        {
            if(!English)return voucher.description;
            var n=voucher.amount.ToString();
            return voucher.effect switch
            {
                DoubleDown.Scoring.VoucherEffect.Hands=>"+"+n+" HANDS IN NORMAL ROUNDS",
                DoubleDown.Scoring.VoucherEffect.RelicSlots=>"+"+n+" RELIC SLOTS",
                DoubleDown.Scoring.VoucherEffect.ConsumableSlots=>"+"+n+" CONSUMABLE SLOTS",
                DoubleDown.Scoring.VoucherEffect.RelicDiscount=>"-"+n+" RELIC PRICE",
                DoubleDown.Scoring.VoucherEffect.ConsumableDiscount=>"-"+n+" CONSUMABLE PRICE",
                DoubleDown.Scoring.VoucherEffect.RerollDiscount=>"-"+n+" REROLL PRICE",
                DoubleDown.Scoring.VoucherEffect.Interest=>"+"+n+" MAX INTEREST",
                DoubleDown.Scoring.VoucherEffect.RoundGold=>"+"+n+" GOLD PER ROUND",
                DoubleDown.Scoring.VoucherEffect.Bankroll=>"+"+n+" CHIPS PER ROUND",
                _=>voucher.description
            };
        }
        public static string BossEffect(BossRule boss)
        {
            if(!English)return boss.Description;
            var parts=new System.Collections.Generic.List<string>();
            if(boss.PlayerAcesLow)parts.Add("ACES COUNT AS 1");
            if(boss.DealerStandTotal>17)parts.Add("DEALER STANDS ON "+boss.DealerStandTotal);
            if(boss.MaximumHands>0)parts.Add("ONLY "+boss.MaximumHands+" HANDS");
            if(boss.DisableDouble)parts.Add("NO DOUBLING");
            if(boss.DisableSplit)parts.Add("NO SPLITTING");
            if(boss.DebuffedSuit>=0)parts.Add("SUIT "+new[]{"CLUBS","DIAMONDS","HEARTS","SPADES"}[boss.DebuffedSuit]+" IS DEBUFFED");
            if(boss.GoalFactor>1)parts.Add("TARGET x"+boss.GoalFactor.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture));
            return string.Join(" / ",parts);
        }
        public static string SessionMessage(string spanish)
        {
            if(!English)return spanish;
            return spanish switch
            {
                "Elige una apuesta para comenzar."=>"Choose a bet to begin.",
                "La banca muestra un as. ¿Quieres seguro?"=>"Dealer shows an ace. Take insurance?",
                "Pide otra carta o plántate."=>"Hit or stand.",
                "Juega la primera mano dividida."=>"Play the first split hand.",
                "Tu turno: pide, plántate, dobla o divide."=>"Your turn: hit, stand, double or split.",
                "Turno de la banca."=>"Dealer's turn.",
                "Empate: recuperas la apuesta."=>"Push: your bet is returned.",
                _ when spanish.StartsWith("Juega la mano ")=>"Play hand "+spanish.Substring("Juega la mano ".Length),
                _ when spanish.StartsWith("Ganaste ")=>"Won "+spanish.Substring("Ganaste ".Length).Replace(" fichas."," chips."),
                _ when spanish.StartsWith("Perdiste ")=>"Lost "+spanish.Substring("Perdiste ".Length).Replace(" fichas."," chips."),
                _=>spanish
            };
        }
        public static void Toggle()
        {
            PlayerPrefs.SetInt(Preference, English ? 0 : 1);
            PlayerPrefs.Save();
        }
    }
}
