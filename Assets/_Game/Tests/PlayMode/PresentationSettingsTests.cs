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
        [UnityTest]
        public IEnumerator SettingsAreModalAndNewAssetsRender()
        {
            SceneManager.LoadScene("BlackjackTable");
            yield return new WaitForSecondsRealtime(0.8f);
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
                Canvas.ForceUpdateCanvases();
                camera.targetTexture = target;
                camera.Render();
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
