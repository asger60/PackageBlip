using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Blip))]
public class BlipObjectInspector : Editor
{
    private const string PlayIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconPlay.png";
    private const string StopIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconStop.png";
    private const string StoppingIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconStopping.png";

    private Button _previewButton;
    private Image _previewIcon;
    private Blip _blip;
    private VisualElement _extendedInspector;
    private Slider _parameterSlider;
    private Label _summaryLabel;


    public override VisualElement CreateInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        VisualElement root = new VisualElement();
        _blip = target as Blip;

        _summaryLabel = new Label
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                whiteSpace = WhiteSpace.Normal,
                marginBottom = 4
            }
        };
        RefreshSummary();
        root.Add(_summaryLabel);

        var foldOut = new Foldout
        {
            value = BlipRuntime.ShowExtendedSettings,
            text = "Settings"
        };
        root.Add(foldOut);
        _extendedInspector = new VisualElement();
        InspectorElement.FillDefaultInspector(_extendedInspector, serializedObject, this);
        _extendedInspector.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.Flex : DisplayStyle.None);
        //_summaryLabel.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.None : DisplayStyle.Flex);

        root.Add(_extendedInspector);

        foldOut.RegisterValueChangedCallback(e =>
        {
            BlipRuntime.ShowExtendedSettings = foldOut.value;
            _extendedInspector.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.Flex : DisplayStyle.None);
           // _summaryLabel.style.display = new StyleEnum<DisplayStyle>(foldOut.value ? DisplayStyle.None : DisplayStyle.Flex);
        });

        root.TrackSerializedObjectValue(serializedObject, property => { RefreshSummary(); });


        var spacer = new VisualElement
        {
            style =
            {
                height = new StyleLength(10)
            }
        };
        root.Add(spacer);

        _parameterSlider = new Slider("Test parameter", 0, 1f)
        {
            showInputField = true,
        };
        RefreshParameterActive();
        _parameterSlider.RegisterValueChangedCallback(evt => { BlipRuntime.SetPreviewParameter(evt.newValue); });


        root.TrackSerializedObjectValue(serializedObject, property => { RefreshParameterActive(); });


        root.Add(_parameterSlider);

        _previewButton = new Button(() =>
        {
            if (_blip.GetLooping())
            {
                if (BlipRuntime.IsPreviewing(_blip))
                {
                    BlipRuntime.Stop(_blip, BlipRuntime.GetPreviewGameObject(), () => { SetPreviewState("Preview"); });
                    SetPreviewState("Stopping");
                }
                else
                {
                    BlipRuntime.Play(_blip, BlipRuntime.GetPreviewGameObject());
                    SetPreviewState("Stop");
                }
            }
            else
            {
                BlipRuntime.Play(_blip, BlipRuntime.GetPreviewGameObject());
                SetPreviewState("Preview");
            }
        });

        _previewButton.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        _previewButton.style.justifyContent = Justify.Center;
        _previewButton.style.alignItems = Align.Center;

        _previewIcon = new Image
        {
            scaleMode = ScaleMode.ScaleToFit,
            style =
            {
                width = 32,
                height = 32
            }
        };
        _previewButton.Add(_previewIcon);

        SetPreviewState("Preview");

        root.Add(_previewButton);

        return root;
    }

    void RefreshSummary()
    {
        string playMode = _blip.GetLooping() ? "Looping" : "One Shot";
        int clipCount = _blip.AudioClips?.Length ?? 0;
        _summaryLabel.text = $"{_blip.name} \n {playMode}, {_blip.PositionType}, {clipCount} clip{(clipCount == 1 ? "" : "s")}";
    }

    void RefreshParameterActive()
    {
        _parameterSlider.style.display = _blip.ExternalPitchControl || _blip.ExternalVolumeControl
            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    void SetPreviewState(string state)
    {
        string iconPath = state switch
        {
            "Preview" => PlayIconPath,
            "Stopping" => StoppingIconPath,
            _ => StopIconPath
        };
        _previewIcon.image = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
    }
}