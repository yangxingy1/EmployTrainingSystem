using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BreakerShutdownStationStaticSceneBuilder
{
    private const string MenuPath = "Tools/Func/Create Static Breaker Shutdown Station In Scene";
    private static readonly Vector3 DefaultStationPosition = new Vector3(14f, -87.44f, -10f);
    private static readonly Vector3 DefaultStationScale = new Vector3(10f, 10f, 10f);
    private static readonly Vector3 GeneratedRootLocalPosition = new Vector3(0f, 0f, 9.35f);

    [MenuItem(MenuPath)]
    public static void CreateStaticBreakerShutdownStationInScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Exit Play Mode First",
                "Create or rebuild the static breaker shutdown station before entering Play Mode.",
                "OK");
            return;
        }

        GameObject station = GameObject.Find(BreakerShutdownStationBuilder.StaticStationName);
        BreakerShutdownStationBuilder builder;

        if (station == null)
        {
            station = new GameObject(BreakerShutdownStationBuilder.StaticStationName);
            Undo.RegisterCreatedObjectUndo(station, "Create Static Breaker Shutdown Station");
            builder = Undo.AddComponent<BreakerShutdownStationBuilder>(station);
            station.transform.position = DefaultStationPosition;
            station.transform.localScale = DefaultStationScale;
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(station, "Rebuild Static Breaker Shutdown Station");
            builder = station.GetComponent<BreakerShutdownStationBuilder>();
            if (builder == null)
            {
                builder = Undo.AddComponent<BreakerShutdownStationBuilder>(station);
            }

            if (IsOldDefaultTransform(station.transform))
            {
                station.transform.position = DefaultStationPosition;
                station.transform.localScale = DefaultStationScale;
            }
        }

        builder.stationWorldPosition = station.transform.position;
        builder.buildOnStart = false;
        builder.addColliders = true;
        builder.autoStartKey = KeyCode.J;

        Quaternion originalRotation = station.transform.rotation;
        Vector3 originalScale = station.transform.localScale;

        builder.BuildStation();
        ApplyGeneratedRootOffset(station);
        station.transform.rotation = originalRotation;
        station.transform.localScale = originalScale;

        station.name = BreakerShutdownStationBuilder.StaticStationName;
        SetStaticFlagsRecursively(station, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

        EditorUtility.SetDirty(builder);
        EditorSceneManager.MarkSceneDirty(station.scene);
        EditorSceneManager.SaveScene(station.scene);
        Selection.activeGameObject = station;

        Debug.Log($"Created static breaker shutdown station scene object: {station.name}");
    }

    private static void SetStaticFlagsRecursively(GameObject root, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(root, IsDynamicStationPart(root) ? 0 : flags);

        foreach (Transform child in root.transform)
        {
            SetStaticFlagsRecursively(child.gameObject, flags);
        }
    }

    private static bool IsDynamicStationPart(GameObject obj)
    {
        return obj.name == "Red_Rotating_Beacon_Pivot"
            || obj.name == "Beacon_Status_Lens"
            || obj.name == "Beacon_Red_Point_Light"
            || obj.name == "Voltage_Gauge_Needle_Pivot"
            || obj.name == "Voltage_Gauge_Needle"
            || obj.name == "Auto_Start_Button_Cap_Clickable"
            || obj.name == "Confirm_Button_Cap_Clickable"
            || obj.name.EndsWith("_Lever_Pivot")
            || obj.name.EndsWith("_Lever_Clickable")
            || obj.name.EndsWith("_Lever_Rod_Left")
            || obj.name.EndsWith("_Lever_Rod_Right")
            || obj.name.EndsWith("_Lever_End_Left")
            || obj.name.EndsWith("_Lever_End_Right")
            || obj.name.EndsWith("_Red_Handle")
            || obj.name.EndsWith("_Red_Grip_Tab");
    }

    private static bool IsOldDefaultTransform(Transform target)
    {
        bool atOriginalScriptDefault = Vector3.Distance(target.position, Vector3.zero) < 0.01f
            && Vector3.Distance(target.localScale, Vector3.one) < 0.01f;

        bool atPreviousEditorDefault = Vector3.Distance(target.position, new Vector3(14f, -87.44f, 87f)) < 0.01f
            && Vector3.Distance(target.localScale, DefaultStationScale) < 0.01f;

        return atOriginalScriptDefault || atPreviousEditorDefault;
    }

    private static void ApplyGeneratedRootOffset(GameObject station)
    {
        Transform generatedRoot = station.transform.Find(BreakerShutdownStationBuilder.GeneratedModelName);
        if (generatedRoot != null)
        {
            generatedRoot.localPosition = GeneratedRootLocalPosition;
        }
    }
}
