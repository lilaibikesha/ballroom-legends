using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BallroomLegends.EditorTools
{
    /// <summary>
    /// Small helpers for building UGUI + TextMeshPro layouts from code (editor only).
    /// Everything is laid out on a 1920 x 1080 reference canvas, positions measured from
    /// the center of the screen. After it's built you can move things around by hand.
    /// </summary>
    internal static class UiBuilder
    {
        public static readonly Color Gold = new Color(0.96f, 0.80f, 0.42f);
        public static readonly Color Panel = new Color(0.20f, 0.13f, 0.30f, 0.95f);
        public static readonly Color ButtonColor = new Color(0.56f, 0.20f, 0.44f);
        public static readonly Color ErrorColor = new Color(1f, 0.45f, 0.45f);
        public static readonly Color FloorColor = new Color(0.42f, 0.27f, 0.16f);
        public static readonly Color FloorEdgeColor = new Color(0.62f, 0.45f, 0.24f);

        private static TMP_DefaultControls.Resources _controlResources;
        private static bool _controlResourcesLoaded;

        /// <summary>The default UI sprites, the same ones GameObject > UI > ... uses.</summary>
        private static TMP_DefaultControls.Resources ControlResources
        {
            get
            {
                if (!_controlResourcesLoaded)
                {
                    _controlResources = new TMP_DefaultControls.Resources
                    {
                        standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                        background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                        inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                        checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                        dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
                        mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
                    };
                    _controlResourcesLoaded = true;
                }

                return _controlResources;
            }
        }

        /// <summary>
        /// TextMeshPro needs its "Essential Resources" (default font, shaders, settings) in the
        /// project. Imports them if missing. Returns false if that didn't work.
        /// </summary>
        public static bool EnsureTmpEssentials()
        {
            if (HasTmpSettings())
            {
                return true;
            }

            var ugui = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            if (ugui != null)
            {
                string package = Path.Combine(ugui.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
                if (File.Exists(package))
                {
                    AssetDatabase.ImportPackage(package, false);
                    AssetDatabase.Refresh();
                }
            }

            if (HasTmpSettings())
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Import TextMesh Pro first",
                "Click Window > TextMeshPro > Import TMP Essential Resources, then run " +
                "Ballroom Legends > Create Initial Scenes again.",
                "OK");
            return false;
        }

        private static bool HasTmpSettings() => Resources.Load<TMP_Settings>("TMP Settings") != null;

        /// <summary>Screen-space canvas scaled for 1920 x 1080, plus the EventSystem clicks need.</summary>
        public static RectTransform CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.layer = LayerMask.NameToLayer("UI");

            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // This project uses the new Input System only, so the EventSystem needs the
            // InputSystemUIInputModule (the old StandaloneInputModule would throw errors).
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            return (RectTransform)canvasObject.transform;
        }

        /// <summary>Invisible full-screen container, used for panels you switch on and off.</summary>
        public static RectTransform CreateContainer(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>A colored box.</summary>
        public static RectTransform CreateBox(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, position, size);

            var image = go.GetComponent<Image>();
            image.sprite = ControlResources.background;
            image.type = Image.Type.Sliced;
            image.color = color;
            return rect;
        }

        /// <summary>A colored circle (the round "knob" sprite). Purely decorative: it ignores clicks.</summary>
        public static RectTransform CreateCircle(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, position, size);

            var image = go.GetComponent<Image>();
            image.sprite = ControlResources.knob;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        public static TMP_Text CreateText(Transform parent, string name, string text, float fontSize,
            Vector2 position, Vector2 size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject go = TMP_DefaultControls.CreateText(ControlResources);
            go.name = name;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, position, size);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            return label;
        }

        /// <summary>
        /// Button with a TMP label. If onClick is given it's saved as a persistent listener,
        /// exactly what you'd see after wiring On Click () by hand in the Inspector.
        /// </summary>
        public static Button CreateButton(Transform parent, string name, string label,
            Vector2 position, Vector2 size, UnityAction onClick, float fontSize = 36f)
        {
            GameObject go = TMP_DefaultControls.CreateButton(ControlResources);
            go.name = name;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, position, size);

            go.GetComponent<Image>().color = ButtonColor;

            var text = go.GetComponentInChildren<TMP_Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.color = Color.white;

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                UnityEventTools.AddPersistentListener(button.onClick, onClick);
            }

            return button;
        }

        public static TMP_InputField CreateInputField(Transform parent, string name, string placeholder,
            Vector2 position, Vector2 size, int characterLimit)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(ControlResources);
            go.name = name;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, position, size);

            var input = go.GetComponent<TMP_InputField>();
            input.characterLimit = characterLimit;
            input.pointSize = 40f;

            if (input.placeholder is TMP_Text placeholderText)
            {
                placeholderText.text = placeholder;
            }

            return input;
        }

        /// <summary>Anchors to the screen center, then sets position and size.</summary>
        public static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>
        /// Fills a private [SerializeField] field, exactly like dragging something into the
        /// Inspector slot. SerializedObject is how editor code edits serialized fields.
        /// </summary>
        public static void SetField(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError($"[Setup] Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string fieldName, Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null || !property.isArray)
            {
                Debug.LogError($"[Setup] Array '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
