using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BlipIconSetup
{
    private const string IconPath = "Packages/com.floppyclub.blip/Editor/PNG/BlipIcon.png";

    static BlipIconSetup()
    {
        SetIcon();
    }

    [MenuItem("Tools/Blip/Set Blip Icon")]
    private static void SetIcon()
    {
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon == null) return;

        var scriptPath = AssetDatabase.GUIDToAssetPath(FindBlipScriptGuid());
        if (string.IsNullOrEmpty(scriptPath)) return;

        if (!(AssetImporter.GetAtPath(scriptPath) is MonoImporter importer)) return;
        if (importer.GetIcon() == icon) return;

        importer.SetIcon(icon);
        importer.SaveAndReimport();
    }

    private static string FindBlipScriptGuid()
    {
        foreach (var guid in AssetDatabase.FindAssets("Blip t:MonoScript"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() == typeof(Blip)) return guid;
        }

        return null;
    }
}
