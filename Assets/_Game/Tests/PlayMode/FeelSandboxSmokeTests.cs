using System.Collections;
using System.Linq;
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
                var visual = card.transform.Find("Visual");
                Assert.NotNull(visual);
                Assert.That(visual.localScale.x, Is.EqualTo(2f).Within(0.02f));
                Assert.That(visual.localRotation.x, Is.EqualTo(0f).Within(0.001f));
                Assert.That(visual.localRotation.y, Is.EqualTo(0f).Within(0.001f));
                var face = visual.Find("Card Face").GetComponent<SpriteRenderer>();
                Assert.AreEqual("DoubleDown/CardInk", face.sharedMaterial.shader.name);
                var ink = new MaterialPropertyBlock();
                face.GetPropertyBlock(ink);
                var diamond = face.sprite.name.StartsWith("card_diamonds_");
                Assert.That(ink.GetFloat("_Recolor"), Is.EqualTo(diamond ? 1f : 0f).Within(0.01f));
            }

            var ordered = cards.OrderBy(card => card.Position.x).ToArray();
            Assert.That(ordered[3].Position.y, Is.GreaterThan(ordered[0].Position.y + 0.10f));
            Assert.That(ordered[0].transform.Find("Visual").localEulerAngles.z, Is.InRange(6f, 11f));
            Assert.That(Mathf.DeltaAngle(0f, ordered[6].transform.Find("Visual").localEulerAngles.z),
                Is.InRange(-11f, -6f));

            var table = GameObject.Find("Geometric Table (Shader)");
            Assert.NotNull(table);
            var material = table.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.NotNull(material);
            Assert.AreEqual("DoubleDown/GeometricTable", material.shader.name);
            Assert.IsTrue(material.shader.isSupported);
            Assert.NotNull(GameObject.Find("Global Light 2D"));
        }
    }
}
