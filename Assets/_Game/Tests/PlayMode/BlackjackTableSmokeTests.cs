using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DoubleDown.Blackjack;
using DoubleDown.Presentation;
using DoubleDown.Scoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DoubleDown.Tests
{
    public sealed class BlackjackTableSmokeTests
    {
        [UnityTest]
        public IEnumerator TableLoadsAndStartsAPlayableRound()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.7f);

            var controller = Object.FindFirstObjectByType<BlackjackTableController>();
            Assert.NotNull(controller);
            var sessionField = typeof(BlackjackTableController).GetField("session",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(sessionField);
            var session = (BlackjackSession)sessionField.GetValue(controller);
            Assert.AreEqual(1, session.RoundIndex);
            Assert.AreNotEqual(BlackjackPhase.Ready, session.Phase);
            Assert.AreEqual(2, session.DealerCards.Count);
            Assert.AreEqual(2, session.Hands[0].Cards.Count);
            Assert.AreEqual(4, Object.FindObjectsByType<BlackjackCardVisual>(FindObjectsSortMode.None).Length);

            var hud = GameObject.Find("Blackjack HUD");
            Assert.NotNull(hud);
            Assert.AreEqual(RenderMode.WorldSpace, hud.GetComponent<Canvas>().renderMode);
            Assert.IsNull(hud.transform.Find("Top Bar/Title"));
            foreach (var label in hud.GetComponentsInChildren<Text>())
                if (label.rectTransform.position.x < -2.5f) Assert.That(label.fontSize, Is.EqualTo(16).Or.EqualTo(32), label.name);
            Canvas.ForceUpdateCanvases();
            foreach (var path in new[] { "Goal Panel/Goal", "Score Meter/Score", "Score Meter/Formula", "Score Meter/Multiplier", "Chips Wallet/Bankroll", "Gold Wallet/Gold" })
            {
                var number = hud.transform.Find(path).GetComponent<Text>();
                Assert.GreaterOrEqual(number.cachedTextGenerator.vertexCount, number.text.Length * 4, path + " must render its digits");
            }
            Assert.NotNull(GameObject.Find("UI Event System"));
            Assert.NotNull(GameObject.Find("Casino Vortex (Shader)"));
            var hit = hud.transform.Find("Action Panel/Hit").GetComponent<Button>();
            Assert.IsTrue(hit.interactable);
            var hitRect = hit.GetComponent<RectTransform>();
            var screenPoint = Camera.main.WorldToScreenPoint(
                hitRect.TransformPoint(hitRect.rect.center));
            var pointer = new PointerEventData(EventSystem.current) { position = screenPoint };
            var raycasts = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycasts);
            Assert.IsTrue(raycasts.Exists(result => result.gameObject == hit.gameObject),
                "The Hit button must receive pointer raycasts.");
            hit.onClick.Invoke();
            yield return null;
            Assert.AreEqual(3, session.Hands[0].Cards.Count);
        }

        [UnityTest]
        public IEnumerator SettledHandUpdatesScoreAndRemainingHands()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.2f);
            var controller = Object.FindFirstObjectByType<BlackjackTableController>();
            var session = (BlackjackSession)typeof(BlackjackTableController)
                .GetField("session", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
            var scoring = (ScoreRun)typeof(BlackjackTableController)
                .GetField("scoreRun", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
            if (session.Phase == BlackjackPhase.Insurance) session.ChooseInsurance(false);
            if (session.Phase == BlackjackPhase.PlayerTurn) session.Stand();
            while (session.Phase == BlackjackPhase.DealerTurn) session.AdvanceDealerStep();
            Assert.AreEqual(BlackjackPhase.Settled, session.Phase);
            Assert.AreEqual(3, scoring.HandsRemaining);
            yield return new WaitForSecondsRealtime(0.9f);
            var meter = GameObject.Find("Blackjack HUD").transform.Find("Score Meter");
            Assert.AreEqual(scoring.Score.ToString(), meter.Find("Score").GetComponent<Text>().text);
            Assert.IsNotEmpty(meter.Find("Formula").GetComponent<Text>().text);
            scoring.RecordDeal(new[] { new ScoreBreakdown(BigNumber.From(999), 1) });
            Assert.AreEqual(ScoreRunState.Won, scoring.State);
            GameObject.Find("Blackjack HUD").transform.Find("Action Panel/Deal")
                .GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(Object.FindFirstObjectByType<RunShop>().IsOpen);
            GameObject.Find("Run Shop").transform.Find("Continue").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(2, scoring.Round);
            Assert.AreEqual(ScoreRunState.Playing, scoring.State);
        }

        [UnityTest]
        public IEnumerator ButtonsMoveWithoutBrighteningOnHoverAndPress()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.2f);
            var panel = GameObject.Find("Blackjack HUD").transform.Find("Action Panel");
            var hit = panel.Find("Hit").gameObject;
            var rect = hit.GetComponent<RectTransform>();
            var restingColor = hit.GetComponent<Image>().color;
            var restingY = rect.anchoredPosition.y;
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerEnterHandler>(hit, pointer, ExecuteEvents.pointerEnterHandler);
            yield return new WaitForSecondsRealtime(0.12f);
            var hoverY = rect.anchoredPosition.y;
            Assert.Greater(hoverY, restingY);
            Assert.AreEqual(Vector3.one, rect.localScale);
            Assert.AreEqual(restingColor, hit.GetComponent<Image>().color);
            Assert.IsNull(hit.transform.Find("Hover shine"));
            ExecuteEvents.Execute<IPointerDownHandler>(hit, pointer, ExecuteEvents.pointerDownHandler);
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.AreEqual(Vector3.one, rect.localScale);
            Assert.AreEqual(restingColor, hit.GetComponent<Image>().color);
            Assert.Less(rect.anchoredPosition.y, hoverY);
            var split = panel.Find("Split").GetComponent<CanvasGroup>();
            Assert.Less(split.alpha, 0.5f);
        }
    }
}
