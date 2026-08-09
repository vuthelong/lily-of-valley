using LilyOfValley.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class UiFactory
    {
        #region Public Methods
        public static FpsCounterUI CreateFpsCounter(Transform parent)
        {
            var text = CreateText("FPS", parent, 26f, TextAlignmentOptions.TopRight);
            text.text = "-- FPS";
            AnchorCorner(text.rectTransform, new Vector2(1f, 1f), new Vector2(-32f, -24f), new Vector2(320f, 40f));

            var counter = text.gameObject.AddComponent<FpsCounterUI>();
            ApplyFields(counter, so => SetObject(so, "label", text));
            return counter;
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = name;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, float preferredWidth, out TMP_Text label)
        {
            var root = CreateUIObject(name, parent);

            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.20f, 0.23f, 0.30f, 1f);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            if (preferredWidth > 0f) AddLayoutElement(root.gameObject, preferredWidth, 44f);

            label = CreateText("Text", root, 22f, TextAlignmentOptions.Center);
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        public static void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            var element = target.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                element.preferredWidth = preferredWidth;
                element.minWidth = preferredWidth;
                element.flexibleWidth = 0f;
            }

            if (preferredHeight <= 0f) return;

            element.preferredHeight = preferredHeight;
            element.minHeight = preferredHeight;
        }

        public static void AnchorCorner(RectTransform rect, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }
        #endregion
    }
}
