using System;
using UnityEngine;

namespace DoubleDown.Presentation
{
    public sealed class FeelCardView : MonoBehaviour
    {
        private FeelProfile profile;
        private Sprite frontSprite;
        private Sprite backSprite;
        private SpriteRenderer faceRenderer;
        private SpriteRenderer shadowRenderer;
        private SpriteRenderer highlightRenderer;
        private Transform visual;
        private Transform shadow;
        private Action<FeelSound> soundRequested;
        private Vector3 layoutTarget;
        private Vector3 dragTarget;
        private Vector3 velocity;
        private Vector3 dealStart;
        private float dealDelay;
        private float dealTime;
        private float flipTime;
        private float punchTime;
        private float impactTime;
        private float lift;
        private float scale = 1f;
        private float baseAngle;
        private float swayPhase;
        private int order;
        private bool dealing;
        private bool flipping;
        private bool flipTargetFaceUp;
        private bool dealSoundPlayed;

        public bool IsHovered { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsPressed { get; private set; }
        public bool IsDragging { get; private set; }
        public bool IsFaceUp { get; private set; }
        public bool IsDealing => dealing;
        public Vector3 Position => transform.position;

        public void Initialize(
            FeelProfile feelProfile,
            Sprite front,
            Sprite back,
            Sprite shadowSprite,
            Action<FeelSound> onSound,
            int index)
        {
            profile = feelProfile;
            frontSprite = front;
            backSprite = back;
            soundRequested = onSound;
            swayPhase = index * 1.73f;

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);

            var highlight = new GameObject("Selection Glow");
            highlight.transform.SetParent(visual, false);
            highlight.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            highlightRenderer = highlight.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = frontSprite;
            highlightRenderer.color = new Color(0.95f, 0.72f, 0.25f, 0.42f);
            highlightRenderer.enabled = false;

            var face = new GameObject("Card Face");
            face.transform.SetParent(visual, false);
            faceRenderer = face.AddComponent<SpriteRenderer>();
            faceRenderer.sprite = backSprite;

            shadow = new GameObject("Shadow").transform;
            shadow.SetParent(transform, false);
            shadowRenderer = shadow.gameObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = shadowSprite;
            shadowRenderer.color = new Color(1f, 1f, 1f, 0.55f);

            SetSortingOrder(index);
            gameObject.SetActive(false);
        }

        public void ResetForDeal(Sprite front, Vector3 start, Vector3 destination, float angle, float delay)
        {
            gameObject.SetActive(true);
            frontSprite = front;
            faceRenderer.sprite = backSprite;
            highlightRenderer.sprite = front;
            IsFaceUp = false;
            IsSelected = false;
            IsPressed = false;
            IsHovered = false;
            IsDragging = false;
            dealing = true;
            flipping = false;
            dealSoundPlayed = false;
            dealDelay = delay;
            dealTime = 0f;
            flipTime = 0f;
            punchTime = 0f;
            impactTime = 0f;
            lift = 0f;
            scale = profile.cardScale;
            velocity = Vector3.zero;
            dealStart = start;
            layoutTarget = destination;
            baseAngle = angle;
            transform.position = start;
            visual.localPosition = Vector3.zero;
            visual.localScale = Vector3.one * scale;
            visual.localRotation = Quaternion.identity;
            highlightRenderer.enabled = false;
        }

        public void SetLayout(Vector3 target, float angle, int index)
        {
            layoutTarget = target;
            baseAngle = angle;
            SetSortingOrder(index);
        }

        public void SetSortingOrder(int index)
        {
            order = index * 3;
            UpdateSorting();
        }

        public void SetHover(bool value)
        {
            if (IsHovered == value)
            {
                return;
            }

            IsHovered = value;
            if (value)
            {
                soundRequested?.Invoke(FeelSound.Hover);
                Punch(0.09f);
            }

            UpdateSorting();
        }

        public void SetSelected(bool value)
        {
            IsSelected = value;
            highlightRenderer.enabled = value;
            Punch(value ? 0.2f : 0.1f);
            UpdateSorting();
        }

        public void BeginDrag()
        {
            IsDragging = true;
            soundRequested?.Invoke(FeelSound.Pickup);
            UpdateSorting();
        }

        public void BeginPress()
        {
            IsPressed = true;
            Punch(0.07f);
        }

        public void EndPress()
        {
            IsPressed = false;
        }

        public void SetDragTarget(Vector3 target)
        {
            dragTarget = target;
        }

        public void EndDrag()
        {
            IsDragging = false;
            impactTime = 0.18f;
            soundRequested?.Invoke(FeelSound.Drop);
            UpdateSorting();
        }

        public void SwapPulse(float direction)
        {
            Punch(0.12f);
            swayPhase += direction * 0.75f;
        }

        public void Flip()
        {
            if (flipping || dealing)
            {
                return;
            }

            flipTargetFaceUp = !IsFaceUp;
            flipping = true;
            flipTime = 0f;
            soundRequested?.Invoke(FeelSound.Flip);
        }

        public bool HitTest(Vector3 worldPoint)
        {
            return gameObject.activeSelf && !dealing && faceRenderer.bounds.Contains(worldPoint);
        }

        public void Tick(float deltaTime, Vector3 pointerWorld, float motionFactor)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            var dt = Mathf.Min(deltaTime, 1f / 30f);
            if (dealing)
            {
                TickDeal(dt, motionFactor);
            }
            else
            {
                var target = IsDragging ? dragTarget : layoutTarget;
                velocity += (target - transform.position) * (profile.springStrength * dt);
                velocity *= Mathf.Exp(-profile.springDamping * dt);
                velocity = Vector3.ClampMagnitude(velocity, 18f);
                transform.position += velocity * dt;
            }

            var response = 1f - Mathf.Exp(-profile.visualResponse * dt);
            var targetLift = IsDragging ? 0.34f : IsPressed ? profile.hoverLift * 0.35f : IsHovered ? profile.hoverLift : 0f;
            if (IsSelected)
            {
                targetLift += profile.selectLift;
            }

            lift = Mathf.Lerp(lift, targetLift * motionFactor, response);
            visual.localPosition = Vector3.up * lift;

            var targetScale = profile.cardScale * (IsDragging ? profile.dragScale : IsPressed ? 1.18f : IsHovered ? profile.hoverScale : 1f);
            scale = Mathf.Lerp(scale, targetScale, response);
            var punch = 0f;
            if (punchTime > 0f)
            {
                punchTime = Mathf.Max(0f, punchTime - dt);
                var progress = 1f - punchTime / 0.22f;
                punch = Mathf.Sin(progress * Mathf.PI) * punchTime * 0.75f * motionFactor;
            }

            var squash = 0f;
            if (impactTime > 0f)
            {
                impactTime = Mathf.Max(0f, impactTime - dt);
                squash = Mathf.Sin((1f - impactTime / 0.18f) * Mathf.PI) * profile.squashAmount * motionFactor;
            }

            var flipScale = TickFlip(dt);
            visual.localScale = new Vector3(
                (scale + punch + squash) * flipScale,
                scale + punch - squash,
                1f);

            var pointerOffset = pointerWorld - transform.position;
            var tiltX = IsHovered || IsDragging ? Mathf.Clamp(pointerOffset.y * profile.cursorTilt, -profile.cursorTilt, profile.cursorTilt) : 0f;
            var tiltY = IsHovered || IsDragging ? Mathf.Clamp(-pointerOffset.x * profile.cursorTilt, -profile.cursorTilt, profile.cursorTilt) : 0f;
            var sway = Mathf.Sin(Time.unscaledTime * profile.swaySpeed + swayPhase) * profile.swayAngle;
            var dragRotation = IsDragging ? Mathf.Clamp(-velocity.x * profile.velocityTilt, -20f, 20f) : 0f;
            var targetRotation = Quaternion.Euler(
                tiltX * motionFactor,
                tiltY * motionFactor,
                baseAngle + (sway + dragRotation) * motionFactor);
            visual.localRotation = Quaternion.Slerp(visual.localRotation, targetRotation, response);

            shadow.localPosition = new Vector3(0.08f + lift * 0.25f, -0.1f - lift * 0.35f, 0f);
            shadow.localScale = Vector3.one * (profile.cardScale + lift * 0.12f);
            var shadowColor = shadowRenderer.color;
            shadowColor.a = Mathf.Lerp(shadowColor.a, IsPressed && !IsDragging ? 0.35f : Mathf.Clamp(0.5f + lift * 0.22f, 0f, 0.8f), response);
            shadowRenderer.color = shadowColor;
        }

        private void TickDeal(float dt, float motionFactor)
        {
            if (dealDelay > 0f)
            {
                dealDelay -= dt;
                return;
            }

            if (!dealSoundPlayed)
            {
                dealSoundPlayed = true;
                soundRequested?.Invoke(FeelSound.Deal);
            }

            dealTime += dt;
            var t = Mathf.Clamp01(dealTime / profile.dealDuration);
            var eased = profile.dealCurve.Evaluate(t);
            transform.position = Vector3.LerpUnclamped(dealStart, layoutTarget, eased) +
                                 Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.68f * motionFactor);
            if (t >= 1f)
            {
                dealing = false;
                velocity = Vector3.zero;
                impactTime = 0.18f;
                Flip();
            }
        }

        private float TickFlip(float dt)
        {
            if (!flipping)
            {
                return 1f;
            }

            flipTime += dt;
            var t = Mathf.Clamp01(flipTime / profile.flipDuration);
            if (t >= 0.5f && IsFaceUp != flipTargetFaceUp)
            {
                IsFaceUp = flipTargetFaceUp;
                faceRenderer.sprite = IsFaceUp ? frontSprite : backSprite;
            }

            if (t >= 1f)
            {
                flipping = false;
                return 1f;
            }

            return Mathf.Max(0.03f, Mathf.Abs(Mathf.Cos(t * Mathf.PI)));
        }

        private void Punch(float duration)
        {
            punchTime = Mathf.Max(punchTime, duration);
        }

        private void UpdateSorting()
        {
            if (faceRenderer == null)
            {
                return;
            }

            var visualOrder = order + (IsDragging ? 1000 : IsHovered ? 500 : IsSelected ? 250 : 0);
            faceRenderer.sortingOrder = visualOrder;
            highlightRenderer.sortingOrder = visualOrder - 1;
            shadowRenderer.sortingOrder = visualOrder - 2;
        }
    }

    public enum FeelSound
    {
        Hover,
        Pickup,
        Drop,
        Swap,
        Flip,
        Deal,
        Select
    }
}
