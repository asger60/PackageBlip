using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Blip.ControlledValue))]
public class ControlledValueDrawer : PropertyDrawer
{
    private const long RefreshIntervalMs = 50;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        var label = new Label(property.displayName)
        {
            style =
            {
                unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                marginTop = 8,
                marginLeft = 3
            }
        };
        container.Add(label);

        PropertyField valueField = new PropertyField(property.FindPropertyRelative("value"));
        container.Add(valueField);

        // Read-only slider showing the value produced by the external control (random roll or game parameter)
        var externalValueSlider = new Slider("Value", 0, 1)
        {
            showInputField = true
        };
        externalValueSlider.AddToClassList(BaseField<float>.alignedFieldUssClassName);
        externalValueSlider.SetEnabled(false);
        container.Add(externalValueSlider);

        var controlProperty = property.FindPropertyRelative("control");
        var controlType = controlProperty.FindPropertyRelative("externalControl");
        var valueMin = controlProperty.FindPropertyRelative("valueMin");
        var valueMax = controlProperty.FindPropertyRelative("valueMax");

        PropertyField controlField = new PropertyField(controlProperty);
        container.Add(controlField);

        var blip = property.serializedObject.targetObject as Blip;

        void RefreshMode()
        {
            bool external = controlType.enumValueIndex != (int)Blip.ControlSource.ExternalControlTypes.None;
            valueField.style.display = external ? DisplayStyle.None : DisplayStyle.Flex;
            externalValueSlider.style.display = external ? DisplayStyle.Flex : DisplayStyle.None;
            externalValueSlider.lowValue = Mathf.Min(0, valueMin.floatValue, valueMax.floatValue);
            externalValueSlider.highValue = Mathf.Max(1, valueMin.floatValue, valueMax.floatValue);
        }

        void RefreshExternalValue()
        {
            if (!blip || externalValueSlider.style.display.value == DisplayStyle.None)
                return;

            // Read the live struct from the asset, since the current random value is not serialized
            var controlledValue = (Blip.ControlledValue)fieldInfo.GetValue(blip);
            switch (controlledValue.ExternalControl)
            {
                case Blip.ControlSource.ExternalControlTypes.RandomEachPlay:
                    externalValueSlider.label = controlledValue.HasCurrentValue ? "Value (Random)" : "Value (Random, not played)";
                    break;
                case Blip.ControlSource.ExternalControlTypes.FollowGameParameter:
                    externalValueSlider.label = "Value (Parameter)";
                    break;
            }

            externalValueSlider.SetValueWithoutNotify(controlledValue.GetDisplayValue(blip.ParameterValue));
        }

        RefreshMode();
        container.TrackPropertyValue(controlProperty, _ =>
        {
            RefreshMode();
            RefreshExternalValue();
        });
        container.schedule.Execute(RefreshExternalValue).Every(RefreshIntervalMs);

        return container;
    }
}
