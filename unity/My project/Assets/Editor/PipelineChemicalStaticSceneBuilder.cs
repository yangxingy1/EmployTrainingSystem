using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 编辑器工具：一键生成化工厂管道培训静态场景。
/// 支持 Prefab 优先模式：如果存在已保存的 Prefab，优先使用 Prefab 而非重新生成代码模型。
/// </summary>
public static class PipelineChemicalStaticSceneBuilder
{
    private const string StaticRootName = PipelineChemicalSceneBuilder.StaticSceneName;
    private const string GeneratedModelName = PipelineChemicalSceneBuilder.SceneRootName;

    /// <summary>手动修改后保存的模型 Prefab 路径</summary>
    private const string ModelPrefabPath = "Assets/Prefabs/ChemicalPipelineModel.prefab";

    private static readonly Vector3 DefaultScenePosition = new Vector3(0f, 0f, 0f);
    private static readonly Vector3 DefaultSceneScale = new Vector3(1f, 1f, 1f);

    // ══════════════════════════════════════════════════════════════
    //  MenuItem：创建场景（Prefab 优先）
    // ══════════════════════════════════════════════════════════════

    [MenuItem("Tools/Pipeline/Create Static Chemical Pipeline Scene In Current Scene")]
    public static void CreateStaticChemicalPipelineInScene()
    {
        CreateOrRebuildStaticPipeline();
    }

    [MenuItem("Tools/Pipeline/Create Static Chemical Pipeline (in SampleScene)")]
    public static void CreateStaticChemicalPipelineInSampleScene()
    {
        string samplePath = "Assets/Scenes/SampleScene.unity";
        Scene scene = EditorSceneManager.OpenScene(samplePath, OpenSceneMode.Single);
        CreateOrRebuildStaticPipeline();
        EditorSceneManager.SaveScene(scene, samplePath);
        Debug.Log("[PipelineBuilder] 已在 SampleScene 中创建化工厂管道场景并保存。");
    }

    // ══════════════════════════════════════════════════════════════
    //  MenuItem：保存 / 恢复 Prefab
    // ══════════════════════════════════════════════════════════════

    [MenuItem("Tools/Pipeline/Save Current Model as Prefab")]
    public static void SaveModelAsPrefab()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject staticRoot = FindRootInScene(scene);

        if (staticRoot == null)
        {
            Debug.LogError("[PipelineBuilder] 场景中未找到 '" + StaticRootName + "'——请先生成模型。");
            return;
        }

        Transform modelRoot = staticRoot.transform.Find(GeneratedModelName);
        if (modelRoot == null)
        {
            Debug.LogError("[PipelineBuilder] 未找到生成的模型 '" + GeneratedModelName + "'。");
            return;
        }

        // 确保目录存在
        EnsureDirectoryExists("Assets/Prefabs");
        EnsureDirectoryExists("Assets/Materials/Pipeline");

        // 如果已存在 Prefab，先询问
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath);
        if (existingPrefab != null)
        {
            if (!EditorUtility.DisplayDialog("覆盖确认",
                "已存在模型 Prefab：\n" + ModelPrefabPath + "\n\n是否覆盖？\n（覆盖后旧 Prefab 将被替换，当前场景中的模型不受影响）",
                "覆盖", "取消"))
                return;

            // 断开原有 Prefab 连接（如果有），防止覆盖后自动更新场景中的实例
            PrefabUtility.UnpackPrefabInstance(modelRoot.gameObject,
                PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        // ★ 关键步骤：先把所有运行时材质保存为独立 .mat 文件，
        //    避免 Prefab 子资产材质丢失 Shader 引用导致品红色。
        SaveModelMaterialsAsAssets(modelRoot.gameObject);

        // 保存为 Prefab
        PrefabUtility.SaveAsPrefabAsset(modelRoot.gameObject, ModelPrefabPath);
        AssetDatabase.SaveAssets();

        Debug.Log("[PipelineBuilder] 模型已保存为 Prefab：\n" +
            "  路径: " + ModelPrefabPath + "\n" +
            "  之后执行 Tools > Pipeline > Create Static... 将优先使用此 Prefab。");
    }

    [MenuItem("Tools/Pipeline/Regenerate Model from Code (Discard Prefab)")]
    public static void RegenerateFromCode()
    {
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath);

        if (existingPrefab != null)
        {
            if (!EditorUtility.DisplayDialog("确认重新生成",
                "这将从代码重新生成模型，并覆盖现有的 Prefab：\n" + ModelPrefabPath + "\n\n你在 Unity 中的手动修改将丢失。\n确定要继续吗？",
                "重新生成", "取消"))
                return;
        }

        // 强制从代码重建（先删除 Prefab，让重建逻辑走代码路径）
        if (existingPrefab != null)
        {
            AssetDatabase.DeleteAsset(ModelPrefabPath);
            AssetDatabase.SaveAssets();
        }

        CreateOrRebuildStaticPipeline();

        // 重建后自动保存为新 Prefab
        Scene scene = SceneManager.GetActiveScene();
        GameObject staticRoot = FindRootInScene(scene);
        if (staticRoot != null)
        {
            Transform modelRoot = staticRoot.transform.Find(GeneratedModelName);
            if (modelRoot != null)
            {
                EnsureDirectoryExists("Assets/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(modelRoot.gameObject, ModelPrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[PipelineBuilder] 已重新生成并保存 Prefab。");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  MenuItem：修复已有弧段（烘焙 mesh 旋转 + 修复材质）
    // ══════════════════════════════════════════════════════════════

    [MenuItem("Tools/Pipeline/Fix Gauge Arc Meshes & Materials")]
    public static void FixGaugeArcMeshesAndMaterials()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject staticRoot = FindRootInScene(scene);

        if (staticRoot == null)
        {
            Debug.LogError("[PipelineFix] 场景中未找到 '" + StaticRootName + "'。");
            return;
        }

        Transform modelRoot = staticRoot.transform.Find(GeneratedModelName);
        if (modelRoot == null)
        {
            Debug.LogError("[PipelineFix] 未找到生成的模型 '" + GeneratedModelName + "'。");
            return;
        }

        Color green = new Color(0.18f, 0.82f, 0.30f);
        Color red   = new Color(0.90f, 0.15f, 0.10f);
        int meshFixed = 0;
        int matFixed = 0;
        string matDir = "Assets/Materials/Pipeline";

        foreach (Transform t in modelRoot.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (!n.Contains("GreenArc") && !n.Contains("RedArc")) continue;

            Undo.RecordObject(t.gameObject, "Fix Gauge Arc");

            // ── 1. 材质修复 ──────────────────────────────────────
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material mat = mr.sharedMaterial;
                // 用 GetAssetPath 判断是否已是持久化资产（比 Contains 可靠）
                string existingAssetPath = (mat != null) ? AssetDatabase.GetAssetPath(mat) : null;
                bool matIsAsset = !string.IsNullOrEmpty(existingAssetPath);

                if (mat == null)
                {
                    mat = new Material(Shader.Find("Standard"));
                    matIsAsset = false;
                }

                Color targetColor = n.Contains("GreenArc") ? green : red;

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_BaseColor", targetColor);
                mat.SetColor("_Color", targetColor);
                mat.SetColor("_EmissionColor", targetColor * 0.6f);
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Glossiness", 0.55f);

                if (matIsAsset)
                {
                    // 材质已是独立资产 → 原地更新
                    EditorUtility.SetDirty(mat);
                }
                else
                {
                    // 材质是运行时创建的 → 保存为新 .mat 文件
                    EnsureDirectoryExists(matDir);
                    string matPath = matDir + "/" + SanitizeMaterialName(t.name) + ".mat";

                    // 路径已存在 → 把属性拷到已有资产（避免 CreateAsset 名字冲突）
                    Material existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (existing != null)
                    {
                        existing.CopyPropertiesFromMaterial(mat);
                        EditorUtility.SetDirty(existing);
                        mat = existing;
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(mat, matPath);
                    }
                }

                mr.sharedMaterial = mat;
                EditorUtility.SetDirty(mr);
                matFixed++;
            }

            // ── 2. Mesh 旋转烘焙 ─────────────────────────────────
            Vector3 euler = t.localRotation.eulerAngles;
            bool hasXRotation = Mathf.Abs(euler.x) > 1f && Mathf.Abs(euler.x - 360f) > 1f;

            if (hasXRotation)
            {
                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Mesh mesh = mf.sharedMesh;
                    string meshAssetPath = AssetDatabase.GetAssetPath(mesh);
                    bool meshIsAsset = !string.IsNullOrEmpty(meshAssetPath);

                    if (meshIsAsset)
                    {
                        // 已是资产 → 复制一份再改
                        mesh = Object.Instantiate(mesh);
                        mesh.name = t.name + "_Mesh";
                    }

                    Undo.RecordObject(mf, "Bake Arc Mesh");
                    Vector3[] verts = mesh.vertices;
                    Vector3[] norms = mesh.normals;

                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new Vector3(verts[i].x, 0f, verts[i].y);
                        norms[i] = Vector3.up;
                    }

                    mesh.vertices = verts;
                    mesh.normals = norms;
                    mesh.RecalculateBounds();

                    // 保存为新资产
                    EnsureDirectoryExists(matDir);
                    string meshPath = matDir + "/" + SanitizeMaterialName(t.name) + "_Mesh.asset";

                    // 如果路径已存在同名的不同 mesh，先删再建
                    Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    if (existingMesh != null && existingMesh != mesh)
                    {
                        // 用 Instantiate 复制一份 existingMesh 的数据...算了，直接 overwrite
                        AssetDatabase.DeleteAsset(meshPath);
                    }

                    AssetDatabase.CreateAsset(mesh, meshPath);
                    mf.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    EditorUtility.SetDirty(mf);

                    meshFixed++;
                }

                t.localRotation = Quaternion.identity;
                EditorUtility.SetDirty(t);
            }
        }

        // ── 3. Cap 父级修复 ──────────────────────────────────────
        // Cap（中心轴盖）是 NeedlePivot 的子物体，且因 Axis.Z 圆柱体带了
        // localRotation=(90,0,0)，与父级 Z 轴旋转叠加导致万向节锁。
        int capsFixed = 0;
        foreach (Transform t in modelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.Contains("NeedlePivot")) continue;

            Transform gaugeRoot = t.parent;
            if (gaugeRoot == null) continue;

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Transform child = t.GetChild(i);
                if (child.name.Contains("_Cap") || child.name.Contains("Cap"))
                {
                    Undo.SetTransformParent(child, gaugeRoot, "Fix Cap Parent");
                    capsFixed++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[PipelineFix] 完成！材质修复: " + matFixed + " 个, Mesh 烘焙: " + meshFixed + " 个"
            + (capsFixed > 0 ? ", Cap 父级修复: " + capsFixed + " 个" : ""));
    }

    // ══════════════════════════════════════════════════════════════
    //  MenuItem：验证
    // ══════════════════════════════════════════════════════════════

    [MenuItem("Tools/Pipeline/Validate Chemical Pipeline Scene")]
    public static void ValidateChemicalPipelineScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject staticRoot = FindRootInScene(scene);

        if (staticRoot == null)
        {
            Debug.LogWarning("[PipelineValidator] 场景中未找到 '" + StaticRootName + "'。");
            return;
        }

        Transform modelRoot = staticRoot.transform.Find(GeneratedModelName);
        if (modelRoot == null)
        {
            Debug.LogWarning("[PipelineValidator] 未找到生成的模型根 '" + GeneratedModelName + "'。");
            return;
        }

        PipelineChemicalSceneBuilder builder = staticRoot.GetComponent<PipelineChemicalSceneBuilder>();
        PipelineTrainingManager trainingMgr = modelRoot.GetComponent<PipelineTrainingManager>();

        // 如果 builder 引用为空，尝试从模型重新扫描
        if (builder != null)
        {
            builder.ReWireFromModel(modelRoot.gameObject);
        }

        bool builderOk = builder != null;
        bool trainingOk = trainingMgr != null;

        Debug.Log("[PipelineValidator] 验证结果：" +
            "\n  根节点: " + staticRoot.name +
            "\n  PipelineChemicalSceneBuilder: " + (builderOk ? "√" : "✗ 缺失") +
            "\n  PipelineTrainingManager: " + (trainingOk ? "√" : "✗ 缺失") +
            "\n  Prefab备份: " + (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath) != null ? "√" : "✗ 不存在") +
            "\n  inletValveWheel: " + (builder != null && builder.inletValveWheel != null ? "√" : "✗") +
            "\n  controlValveWheel: " + (builder != null && builder.controlValveWheel != null ? "√" : "✗") +
            "\n  outletValveWheel: " + (builder != null && builder.outletValveWheel != null ? "√" : "✗") +
            "\n  gaugeP1Needle: " + (builder != null && builder.gaugeP1Needle != null ? "√" : "✗") +
            "\n  gaugeP2Needle: " + (builder != null && builder.gaugeP2Needle != null ? "√" : "✗") +
            "\n  flowMeterDisplay: " + (builder != null && builder.flowMeterDisplay != null ? "√" : "✗") +
            "\n  eStopButton: " + (builder != null && builder.eStopButton != null ? "√" : "✗") +
            "\n  inspectionZones: " + (builder != null && builder.inspectionZones != null ? builder.inspectionZones.Count + " 个" : "✗"));

        if (builderOk && trainingOk)
        {
            Debug.Log("[PipelineValidator] 验证通过！");
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  核心逻辑
    // ══════════════════════════════════════════════════════════════

    private static void CreateOrRebuildStaticPipeline()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject staticRoot = FindOrCreateRoot(scene);

        PipelineChemicalSceneBuilder builder = staticRoot.GetComponent<PipelineChemicalSceneBuilder>();
        if (builder == null)
            builder = staticRoot.AddComponent<PipelineChemicalSceneBuilder>();

        ConfigureBuilder(builder);

        // 检查是否存在手动保存的 Prefab —— 优先使用
        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath);

        if (savedPrefab != null)
        {
            // ── Prefab 模式：直接实例化已保存的模型 ──────────────
            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab, staticRoot.transform);
            modelInstance.name = GeneratedModelName; // 去掉 "(Clone)" 后缀

            // 从实例中重新扫描交互对象引用
            builder.ReWireFromModel(modelInstance);

            // ★ 修复 Prefab 加载后 sceneBuilder 引用丢失的问题
            //   sceneBuilder 指向父级 staticRoot 上的组件，Prefab 中无法序列化跨层级引用
            PipelineSceneRuntime runtime = modelInstance.GetComponent<PipelineSceneRuntime>();
            if (runtime != null)
                runtime.sceneBuilder = builder;

            ApplyStaticFlags(staticRoot);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PipelineBuilder] 已从 Prefab 加载化工厂管道模型。\n" +
                "  来源: " + ModelPrefabPath + "\n" +
                "  如需从代码重新生成：Tools > Pipeline > Regenerate Model from Code");
        }
        else
        {
            // ── 代码模式：从 Builder 程序化生成 ──────────────────
            builder.Build();

            ApplyStaticFlags(staticRoot);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PipelineBuilder] 化工厂管道培训场景已从代码生成。\n" +
                "  修改模型后请执行 Tools > Pipeline > Save Current Model as Prefab 保存\n" +
                "  可通过 Tools > Pipeline/Validate Chemical Pipeline Scene 验证。");
        }
    }

    private static GameObject FindOrCreateRoot(Scene scene)
    {
        GameObject existing = FindRootInScene(scene);
        if (existing != null)
        {
            // 清除旧模型
            PipelineChemicalSceneBuilder oldBuilder = existing.GetComponent<PipelineChemicalSceneBuilder>();
            if (oldBuilder != null)
                oldBuilder.ClearModel(GeneratedModelName);
            return existing;
        }

        GameObject root = new GameObject(StaticRootName);
        root.transform.position = DefaultScenePosition;
        root.transform.localScale = DefaultSceneScale;
        return root;
    }

    private static GameObject FindRootInScene(Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == StaticRootName)
                return go;
        }
        return null;
    }

    private static void ConfigureBuilder(PipelineChemicalSceneBuilder builder)
    {
        builder.buildOnStart = false;
        builder.addColliders = true;
        builder.faceSouth = true;
        builder.sceneOrigin = Vector3.zero;
    }

    private static void ApplyStaticFlags(GameObject root)
    {
        Transform modelRoot = root.transform.Find(GeneratedModelName);
        if (modelRoot == null) return;

        SetStaticRecursively(modelRoot.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
    }

    private static void SetStaticRecursively(GameObject obj, StaticEditorFlags flags)
    {
        if (IsDynamicPart(obj.name))
        {
            obj.isStatic = false;
            return;
        }

        obj.isStatic = true;
        GameObjectUtility.SetStaticEditorFlags(obj, flags);

        foreach (Transform child in obj.transform)
            SetStaticRecursively(child.gameObject, flags);
    }

    private static bool IsDynamicPart(string name)
    {
        string lower = name.ToLower();
        return lower.Contains("handwheel") ||
               lower.Contains("needle") ||
               lower.Contains("needlepivot") ||
               lower.Contains("stem") ||
               lower.Contains("button") ||
               lower.Contains("lamp") ||
               lower.Contains("displaytext") ||
               lower.Contains("inspectionpoint");
    }

    /// <summary>
    /// 把模型中的所有运行时材质保存为独立 .mat 资产文件。
    /// 解决 Prefab 子资产材质丢失 Shader 引用导致品红色的问题。
    /// </summary>
    private static void SaveModelMaterialsAsAssets(GameObject modelRoot)
    {
        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        var savedMaterials = new System.Collections.Generic.Dictionary<Material, Material>(); // 运行时材质 → 资产材质

        foreach (Renderer renderer in renderers)
        {
            Material[] sharedMats = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < sharedMats.Length; i++)
            {
                Material runtimeMat = sharedMats[i];
                if (runtimeMat == null) continue;

                // 已经是持久化资产 → 跳过
                if (AssetDatabase.Contains(runtimeMat)) continue;

                // 已经在本轮保存过 → 复用
                if (savedMaterials.TryGetValue(runtimeMat, out Material savedAsset))
                {
                    sharedMats[i] = savedAsset;
                    changed = true;
                    continue;
                }

                // 保存为新 .mat 文件
                string safeName = SanitizeMaterialName(runtimeMat.name);
                string matPath = "Assets/Materials/Pipeline/" + safeName + ".mat";

                // 如果文件已存在，先加载看看能否复用
                Material existingAsset = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (existingAsset != null)
                {
                    // 更新已存在的材质属性
                    existingAsset.CopyPropertiesFromMaterial(runtimeMat);
                    EditorUtility.SetDirty(existingAsset);
                    savedMaterials[runtimeMat] = existingAsset;
                    sharedMats[i] = existingAsset;
                    changed = true;
                }
                else
                {
                    // 创建新材质资产
                    Material newAsset = new Material(runtimeMat);
                    newAsset.name = runtimeMat.name;
                    AssetDatabase.CreateAsset(newAsset, matPath);
                    savedMaterials[runtimeMat] = newAsset;
                    sharedMats[i] = newAsset;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = sharedMats;
                EditorUtility.SetDirty(renderer.gameObject);
            }
        }

        if (savedMaterials.Count > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[PipelineBuilder] 已保存 " + savedMaterials.Count + " 个材质到 Assets/Materials/Pipeline/");
        }
    }

    /// <summary>材质名中的非法文件名字符替换</summary>
    private static string SanitizeMaterialName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "UnnamedMaterial";
        return name.Replace("\\", "_").Replace("/", "_").Replace(":", "_")
                   .Replace("*", "_").Replace("?", "_").Replace("\"", "_")
                   .Replace("<", "_").Replace(">", "_").Replace("|", "_");
    }

    private static void EnsureDirectoryExists(string path)
    {
        string fullPath = System.IO.Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
        if (!System.IO.Directory.Exists(fullPath))
            System.IO.Directory.CreateDirectory(fullPath);
    }
}
