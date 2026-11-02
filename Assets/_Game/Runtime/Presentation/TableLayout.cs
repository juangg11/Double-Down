using UnityEngine;

namespace DoubleDown.Presentation
{
    // Logical 640x360 coordinates, measured from the top-left corner.
    public static class TableLayout
    {
        public static readonly Rect RunRail = new Rect(8, 8, 148, 344);
        public static readonly Rect Relics = new Rect(180, 20, 436, 64);
        public static readonly Rect DealerCards = new Rect(278, 98, 344, 92);
        public static readonly Rect PlayerCards = new Rect(278, 200, 344, 92);
        public static readonly Rect Actions = new Rect(176, 308, 448, 40);
        public static Vector3 World(float x, float y) => new Vector3((x-320f)/64f, (180f-y)/64f, 0f);
        public static Vector3 CardPosition(int index, int count, bool dealer, int hand, bool split)
        {
            var centre = split && !dealer ? 360f + hand * 176f : 450f;
            var span = split && !dealer ? 76f : 240f;
            var spacing = Mathf.Min(split && !dealer ? 28f : 46f, span / Mathf.Max(1,count-1));
            return World(centre + (index-(count-1)*0.5f)*spacing, dealer ? 144f : 246f);
        }
    }
}
