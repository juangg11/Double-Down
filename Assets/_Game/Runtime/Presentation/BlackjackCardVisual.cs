using DoubleDown.Blackjack;
using UnityEngine;

namespace DoubleDown.Presentation
{
    public sealed class BlackjackCardVisual : MonoBehaviour
    {
        private static readonly int RecolorId = Shader.PropertyToID("_Recolor");
        private SpriteRenderer face;
        private MaterialPropertyBlock properties;
        private Sprite front;
        private Sprite back;
        private PlayingCard card;
        private Vector3 targetPosition;
        private float targetAngle;
        private bool faceUp;
        private float edition;
        private float editionStrength;
        private bool desiredFace;
        private float travel, delay, flip = 1f, pulse, size = 1.35f;
        private readonly Vector3 deckPosition = new Vector3(4.8f, 2.2f, 0f);
        public void SetSize(float value) => size = value;
        public void Pulse() => pulse = TablePresentation.ReducedMotion ? 0f : 1f;

        public void Initialize(PlayingCard playingCard, Sprite frontSprite, Sprite backSprite,
            Material inkMaterial, int order)
        {
            card = playingCard;
            front = frontSprite;
            back = backSprite;
            face = GetComponent<SpriteRenderer>();
            if (face == null) face = gameObject.AddComponent<SpriteRenderer>();
            face.sharedMaterial = inkMaterial;
            face.sortingOrder = order;
            properties ??= new MaterialPropertyBlock();
            transform.position = deckPosition;
            travel = 0f; delay = order <= 4 ? Mathf.Max(0, order - 1) * 0.06f : 0f;
            pulse = 0f; flip = 1f; desiredFace = false;
            transform.localScale = Vector3.one * size;
            transform.rotation = Quaternion.identity;
            properties.SetFloat("_Edition", 0f);
            edition = 0f;
            ApplyFace(false);
            gameObject.SetActive(true);
        }

        public void SetTarget(Vector3 position, float angle, int order)
        {
            targetPosition = position;
            targetAngle = angle;
            face.sortingOrder = order;
        }

        public void SetFaceUp(bool visible)
        {
            if (desiredFace == visible) return;
            desiredFace = visible;
            flip = 0f;
        }

        private void ApplyFace(bool visible)
        {
            faceUp = visible;
            face.sprite = visible ? front : back;
            properties.SetFloat(RecolorId, visible && card.Suit == CardSuit.Diamonds ? 1f : 0f);
            face.SetPropertyBlock(properties);
        }

        public void SetEdition(float edition)
        {
            this.edition = edition;
            editionStrength = TablePresentation.EffectStrength;
            properties.SetFloat("_Edition", edition);
            properties.SetFloat("_EditionStrength", editionStrength);
            face.SetPropertyBlock(properties);
        }

        public void Tick(float deltaTime)
        {
            if (editionStrength != TablePresentation.EffectStrength) SetEdition(edition);
            if (TablePresentation.ReducedMotion)
            {
                travel = flip = 1f; pulse = 0f;
                ApplyFace(desiredFace);
                transform.SetPositionAndRotation(targetPosition, Quaternion.Euler(0, 0, targetAngle));
                transform.localScale = Vector3.one * size;
                return;
            }
            deltaTime = Mathf.Min(deltaTime, 0.05f);
            if (delay > 0f) { delay -= deltaTime; return; }
            if (travel < 1f)
            {
                travel = Mathf.Min(1f, travel + deltaTime / 0.34f);
                var eased = 1f - Mathf.Pow(1f - travel, 3f);
                transform.position = Vector3.Lerp(deckPosition, targetPosition, eased) + Vector3.up * (Mathf.Sin(travel * Mathf.PI) * 0.24f);
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-12f, targetAngle, eased));
            }
            else
            {
                var response = 1f - Mathf.Exp(-18f * deltaTime);
                transform.position = Vector3.Lerp(transform.position, targetPosition, response);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), response);
            }
            if (travel >= 0.6f && flip < 1f)
            {
                flip = Mathf.Min(1f, flip + deltaTime / 0.22f);
                if (flip >= 0.5f && faceUp != desiredFace) ApplyFace(desiredFace);
            }
            pulse = Mathf.Max(0, pulse - deltaTime / 0.25f);
            var bounce = Mathf.Sin(pulse * Mathf.PI) * 0.045f;
            var width = travel < 0.6f ? 1f : Mathf.Max(0.08f, Mathf.Abs(Mathf.Cos(flip * Mathf.PI)));
            transform.localScale = new Vector3(size * width * (1f + bounce), size * (1f + bounce), 1f);
        }
    }
}
