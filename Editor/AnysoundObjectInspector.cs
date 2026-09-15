using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Blip))]
public class AnysoundObjectInspector : Editor
{
    private Button _previewButton;
    private Blip _blip;
    private VisualElement _extendedInspector;
    private Slider _parameterSlider;


    public override VisualElement CreateInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        VisualElement root = new VisualElement();
        var foldOut = new Foldout
        {
            value = BlipRuntime.ShowExtendedSettings,
            text = "Settings"
        };
        root.Add(foldOut);
        _extendedInspector = new VisualElement();
        InspectorElement.FillDefaultInspector(_extendedInspector, serializedObject, this);
        _extendedInspector.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.Flex : DisplayStyle.None);

        root.Add(_extendedInspector);

        foldOut.RegisterValueChangedCallback(e =>
        {
            BlipRuntime.ShowExtendedSettings = foldOut.value;
            _extendedInspector.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.Flex : DisplayStyle.None);
        });
        _blip = target as Blip;


        var spacer = new VisualElement();
        spacer.style.height = new StyleLength(10);
        root.Add(spacer);

        _parameterSlider = new Slider("Test parameter", 0, 1f)
        {
            showInputField = true,
        };
        RefreshParameterActive();
        _parameterSlider.RegisterValueChangedCallback(evt => { BlipRuntime.SetPreviewParameter(evt.newValue); });


        root.TrackSerializedObjectValue(serializedObject, property =>
        {
            RefreshParameterActive();
        });


        root.Add(_parameterSlider);

        _previewButton = new Button(() =>
        {
            if (_blip.GetLooping())
            {
                if (BlipRuntime.IsPreviewing(_blip))
                {
                    BlipRuntime.StopPreview(_blip, () => { SetPreviewButtonText("Preview"); });
                    SetPreviewButtonText("Stopping");
                }
                else
                {
                    BlipRuntime.StartPreview(_blip);
                    SetPreviewButtonText("Stop");
                }
            }
            else
            {
                BlipRuntime.StartPreview(_blip);
                SetPreviewButtonText("Preview");
            }
        });
        SetPreviewButtonText("Preview");

        root.Add(_previewButton);

        return root;
    }

    void RefreshParameterActive()
    {
        _parameterSlider.style.display = _blip.ExternalPitchControl || _blip.ExternalVolumeControl
            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    void SetPreviewButtonText(string text)
    {
        _previewButton.text = text;
    }
}