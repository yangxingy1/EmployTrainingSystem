using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FactoryOneStaticSceneBuilder
{
    private const string MenuPath = "Tools/Factory/Create Static Factory 1 In Scene";
    private const string FactoryControllerName = "Factory1Controller";
    private const float DesiredFactorySize = 1000f;

    [MenuItem(MenuPath)]
    public static void CreateStaticFactoryInScene()
    {
        AssetDatabase.ImportAsset(FactoryOneSceneController.DefaultFactoryAssetPath, ImportAssetOptions.ForceUpdate);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FactoryOneSceneController.DefaultFactoryAssetPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "Factory FBX not found",
                $"Could not load {FactoryOneSceneController.DefaultFactoryAssetPath}. Check that the FBX exists under Assets/Factory.",
                "OK");
            return;
        }

        GameObject factory = GameObject.Find(FactoryOneSceneController.StaticFactoryName);
        if (factory == null)
        {
            Object prefabInstance = PrefabUtility.InstantiatePrefab(prefab);
            factory = prefabInstance as GameObject;
            if (factory == null)
            {
                EditorUtility.DisplayDialog("Factory creation failed", "Unity could not instantiate the FBX prefab into the open scene.", "OK");
                return;
            }

            Undo.RegisterCreatedObjectUndo(factory, "Create Static Factory");
            factory.name = FactoryOneSceneController.StaticFactoryName;
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(factory, "Update Static Factory");
        }

        factory.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        factory.transform.localScale = Vector3.one;
        ScaleAndCenter(factory, DesiredFactorySize, Vector3.zero);
        GameObjectUtility.SetStaticEditorFlags(factory, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);

        FactoryOneSceneController controller = CreateOrUpdateController(factory);

        Selection.objects = new Object[] { factory, controller.gameObject };
        EditorSceneManager.MarkSceneDirty(factory.scene);
        EditorSceneManager.SaveScene(factory.scene);

        Debug.Log($"Created static factory scene objects: {factory.name}, {controller.gameObject.name}");
    }

    private static FactoryOneSceneController CreateOrUpdateController(GameObject factory)
    {
        GameObject controllerObject = GameObject.Find(FactoryControllerName);
        if (controllerObject == null)
        {
            controllerObject = new GameObject(FactoryControllerName);
            Undo.RegisterCreatedObjectUndo(controllerObject, "Create Factory Controller");
        }

        FactoryOneSceneController controller = controllerObject.GetComponent<FactoryOneSceneController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<FactoryOneSceneController>(controllerObject);
        }

        controller.sceneFactoryRoot = factory;
        controller.spawnFactoryAtRuntime = false;
        controller.desiredFactorySize = DesiredFactorySize;
        controller.factoryCenter = Vector3.zero;
        controller.eyeHeight = 1.7f;
        controller.cameraHeightOffset = 0f;
        controller.useFixedStartCameraWorldHeight = false;
        controller.forceKnownGoodStartCameraWorldHeight = false;
        controller.startCameraWorldHeight = FactoryOneSceneController.KnownGoodStartCameraWorldHeight;
        controller.startGroundClearance = 0.15f;
        controller.verticalMoveSpeed = 1f;
        controller.heightLogInterval = 0.25f;
        controller.printHeightKey = KeyCode.H;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void ScaleAndCenter(GameObject root, float desiredSize, Vector3 targetCenter)
    {
        if (!TryGetRenderBounds(root, out Bounds bounds))
        {
            Debug.LogWarning("Factory FBX has no renderers. It was instantiated but could not be scaled automatically.");
            return;
        }

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxSize > 0.001f)
        {
            root.transform.localScale = Vector3.one * (desiredSize / maxSize);
        }

        if (TryGetRenderBounds(root, out Bounds scaledBounds))
        {
            root.transform.position += targetCenter - scaledBounds.center;
        }
    }

    private static bool TryGetRenderBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = default;

        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }
}
