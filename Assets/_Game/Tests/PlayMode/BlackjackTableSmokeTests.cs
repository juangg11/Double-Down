using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DoubleDown.Blackjack;
using DoubleDown.Presentation;
using NUnit.Framework;
using TMPro;
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
            var title = hud.transform.Find("Top Bar/Title").GetComponent<TMP_Text>();
            Assert.NotNull(title.font);
            Assert.AreEqual("DOUBLE DOWN", title.text);
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
    }
}
