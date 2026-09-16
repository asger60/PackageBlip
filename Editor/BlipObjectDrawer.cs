using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Blip))]
public class BlipObjectDrawer : PropertyDrawer
{
    private const string PlayIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconPlay.png";
    private const string StopIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconStop.png";
    private const string StoppingIconPath = "Packages/com.floppyclub.blip/Editor/PNG/IconStopping.png";

    private string _previewState = "Preview"; // Initialize with default state

    private const float PreviewIconScale = .75f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty/EndProperty is good practice for PropertyDrawers
        // It ensures Undo/Redo is handled correctly and GUI is enabled/disabled
        EditorGUI.BeginProperty(position, label, property);

        // Get the rect for the content after drawing the label
        Rect contentRect = EditorGUI.PrefixLabel(position, label);

        float buttonWidth = 20;
        float spacing = 5;

        // Use objectReferenceValue for checking if an object is assigned to the property
        bool hasValue = property.objectReferenceValue != null;

        // Determine which button to show based on whether the property has a value
        bool showCreateButton = !hasValue;
        bool showPreviewButton = hasValue;

        // Calculate rects for the property field and the button
        Rect propertyFieldRect = new Rect(contentRect); // Start with full content rect
        Rect buttonRect = new Rect(contentRect);

        // Adjust widths and positions if a button is present
        if (showCreateButton || showPreviewButton)
        {
            propertyFieldRect.width = contentRect.width - buttonWidth - spacing;
            buttonRect.x = propertyFieldRect.xMax + spacing;
            buttonRect.width = buttonWidth;
        }

        // Draw the property field
        // Pass GUIContent.none to PropertyField so it doesn't draw an additional label within its own rect
        EditorGUI.PropertyField(propertyFieldRect, property, GUIContent.none, true);

        // Draw buttons conditionally
        if (showCreateButton)
        {
            if (GUI.Button(buttonRect, "Create"))
            {
                CreateNew(property);
            }
        }
        else if (showPreviewButton)
        {
            bool clicked = GUI.Button(buttonRect, GUIContent.none, GUIStyle.none);

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPreviewIconPath());
            if (icon)
            {
                float iconSize = buttonRect.height * PreviewIconScale;
                Rect iconRect = new Rect(
                    buttonRect.center.x - iconSize / 2f,
                    buttonRect.center.y - iconSize / 2f,
                    iconSize,
                    iconSize);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }

            if (clicked)
            {
                Blip target = (Blip)property.objectReferenceValue; // Cast the object reference
                if (target) // Add null check for safety
                {
                    if (target.GetLooping())
                    {
                        if (BlipRuntime.IsPreviewing(target))
                        {
                            BlipRuntime.Stop(target, BlipRuntime.GetPreviewGameObject(), () => { SetPreviewState("Preview"); });
                            SetPreviewState("Stopping");
                        }
                        else
                        {
                            BlipRuntime.Play(target, BlipRuntime.GetPreviewGameObject());
                            SetPreviewState("Stop");
                        }
                    }
                    else
                    {
                        BlipRuntime.Play(target, BlipRuntime.GetPreviewGameObject());
                        SetPreviewState("Preview");
                    }
                }
            }
        }

        EditorGUI.EndProperty();
    }

    // Adapt CreateNew to take SerializedProperty as an argument
    void CreateNew(SerializedProperty property)
    {
        BlipRuntime.Init();
        Blip newSound = ScriptableObject.CreateInstance<Blip>();
        // Generate a unique asset path for the new ScriptableObject
        var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + property.displayName + ".asset");

        AssetDatabase.CreateAsset(newSound, uniqueFileName);
        // Load the asset back to ensure it's properly recognized by Unity
        var assetInProject = AssetDatabase.LoadAssetAtPath<Blip>(AssetDatabase.GetAssetPath(newSound));
        Debug.Log($"Created new Blip asset: {assetInProject.name}", assetInProject);
        property.objectReferenceValue = assetInProject;
        // Apply modified properties to ensure the change is saved to the SerializedObject
        property.serializedObject.ApplyModifiedProperties();
    }

    // Method to update the state (and therefore icon) of the preview button
    void SetPreviewState(string state)
    {
        _previewState = state;
    }

    string GetPreviewIconPath()
    {
        return _previewState switch
        {
            "Preview" => PlayIconPath,
            "Stopping" => StoppingIconPath,
            _ => StopIconPath
        };
    }

    // Override GetPropertyHeight to ensure the drawer takes up a single line height
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}