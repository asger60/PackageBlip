using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.SoundPositionMode))]
public class SoundPositionModeDrawer : PropertyDrawer
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

        var typeProperty = property.FindPropertyRelative("soundPositionType");
        PropertyField typeField = new PropertyField(typeProperty);
        container.Add(typeField);

        PropertyField distanceMaxField = new PropertyField(property.FindPropertyRelative("maxDistance"));
        PropertyField distanceMinField = new PropertyField(property.FindPropertyRelative("minDistance"));

        distanceMaxField.style.display = new StyleEnum<DisplayStyle>(typeProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);
        distanceMinField.style.display = new StyleEnum<DisplayStyle>(typeProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);

        container.Add(distanceMinField);
        container.Add(distanceMaxField);
            


        typeField.RegisterValueChangeCallback(evt =>
        {
            distanceMaxField.style.display =
                new StyleEnum<DisplayStyle>(typeProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);
            distanceMinField.style.display =
                new StyleEnum<DisplayStyle>(typeProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);
        });

        return container;
    }
}