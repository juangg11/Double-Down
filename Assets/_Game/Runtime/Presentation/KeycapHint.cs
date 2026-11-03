using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoubleDown.Presentation
{
    public sealed class KeycapHint : MonoBehaviour
    {
        private Image image;
        private Sprite up;
        private Sprite down;
        private Key key;
        public void Initialize(Image target, Sprite sheet, Key keyboardKey)
        {
            if (up != null) Destroy(up);
            if (down != null) Destroy(down);
            image = target; key = keyboardKey;
            var texture = sheet.texture;
            var width = texture.width / 2;
            up = Sprite.Create(texture, new Rect(0, 0, width, texture.height), new Vector2(0.5f, 0.5f), 64);
            down = Sprite.Create(texture, new Rect(width, 0, width, texture.height), new Vector2(0.5f, 0.5f), 64);
            image.sprite = up;
        }
        private void Update()
        {
            if (image != null) image.sprite = Keyboard.current != null && Keyboard.current[key].isPressed ? down : up;
        }
        private void OnDestroy() { if (up != null) Destroy(up); if (down != null) Destroy(down); }
    }
}
