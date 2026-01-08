using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;
// to convert all animations in an animator controller from "generic" to "humanoid"
public class AnimatorToHumanoid
{
    [MenuItem("Tools/Animator/Convert Animator Animations to Humanoid")]
    static void ConvertAnimatorAnimations()
    {
        AnimatorController controller =
            Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("Please select an Animator Controller.");
            return;
        }

        HashSet<string> processedPaths = new HashSet<string>();

        foreach (var clip in controller.animationClips)
        {
            if (clip == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(clip);

            // Animation clips inside FBX files have paths like:
            // Assets/Models/character.fbx
            if (!assetPath.EndsWith(".fbx"))
                continue;

            if (processedPaths.Contains(assetPath))
                continue;

            processedPaths.Add(assetPath);

            ModelImporter importer =
                AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (importer == null)
                continue;

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"Converted to Humanoid: {assetPath}");
            }
        }

        Debug.Log("Animator animation conversion complete.");
    }
}
