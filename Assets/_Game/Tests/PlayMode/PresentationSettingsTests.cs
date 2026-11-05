using System.Collections;
using System.Collections.Generic;
using System.IO;
using DoubleDown.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DoubleDown.Tests
{
    public class PresentationSettingsTests
    {
        private int savedLanguage;
        [SetUp] public void SetLanguageForTest() { savedLanguage=PlayerPrefs.GetInt("ui.language",0); PlayerPrefs.SetInt("ui.language",0); }
        [TearDown] public void RestoreLanguage() { PlayerPrefs.SetInt("ui.language",savedLanguage); PlayerPrefs.Save(); }

        [UnityTest]
        public IEnumerator SettingsAreModalAndNewAssetsRender()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.8f);
            ChooseStarter();
            Assert.NotNull(Resources.Load<Sprite>("TablePresentation/Keys/H"));
            Assert.NotNull(Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
            var overlay = GameObject.Find("Pixel Glass").GetComponent<Image>();
            Assert.IsFalse(overlay.raycastTarget);
            Assert.IsTrue(overlay.material.shader.isSupported);
            var presentation = Object.FindFirstObjectByType<TablePresentation>();
            presentation.Toggle();
            yield return null;
            Assert.IsTrue(presentation.IsOpen);
            var results = new List<RaycastResult>();
            var hit = GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Hit").GetComponent<RectTransform>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
                { position = Camera.main.WorldToScreenPoint(hit.TransformPoint(hit.rect.center)) }, results);
            Assert.IsNotEmpty(results);
            Assert.IsTrue(results[0].gameObject.transform.IsChildOf(GameObject.Find("Presentation Settings").transform));
            var motion = GameObject.Find("Presentation Settings").transform
                .Find("Settings Body/Motion").GetComponent<Button>();
            var original = TablePresentation.ReducedMotion;
            motion.onClick.Invoke();
            Assert.AreNotEqual(original, TablePresentation.ReducedMotion);
            motion.onClick.Invoke();
            Capture("Phase4Settings.png");
            presentation.Toggle();
            Assert.IsFalse(presentation.IsOpen);
            Capture("Phase4Preview.png");
        }

        [UnityTest]
        public IEnumerator RelicsCanBeEquippedAndReorderedOnlyBetweenHands()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.8f);
            var rack = Object.FindFirstObjectByType<RelicRack>();
            Assert.IsNull(rack.Loadout[0]);
            ChooseStarter();
            var starterId=rack.Loadout[0].id;
            rack.Toggle();
            yield return null;
            var body = GameObject.Find("Relic Collection").transform.Find("Body");
            body.Find("Relic 02").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(body.Find("Equip").GetComponent<Button>().interactable);
            body.Find("Equip").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(starterId, rack.Loadout[0].id);
            rack.Toggle();
            var controller = Object.FindFirstObjectByType<BlackjackTableController>();
            var session = (DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController)
                .GetField("session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(controller);
            session.Stand();
            while(session.Phase == DoubleDown.Blackjack.BlackjackPhase.DealerTurn) session.AdvanceDealerStep();
            yield return new WaitForSecondsRealtime(0.9f);
            var shop=Object.FindFirstObjectByType<RunShop>();
            var scoring = (DoubleDown.Scoring.ScoreRun)typeof(BlackjackTableController)
                .GetField("scoreRun", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(controller);
            scoring.RecordDeal(new[] { new DoubleDown.Scoring.ScoreBreakdown(DoubleDown.Scoring.BigNumber.From(999), 1) });
            GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Deal").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(shop.IsOpen);
            var purchased=shop.Economy.Offers[0];
            GameObject.Find("Run Shop").transform.Find("Offer 0/Buy").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(shop.Economy.Owns(purchased));
            Assert.AreEqual(purchased,rack.Loadout[1].id);
            rack.Toggle();
            GameObject.Find("Blackjack HUD").transform.Find("Relic Slot 1").GetComponent<Button>().onClick.Invoke();
            body.Find("Relic "+purchased).GetComponent<Button>().onClick.Invoke();
            body.Find("Down").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(purchased,rack.Loadout[2].id);
            yield return new WaitForSecondsRealtime(0.3f);
            Capture("RelicsReorganized.png");
            rack.Toggle();
            yield return new WaitForSecondsRealtime(0.3f);
            Capture("Phase6Shop.png");
            Assert.IsFalse(GameObject.Find("Run Shop").transform.Find("Offer 0/Buy").GetComponent<Button>().interactable);
            GameObject.Find("Run Shop").transform.Find("Continue").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.9f);
            Capture("TableReorganized.png");
        }

        [UnityTest]
        public IEnumerator SplitHandsRemainClearOfPermanentInterface()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.3f);
            ChooseStarter();
            var controller=Object.FindFirstObjectByType<BlackjackTableController>();
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var field=typeof(BlackjackTableController).GetField("session",flags);
            var handler=(System.Action<DoubleDown.Blackjack.BlackjackEvent>)System.Delegate.CreateDelegate(
                typeof(System.Action<DoubleDown.Blackjack.BlackjackEvent>),controller,
                typeof(BlackjackTableController).GetMethod("OnGameEvent",flags));
            ((DoubleDown.Blackjack.BlackjackSession)field.GetValue(controller)).EventRaised-=handler;
            var cards=new[]{
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Clubs,DoubleDown.Blackjack.CardRank.Eight),
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Diamonds,DoubleDown.Blackjack.CardRank.Six),
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Spades,DoubleDown.Blackjack.CardRank.Eight),
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Hearts,DoubleDown.Blackjack.CardRank.Ten),
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Clubs,DoubleDown.Blackjack.CardRank.Two),
                new DoubleDown.Blackjack.PlayingCard(DoubleDown.Blackjack.CardSuit.Diamonds,DoubleDown.Blackjack.CardRank.Three)};
            var session=new DoubleDown.Blackjack.BlackjackSession(new DoubleDown.Blackjack.BlackjackRules(startingBankroll:200),1,cards);
            field.SetValue(controller,session);session.EventRaised+=handler;
            session.StartRound(10);Assert.IsTrue(session.Split());
            yield return new WaitForSecondsRealtime(0.9f);
            foreach(var card in Object.FindObjectsByType<BlackjackCardVisual>(FindObjectsSortMode.None))
            {
                var bounds=card.GetComponent<SpriteRenderer>().bounds;
                var rect=new Rect(bounds.min.x*64+320,180-bounds.max.y*64,bounds.size.x*64,bounds.size.y*64);
                Assert.IsFalse(rect.Overlaps(TableLayout.RunRail));
                Assert.IsFalse(rect.Overlaps(TableLayout.Relics));
                Assert.IsFalse(rect.Overlaps(TableLayout.Actions));
            }
            Capture("SplitReorganized.png");
            var totals = GameObject.Find("Blackjack HUD").transform.Find("Player Total").GetComponent<Text>();
            StringAssert.Contains("MANO 1  10\nTU TURNO", totals.text);
            session.Stand();
            StringAssert.Contains("MANO 2  11\nTU TURNO", totals.text);
        }

        [UnityTest]
        public IEnumerator ThirdRoundAnnouncesBossBeforeDealingAndFourthRestoresRules()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.5f);
            ChooseStarter();
            var controller = Object.FindFirstObjectByType<BlackjackTableController>();
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var session = (DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController).GetField("session", flags).GetValue(controller);
            var scoring = (DoubleDown.Scoring.ScoreRun)typeof(BlackjackTableController).GetField("scoreRun", flags).GetValue(controller);
            var boss = Object.FindFirstObjectByType<BossEncounterView>();
            for (var round = 1; round <= 3; round++)
            {
                if (session.Phase == DoubleDown.Blackjack.BlackjackPhase.Insurance) session.ChooseInsurance(false);
                if (session.Phase == DoubleDown.Blackjack.BlackjackPhase.PlayerTurn) session.Stand();
                while (session.Phase == DoubleDown.Blackjack.BlackjackPhase.DealerTurn) session.AdvanceDealerStep();
                yield return new WaitForSecondsRealtime(0.9f);
                if (scoring.State == DoubleDown.Scoring.ScoreRunState.Playing)
                    scoring.RecordDeal(new[] { new DoubleDown.Scoring.ScoreBreakdown(DoubleDown.Scoring.BigNumber.From(999), 1) });
                GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Deal").GetComponent<Button>().onClick.Invoke();
                Assert.IsTrue(Object.FindFirstObjectByType<RunShop>().IsOpen);
                GameObject.Find("Run Shop").transform.Find("Continue").GetComponent<Button>().onClick.Invoke();
                if (round == 2)
                {
                    Assert.IsTrue(boss.IsOpen);
                    Assert.AreEqual(2, session.RoundIndex, "The third deal must wait for the boss announcement.");
                    var panel = GameObject.Find("Boss Encounter").transform;
                    StringAssert.Contains("MESA 01", panel.Find("Heading").GetComponent<Text>().text);
                    Assert.IsNotEmpty(panel.Find("Rule").GetComponent<Text>().text);
                    yield return new WaitForSecondsRealtime(0.4f);
                    Capture("BossRoundPreview.png");
                    panel.Find("Enter").GetComponent<Button>().onClick.Invoke();
                    Assert.IsFalse(boss.IsOpen);
                    Assert.AreEqual(3, session.RoundIndex);
                    Assert.AreEqual(18, session.DealerStandTotal, "The configured seed starts with BANCA 18.");
                    yield return new WaitForSecondsRealtime(0.8f);
                    Capture("BossTablePreview.png");
                }
                else Assert.IsFalse(boss.IsOpen);
            }
            Assert.AreEqual(4, scoring.Round);
            Assert.AreEqual(4, scoring.HandLimit);
            Assert.AreEqual(17, session.DealerStandTotal);
            Assert.IsFalse(session.PlayerAcesLow);
        }

        [UnityTest]
        public IEnumerator ControlsCanBeReassignedFromSettingsWithoutPlayingACard()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.8f);
            ChooseStarter();
            Capture("ReadableHud.png");
            var presentation = Object.FindFirstObjectByType<TablePresentation>();
            var controls = Object.FindFirstObjectByType<TableControls>();
            presentation.Toggle();
            GameObject.Find("Presentation Settings").transform.Find("Settings Body/Controls").GetComponent<Button>().onClick.Invoke();
            var view = Object.FindFirstObjectByType<ControlSettingsView>();
            Assert.IsTrue(view.IsOpen);
            var body = GameObject.Find("Controls Settings").transform.Find("Body");
            body.Find("Bind Hit").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(TableAction.Hit, controls.Capturing);
            Assert.IsNull(body.Find("Bind Hit/Key"));
            Assert.AreEqual(string.Empty,body.Find("Bind Hit/Key Name").GetComponent<Text>().text);
            Assert.Less(body.Find("Bind Hit").GetComponent<Image>().color.r,0.6f);
            Assert.IsTrue(controls.CaptureKey(UnityEngine.InputSystem.Key.K, false));
            Assert.AreEqual(UnityEngine.InputSystem.Key.K, controls.Bindings[TableAction.Hit]);
            Assert.IsFalse(controls.Pressed(TableAction.Hit));
            yield return new WaitForSecondsRealtime(0.3f);
            Capture("ControlsPreview.png");
            controls.BeginCapture(TableAction.Stand);
            controls.CancelCapture();
            Assert.IsNull(controls.Capturing);
            view.Hide(); presentation.Toggle();
            var controller = Object.FindFirstObjectByType<BlackjackTableController>();
            var session = (DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController)
                .GetField("session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(controller);
            var before = session.Hands[0].Cards.Count;
            yield return new WaitForSecondsRealtime(0.05f);
            var previousBackground = UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
            UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            var previousEditorInput = UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode;
            UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            var previousMode = UnityEngine.InputSystem.InputSystem.settings.updateMode;
            UnityEngine.InputSystem.InputSystem.settings.updateMode = UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsManually;
            var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            try
            {
                Assert.IsFalse(controls.Pressed(TableAction.Hit));
                Assert.IsNull(controls.Capturing);
                Assert.AreNotEqual(Time.frameCount, typeof(TableControls).GetField("consumedFrame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(controls));
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.H));
                UnityEngine.InputSystem.InputSystem.Update();
                controller.SendMessage("HandleKeyboard");
                Assert.AreEqual(before, session.Hands[0].Cards.Count, "The old binding must no longer deal a card.");
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.K));
                UnityEngine.InputSystem.InputSystem.Update();
                controller.SendMessage("HandleKeyboard");
                Assert.IsTrue(keyboard.kKey.isPressed, "The test keyboard must receive the queued K event.");
                Assert.AreSame(keyboard, UnityEngine.InputSystem.Keyboard.current);
                Assert.IsTrue(keyboard.kKey.wasPressedThisFrame, "K must have a press edge.");
                Assert.IsTrue(controls.Pressed(TableAction.Hit), "K must resolve to the Hit action in this frame.");
                Assert.AreEqual(before + 1, session.Hands[0].Cards.Count, "The rebound key must deal exactly one card.");
            }
            finally
            {
                UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
                UnityEngine.InputSystem.InputSystem.settings.updateMode = previousMode;
                UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
                UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
#endif
                controls.RestoreDefaults(false);
            }
        }

        [UnityTest]
        public IEnumerator PhaseSixShopAndWorkshopApplyPurchasesToTheRun()
        {
            SceneManager.LoadScene("BlackjackTable");yield return new WaitForSecondsRealtime(0.8f);
            ChooseStarter();
            var controller=Object.FindFirstObjectByType<BlackjackTableController>();
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var session=(DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController).GetField("session",flags).GetValue(controller);
            var inventory=(DoubleDown.Scoring.RunInventory)typeof(BlackjackTableController).GetField("inventory",flags).GetValue(controller);
            var scoring=(DoubleDown.Scoring.ScoreRun)typeof(BlackjackTableController).GetField("scoreRun",flags).GetValue(controller);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.Insurance)session.ChooseInsurance(false);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.PlayerTurn)session.Stand();
            while(session.Phase==DoubleDown.Blackjack.BlackjackPhase.DealerTurn)session.AdvanceDealerStep();
            yield return new WaitForSecondsRealtime(1.2f);
            if(scoring.State==DoubleDown.Scoring.ScoreRunState.Playing)scoring.RecordDeal(new[]{new DoubleDown.Scoring.ScoreBreakdown(DoubleDown.Scoring.BigNumber.From(999),1)});
            GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Deal").GetComponent<Button>().onClick.Invoke();
            var shop=Object.FindFirstObjectByType<RunShop>();Assert.IsTrue(shop.IsOpen);shop.Economy.GrantGold(100);
            var shopRoot=GameObject.Find("Run Shop").transform;
            shopRoot.Find("Tab 1").GetComponent<Button>().onClick.Invoke();
            var item=shop.ConsumableOffers[0];Assert.NotNull(item);
            shopRoot.Find("Offer 0/Buy").GetComponent<Button>().onClick.Invoke();Assert.AreEqual(1,inventory.Consumables.Count);
            yield return new WaitForSecondsRealtime(0.3f);Capture("Phase6Consumables.png");
            shopRoot.Find("Tab 2").GetComponent<Button>().onClick.Invoke();
            var voucher=shop.VoucherOffers[0];Assert.NotNull(voucher);
            shopRoot.Find("Offer 0/Buy").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(inventory.OwnsVoucher(voucher.id));
            yield return new WaitForSecondsRealtime(0.3f);Capture("Phase6Vouchers.png");
            var workshop=Object.FindFirstObjectByType<DeckWorkshop>();workshop.Toggle();
            var body=GameObject.Find("Deck Workshop").transform.Find("Body");
            var target=0;while(!inventory.Deck.CanUse(item,inventory.Deck.Cards[target].Card.InstanceId))target++;
            Assert.Less(target,18);body.Find("Card "+target).GetComponent<Button>().onClick.Invoke();
            body.Find("Consumable 0").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.3f);Capture("Phase6Deck.png");
            var apply=body.Find("Apply").GetComponent<Button>();Assert.IsTrue(apply.interactable);apply.onClick.Invoke();Assert.AreEqual(0,inventory.Consumables.Count);
            body.Find("Next").GetComponent<Button>().onClick.Invoke();StringAssert.Contains("2/",body.Find("Page").GetComponent<Text>().text);
            workshop.Toggle();
            shopRoot.Find("Continue").GetComponent<Button>().onClick.Invoke();Assert.AreEqual(2,scoring.Round);
            yield return new WaitForSecondsRealtime(0.8f);Assert.AreNotEqual(DoubleDown.Blackjack.BlackjackPhase.Ready,session.Phase);
            workshop.Toggle();body=GameObject.Find("Deck Workshop").transform.Find("Body");Assert.IsFalse(body.Find("Apply").GetComponent<Button>().interactable);
            workshop.Toggle();
            var rack=Object.FindFirstObjectByType<RelicRack>();
            GameObject.Find("Blackjack HUD").transform.Find("Relic Slot 1").GetComponent<Button>().onClick.Invoke();
            var collection=GameObject.Find("Relic Collection").transform.Find("Body");
            collection.Find("Next Page").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(collection.Find("Relic 15").gameObject.activeInHierarchy);Assert.IsFalse(collection.Find("Relic 00").gameObject.activeInHierarchy);
            yield return new WaitForSecondsRealtime(0.3f);Capture("Phase6Collection.png");
        }

        [UnityTest]
        public IEnumerator PrismConsumableChangesCardShaderEdition()
        {
            SceneManager.LoadScene("BlackjackTable");yield return new WaitForSecondsRealtime(0.6f);
            ChooseStarter();
            var controller=Object.FindFirstObjectByType<BlackjackTableController>();
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var session=(DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController).GetField("session",flags).GetValue(controller);
            var inventory=(DoubleDown.Scoring.RunInventory)typeof(BlackjackTableController).GetField("inventory",flags).GetValue(controller);
            var card=session.Hands[0].Cards[0];
            var prism=System.Array.Find(Resources.LoadAll<DoubleDown.Scoring.ConsumableDefinition>("Consumables"),x=>x.id=="C10");
            Assert.IsTrue(inventory.Deck.Use(prism,card.InstanceId));
            Assert.AreEqual(2,inventory.Deck.Find(card.InstanceId)[DoubleDown.Scoring.CardLayer.Edition].visualEdition);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.Insurance)session.ChooseInsurance(false);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.PlayerTurn)session.Stand();
            while(session.Phase==DoubleDown.Blackjack.BlackjackPhase.DealerTurn)session.AdvanceDealerStep();
            var visuals=(Dictionary<DoubleDown.Blackjack.PlayingCard,BlackjackCardVisual>)typeof(BlackjackTableController).GetField("visibleCards",flags).GetValue(controller);
            var block=new MaterialPropertyBlock();visuals[card].GetComponent<SpriteRenderer>().GetPropertyBlock(block);
            Assert.AreEqual(2f,block.GetFloat("_Edition"));
            yield return new WaitForSecondsRealtime(0.3f);Capture("PrismaticCard.png");
        }

        [UnityTest]
        public IEnumerator FullRelicRackDisablesShopPurchase()
        {
            SceneManager.LoadScene("BlackjackTable");yield return new WaitForSecondsRealtime(0.6f);
            ChooseStarter();
            var rack=Object.FindFirstObjectByType<RelicRack>();
            foreach(var relic in Resources.LoadAll<DoubleDown.Scoring.RelicDefinition>("Relics"))
                if(rack.HasEmptySlot && relic.id!=rack.Loadout[0].id)rack.EquipFirstFree(relic);
            Assert.IsFalse(rack.HasEmptySlot);
            var controller=Object.FindFirstObjectByType<BlackjackTableController>();
            var session=(DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController)
                .GetField("session",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(controller);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.Insurance)session.ChooseInsurance(false);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.PlayerTurn)session.Stand();
            while(session.Phase==DoubleDown.Blackjack.BlackjackPhase.DealerTurn)session.AdvanceDealerStep();
            var shop=Object.FindFirstObjectByType<RunShop>();
            shop.Economy.GrantGold(100);
            shop.Open(1,()=>{});
            var offer=GameObject.Find("Run Shop").transform.Find("Offer 0/Buy").GetComponent<Button>();
            Assert.IsFalse(offer.interactable);
        }

        [UnityTest]
        public IEnumerator EnglishMenuAndStartingRelicUseTranslatedEffects()
        {
            SceneManager.LoadScene("MainMenu");yield return new WaitForSecondsRealtime(0.6f);
            var menu=GameObject.Find("Main Menu HUD").transform;
            menu.Find("Language").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("NEW RUN",menu.Find("New Run/Label").GetComponent<Text>().text);
            menu.Find("New Run").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(0.6f);
            var choice=GameObject.Find("Starter Relics").transform.Find("Body");
            Assert.AreEqual("CHOOSE A STARTING RELIC",choice.Find("Heading").GetComponent<Text>().text);
            var effect=choice.Find("Starter Effect 0").GetComponent<Text>().text;
            Assert.IsTrue(effect.Contains("CHIPS") || effect.Contains("MULT"));
            choice.Find("Choose 0").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("HIT",GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Hit/Label").GetComponent<Text>().text);
        }

        [UnityTest]
        public IEnumerator PortfolioMenuHelpDefeatRetryAndReturnToMenuWork()
        {
            SceneManager.LoadScene("MainMenu");yield return new WaitForSecondsRealtime(0.8f);
            var menu=Object.FindFirstObjectByType<PortfolioMenu>();Assert.NotNull(menu);Capture("PortfolioMenu.png");
            var hud=GameObject.Find("Main Menu HUD").transform;
            var language=hud.Find("Language").GetComponent<Button>();
            var initialLanguage=GameLanguage.English;
            if(!initialLanguage)language.onClick.Invoke();
            Assert.AreEqual("NEW RUN",hud.Find("New Run/Label").GetComponent<Text>().text);
            Assert.AreEqual("SETTINGS",hud.Find("Settings/Label").GetComponent<Text>().text);
            Capture("PortfolioMenuEN.png");
            language.onClick.Invoke();
            if(initialLanguage)language.onClick.Invoke();
            hud.Find("How To Play").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(menu.ModalOpen);
            yield return new WaitForSecondsRealtime(0.3f);Capture("PortfolioHelp.png");
            GameObject.Find("How To Play Panel").transform.Find("Body/Back").GetComponent<Button>().onClick.Invoke();Assert.IsFalse(menu.ModalOpen);
            hud.Find("New Run").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(0.8f);
            Capture("StarterChoice.png");
            ChooseStarter();
            var controller=Object.FindFirstObjectByType<BlackjackTableController>();Assert.NotNull(controller);
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var session=(DoubleDown.Blackjack.BlackjackSession)typeof(BlackjackTableController).GetField("session",flags).GetValue(controller);
            var score=(DoubleDown.Scoring.ScoreRun)typeof(BlackjackTableController).GetField("scoreRun",flags).GetValue(controller);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.Insurance)session.ChooseInsurance(false);
            if(session.Phase==DoubleDown.Blackjack.BlackjackPhase.PlayerTurn)session.Stand();
            while(session.Phase==DoubleDown.Blackjack.BlackjackPhase.DealerTurn)session.AdvanceDealerStep();
            score.Forfeit();yield return new WaitForSecondsRealtime(1.2f);
            var ending=Object.FindFirstObjectByType<RunEndView>();Assert.IsTrue(ending.IsOpen);Capture("PortfolioDefeat.png");
            GameObject.Find("Run End").transform.Find("Body/Retry").GetComponent<Button>().onClick.Invoke();Assert.IsFalse(ending.IsOpen);Assert.AreEqual(1,score.Round);
            yield return new WaitForSecondsRealtime(0.8f);
            ChooseStarter();
            var settings=Object.FindFirstObjectByType<TablePresentation>();settings.Toggle();
            GameObject.Find("Presentation Settings").transform.Find("Settings Body/Main Menu").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(ending.Confirming);
            GameObject.Find("Leave Run").transform.Find("Body/Cancel").GetComponent<Button>().onClick.Invoke();Assert.IsFalse(ending.Confirming);
            ending.ConfirmLeave();GameObject.Find("Leave Run").transform.Find("Body/Confirm").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);Assert.NotNull(Object.FindFirstObjectByType<PortfolioMenu>());
        }

        private static void ChooseStarter()
        {
            var starter=GameObject.Find("Starter Relics");
            Assert.NotNull(starter);
            starter.transform.Find("Body/Choose 0").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(Object.FindFirstObjectByType<RunShop>().Economy.Owns(Object.FindFirstObjectByType<RelicRack>().Loadout[0].id));
        }

        private static void Capture(string filename)
        {
            var directory = System.Environment.GetEnvironmentVariable("DOUBLEDOWN_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            var camera = Camera.main;
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var target = new RenderTexture(640, 360, 24);
            var picture = new Texture2D(640, 360, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = target });
                RenderTexture.active = target;
                picture.ReadPixels(new Rect(0, 0, 640, 360), 0, 0);
                picture.Apply();
                File.WriteAllBytes(Path.Combine(directory, filename), picture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                Object.Destroy(target);
                Object.Destroy(picture);
            }
        }
    }
}
