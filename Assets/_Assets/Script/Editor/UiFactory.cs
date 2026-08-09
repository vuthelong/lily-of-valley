using LilyOfValley.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class UiFactory
    {
        #region Field

        private const string LabelFieldName = "label";

        private const string FpsCounterName = "FPS";
        private const string FpsCounterPlaceholder = "-- FPS";
        private const float FpsCounterFontSize = 26f;

        private const string ButtonLabelName = "Text";
        private const float ButtonLabelFontSize = 22f;
        private const float ButtonHeight = 44f;

        private const float CanvasMatchWidthOrHeight = 0.5f;

        private static readonly Vector2 FpsCounterAnchor = new(1f, 1f);
        private static readonly Vector2 FpsCounterOffset = new(-32f, -24f);
        private static readonly Vector2 FpsCounterSize = new(320f, 40f);
        private static readonly Vector2 CanvasReferenceResolution = new(1920f, 1080f);

        private static readonly Color ButtonBackgroundColor = new(0.20f, 0.23f, 0.30f, 1f);

        #endregion

        #region Widget Creation

        public static FpsCounterUI CreateFpsCounter(Transform parent)
        {
            var text = CreateText(FpsCounterName, parent, FpsCounterFontSize, TextAlignmentOptions.TopRight);
            text.text = FpsCounterPlaceholder;
            AnchorCorner(text.rectTransform, FpsCounterAnchor, FpsCounterOffset, FpsCounterSize);

            var counter = text.gameObject.AddComponent<FpsCounterUI>();
            ApplyFields(counter, serialized => SetObject(serialized, LabelFieldName, text));

            return counter;
        }

        public static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = CanvasReferenceResolution;
            scaler.matchWidthOrHeight = CanvasMatchWidthOrHeight;

            return canvas;
        }

        public static RectTransform CreateUIObject(string name, Transform parent)
        {
            var uiObject = new GameObject(name, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);

            return (RectTransform)uiObject.transform;
        }

        public static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            var text = textObject.AddComponent<TextMeshProUGUI>();
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
            image.color = ButtonBackgroundColor;

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            if (preferredWidth > 0f) AddLayoutElement(root.gameObject, preferredWidth, ButtonHeight);

            label = CreateText(ButtonLabelName, root, ButtonLabelFontSize, TextAlignmentOptions.Center);

            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return button;
        }

        #endregion

        #region Layout

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
