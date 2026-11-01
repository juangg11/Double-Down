using UnityEngine;

namespace DoubleDown.Presentation
{
    [CreateAssetMenu(fileName = "FeelProfile", menuName = "Double Down/Feel Profile")]
    public sealed class FeelProfile : ScriptableObject
    {
        [Header("Hand")]
        [Range(0.35f, 1.2f)] public float cardSpacing = 0.86f;
        [Range(0f, 0.8f)] public float fanHeight = 0.14f;
        [Range(0f, 30f)] public float fanAngle = 8.4f;
        [Range(0.1f, 2f)] public float cardScale = 2f;
        public AnimationCurve fanCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 6.9f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -6.9f, 0f));

        [Header("Follow")]
        [Range(15f, 200f)] public float springStrength = 88f;
        [Range(2f, 32f)] public float springDamping = 15f;
        [Range(0.03f, 0.25f)] public float dragThreshold = 0.08f;

        [Header("Hover and selection")]
        [Range(0f, 0.8f)] public float hoverLift = 0.22f;
        [Range(1f, 1.5f)] public float hoverScale = 1.08f;
        [Range(0f, 0.8f)] public float selectLift = 0.38f;
        [Range(1f, 1.5f)] public float dragScale = 1.12f;
        [Range(1f, 30f)] public float visualResponse = 17f;

        [Header("Tilt and sway")]
        [Range(0f, 30f)] public float cursorTilt = 2f;
        [Range(0f, 20f)] public float velocityTilt = 3f;
        [Range(0f, 5f)] public float swayAngle = 0.3f;
        [Range(0f, 3f)] public float swaySpeed = 1.05f;

        [Header("Actions")]
        [Range(0.15f, 1.2f)] public float dealDuration = 0.43f;
        public AnimationCurve dealCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.7f), new Keyframe(1f, 1f, 0f, 0f));
        [Range(0.1f, 0.8f)] public float flipDuration = 0.3f;
        [Range(0f, 0.2f)] public float squashAmount = 0.06f;
        [Range(0f, 1f)] public float sfxVolume = 0.55f;
    }
}
