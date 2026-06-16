using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ElectricalCabinetStaticSceneBuilder
{
    private const string MenuPath = "Tools/Func/Create Static Electrical Cabinet In Scene";
    private static readonly Vector3 DefaultCabinetPosition = new Vector3(10f, -87.44f, -10f);
    private static readonly Vector3 DefaultCabinetScale = new Vector3(10f, 10f, 10f);

    [MenuItem(MenuPath)]
    public static void CreateStaticElectricalCabinetInScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Exit Play Mode First",
                "Create or rebuild the static electrical cabinet before entering Play Mode.",
                "OK");
            return;
        }

        GameObject cabinet = GameObject.Find(ElectricalControlCabinetBuilder.StaticCabinetName);
        ElectricalControlCabinetBuilder builder;

        if (cabinet == null)
        {
            cabinet = new GameObject(ElectricalControlCabinetBuilder.StaticCabinetName);
            Undo.RegisterCreatedObjectUndo(cabinet, "Create Static Electrical Cabinet");
            builder = Undo.AddComponent<ElectricalControlCabinetBuilder>(cabinet);
            cabinet.transform.position = DefaultCabinetPosition;
            cabinet.transform.localScale = DefaultCabinetScale;
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(cabinet, "Rebuild Static Electrical Cabinet");
            builder = cabinet.GetComponent<ElectricalControlCabinetBuilder>();
            if (builder == null)
            {
                builder = Undo.AddComponent<ElectricalControlCabinetBuilder>(cabinet);
            }

            if (IsOldDefaultTransform(cabinet.transform))
            {
                cabinet.transform.position = DefaultCabinetPosition;
                cabinet.transform.localScale = DefaultCabinetScale;
            }
        }

        builder.cabinetWorldPosition = cabinet.transform.position;
        builder.faceWest = true;
        builder.buildOnStart = false;
        builder.addColliders = true;
        builder.addEInteraction = true;

        Quaternion originalRotation = cabinet.transform.rotation;
        Vector3 originalScale = cabinet.transform.localScale;

        builder.BuildCabinet();
        cabinet.transform.rotation = originalRotation;
        cabinet.transform.localScale = originalScale;

        cabinet.name = ElectricalControlCabinetBuilder.StaticCabinetName;
        SetStaticFlagsRecursively(cabinet, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

        EditorUtility.SetDirty(builder);
        EditorSceneManager.MarkSceneDirty(cabinet.scene);
        EditorSceneManager.SaveScene(cabinet.scene);
        Selection.activeGameObject = cabinet;

        Debug.Log($"Created static electrical cabinet scene object: {cabinet.name}");
    }

    private static void SetStaticFlagsRecursively(GameObject root, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(root, IsDynamicCabinetPart(root) ? 0 : flags);

        foreach (Transform child in root.transform)
        {
            SetStaticFlagsRecursively(child.gameObject, flags);
        }
    }

    private static bool IsDynamicCabinetPart(GameObject obj)
    {
        return obj.name == "Main_Breaker_Handle_Clickable";
    }

    private static bool IsOldDefaultTransform(Transform target)
    {
        return Vector3.Distance(target.position, new Vector3(10f, 0f, -10f)) < 0.01f
            && Vector3.Distance(target.localScale, Vector3.one) < 0.01f;
    }
}
