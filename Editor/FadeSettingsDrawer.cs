using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ActivationSettings))]
public class FadeSettingsDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var label = new Label(property.displayName);
        label.style.marginTop = 8;
        label.style.marginLeft = 3;
        label.style.unityFont = new StyleFont(EditorStyles.boldFont);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;


        container.Add(label);

        // Play settings fade the volume up, stop settings fade it down
        var graph = new FadeGraph(property.serializedObject, property.propertyPath, property.name == "playSettings");
        container.Add(graph);

        container.Add(new PropertyField(property.FindPropertyRelative("delay")));

        container.Add(new PropertyField(property.FindPropertyRelative("fadeDuration")));

        container.TrackPropertyValue(property, _ => graph.Refresh());

        return container;
    }

    /// <summary>
    /// Draws the volume over time as a line: flat during the delay, then ramping up/down over the fade duration.
    /// </summary>
    private class FadeGraph : VisualElement
    {
        private const float Padding = 5f;
        private const float MinTimeScale = 1f;

        private readonly SerializedObject _serializedObject;
        private readonly string _propertyPath;
        private readonly bool _fadeIn;
        private readonly Label _timeLabel;

        public FadeGraph(SerializedObject serializedObject, string propertyPath, bool fadeIn)
        {
            _serializedObject = serializedObject;
            _propertyPath = propertyPath;
            _fadeIn = fadeIn;

            style.height = 40;
            style.marginLeft = 3;
            style.marginTop = 4;
            style.marginBottom = 4;
            style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.72f, 0.72f, 0.72f);
            style.borderTopLeftRadius = style.borderTopRightRadius = style.borderBottomLeftRadius = style.borderBottomRightRadius = 3;

            _timeLabel = new Label
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    right = 4,
                    top = 2,
                    fontSize = 9,
                    color = new Color(0.5f, 0.5f, 0.5f)
                }
            };
            Add(_timeLabel);

            generateVisualContent += Draw;
            Refresh();
        }

        public void Refresh()
        {
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property != null)
            {
                GetTimes(property, out float delay, out float fade);
                _timeLabel.text = $"{delay + fade:0.00} s";
            }

            MarkDirtyRepaint();
        }

        private static void GetTimes(SerializedProperty settings, out float delay, out float fade)
        {
            delay = settings.FindPropertyRelative("delay").floatValue;
            fade = settings.FindPropertyRelative("fadeDuration").floatValue;
        }

        private void Draw(MeshGenerationContext context)
        {
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property == null) return;

            GetTimes(property, out float delay, out float fade);

            // Some tail room after the fade
            float timeScale = Mathf.Max(MinTimeScale, delay + fade) * 1.15f;

            Rect rect = contentRect;
            float left = rect.xMin + Padding;
            float width = rect.width - Padding * 2;
            float low = rect.yMax - Padding;
            float high = rect.yMin + Padding + 8;
            float startY = _fadeIn ? low : high;
            float endY = _fadeIn ? high : low;

            float X(float time) => left + time / timeScale * width;

            Painter2D painter = context.painter2D;

            // Delay region
            if (delay > 0)
            {
                painter.fillColor = new Color(0.5f, 0.5f, 0.5f, 0.15f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(X(0), rect.yMin));
                painter.LineTo(new Vector2(X(delay), rect.yMin));
                painter.LineTo(new Vector2(X(delay), rect.yMax));
                painter.LineTo(new Vector2(X(0), rect.yMax));
                painter.ClosePath();
                painter.Fill();
            }

            // Volume line
            painter.strokeColor = Color.white;
            painter.lineWidth = 2f;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(new Vector2(X(0), startY));
            painter.LineTo(new Vector2(X(delay), startY));
            painter.LineTo(new Vector2(X(delay + fade), endY));
            painter.LineTo(new Vector2(left + width, endY));
            painter.Stroke();
        }
    }
}
