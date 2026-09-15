using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.FadeSettings))]
public class FadeSettingsDrawer : PropertyDrawer
{
    private PropertyField _durationField;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var label = new Label(property.displayName);
        label.style.marginTop = 8;
        label.style.marginLeft = 3;
        container.Add(label);
        PropertyField boolField = new PropertyField(property.FindPropertyRelative("useFade"));
        container.Add(boolField);

        _durationField = new PropertyField(property.FindPropertyRelative("fadeDuration"));
        container.Add(_durationField);
        boolField.RegisterValueChangeCallback(evt => { SetDurationVisible(evt.changedProperty.boolValue); });

        SetDurationVisible((property.FindPropertyRelative("useFade").boolValue));

        return container;
    }

    void SetDurationVisible(bool state)
    {
        _durationField.style.display = new StyleEnum<DisplayStyle>(state ? DisplayStyle.Flex : DisplayStyle.None);
    }
}