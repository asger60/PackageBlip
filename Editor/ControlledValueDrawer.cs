using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ControlledValue))]
public class ControlledValueDrawer : PropertyDrawer
{
    private PropertyField _durationField;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var label = new Label(property.displayName);
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        label.style.marginTop = 8;
        label.style.marginLeft = 3;
        container.Add(label);

        PropertyField valueField = new PropertyField(property.FindPropertyRelative("value"));
        container.Add(valueField);
        PropertyField controlField = new PropertyField(property.FindPropertyRelative("control"));
        var controlActive = property.FindPropertyRelative("controlActive");
        PropertyField controlLabel = new PropertyField(controlActive);
            
            
        controlLabel.BindProperty(controlActive);
        controlLabel.RegisterValueChangeCallback(evt =>
        {
            controlField.style.display = new StyleEnum<DisplayStyle>(controlActive.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
        });

        container.Add(controlLabel);


        controlField.style.display = new StyleEnum<DisplayStyle>(controlActive.boolValue ? DisplayStyle.Flex : DisplayStyle.None);


        container.Add(controlField);

        return container;
    }
}