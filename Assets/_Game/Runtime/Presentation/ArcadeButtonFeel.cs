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
        private Image shadow;
        private AudioSource source;
        private AudioClip sound;
        private AudioClip hoverSound;
        private Color resting;
        private Vector2 origin;
        private bool hovered;
        private bool pressed;

        public void Initialize(Image faceImage, Image shadowImage,
            AudioSource audio, AudioClip click)
        {
            rect = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
            face = faceImage;
            shadow = shadowImage;
            source = audio;
            sound = click;
            hoverSound = Resources.Load<AudioClip>("TablePresentation/Audio/rollover2");
            resting = face.color;
            origin = rect.anchoredPosition;
        }

        private void OnEnable() { if (shadow != null) shadow.enabled = true; }
        private void OnDisable()
        {
            hovered = pressed = false;
            if (shadow != null) shadow.enabled = false;
            if (rect != null) rect.anchoredPosition = origin;
        }

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
            if (source != null && sound != null)
            {
                source.pitch = Random.Range(0.96f, 1.05f);
                source.PlayOneShot(sound, 0.42f);
            }
        }
        public void OnPointerUp(PointerEventData eventData) { pressed = false; }

        private void Update()
        {
            if (rect == null) return;
            var active = button.interactable;
            group.alpha = active ? 1f : 0.42f;
            var lift = active && hovered ? (pressed ? -1f : 1f) : 0f;
            if (TablePresentation.ReducedMotion) lift = 0f;
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition,
                origin + new Vector2(0f, lift), Mathf.Min(1f, Time.unscaledDeltaTime * 22f));
            // Keep glyphs at their native scale and preserve the matte colour in every pointer state.
            rect.localScale = Vector3.one;
            face.color = resting;
        }
    }
}
