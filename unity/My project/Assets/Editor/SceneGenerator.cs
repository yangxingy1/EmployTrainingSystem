#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Unity Editor 工具：一键生成化工管道培训场景并设为启动场景。
/// 使用方法: Unity 菜单 → Training Platform → Generate Pipeline Scene
/// </summary>
public class SceneGenerator
{
    const string SCENES_PATH = "Assets/Scenes/";
    const string SCENE_NAME  = "PipelineScene";

    [MenuItem("Training Platform/Generate Pipeline Scene")]
    static void GeneratePipelineScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // 创建场景
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 挂 PipelineSceneBootstrap
        var go = new GameObject("PipelineSceneBootstrap");
        go.AddComponent(System.Type.GetType("PipelineSceneBootstrap,Assembly-CSharp"));

        // 保存
        string path = SCENES_PATH + SCENE_NAME + ".unity";
        EditorSceneManager.SaveScene(scene, path);

        // 设为 Build Settings 唯一场景
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(path, true)
        };
        EditorBuildSettings.scenes = buildScenes.ToArray();

        // 打开
        EditorSceneManager.OpenScene(path);
        Debug.Log("[SceneGenerator] PipelineScene 已创建并设为启动场景。按 Play 直接进入。");
    }
}
#endif
