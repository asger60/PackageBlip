using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class BlipUtils
{
    [MenuItem("FloppyClub/Blip/Add Blip to active scene", false, 10)]
    public static void CreateBlipRuntime(MenuCommand menuCommand)
    {
        var go = new GameObject("BlipRuntime");
        go.AddComponent<BlipRuntime>();
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create BlipRuntime");
        Selection.activeObject = go;
    }

    [MenuItem("Assets/Create/Blip from selected clips", false)]
    public static void CreateAnysoundFromSelectedClips()
    {
        var selectedAudioClips = Selection.objects.OfType<AudioClip>().ToArray();
        if (selectedAudioClips.Length == 0) return;

        if (selectedAudioClips.Length > 1)
        {
            int option = EditorUtility.DisplayDialogComplex("Create Blip from selected clips",
                $"You have selected {selectedAudioClips.Length} audio clips. How would you like to create Blip objects?",
                "Individual Blips", "Cancel", "Single Blip (all clips)");

            switch (option)
            {
                case 0: // Individual
                    CreateIndividualAnysounds(selectedAudioClips);
                    break;
                case 1: // Cancel
                    return;
                case 2: // Single
                    CreateSingleAnysound(selectedAudioClips);
                    break;
            }
        }
        else
        {
            CreateSingleAnysound(selectedAudioClips);
        }
    }

    private static void CreateSingleAnysound(AudioClip[] audioClips)
    {
        var anysound = ScriptableObject.CreateInstance<Blip>();
        anysound.AudioClips = audioClips;

        string path = AssetDatabase.GetAssetPath(audioClips[0]);
        string directory = Path.GetDirectoryName(path);
        string fileName = audioClips.Length == 1 ? audioClips[0].name : "New Blip";
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}.asset");

        AssetDatabase.CreateAsset(anysound, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = anysound;
    }

    private static void CreateIndividualAnysounds(AudioClip[] audioClips)
    {
        Object lastAnysound = null;
        foreach (var clip in audioClips)
        {
            var anysound = ScriptableObject.CreateInstance<Blip>();
            anysound.AudioClips = new[] { clip };

            string path = AssetDatabase.GetAssetPath(clip);
            string directory = Path.GetDirectoryName(path);
            string fileName = clip.name;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}.asset");

            AssetDatabase.CreateAsset(anysound, assetPath);
            lastAnysound = anysound;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        if (lastAnysound != null)
        {
            Selection.activeObject = lastAnysound;
        }
    }

    [MenuItem("Assets/Create/Blip from selected clips", true)]
    public static bool CreateAnysoundFromSelectedClipsValidate()
    {
        return Selection.objects.OfType<AudioClip>().Any();
    }
}