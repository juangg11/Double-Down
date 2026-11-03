using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class ControlSettingsView : MonoBehaviour
    {
        public bool IsOpen => root != null && root.activeInHierarchy;
        private GameObject root;
        private Font font;
        private TableControls controls;
        private Text status;
        private readonly KeycapHint[] caps = new KeycapHint[ControlBindings.Actions.Length];
        private readonly Image[] icons = new Image[ControlBindings.Actions.Length];
        private readonly Text[] fallback = new Text[ControlBindings.Actions.Length];
        public void Initialize(Transform parent, Font pixelFont, TableControls input)
        {
            font = pixelFont; controls = input;
            root = Panel(parent, "Controls Settings", 0, 0, 640, 360, new Color(0, 0, 0, 0.85f));
            var body = Panel(root.transform, "Body", 34, 24, 572, 312, CasinoUiSkin.Ink);
            CasinoUiSkin.Surface(body.GetComponent<Image>(), "Frame", CasinoUiSkin.Ink);
            Label(body.transform, "Heading", "CONTROLES", 16, 8, 540, 36, 32);
            for (var i = 0; i < ControlBindings.Actions.Length; i++)
            {
                var action = ControlBindings.Actions[i];
                var x = 14 + (i / 6) * 278;
                var y = 52 + (i % 6) * 34;
                Label(body.transform, "Action " + action, ControlBindings.Name(action), x, y, 176, 32, 16);
                var button = Button(body.transform, "Bind " + action, "", x + 180, y, 76, 32, () => controls.BeginCapture(action));
                var icon = Panel(button.transform, "Key", 6, 0, 64, 32, Color.white).GetComponent<Image>();
                icon.preserveAspect = true; icon.raycastTarget = false;
                icons[i] = icon; caps[i] = icon.gameObject.AddComponent<KeycapHint>();
                fallback[i] = Label(button.transform, "Key Name", "", 2, 0, 72, 32, 16);
                fallback[i].alignment = TextAnchor.MiddleCenter;
                fallback[i].color = CasinoUiSkin.Ink;
            }
            status = Label(body.transform, "Status", "", 16, 258, 540, 20, 16);
            Button(body.transform, "Defaults", "RESTABLECER", 16, 282, 256, 24, () => controls.RestoreDefaults());
            Button(body.transform, "Back", "VOLVER", 300, 282, 256, 24, Hide);
            body.AddComponent<PanelMotion>();
            controls.Changed += Refresh;
            Refresh(); root.SetActive(false);
        }
        public void Show() { root.SetActive(true); root.transform.SetAsLastSibling(); Refresh(); }
        public void Hide() { if (controls.Capturing.HasValue) controls.CancelCapture(); root.SetActive(false); }
        private void OnDestroy() { if (controls != null) controls.Changed -= Refresh; }
        private void Refresh()
        {
            status.text = controls.Capturing.HasValue ? "PULSA UNA TECLA / ESC CANCELA" : "TECLA REPETIDA: SE INTERCAMBIAN LAS ACCIONES";
            for (var i = 0; i < ControlBindings.Actions.Length; i++)
            {
                var action = ControlBindings.Actions[i]; var key = controls.Bindings[action];
                var sprite = Resources.Load<Sprite>("TablePresentation/Keys/" + GlyphName(key));
                fallback[i].text = sprite == null ? KeyName(key) : "";
                sprite = sprite != null ? sprite : Resources.Load<Sprite>("TablePresentation/Keys/BlankLg");
                caps[i].Initialize(icons[i], sprite, key);
                if (controls.Capturing == action) fallback[i].text = "...";
            }
        }
        private static string GlyphName(Key key) => key switch
        {
            Key.LeftShift or Key.RightShift => "Shift", Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt or Key.RightAlt => "Alt", _ => key.ToString().Replace("Digit", "")
        };
        private static string KeyName(Key key) => key switch
        {
            Key.LeftArrow => "IZQ", Key.RightArrow => "DER", Key.UpArrow => "ARR", Key.DownArrow => "ABA",
            Key.Backspace => "BORRAR", Key.Enter => "ENTER", Key.Tab => "TAB", _ => key.ToString().ToUpperInvariant()
        };
        private static GameObject Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); go.GetComponent<Image>().color = color; return go;
        }
        private Text Label(Transform parent, string name, string value, float x, float y, float w, float h, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
            var text = go.GetComponent<Text>(); text.font = font; text.material = Graphic.defaultGraphicMaterial;
            text.text = value; text.fontSize = size; text.alignment = TextAnchor.MiddleLeft; text.color = CasinoUiSkin.Paper; text.raycastTarget = false; return text;
        }
        private Button Button(Transform parent, string name, string value, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var go = Panel(parent, name, x, y, w, h, CasinoUiSkin.Felt); CasinoUiSkin.Surface(go.GetComponent<Image>(), "Button", CasinoUiSkin.Felt);
            go.AddComponent<CanvasGroup>(); var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action); var text = Label(go.transform, "Label", value, 2, 0, w - 4, h, 16); text.alignment = TextAnchor.MiddleCenter;
            go.AddComponent<ArcadeButtonFeel>().Initialize(go.GetComponent<Image>(), null, GetComponent<AudioSource>(), Resources.Load<AudioClip>("TablePresentation/Audio/click3"));
            return button;
        }
    }
}
