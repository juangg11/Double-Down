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
            transform.position = new Vector3(-4.35f, 1.75f, 0f);
            transform.localScale = Vector3.one * 2f;
            transform.rotation = Quaternion.identity;
            properties.SetFloat("_Edition", 0f);
            edition = 0f;
            faceUp = true;
            SetFaceUp(false);
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
            if (faceUp == visible && face.sprite != null) return;
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
            var response = TablePresentation.ReducedMotion ? 1f :
                1f - Mathf.Exp(-12f * Mathf.Min(deltaTime, 1f / 30f));
            transform.position = Vector3.Lerp(transform.position, targetPosition, response);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.Euler(0f, 0f, targetAngle), response);
        }
    }
}
