using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FireExtinguisherStaticSceneBuilder
{
    private const string MenuPath = "Tools/Func/Create Static Fire Extinguisher In Scene";
    private static readonly Vector3 DefaultExtinguisherPosition = new Vector3(12f, -87.44f, -10f);
    private static readonly Vector3 DefaultExtinguisherScale = new Vector3(10f, 10f, 10f);

    [MenuItem(MenuPath)]
    public static void CreateStaticFireExtinguisherInScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Exit Play Mode First",
                "Create or rebuild the static fire extinguisher before entering Play Mode.",
                "OK");
            return;
        }

        GameObject extinguisher = GameObject.Find(FireExtinguisherBuilder.StaticExtinguisherName);
        FireExtinguisherBuilder builder;

        if (extinguisher == null)
        {
            extinguisher = new GameObject(FireExtinguisherBuilder.StaticExtinguisherName);
            Undo.RegisterCreatedObjectUndo(extinguisher, "Create Static Fire Extinguisher");
            builder = Undo.AddComponent<FireExtinguisherBuilder>(extinguisher);
            extinguisher.transform.position = DefaultExtinguisherPosition;
            extinguisher.transform.localScale = DefaultExtinguisherScale;
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(extinguisher, "Rebuild Static Fire Extinguisher");
            builder = extinguisher.GetComponent<FireExtinguisherBuilder>();
            if (builder == null)
            {
                builder = Undo.AddComponent<FireExtinguisherBuilder>(extinguisher);
            }

            if (IsOldDefaultTransform(extinguisher.transform))
            {
                extinguisher.transform.position = DefaultExtinguisherPosition;
                extinguisher.transform.localScale = DefaultExtinguisherScale;
            }
        }

        builder.extinguisherWorldPosition = extinguisher.transform.position;
        builder.faceNorth = true;
        builder.buildOnStart = false;
        builder.addColliders = true;

        Quaternion originalRotation = extinguisher.transform.rotation;
        Vector3 originalScale = extinguisher.transform.localScale;

        builder.BuildExtinguisher();
        extinguisher.transform.rotation = originalRotation;
        extinguisher.transform.localScale = originalScale;

        extinguisher.name = FireExtinguisherBuilder.StaticExtinguisherName;
        SetStaticFlagsRecursively(extinguisher, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

        EditorUtility.SetDirty(builder);
        EditorSceneManager.MarkSceneDirty(extinguisher.scene);
        EditorSceneManager.SaveScene(extinguisher.scene);
        Selection.activeGameObject = extinguisher;

        Debug.Log($"Created static fire extinguisher scene object: {extinguisher.name}");
    }

    private static void SetStaticFlagsRecursively(GameObject root, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(root, IsDynamicExtinguisherPart(root) ? 0 : flags);

        foreach (Transform child in root.transform)
        {
            SetStaticFlagsRecursively(child.gameObject, flags);
        }
    }

    private static bool IsDynamicExtinguisherPart(GameObject obj)
    {
        return obj.name == "Pressure_Gauge_Metal_Ring"
            || obj.name == "Pressure_Gauge_White_Face"
            || obj.name == "Pressure_Gauge_Green_Zone"
            || obj.name == "Pressure_Gauge_Needle_Pivot"
            || obj.name == "Pressure_Gauge_Needle";
    }

    private static bool IsOldDefaultTransform(Transform target)
    {
        return Vector3.Distance(target.position, new Vector3(12f, 0f, -10f)) < 0.01f
            && Vector3.Distance(target.localScale, Vector3.one) < 0.01f;
    }
}
