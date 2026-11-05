using UnityEngine;
namespace DoubleDown.Scoring
{
    public enum CardLayer { Edition, Enhancement, Seal }
    [CreateAssetMenu(menuName="Double Down/Card Modification")]
    public sealed class CardModificationDefinition : ScriptableObject
    {
        public string id, displayName;
        [TextArea] public string description;
        public Sprite icon;
        public CardLayer layer;
        public float chips, multiplier;
        public float multiplierFactor = 1;
        public int gold;
        public bool wild, destroyAfterScoring, repeatBonuses;
        public float multiplierPerPosition;
        [Range(0,3)] public int visualEdition;
    }
}
