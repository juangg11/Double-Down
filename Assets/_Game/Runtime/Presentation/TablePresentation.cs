using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    /// <summary>Local presentation preferences; never changes blackjack rules or score.</summary>
    public sealed class TablePresentation : MonoBehaviour
    {
        public static bool ReducedMotion { get; private set; }
        public static float EffectStrength { get; private set; } = 1f;
        public bool IsOpen => drawer != null && drawer.activeSelf;
        private GameObject drawer;
        private Material glass;
        private Material background;
        private Font font;
        private float crt;
        private float flow;
        private float volume;
        private Text values;
        private TableControls controls;
        private ControlSettingsView controlMenu;

        public void Initialize(Canvas canvas, Font pixelFont, Material backdrop)
        {
            font = pixelFont;
            controls = GetComponent<TableControls>();
            background = backdrop;
            crt = Mathf.Clamp01(PlayerPrefs.GetFloat("visual.crt", 0.25f));
            flow = Mathf.Clamp01(PlayerPrefs.GetFloat("visual.flow", 0.55f));
            volume = Mathf.Clamp01(PlayerPrefs.GetFloat("audio.volume", 0.7f));
            EffectStrength = Mathf.Clamp01(PlayerPrefs.GetFloat("visual.effects", 0.7f));
            ReducedMotion = PlayerPrefs.GetInt("visual.reduced", 0) == 1;
            var template = Resources.Load<Material>("TablePresentation/PixelGlass");
            if (template != null)
            {
                glass = new Material(template);
                var overlay = Panel(canvas.transform, "Pixel Glass", 0, 0, 640, 360, Color.white);
                overlay.GetComponent<Image>().material = glass;
                overlay.GetComponent<Image>().raycastTarget = false;
            }
            drawer = Panel(canvas.transform, "Presentation Settings", 0, 0, 640, 360,
                new Color(0f, 0f, 0f, 0.65f));
            var body = Panel(drawer.transform, "Settings Body", 120, 44, 400, 272,
                new Color(0.018f, 0.035f, 0.06f, 0.99f));
            CasinoUiSkin.Surface(body.GetComponent<Image>(), "Frame", CasinoUiSkin.Ink);
            Label(body.transform, "Heading", "AJUSTES DE LA MESA", 20, 14, 360, 30).fontSize = 24;
            values = Label(body.transform, "Values", "", 22, 50, 350, 90);
            Button(body.transform, "CRT", "CRT +", 20, 144, () => { crt = Step(crt); Apply(); });
            Button(body.transform, "Flow", "FONDO +", 114, 144, () => { flow = Step(flow); Apply(); });
            Button(body.transform, "Effects", "EFECTOS +", 208, 144, () => { EffectStrength = Step(EffectStrength); Apply(); });
            Button(body.transform, "Volume", "VOLUMEN +", 302, 144, () => { volume = Step(volume); Apply(); });
            Button(body.transform, "Motion", "MOVIMIENTO", 20, 186, () => { ReducedMotion = !ReducedMotion; Apply(); }, 170);
            Button(body.transform, "Controls", "CONTROLES", 210, 186, () => controlMenu.Show(), 170);
            Button(body.transform, "Close", "VOLVER", 20, 228, Toggle, 360);
            controlMenu = gameObject.AddComponent<ControlSettingsView>();
            controlMenu.Initialize(drawer.transform, font, controls);
            body.AddComponent<PanelMotion>();
            Button(canvas.transform, "Open Settings", "AJUSTES", 548, 1, Toggle, 68, 18);
            drawer.SetActive(false);
            Apply();
        }

        private static float Step(float current) => current >= 0.99f ? 0f : Mathf.Min(1f, current + 0.25f);
        private void Update()
        {
            if (controlMenu.IsOpen) { if (controls.EscapePressed) controlMenu.Hide(); return; }
            var rack = GetComponent<RelicRack>();
            if (rack != null && rack.IsOpen)
            {
                if (controls.EscapePressed) rack.Toggle();
                return;
            }
            if (controls.EscapePressed) Toggle();
        }
        public void Toggle() { if (drawer.activeSelf) controlMenu.Hide(); drawer.SetActive(!drawer.activeSelf); if (drawer.activeSelf) drawer.transform.SetAsLastSibling(); }
        private void Apply()
        {
            if (glass != null) glass.SetFloat("_Intensity", crt);
            if (background != null) background.SetFloat("_Speed", ReducedMotion ? 0f : flow);
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null) audioSource.volume = volume;
            values.text = $"CRT {crt:P0}    FONDO {flow:P0}\nEFECTOS {EffectStrength:P0}    VOLUMEN {volume:P0}\nMOVIMIENTO: {(ReducedMotion ? "REDUCIDO" : "NORMAL")}";
            PlayerPrefs.SetFloat("visual.crt", crt);
            PlayerPrefs.SetFloat("visual.flow", flow);
            PlayerPrefs.SetFloat("visual.effects", EffectStrength);
            PlayerPrefs.SetFloat("audio.volume", volume);
            PlayerPrefs.SetInt("visual.reduced", ReducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }
        private void OnDestroy() { if (glass != null) Destroy(glass); }

        private static GameObject Panel(Transform parent, string name, float x, float y,
            float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<Image>().color = color;
            return go;
        }
        private Text Label(Transform parent, string name, string content, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), x, y, w, h);
            var text = go.GetComponent<Text>();
            text.font = font; text.material = Graphic.defaultGraphicMaterial; text.fontSize = 16; text.text = content;
            text.color = new Color(1f, 0.81f, 0.48f); text.raycastTarget = false;
            return text;
        }
        private void Button(Transform parent, string name, string content, float x, float y,
            UnityEngine.Events.UnityAction action, float width = 78, float height = 30)
        {
            var shadow = Panel(parent, name + " Shadow", x + 2, y + 3, width, height,
                new Color(0.005f, 0.015f, 0.025f)).GetComponent<Image>();
            shadow.raycastTarget = false;
            var go = Panel(parent, name, x, y, width, height, new Color(0.08f, 0.25f, 0.29f));
            var button = go.AddComponent<Button>();
            go.AddComponent<CanvasGroup>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);
            CasinoUiSkin.Surface(go.GetComponent<Image>(), "Button", CasinoUiSkin.Felt);
            CasinoUiSkin.Surface(shadow, "Button", CasinoUiSkin.Ink);
            var label = Label(go.transform, "Label", content, 2, 0, width - 4, height);
            label.alignment = TextAnchor.MiddleCenter;
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(), shadow, GetComponent<AudioSource>(), Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
        }
        private static void Rect(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        }
    }
}
