using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallroomLegends.Floor
{
    /// <summary>
    /// Placeholder art made in code, so the ballroom has dancers before we have real sprites:
    /// a circle and a ring, drawn pixel by pixel into a texture, plus helpers that build
    /// UI pieces from them. Swap in real art later by giving the Images other sprites.
    /// </summary>
    public static class FloorShapes
    {
        private const int TextureSize = 128;

        private static Sprite _circle;
        private static Sprite _ring;

        public static Sprite Circle
        {
            get
            {
                // "== null", not "??=": Unity objects use their own null check, and ??= skips it.
                if (_circle == null)
                {
                    _circle = CreateSprite("Floor circle", 0f);
                }

                return _circle;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (_ring == null)
                {
                    _ring = CreateSprite("Floor ring", 10f);
                }

                return _ring;
            }
        }

        /// <summary>
        /// How see-through a pixel of the shape is: 1 inside, 0 outside, and a 1-pixel soft
        /// edge in between so the circle isn't jagged. A ring also fades out its middle.
        /// Pure function, unit tested.
        /// </summary>
        public static float ShapeAlpha(float x, float y, int size, float ringThickness)
        {
            float radius = size / 2f;
            float dx = x + 0.5f - radius;
            float dy = y + 0.5f - radius;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);

            float alpha = Mathf.Clamp01(radius - distance);
            if (ringThickness > 0f)
            {
                alpha *= Mathf.Clamp01(distance - (radius - ringThickness));
            }

            return alpha;
        }

        private static Sprite CreateSprite(string name, float ringThickness)
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    byte alpha = (byte)Mathf.RoundToInt(255f * ShapeAlpha(x, y, TextureSize, ringThickness));
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        // ---------- Building blocks ----------

        /// <summary>An empty UI object, centered on its parent.</summary>
        public static RectTransform CreateRect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>A colored circle (or oval, if the size isn't square). Ignores clicks.</summary>
        public static Image CreateCircle(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool ring = false)
        {
            RectTransform rect = CreateRect(parent, name, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ring ? Ring : Circle;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static TMP_Text CreateText(Transform parent, string name, string text, float fontSize,
            Vector2 position, Vector2 size, Color color, bool bold = false)
        {
            RectTransform rect = CreateRect(parent, name, position, size);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            if (bold)
            {
                label.fontStyle = FontStyles.Bold;
            }

            return label;
        }
    }
}
