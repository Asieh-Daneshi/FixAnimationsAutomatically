using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editor Script to automatically fix FBX import settings
///
/// HOW TO USE:
/// 1. Copy this file to: Assets/Editor/FixFBXImportSettings.cs
/// 2. In Unity menu: Tools > Fix FBX Import Settings
/// 3. Select the folder containing your FBX files
/// 4. Script will automatically configure all FBX files
/// </summary>
public class FixFBXImportSettings : EditorWindow
{
    private string folderPath = "Assets/Animations/Gestures";

    [MenuItem("Tools/Fix FBX Import Settings")]
    static void Init()
    {
        FixFBXImportSettings window = (FixFBXImportSettings)EditorWindow.GetWindow(typeof(FixFBXImportSettings));
        window.titleContent = new GUIContent("Fix FBX Imports");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Fix FBX Import Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will configure all FBX files to prevent avatar rotation:");
        GUILayout.Label("- Bake Root Transform Rotation");
        GUILayout.Label("- Bake Root Transform Position (Y)");
        GUILayout.Label("- Bake Root Transform Position (XZ)");
        GUILayout.Label("- Enable Loop Time");
        GUILayout.Space(10);

        folderPath = EditorGUILayout.TextField("FBX Folder Path:", folderPath);
        GUILayout.Space(10);

        if (GUILayout.Button("Fix All FBX Files in Folder", GUILayout.Height(40)))
        {
            FixAllFBXInFolder();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Fix Selected FBX Files", GUILayout.Height(40)))
        {
            FixSelectedFBX();
        }
    }

    void FixAllFBXInFolder()
    {
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Folder does not exist: " + folderPath, "OK");
            return;
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);

        if (fbxFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No FBX Files", "No FBX files found in: " + folderPath, "OK");
            return;
        }

        int fixedCount = 0;
        foreach (string fbxPath in fbxFiles)
        {
            if (FixFBXImport(fbxPath))
            {
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Fixed {fixedCount} out of {fbxFiles.Length} FBX files!\n\nAll animations should now work correctly without rotation.",
            "OK");
    }

    void FixSelectedFBX()
    {
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more FBX files in the Project window.", "OK");
            return;
        }

        int fixedCount = 0;
        foreach (Object obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (path.EndsWith(".fbx") || path.EndsWith(".FBX"))
            {
                if (FixFBXImport(path))
                {
                    fixedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Fixed {fixedCount} FBX file(s)!\n\nAnimations should now work without rotation.",
            "OK");
    }

    // bool FixFBXImport(string assetPath)
    // {
        // ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

        // if (importer == null)
        // {
            // Debug.LogWarning($"Could not get ModelImporter for: {assetPath}");
            // return false;
        // }

        // Debug.Log($"Fixing import settings for: {Path.GetFileName(assetPath)}");

        // // Get the first animation clip (there should only be one per FBX)
        // ModelImporterClipAnimation[] clipAnimations = importer.defaultClipAnimations;

        // if (clipAnimations.Length == 0)
        // {
            // Debug.LogWarning($"No animation clips found in: {assetPath}");
            // return false;
        // }

        // // Fix settings for each clip
		// // Fix settings for each clip
		// // for (int i = 0; i < clipAnimations.Length; i++)
		// // {
			// // ModelImporterClipAnimation clip = clipAnimations[i];

			// // // Looping?
			// // clip.loopTime = false;

			// // // --- ROOT TRANSFORM ROTATION ---
			// // clip.lockRootRotation = false;          // Do NOT bake rotation
			// // clip.keepOriginalOrientation = true;    // Keep original rotation

			// // // --- ROOT TRANSFORM POSITION Y (Feet, At Start) ---
			// // clip.lockRootHeightY = false;           // Allow Unity to compute Y
			// // clip.heightFromFeet = true;             // Use feet instead of center
			// // clip.keepOriginalPositionY = false;     // Do NOT use original Y

			// // // --- ROOT TRANSFORM POSITION XZ (Original) ---
			// // clip.lockRootPositionXZ = false;        // Do NOT bake XZ
			// // clip.keepOriginalPositionXZ = true;     // Use original clip XZ

			// // clipAnimations[i] = clip;
		// // }
        // for (int i = 0; i < clipAnimations.Length; i++)
        // {
            // ModelImporterClipAnimation clip = clipAnimations[i];

            // // Enable looping
            // clip.loopTime = false;

            // // Bake root transform rotation into pose
            // clip.lockRootRotation = true;
            // clip.keepOriginalOrientation = true;

            // // Bake root transform position into pose
            // clip.lockRootHeightY = true;
            // clip.lockRootPositionXZ = true;

            // // Keep original values
            // clip.keepOriginalPositionY = true;
            // clip.keepOriginalPositionXZ = true;

            // clipAnimations[i] = clip;
        // // }

        // // Apply the changes
        // importer.clipAnimations = clipAnimations;

        // // Reimport the asset
        // AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        // return true;
    // }
	
	
	bool FixFBXImport(string assetPath)
{
    ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

    if (importer == null)
        return false;

    // Ensure consistent scale
    importer.globalScale = 1.0f;

    // MAKE SURE it's Humanoid (generic can't fix stretching cleanly)
    importer.animationType = ModelImporterAnimationType.Human;

    var clips = importer.defaultClipAnimations;

    for (int i = 0; i < clips.Length; i++)
    {
        var clip = clips[i];

        clip.loopTime = false;

        // --- ROOT ROTATION (no drifting, no leaning) ---
        clip.lockRootRotation = true;        // Bake into pose
        clip.keepOriginalOrientation = false;
        clip.rotationOffset = 0f;            // Ensure upright

        // --- ROOT Y POSITION (Feet, At Start) ---
        clip.lockRootHeightY = true;         // Bake Y into pose
        clip.heightFromFeet = true;
        clip.keepOriginalPositionY = false;

        // --- ROOT XZ POSITION (NO SLIDING) ---
        clip.lockRootPositionXZ = true;      // Bake XZ into pose → no sliding
        clip.keepOriginalPositionXZ = false;

        clips[i] = clip;
    }

    importer.clipAnimations = clips;

    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    return true;
}
}
