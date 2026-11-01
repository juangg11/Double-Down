using System.Collections;
using DoubleDown.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DoubleDown.Tests
{
    public sealed class FeelSandboxSmokeTests
    {
        [UnityTest]
        public IEnumerator SandboxDealsSevenFaceUpCards()
        {
            SceneManager.LoadScene("FeelSandbox");
            yield return new WaitForSecondsRealtime(1.5f);

            var cards = UnityEngine.Object.FindObjectsByType<FeelCardView>(FindObjectsSortMode.None);
            Assert.AreEqual(7, cards.Length);
            foreach (var card in cards)
            {
                Assert.IsTrue(card.IsFaceUp);
                Assert.IsFalse(card.IsDealing);
            }

        }
    }
}
