using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ControlSource))]
public class ControlSourceDrawer : PropertyDrawer
{
    private PropertyField _durationField;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        //var label = new Label(property.displayName);
        //label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        //label.style.marginTop = 4;
        //label.style.marginLeft = 3;
        //container.Add(label);
            
        var controlElement = new VisualElement();

        container.Add(controlElement);

        var controlType = property.FindPropertyRelative("sourceType");
            
            
        PropertyField controlTypeField = new PropertyField(controlType);
        controlElement.Add(controlTypeField);

        VisualElement controlRandomElement = new VisualElement();
        VisualElement controlExternalElement = new VisualElement();
        controlElement.Add(controlRandomElement);
        controlElement.Add(controlExternalElement);


        var controlRandomAmount = property.FindPropertyRelative("randomControlWidth");
        PropertyField controlRandomAmountField = new PropertyField(controlRandomAmount);
        controlRandomElement.Add(controlRandomAmountField);

        var controlRandomShift = property.FindPropertyRelative("randomShift");
        PropertyField controlRandomShiftField = new PropertyField(controlRandomShift);
        controlRandomElement.Add(controlRandomShiftField);


        var controlExternalMin = property.FindPropertyRelative("valueMin");
        PropertyField controlExternalMinField = new PropertyField(controlExternalMin);
        controlExternalElement.Add(controlExternalMinField);

        var controlExternalMax = property.FindPropertyRelative("valueMax");
        PropertyField controlExternalMaxField = new PropertyField(controlExternalMax);
        controlExternalElement.Add(controlExternalMaxField);

        controlRandomElement.style.display = new StyleEnum<DisplayStyle>(controlType.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);
        controlExternalElement.style.display =
            new StyleEnum<DisplayStyle>(controlType.enumValueIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None);

        controlTypeField.RegisterValueChangeCallback(evt =>
        {
            controlRandomElement.style.display =
                new StyleEnum<DisplayStyle>(evt.changedProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None);
            controlExternalElement.style.display =
                new StyleEnum<DisplayStyle>(evt.changedProperty.enumValueIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None);
        });

        //controlElement.style.display = new StyleEnum<DisplayStyle>(controlActive.boolValue ? DisplayStyle.Flex : DisplayStyle.None);

        return container;
    }
}