using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class FactoryStaticSceneBuilder
{
    private const string FactoryAssetPath = "Assets/Factory/LimeExp_2.FBX";
    private const string StaticFactoryName = "Factory Static";
    private const string ControllerName = "FactoryController";
    private const float DesiredFactorySize = 1000f;
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Factory/Create Static Factory In Scene")]
    public static void CreateStaticFactoryInScene()
    {
        CreateOrUpdateStaticFactoryInCurrentScene(saveScene: true);
    }

    public static void CreateStaticFactoryInSampleScene()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        CreateOrUpdateStaticFactoryInCurrentScene(saveScene: false);
        EditorSceneManager.SaveScene(scene);
    }

    private static void CreateOrUpdateStaticFactoryInCurrentScene(bool saveScene)
    {
        var staticFactory = GameObject.Find(StaticFactoryName);
        if (staticFactory == null)
        {
            staticFactory = CreateStaticFactoryInstance();
        }

        NormalizeFactory(staticFactory.transform, DesiredFactorySize, Vector3.zero);
        AssignFactoryController(staticFactory.transform);

        EditorUtility.SetDirty(staticFactory);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        if (saveScene)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        Selection.activeGameObject = staticFactory;
        Debug.Log($"Static factory ready: {staticFactory.name}", staticFactory);
    }

    private static GameObject CreateStaticFactoryInstance()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FactoryAssetPath);
        if (prefab == null)
        {
            throw new System.InvalidOperationException($"Factory FBX not found at {FactoryAssetPath}");
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            throw new System.InvalidOperationException($"Could not instantiate {FactoryAssetPath}");
        }

        Undo.RegisterCreatedObjectUndo(instance, "Create static factory");
        instance.name = StaticFactoryName;
        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        instance.transform.localScale = Vector3.one;
        return instance;
    }

    private static void NormalizeFactory(Transform factoryRoot, float desiredSize, Vector3 targetCenter)
    {
        var renderers = factoryRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        factoryRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        factoryRoot.localScale = Vector3.one;

        var bounds = CalculateBounds(renderers);
        var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDimension > 0.01f)
        {
            factoryRoot.localScale = Vector3.one * (desiredSize / maxDimension);
        }

        bounds = CalculateBounds(renderers);
        factoryRoot.position -= bounds.center - targetCenter;
    }

    private static Bounds CalculateBounds(Renderer[] renderers)
    {
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void AssignFactoryController(Transform staticFactory)
    {
        var controller = Object.FindObjectOfType<FactoryKeyboardSceneController>();
        if (controller == null)
        {
            var controllerObject = GameObject.Find(ControllerName);
            if (controllerObject == null)
            {
                controllerObject = new GameObject(ControllerName);
                Undo.RegisterCreatedObjectUndo(controllerObject, "Create factory controller");
            }

            controller = controllerObject.AddComponent<FactoryKeyboardSceneController>();
        }

        var serializedController = new SerializedObject(controller);
        serializedController.FindProperty("sceneFactoryRoot").objectReferenceValue = staticFactory;
        serializedController.FindProperty("spawnFactoryAtRuntime").boolValue = false;
        serializedController.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);
    }
}
