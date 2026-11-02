using System.Collections;
using System.Linq;
using System.Reflection;
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
            yield return null;
            var cards = UnityEngine.Object.FindObjectsByType<FeelCardView>(FindObjectsSortMode.None);
            // Animation advances with a capped delta; a wall-clock delay can finish mid-flip in batch mode.
            var deadline = Time.realtimeSinceStartup + 6f;
            while (Time.realtimeSinceStartup < deadline &&
                (cards.Length != 7 || cards.Any(card => !card.IsFaceUp || card.IsDealing ||
                    Mathf.Abs(card.transform.Find("Visual").localScale.x - 2f) > 0.02f)))
            {
                yield return null;
                cards = UnityEngine.Object.FindObjectsByType<FeelCardView>(FindObjectsSortMode.None);
            }
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
                var outline = visual.Find("Selection Glow").GetComponent<SpriteRenderer>();
                Assert.AreEqual("DoubleDown/SelectionOutline", outline.sharedMaterial.shader.name);
                Assert.That(outline.transform.localScale.x, Is.EqualTo(1f).Within(0.001f));
            }

            var ordered = cards.OrderBy(card => card.Position.x).ToArray();
            Assert.That(ordered[3].Position.y, Is.GreaterThan(ordered[0].Position.y + 0.10f));
            Assert.That(ordered[0].transform.Find("Visual").localEulerAngles.z, Is.InRange(6f, 11f));
            Assert.That(Mathf.DeltaAngle(0f, ordered[6].transform.Find("Visual").localEulerAngles.z),
                Is.InRange(-11f, -6f));

            var controller = UnityEngine.Object.FindFirstObjectByType<FeelSandboxController>();
            var findCard = typeof(FeelSandboxController).GetMethod("FindTopCard",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(findCard);
            for (var direction = 0; direction < 2; direction++)
            {
                for (var step = 0; step < ordered.Length; step++)
                {
                    var index = direction == 0 ? step : ordered.Length - 1 - step;
                    var point = new Vector3(ordered[index].Position.x, -1.35f, 0f);
                    var hit = findCard.Invoke(controller, new object[] { point });
                    Assert.AreSame(ordered[index], hit,
                        $"Picking failed at card {index}, direction {direction}.");
                }
            }

            var table = GameObject.Find("Casino Vortex (Shader)");
            Assert.NotNull(table);
            var material = table.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.NotNull(material);
            Assert.AreEqual("DoubleDown/CasinoVortex", material.shader.name);
            Assert.IsTrue(material.shader.isSupported);
            Assert.NotNull(GameObject.Find("Global Light 2D"));
        }
    }
}
