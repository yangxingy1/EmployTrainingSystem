using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FuncStaticSceneMigrationAutoRun
{
    private const string MigrationKey = "EmployTraining.FuncStatics.train1.v2";

    static FuncStaticSceneMigrationAutoRun()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    private static void RunIfNeeded()
    {
        if (EditorPrefs.GetBool(MigrationKey, false))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        FuncStaticSceneMigrationBatch.RegenerateTrain1FuncStatics();
        EditorPrefs.SetBool(MigrationKey, true);
        Debug.Log("[FuncStatics] Regenerated CNC + Breaker static stations in train1.unity (v2 CNC visual refresh).");
    }
}
