using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    [RequireComponent(typeof(Button))]
    public sealed class ArcadeButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private RectTransform rect;
        private Button button;
        private CanvasGroup group;
        private Image face;
        private Image shine;
        private Image shadow;
        private AudioSource source;
        private AudioClip sound;
        private AudioClip hoverSound;
        private Color resting;
        private Vector2 origin;
        private bool hovered;
        private bool pressed;
        private float punch;

        public void Initialize(Image faceImage, Image highlight, Image shadowImage,
            AudioSource audio, AudioClip click)
        {
            rect = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
            face = faceImage;
            shine = highlight;
            shadow = shadowImage;
            source = audio;
            sound = click;
            hoverSound = Resources.Load<AudioClip>("TablePresentation/Audio/rollover2");
            resting = face.color;
            origin = rect.anchoredPosition;
        }

        private void OnEnable() { if (shadow != null) shadow.enabled = true; }
        private void OnDisable() { if (shadow != null) shadow.enabled = false; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            if (button.interactable && source != null && hoverSound != null)
                source.PlayOneShot(hoverSound, 0.12f);
        }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;
            pressed = true;
            punch = 1f;
            if (source != null && sound != null)
            {
                source.pitch = Random.Range(0.96f, 1.05f);
                source.PlayOneShot(sound, 0.42f);
            }
        }
        public void OnPointerUp(PointerEventData eventData) { pressed = false; punch = 1f; }

        private void Update()
        {
            if (rect == null) return;
            punch = Mathf.MoveTowards(punch, 0f, Time.unscaledDeltaTime * 7f);
            var active = button.interactable;
            group.alpha = active ? 1f : 0.38f;
            var lift = active && hovered ? (pressed ? -1f : 2f) : 0f;
            if (TablePresentation.ReducedMotion) lift = 0f;
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition,
                origin + new Vector2(0f, lift), Mathf.Min(1f, Time.unscaledDeltaTime * 22f));
            var scale = active ? 1f + (hovered && !pressed ? 0.035f : 0f) -
                (pressed ? 0.03f : 0f) + punch * 0.015f : 1f;
            if (TablePresentation.ReducedMotion) scale = 1f;
            rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(scale, scale, 1f),
                Mathf.Min(1f, Time.unscaledDeltaTime * 20f));
            face.color = active ? Color.Lerp(resting, Color.white, hovered ? 0.16f : 0f)
                : new Color(resting.r * 0.5f, resting.g * 0.5f, resting.b * 0.5f, 0.7f);
            shine.color = new Color(1f, 0.90f, 0.65f, active && hovered ? 0.30f : 0f);
        }
    }
}
