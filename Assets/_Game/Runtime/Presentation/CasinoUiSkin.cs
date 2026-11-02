using UnityEngine;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public static class CasinoUiSkin
    {
        public static readonly Color Ink = new Color(0.045f, 0.075f, 0.09f);
        public static readonly Color Felt = new Color(0.15f, 0.25f, 0.27f);
        public static readonly Color Paper = new Color(0.93f, 0.86f, 0.71f);
        public static readonly Color Muted = new Color(0.57f, 0.69f, 0.69f);
        public static readonly Color Red = new Color(0.58f, 0.23f, 0.24f);

        // Native four-pixel corners stay intact as the centre stretches to fit each control.
        public static void Surface(Image image, string asset, Color tint)
        {
            image.sprite = Resources.Load<Sprite>("TablePresentation/Chrome/" + asset);
            image.type = Image.Type.Sliced;
            image.color = tint;
        }
    }
}
