using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ControlSource))]
public class ControlSourceDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var controlElement = new VisualElement();

        container.Add(controlElement);

        var controlType = property.FindPropertyRelative("externalControl");


        PropertyField controlTypeField = new PropertyField(controlType);
        controlElement.Add(controlTypeField);

        VisualElement controlExternalElement = new VisualElement();
        controlElement.Add(controlExternalElement);


        var controlExternalMin = property.FindPropertyRelative("valueMin");
        PropertyField controlExternalMinField = new PropertyField(controlExternalMin);
        controlExternalElement.Add(controlExternalMinField);

        var controlExternalMax = property.FindPropertyRelative("valueMax");
        PropertyField controlExternalMaxField = new PropertyField(controlExternalMax);
        controlExternalElement.Add(controlExternalMaxField);


        controlTypeField.RegisterValueChangeCallback(evt =>
        {
            controlExternalElement.style.display =
                new StyleEnum<DisplayStyle>(evt.changedProperty.enumValueIndex is 1 or 2
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);
        });


        return container;
    }
}