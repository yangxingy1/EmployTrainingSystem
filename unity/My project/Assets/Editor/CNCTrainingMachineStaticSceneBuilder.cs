using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CNCTrainingMachineStaticSceneBuilder
{
    private const string MenuPath = "Tools/Func/Create Static CNC Training Machine In Scene";
    private static readonly Vector3 DefaultMachinePosition = new Vector3(16f, -87.44f, -10f);
    private static readonly Vector3 DefaultMachineScale = new Vector3(10f, 10f, 10f);

    [MenuItem(MenuPath)]
    public static void CreateStaticCNCTrainingMachineInScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Exit Play Mode First",
                "Create or rebuild the static CNC training machine before entering Play Mode.",
                "OK");
            return;
        }

        GameObject machine = GameObject.Find(CNCTrainingMachineBuilder.StaticMachineName);
        CNCTrainingMachineBuilder builder;

        if (machine == null)
        {
            machine = new GameObject(CNCTrainingMachineBuilder.StaticMachineName);
            Undo.RegisterCreatedObjectUndo(machine, "Create Static CNC Training Machine");
            builder = Undo.AddComponent<CNCTrainingMachineBuilder>(machine);
            machine.transform.position = DefaultMachinePosition;
            machine.transform.localScale = DefaultMachineScale;
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(machine, "Rebuild Static CNC Training Machine");
            builder = machine.GetComponent<CNCTrainingMachineBuilder>();
            if (builder == null)
            {
                builder = Undo.AddComponent<CNCTrainingMachineBuilder>(machine);
            }

            if (IsOldDefaultTransform(machine.transform))
            {
                machine.transform.position = DefaultMachinePosition;
                machine.transform.localScale = DefaultMachineScale;
            }
        }

        builder.machineWorldPosition = machine.transform.position;
        builder.faceSouth = true;
        builder.buildOnStart = false;
        builder.addColliders = true;
        builder.powerKey = KeyCode.Alpha1;
        builder.doorKey = KeyCode.Alpha2;
        builder.clampKey = KeyCode.Alpha3;
        builder.cycleStartKey = KeyCode.Alpha4;
        builder.emergencyStopKey = KeyCode.Alpha5;
        builder.resetKey = KeyCode.Alpha6;
        builder.modeKey = KeyCode.Alpha7;

        Quaternion originalRotation = machine.transform.rotation;
        Vector3 originalScale = machine.transform.localScale;

        builder.BuildMachine();
        machine.transform.rotation = originalRotation;
        machine.transform.localScale = originalScale;

        machine.name = CNCTrainingMachineBuilder.StaticMachineName;
        SetStaticFlagsRecursively(machine, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

        EditorUtility.SetDirty(builder);
        EditorSceneManager.MarkSceneDirty(machine.scene);
        EditorSceneManager.SaveScene(machine.scene);
        Selection.activeGameObject = machine;

        Debug.Log($"Created static CNC training machine scene object: {machine.name}");
    }

    private static void SetStaticFlagsRecursively(GameObject root, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(root, IsDynamicMachinePart(root) ? 0 : flags);

        foreach (Transform child in root.transform)
        {
            SetStaticFlagsRecursively(child.gameObject, flags);
        }
    }

    private static bool IsDynamicMachinePart(GameObject obj)
    {
        return obj.name.StartsWith("Safety_Door_")
            || obj.name.StartsWith("Clamp_Jaw_")
            || obj.name.StartsWith("Clamp_Handle_")
            || obj.name.StartsWith("Main_Power_Switch_")
            || obj.name == "Mode_Select_Knob_Clickable"
            || obj.name == "Spindle_Rotator"
            || obj.name == "Spindle_Tool"
            || obj.name.StartsWith("CNC_Status_")
            || obj.name == "Cycle_Start_Button_Clickable"
            || obj.name == "Emergency_Stop_Button_Clickable"
            || obj.name == "Reset_Button_Clickable"
            || obj.name.StartsWith("Stack_Light_");
    }

    private static bool IsOldDefaultTransform(Transform target)
    {
        bool atSourceScriptDefault = Vector3.Distance(target.position, new Vector3(0f, 0f, -6f)) < 0.01f
            && Vector3.Distance(target.localScale, Vector3.one) < 0.01f;

        bool atOriginDefault = Vector3.Distance(target.position, Vector3.zero) < 0.01f
            && Vector3.Distance(target.localScale, Vector3.one) < 0.01f;

        return atSourceScriptDefault || atOriginDefault;
    }
}
