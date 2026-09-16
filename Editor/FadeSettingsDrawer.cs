using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ActivationSettings))]
public class FadeSettingsDrawer : PropertyDrawer
{
    private PropertyField _fadeDurationField;
    private PropertyField _delayField;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var label = new Label(property.displayName);
        label.style.marginTop = 8;
        label.style.marginLeft = 3;
        label.style.unityFont = new StyleFont(EditorStyles.boldFont);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;


        container.Add(label);
        container.Add(new PropertyField(property.FindPropertyRelative("delay")));

        _fadeDurationField = new PropertyField(property.FindPropertyRelative("fadeDuration"));
        PropertyField boolField = new PropertyField(property.FindPropertyRelative("useFade"));
        container.Add(boolField);


        container.Add(_fadeDurationField);

        boolField.RegisterValueChangeCallback(evt => { SetDurationVisible(evt.changedProperty.boolValue); });

        SetDurationVisible((property.FindPropertyRelative("useFade").boolValue));

        return container;
    }

    void SetDurationVisible(bool state)
    {
        _fadeDurationField.style.display = new StyleEnum<DisplayStyle>(state ? DisplayStyle.Flex : DisplayStyle.None);
    }
}