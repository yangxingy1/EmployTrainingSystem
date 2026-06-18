using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FreshTrainBootstrap : MonoBehaviour
{
    const float DefaultTrainingCameraWorldY = FactoryOneSceneController.KnownGoodStartCameraWorldHeight;
    const float DefaultTrainingEyeHeight = 1.7f;
    const float CubeGroundSink = 12.0f;

    [System.Serializable]
    class FreshTrainingTile
    {
        public string taskId;
        public string displayName;
        public string sceneName;
        public string instruction;
        public Color color;

        public FreshTrainingTile(string taskId, string displayName, string sceneName, string instruction, Color color)
        {
            this.taskId = taskId;
            this.displayName = displayName;
            this.sceneName = sceneName;
            this.instruction = instruction;
            this.color = color;
        }
    }

    const int GridSize = 3;
    const float CubeSize = 30.0f;
    const float CubeGap = 50.0f;
    const float HorizontalSpacing = CubeSize + CubeGap;
    const float ZStackSpacing = CubeSize + CubeGap;
    const float InteractionDistance = 30.0f;
    static readonly Vector3 GridWorldOffset = new Vector3(-20f, 0f, 280f);

    readonly FreshTrainingTile[] _tiles =
    {
        new FreshTrainingTile("electric_switch", "电闸拉下", "ElectricSwitch", "握拳后拉动电闸，将竖杆稳定拉到下方。", new Color(0.93f, 0.22f, 0.16f)),
        new FreshTrainingTile("electric_switch", "电闸推上", "ElectricSwitch", "握拳后推动电闸，将竖杆推到上方。", new Color(0.20f, 0.72f, 0.34f)),
        new FreshTrainingTile("electric_switch", "电闸复位", "ElectricSwitch", "握拳抓住横杆，完成一次上推或下拉复位。", new Color(0.96f, 0.61f, 0.12f)),
        new FreshTrainingTile("rotary_valve", "阀门顺拧", "SampleScene", "握拳后拧动阀门，将红色手轮旋转到目标角度。", new Color(0.86f, 0.08f, 0.07f)),
        new FreshTrainingTile("rotary_valve", "阀门定点", "SampleScene", "握拳抓住阀门手轮，缓慢旋转并停在目标位置。", new Color(0.12f, 0.52f, 0.82f)),
        new FreshTrainingTile("rotary_valve", "阀门释放", "SampleScene", "完成旋转后松开手势，确认阀门保持在目标角度。", new Color(0.28f, 0.62f, 0.88f)),
        new FreshTrainingTile("fresh_pipe_switch", "管路断电", "ElectricSwitch", "靠近电闸后握拳拉动，模拟管路检修前断电。", new Color(0.58f, 0.46f, 0.32f)),
        new FreshTrainingTile("fresh_pipe_valve", "管路开阀", "RotaryValve", "握拳后旋转阀门，模拟打开管路阀门。", new Color(0.94f, 0.72f, 0.18f)),
        new FreshTrainingTile("integrated_exam", "综合练习", "SampleScene", "完成一次新手综合训练。", new Color(0.42f, 0.28f, 0.78f)),
    };

    Transform _player;
    FreshTrainingTile _nearestTile;
    Transform _nearestTileTransform;
    float _trainingFloorY;
    bool _hasTrainingFloor;
    string _statusMessage = "靠近训练立方体，按 E 开始新手训练。";
    float _statusUntil;
    bool _enteringTraining;

    IEnumerator Start()
    {
        SceneFlow.EnsureExists();
        SessionManager.EnsureExists();

        yield return null;

        _player = ResolvePlayerTransform();
        NormalizePlayerPoseForFreshTraining();
        BuildTrainingTiles();
        RestoreReturnPositionIfNeeded();
        _player = ResolvePlayerTransform();
        ShowStatus("新手训练场已准备：靠近任意立方体，按 E 开始。", 3.0f);
    }

    void Update()
    {
        if (_enteringTraining) return;

        _player = ResolvePlayerTransform();
        UpdateNearestTile();

        if (Input.GetKeyDown(KeyCode.E))
            TryEnterNearestTraining();
    }

    void OnGUI()
    {
        var message = BuildPromptText();
        if (string.IsNullOrEmpty(message)) return;

        var width = Mathf.Min(760f, Screen.width - 36f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 116f, width, 72f);
        GUI.Box(rect, message);
    }

    void BuildTrainingTiles()
    {
        var root = new GameObject("Fresh Training Cubes");
        ResolveGridFrame(out var center, out var right, out var forward);

        for (int i = 0; i < _tiles.Length; i++)
        {
            int zLayer = i / GridSize;
            int col = i % GridSize;
            var position = center
                + right * ((col - 1) * HorizontalSpacing)
                + forward * ((zLayer - 1) * ZStackSpacing)
                + Vector3.up * (CubeSize * 0.5f - CubeGroundSink);
            CreateTrainingCube(root.transform, _tiles[i], i, position);
        }
    }

    void ResolveGridFrame(out Vector3 center, out Vector3 right, out Vector3 forward)
    {
        var player = ResolvePlayerTransform();
        right = Vector3.right;
        forward = Vector3.forward;
        var floorY = ResolveTrainingFloorY(player);

        if (player != null)
        {
            center = new Vector3(player.position.x, floorY, player.position.z + 10.0f) + GridWorldOffset;
            return;
        }

        center = new Vector3(0f, floorY, 10f) + GridWorldOffset;
    }

    void CreateTrainingCube(Transform parent, FreshTrainingTile tile, int index, Vector3 position)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "FreshTrainCube_" + (index + 1) + "_" + tile.taskId;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * CubeSize;
        SetColor(cube, tile.color);

        var collider = cube.GetComponent<BoxCollider>();
        if (collider != null)
            collider.size = Vector3.one;

        var labelGo = new GameObject("Cube Label");
        labelGo.transform.SetParent(cube.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = (index + 1) + "\n" + tile.displayName;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 34;
        label.characterSize = 0.075f;
        label.color = Color.white;

        var tileMarker = cube.AddComponent<FreshTrainCubeMarker>();
        tileMarker.Configure(tile.taskId, tile.displayName, tile.sceneName, tile.instruction);
    }

    void UpdateNearestTile()
    {
        _nearestTile = null;
        _nearestTileTransform = null;
        if (_player == null) return;

        var markers = FindObjectsOfType<FreshTrainCubeMarker>();
        var nearestDistance = float.MaxValue;

        foreach (var marker in markers)
        {
            if (marker == null) continue;

            var distance = HorizontalDistance(_player.position, marker.transform.position);
            if (distance > InteractionDistance || distance >= nearestDistance) continue;

            nearestDistance = distance;
            _nearestTileTransform = marker.transform;
            _nearestTile = new FreshTrainingTile(
                marker.taskId,
                marker.displayName,
                marker.sceneName,
                marker.instruction,
                Color.white);
        }
    }

    void TryEnterNearestTraining()
    {
        if (_nearestTile == null || _nearestTileTransform == null)
        {
            ShowStatus("请先移动到一个训练立方体前，再按 E 交互。", 1.8f);
            return;
        }

        StartCoroutine(EnterTrainingRoutine(_nearestTile, _nearestTileTransform.position));
    }

    IEnumerator EnterTrainingRoutine(FreshTrainingTile tile, Vector3 tilePosition)
    {
        _enteringTraining = true;

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = tile.instruction;
        session.selectedSuccessMessage = "成功完成训练";

        var returnPosition = ResolveReturnPosition(tilePosition);
        var assignedTask = new AssignedTask(tile.taskId, tile.displayName, tile.sceneName, 0);

        Debug.Log($"[FreshTrain] {tile.displayName}: {tile.instruction}");
        ShowStatus(tile.instruction + "\n正在进入：" + tile.displayName, 0.7f);
        yield return new WaitForSeconds(0.35f);

        SceneFlow.EnsureExists().EnterTraining(assignedTask, returnPosition);
    }

    Vector3 ResolveReturnPosition(Vector3 tilePosition)
    {
        if (_player == null) return tilePosition + Vector3.back * 2f + Vector3.up;

        return _player.position;
    }

    void RestoreReturnPositionIfNeeded()
    {
        var session = SessionManager.Instance;
        if (session == null || !session.hasHubReturnPosition) return;
        if (session.returnSceneName != SceneManager.GetActiveScene().name) return;

        var player = ResolvePlayerTransform();
        if (player == null) return;

        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        var returnPosition = session.hubReturnPosition;
        returnPosition.y = ResolveTrainingFloorY(player);
        player.position = returnPosition;
        if (controller != null) controller.enabled = true;
        session.hasHubReturnPosition = false;

        Debug.Log($"[FreshTrain] Restored player position: {returnPosition}");
    }

    Transform ResolvePlayerTransform()
    {
        var controller = FindObjectOfType<CharacterController>();
        if (controller != null) return controller.transform;

        if (Camera.main != null && Camera.main.transform.parent != null)
            return Camera.main.transform.parent;

        return Camera.main != null ? Camera.main.transform : transform;
    }

    void NormalizePlayerPoseForFreshTraining()
    {
        var player = ResolvePlayerTransform();
        var camera = Camera.main;
        var floorY = ResolveTrainingFloorY(player);

        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            var position = player.position;
            position.y = floorY;
            player.position = position;

            if (controller != null) controller.enabled = true;
        }

        if (camera == null || camera.transform.parent == null) return;

        var local = camera.transform.localPosition;
        local.y = ResolveTrainingEyeHeight();
        camera.transform.localPosition = local;

        Debug.Log($"[FreshTrain] Player floor normalized to {floorY:F3}, camera local height={local.y:F3}.");
    }

    float ResolveTrainingFloorY(Transform player)
    {
        if (_hasTrainingFloor) return _trainingFloorY;

        _trainingFloorY = DefaultTrainingCameraWorldY - ResolveTrainingEyeHeight();

        var factoryController = FindObjectOfType<FactoryOneSceneController>();
        if (factoryController != null)
        {
            _trainingFloorY = factoryController.startCameraWorldHeight
                - Mathf.Max(0.1f, factoryController.eyeHeight + factoryController.cameraHeightOffset);
        }

        if (float.IsNaN(_trainingFloorY) || float.IsInfinity(_trainingFloorY))
            _trainingFloorY = player != null ? player.position.y : 0f;

        _hasTrainingFloor = true;
        return _trainingFloorY;
    }

    float ResolveTrainingEyeHeight()
    {
        var factoryController = FindObjectOfType<FactoryOneSceneController>();
        if (factoryController != null)
            return Mathf.Max(0.1f, factoryController.eyeHeight + factoryController.cameraHeightOffset);

        return DefaultTrainingEyeHeight;
    }

    string BuildPromptText()
    {
        if (Time.time < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        if (_nearestTile == null)
            return "靠近 3x3 训练立方体，按 E 开始交互。";

        return "按 E 开始：" + _nearestTile.displayName + "\n" + _nearestTile.instruction;
    }

    void ShowStatus(string message, float seconds)
    {
        _statusMessage = message;
        _statusUntil = Time.time + seconds;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
