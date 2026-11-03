using UnityEngine;

namespace DoubleDown.Presentation
{
    public sealed class PanelMotion : MonoBehaviour
    {
        private RectTransform rect;
        private CanvasGroup group;
        private Vector2 origin;
        private float elapsed;
        private void Awake()
        {
            rect = GetComponent<RectTransform>(); origin = rect.anchoredPosition;
            group = GetComponent<CanvasGroup>(); if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        }
        private void OnEnable() { elapsed = 0; Apply(0); }
        private void Update() { elapsed += Time.unscaledDeltaTime; Apply(Mathf.Clamp01(elapsed / 0.18f)); }
        private void Apply(float t)
        {
            if (TablePresentation.ReducedMotion) t = 1;
            var remaining = (1 - t) * (1 - t) * (1 - t);
            rect.anchoredPosition = origin + new Vector2(0, -Mathf.Round(8 * remaining));
            group.alpha = Mathf.Clamp01(t * 3);
        }
        private void OnDisable() { if (rect != null) rect.anchoredPosition = origin; }
    }
}
