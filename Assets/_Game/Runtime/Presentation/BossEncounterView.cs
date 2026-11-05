using System;
using DoubleDown.Scoring;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class BossEncounterView : MonoBehaviour
    {
        public bool IsOpen => root != null && root.activeSelf;
        public int PresentedRound { get; private set; }
        private GameObject root;
        private Font font;
        private Text heading;
        private Text title;
        private Text rule;
        private Text stakes;
        private int pendingRound;
        private int openedFrame;
        private Action enter;

        public void Initialize(Canvas canvas, Font pixelFont)
        {
            font = pixelFont;
            root = new GameObject("Boss Encounter", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            Rect(root.GetComponent<RectTransform>(), 172, 92, 456, 260);
            CasinoUiSkin.Surface(root.GetComponent<Image>(), "Frame", CasinoUiSkin.Ink);
            heading = Label("Heading", 20, 12, 416, 24, 16);
            var icon = new GameObject("Boss Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(root.transform, false);
            Rect(icon.GetComponent<RectTransform>(), 20, 54, 32, 32);
            icon.GetComponent<Image>().sprite = Resources.Load<Sprite>("TablePresentation/Icons/crown_b");
            icon.GetComponent<Image>().color = CasinoUiSkin.Paper;
            icon.GetComponent<Image>().raycastTarget = false;
            title = Label("Name", 72, 50, 364, 38, 24);
            rule = Label("Rule", 20, 100, 416, 60, 16);
            stakes = Label("Stakes", 20, 172, 416, 32, 16);
            var go = new GameObject("Enter", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup));
            go.transform.SetParent(root.transform, false);
            Rect(go.GetComponent<RectTransform>(), 20, 214, 416, 32);
            CasinoUiSkin.Surface(go.GetComponent<Image>(), "Button", CasinoUiSkin.Red);
            var button = go.GetComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Confirm);
            var label = Label("Enter Label", 22, 214, 412, 32, 16);
            label.alignment = TextAnchor.MiddleCenter;
            // Label follows the button so the one-pixel press remains coherent.
            label.transform.SetParent(go.transform, false);
            Rect(label.rectTransform, 2, 0, 412, 32);
            label.text = "JUGAR";
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(), null,
                GetComponent<AudioSource>(), Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
            root.AddComponent<PanelMotion>();
            root.SetActive(false);
        }

        public void Show(BossRule boss, int round, int hands, BigNumber goal, Action onEnter)
        {
            pendingRound = round;
            openedFrame = Time.frameCount;
            enter = onEnter;
            heading.text = $"RONDA JEFE / MESA {round / 3:00}";
            title.text = boss.Name;
            rule.text = boss.Description;
            stakes.text = $"META {goal}   /   {hands} MANOS";
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        public void ResetRun() { PresentedRound = 0; root.SetActive(false); enter = null; }

        private void Confirm()
        {
            if (!IsOpen) return;
            PresentedRound = pendingRound;
            root.SetActive(false);
            enter?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen || Time.frameCount == openedFrame || Keyboard.current == null) return;
            if (GetComponent<DeckWorkshop>()?.IsOpen == true || GetComponent<RelicRack>().IsOpen || GetComponent<TablePresentation>().IsOpen) return;
            if (GetComponent<TableControls>().Pressed(TableAction.Confirm)) Confirm();
        }

        private Text Label(string name, float x, float y, float w, float h, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(root.transform, false);
            Rect(go.GetComponent<RectTransform>(), x, y, w, h);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.material = Graphic.defaultGraphicMaterial;
            text.fontSize = size;
            text.color = CasinoUiSkin.Paper;
            text.raycastTarget = false;
            return text;
        }

        private static void Rect(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
        }
    }
}
