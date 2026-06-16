using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送带分拣训练场景:移动物料、同色分拣箱、点击确认。
/// </summary>
public class ConveyorSortingTrainingTask : MonoBehaviour
{
    class BlockGoal
    {
        public string label;
        public Grabbable block;
        public Vector3 startPosition;
        public SortBin targetBin;
        public Color color;
        public bool placed;
        public bool onConveyor;
        public bool waitingRespawn;
        public float respawnAt;
        public TextMesh packageLabel;
    }

    class SortBin
    {
        public string label;
        public Vector3 zoneCenter;
        public Renderer zoneRenderer;
        public Color color;
        public TextMesh countText;
        public int count;
    }

    public GraspController grasp;
    public Vector2 areaMin = new Vector2(-2.7f, -1.35f);
    public Vector2 areaMax = new Vector2(2.7f, 1.25f);
    public float blockSize = 0.42f;
    public bool clampBlocksToArea = true;

    readonly List<BlockGoal> _goals = new List<BlockGoal>();
    readonly List<SortBin> _bins = new List<SortBin>();
    readonly List<Transform> _beltStripes = new List<Transform>();
    TextMesh _status;
    GameObject _cursor, _button, _restartButton;
    Renderer _cursorRenderer, _buttonRenderer, _restartButtonRenderer;
    LineRenderer _line;
    PhysicMaterial _blockPhysic;
    Grabbable _lastHeld;

    float _startTime;
    Vector3 _buttonLastClickPoint;
    Vector3 _restartLastClickPoint;
    float _lastButtonPressTime = -99f;
    float _lastRestartPressTime = -99f;
    float _buttonHoverStartTime = -99f;
    float _restartHoverStartTime = -99f;
    int _dropCount;
    bool _buttonPressed;
    bool _buttonHasLastClickPoint;
    bool _buttonPressArmed;
    bool _buttonTapReady;
    bool _buttonPressedByFinger;
    bool _restartHasLastClickPoint;
    bool _restartPressArmed;
    bool _restartTapReady;
    bool _restartPressedByFinger;
    bool _buttonGuideActive;
    Vector3 _buttonGuideTarget;

    const float ZoneRadius = 0.62f;
    const float ButtonRadius = 0.42f;
    const float ButtonCooldown = 0.45f;
    const float RestartButtonRadius = 0.44f;
    const float RestartCooldown = 0.65f;
    const float ButtonHoverWidthFactor = 1.12f;
    const float ButtonHoverHeightFactor = 1.18f;
    const float ButtonClickWidthFactor = 0.90f;
    const float ButtonClickHeightFactor = 0.88f;
    const float ButtonTapReadySeconds = 0.11f;
    const float ButtonTapMinDownDelta = 0.028f;
    const float ButtonTapMinDownSpeed = 0.18f;
    const float ButtonTapMaxSideOffsetFactor = 0.62f;
    const float ButtonTapStabilizeMaxDrift = 0.060f;
    const float ConveyorY = 0.58f;
    const float ConveyorStartX = -2.05f;
    const float ConveyorEndX = 1.62f;
    const float ConveyorSpeed = 0.46f;
    const float ConveyorHeight = 0.46f;
    const float ConveyorStripeSpacing = 0.48f;
    const int ConveyorItemPoolSize = 6;
    const int CheckpointTargetCount = 10;
    const float PackageSpawnSpacing = 0.54f;
    const float PackageRespawnDelay = 0.55f;
    const float ConveyorEntryClearDistance = 0.46f;

    void Start()
    {
        _startTime = Time.time;
        _blockPhysic = new PhysicMaterial("TrainingBlock")
        {
            dynamicFriction = 0.7f,
            staticFriction = 0.85f,
            bounciness = 0.02f,
        };

        BuildPracticeArea();
        BuildOperationDevices();
        BuildCursor();
        BuildStatus();
        SpawnBlocksAndZones();
    }

    void Update()
    {
        UpdateReleaseAccounting();
        UpdateConveyor();
        _buttonGuideActive = false;
        UpdateRestartButton();
        UpdatePlacement();
        UpdateButton();
        UpdateCursor();
        UpdateStatus();
        ClampBlocksToArea();
    }

    void BuildPracticeArea()
    {
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "TrainingOperationPlane";
        board.transform.position = new Vector3(0f, areaMin.y - 0.04f, 0.08f);
        board.transform.localScale = new Vector3(5.9f, 0.08f, 0.75f);
        SetColor(board, new Color(0.22f, 0.25f, 0.29f));

        CreateBorder("LeftLimit", new Vector3(areaMin.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("RightLimit", new Vector3(areaMax.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("TopLimit", new Vector3(0f, areaMax.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));
        CreateBorder("BottomLimit", new Vector3(0f, areaMin.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));

        BuildConveyor();

        var titleGo = new GameObject("TrainingTitle");
        titleGo.transform.position = new Vector3(0f, 2.08f, -0.05f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "Conveyor Sorting";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 44;
        title.characterSize = 0.044f;
        title.color = Color.white;
    }

    void CreateBorder(string name, Vector3 position, Vector3 scale)
    {
        var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = name;
        border.transform.position = position;
        border.transform.localScale = scale;
        SetColor(border, new Color(0.45f, 0.52f, 0.60f));
    }

    void BuildConveyor()
    {
        float length = ConveyorEndX - ConveyorStartX;
        Vector3 center = new Vector3((ConveyorStartX + ConveyorEndX) * 0.5f, ConveyorY, 0.08f);

        var belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        belt.name = "MovingConveyorBelt";
        belt.transform.position = center;
        belt.transform.localScale = new Vector3(length, ConveyorHeight, 0.08f);
        Destroy(belt.GetComponent<Collider>());
        SetColor(belt, new Color(0.08f, 0.10f, 0.12f));

        CreateConveyorPart("ConveyorTopRail", center + new Vector3(0f, ConveyorHeight * 0.56f, -0.02f),
            new Vector3(length + 0.18f, 0.045f, 0.07f), new Color(0.55f, 0.61f, 0.66f));
        CreateConveyorPart("ConveyorBottomRail", center + new Vector3(0f, -ConveyorHeight * 0.56f, -0.02f),
            new Vector3(length + 0.18f, 0.045f, 0.07f), new Color(0.55f, 0.61f, 0.66f));

        CreateConveyorRoller("ConveyorInfeedRoller", new Vector3(ConveyorStartX - 0.10f, ConveyorY, -0.02f));
        CreateConveyorRoller("ConveyorOutfeedRoller", new Vector3(ConveyorEndX + 0.10f, ConveyorY, -0.02f));

        _beltStripes.Clear();
        int stripeCount = Mathf.CeilToInt(length / ConveyorStripeSpacing) + 2;
        for (int i = 0; i < stripeCount; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "ConveyorMotionStripe_" + i;
            stripe.transform.position = new Vector3(ConveyorStartX + i * ConveyorStripeSpacing, ConveyorY, -0.06f);
            stripe.transform.localScale = new Vector3(0.12f, ConveyorHeight * 0.78f, 0.05f);
            stripe.transform.rotation = Quaternion.Euler(0f, 0f, -18f);
            Destroy(stripe.GetComponent<Collider>());
            SetColor(stripe, new Color(0.98f, 0.74f, 0.18f));
            _beltStripes.Add(stripe.transform);
        }

        var labelGo = new GameObject("ConveyorLabel");
        labelGo.transform.position = center + new Vector3(0f, ConveyorHeight * 0.75f, -0.05f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "MOVING LINE  ->";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 28;
        label.characterSize = 0.030f;
        label.color = new Color(0.88f, 0.94f, 1f);
    }

    void CreateConveyorPart(string name, Vector3 position, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.position = position;
        part.transform.localScale = scale;
        Destroy(part.GetComponent<Collider>());
        SetColor(part, color);
    }

    void CreateConveyorRoller(string name, Vector3 position)
    {
        var roller = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roller.name = name;
        roller.transform.position = position;
        roller.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        roller.transform.localScale = new Vector3(0.24f, 0.04f, 0.24f);
        Destroy(roller.GetComponent<Collider>());
        SetColor(roller, new Color(0.35f, 0.39f, 0.44f));
    }

    void BuildOperationDevices()
    {
        BuildButton(new Vector3(2.25f, 0.83f, 0f));
        BuildRestartButton(new Vector3(2.25f, -0.05f, 0f));
    }

    void BuildButton(Vector3 center)
    {
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "ConfirmButtonBase";
        baseGo.transform.position = center + new Vector3(0f, -0.18f, 0.06f);
        baseGo.transform.localScale = new Vector3(0.78f, 0.13f, 0.10f);
        SetColor(baseGo, new Color(0.18f, 0.22f, 0.28f));

        _button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _button.name = "ConfirmButton";
        _button.transform.position = center + new Vector3(0f, 0f, -0.02f);
        _button.transform.localScale = new Vector3(0.52f, 0.24f, 0.10f);
        _buttonRenderer = _button.GetComponent<Renderer>();
        SetRendererColor(_buttonRenderer, new Color(0.12f, 0.44f, 0.92f));

        var labelGo = new GameObject("ButtonLabel");
        labelGo.transform.position = center + new Vector3(0f, 0.28f, -0.05f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "CONFIRM";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 34;
        label.characterSize = 0.035f;
        label.color = new Color(0.82f, 0.90f, 1f);
    }

    void BuildRestartButton(Vector3 center)
    {
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "RestartButtonBase";
        baseGo.transform.position = center + new Vector3(0f, -0.15f, 0.06f);
        baseGo.transform.localScale = new Vector3(0.72f, 0.11f, 0.10f);
        SetColor(baseGo, new Color(0.20f, 0.18f, 0.17f));

        _restartButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _restartButton.name = "RestartButton";
        _restartButton.transform.position = center + new Vector3(0f, 0f, -0.02f);
        _restartButton.transform.localScale = new Vector3(0.54f, 0.22f, 0.10f);
        _restartButtonRenderer = _restartButton.GetComponent<Renderer>();
        SetRendererColor(_restartButtonRenderer, new Color(0.82f, 0.24f, 0.18f));

        var labelGo = new GameObject("RestartButtonLabel");
        labelGo.transform.position = center + new Vector3(0f, 0.25f, -0.05f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "RESTART";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 32;
        label.characterSize = 0.033f;
        label.color = new Color(1f, 0.82f, 0.76f);
    }

    void SpawnBlocksAndZones()
    {
        _bins.Clear();
        AddSortingBin("R", new Vector3(-1.50f, -1.08f, 0f), new Color(1f, 0.45f, 0.12f));
        AddSortingBin("B", new Vector3(0f, -1.08f, 0f), new Color(0.25f, 0.62f, 1f));
        AddSortingBin("G", new Vector3(1.50f, -1.08f, 0f), new Color(0.35f, 0.85f, 0.38f));
        SpawnPackagePool();
    }

    void AddSortingBin(string label, Vector3 zoneCenter, Color color)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "SortingBin_" + label;
        zone.transform.position = zoneCenter + new Vector3(0f, -0.05f, 0.12f);
        zone.transform.localScale = new Vector3(1.24f, 0.46f, 0.06f);
        var zoneCol = zone.GetComponent<Collider>();
        if (zoneCol != null) Destroy(zoneCol);
        var zoneRenderer = zone.GetComponent<Renderer>();
        SetRendererColor(zoneRenderer, Color.Lerp(color, Color.black, 0.20f));

        CreateBinWall("SortingBinLeft_" + label, zoneCenter + new Vector3(-0.66f, 0.02f, 0.08f), color);
        CreateBinWall("SortingBinRight_" + label, zoneCenter + new Vector3(0.66f, 0.02f, 0.08f), color);
        CreateBinWall("SortingBinBack_" + label, zoneCenter + new Vector3(0f, 0.25f, 0.08f), color, true);

        var snap = zone.AddComponent<SnapZone>();
        snap.radius = 0.88f;
        snap.magnetism = 0.80f;
        if (grasp != null) grasp.snapZones.Add(snap);

        var labelGo = new GameObject("DropZoneLabel_" + label);
        labelGo.transform.position = zoneCenter + new Vector3(0f, 0.32f, -0.05f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = "BIN " + label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 34;
        text.characterSize = 0.034f;
        text.color = Color.white;

        var countGo = new GameObject("SortingBinCount_" + label);
        countGo.transform.position = zoneCenter + new Vector3(0f, -0.04f, -0.06f);
        var countText = countGo.AddComponent<TextMesh>();
        countText.text = "0";
        countText.anchor = TextAnchor.MiddleCenter;
        countText.alignment = TextAlignment.Center;
        countText.fontSize = 64;
        countText.characterSize = 0.060f;
        countText.color = Color.white;

        var bin = new SortBin
        {
            label = label,
            zoneCenter = zoneCenter,
            zoneRenderer = zoneRenderer,
            color = color,
            countText = countText,
        };
        _bins.Add(bin);
        UpdateBinCountText(bin);
    }

    void SpawnPackagePool()
    {
        for (int i = 0; i < ConveyorItemPoolSize; i++)
        {
            Vector3 start = new Vector3(ConveyorStartX + 0.10f + i * PackageSpawnSpacing, ConveyorY, 0f);
            var block = CreateBlock("Package_" + i, start, Color.white);
            var labelText = AddPackageLabel(block, "");
            var goal = new BlockGoal
            {
                block = block,
                startPosition = start,
                packageLabel = labelText,
                onConveyor = true,
            };
            AssignRandomPackage(goal);
            _goals.Add(goal);
        }
    }

    void AssignRandomPackage(BlockGoal goal)
    {
        if (goal == null || _bins.Count == 0) return;
        SortBin bin = _bins[Random.Range(0, _bins.Count)];
        goal.targetBin = bin;
        goal.label = bin.label;
        goal.color = bin.color;
        goal.placed = false;
        goal.waitingRespawn = false;
        if (goal.block != null)
        {
            goal.block.name = "Package_" + bin.label;
            goal.block.SetBaseColor(bin.color);
            goal.block.CanGrab = true;
        }
        if (goal.packageLabel != null)
            goal.packageLabel.text = bin.label;
    }

    void UpdateBinCountText(SortBin bin)
    {
        if (bin == null || bin.countText == null) return;
        bin.countText.text = bin.count.ToString();
    }

    void CreateBinWall(string name, Vector3 position, Color color, bool horizontal = false)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = horizontal
            ? new Vector3(1.34f, 0.060f, 0.07f)
            : new Vector3(0.060f, 0.58f, 0.07f);
        Destroy(wall.GetComponent<Collider>());
        SetColor(wall, Color.Lerp(color, Color.white, 0.12f));
    }

    TextMesh AddPackageLabel(Grabbable block, string label)
    {
        var labelGo = new GameObject("PackageLabel_" + label);
        labelGo.transform.parent = block.transform;
        labelGo.transform.localPosition = new Vector3(0f, 0f, -0.24f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 52;
        text.characterSize = 0.045f;
        text.color = Color.white;
        return text;
    }

    Grabbable CreateBlock(string name, Vector3 position, Color color)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.position = position;
        block.transform.localScale = Vector3.one * blockSize;
        var col = block.GetComponent<Collider>();
        if (col != null) col.material = _blockPhysic;

        var rb = block.AddComponent<Rigidbody>();
        rb.mass = 0.32f;
        rb.drag = 1.4f;
        rb.angularDrag = 2.2f;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var grabbable = block.AddComponent<Grabbable>();
        grabbable.margin = 0.62f;
        grabbable.RefreshGrabRadius();
        grabbable.SetBaseColor(color);

        if (grasp != null) grasp.grabbables.Add(grabbable);
        return grabbable;
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "GripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("GripHoverLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.012f;
        _line.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _line.enabled = false;
    }

    void BuildStatus()
    {
        var statusGo = new GameObject("TrainingStatus");
        statusGo.transform.position = new Vector3(4.25f, 1.28f, -0.05f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateReleaseAccounting()
    {
        if (grasp == null) return;
        if (_lastHeld != null && grasp.Held == null)
        {
            var goal = FindGoal(_lastHeld);
            if (goal != null && IsPackageActive(goal) && FindBinAtPosition(goal.block.Body.position) == null)
            {
                _dropCount++;
                TrainingFlowController.Active?.RecordMistake("物料释放到错误区域");
                SchedulePackageRespawn(goal);
            }
        }

        _lastHeld = grasp.Held;
    }

    void UpdateConveyor()
    {
        float speed = ConveyorCurrentSpeed();
        float dt = Time.deltaTime;

        foreach (var stripe in _beltStripes)
        {
            if (stripe == null) continue;
            Vector3 p = stripe.position;
            p.x += speed * dt;
            if (p.x > ConveyorEndX + ConveyorStripeSpacing * 0.5f)
                p.x = ConveyorStartX - ConveyorStripeSpacing * 0.5f;
            stripe.position = p;
        }

        foreach (var goal in _goals)
        {
            if (goal.waitingRespawn)
            {
                TryRespawnPackage(goal);
                continue;
            }

            if (goal.block == null || goal.block.Body == null || goal.placed) continue;
            if (grasp != null && grasp.Held == goal.block)
            {
                goal.onConveyor = false;
                continue;
            }

            if (goal.onConveyor)
            {
                var rb = goal.block.Body;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                Vector3 p = rb.position;
                p.x += speed * dt;
                p.y = Mathf.Lerp(p.y, ConveyorY, 0.45f);
                p.z = 0f;
                rb.position = p;

                if (p.x > ConveyorEndX)
                {
                    _dropCount++;
                    TrainingFlowController.Active?.RecordMistake("物料漏拣");
                    SchedulePackageRespawn(goal);
                }
            }
            else if (goal.block.Body.position.y <= areaMin.y + blockSize * 0.35f)
            {
                _dropCount++;
                TrainingFlowController.Active?.RecordMistake("物料掉出操作区");
                SchedulePackageRespawn(goal);
            }
        }
    }

    float ConveyorCurrentSpeed()
    {
        return ConveyorSpeed;
    }

    void SchedulePackageRespawn(BlockGoal goal)
    {
        if (goal == null || goal.block == null || goal.block.Body == null) return;

        goal.placed = true;
        goal.onConveyor = false;
        goal.waitingRespawn = true;
        goal.respawnAt = Time.time + PackageRespawnDelay;
        goal.block.CanGrab = false;
        goal.block.SetHighlight(false);
        goal.block.Body.useGravity = false;
        goal.block.Body.velocity = Vector3.zero;
        goal.block.Body.angularVelocity = Vector3.zero;
        goal.block.gameObject.SetActive(false);
    }

    void TryRespawnPackage(BlockGoal goal)
    {
        if (goal == null || Time.time < goal.respawnAt || !ConveyorEntryClear(goal)) return;

        Vector3 spawn = new Vector3(ConveyorStartX + 0.10f, ConveyorY, 0f);
        goal.startPosition = spawn;
        AssignRandomPackage(goal);
        goal.onConveyor = true;
        goal.placed = false;
        goal.waitingRespawn = false;
        goal.block.gameObject.SetActive(true);
        goal.block.CanGrab = true;
        goal.block.SetHighlight(false);
        goal.block.Body.useGravity = false;
        goal.block.Body.velocity = Vector3.zero;
        goal.block.Body.angularVelocity = Vector3.zero;
        goal.block.Body.position = spawn;
        goal.block.transform.rotation = Quaternion.identity;
    }

    bool ConveyorEntryClear(BlockGoal self)
    {
        float entryX = ConveyorStartX + 0.10f;
        foreach (var goal in _goals)
        {
            if (goal == self || goal == null || goal.block == null || goal.block.Body == null) continue;
            if (goal.waitingRespawn || goal.placed || !goal.onConveyor) continue;
            if (Mathf.Abs(goal.block.Body.position.x - entryX) < ConveyorEntryClearDistance)
                return false;
        }
        return true;
    }

    bool IsPackageActive(BlockGoal goal)
    {
        return goal != null
            && goal.block != null
            && goal.block.Body != null
            && !goal.placed
            && !goal.waitingRespawn
            && goal.block.gameObject.activeSelf;
    }

    void UpdatePlacement()
    {
        foreach (var goal in _goals)
        {
            if (!IsPackageActive(goal)) continue;
            if (grasp != null && grasp.Held == goal.block) continue;
            SortBin bin = FindBinAtPosition(goal.block.Body.position);
            if (bin == null) continue;
            if (goal.block.Body.velocity.magnitude > 0.35f) continue;

            if (bin == goal.targetBin)
            {
                bin.count++;
                TrainingFlowController.Active?.RecordSuccess("正确分拣: " + goal.label);
                UpdateBinCountText(bin);
                SetRendererColor(bin.zoneRenderer, Color.Lerp(bin.color, Color.white, 0.25f));
            }
            else
            {
                _dropCount++;
                TrainingFlowController.Active?.RecordMistake("误投到错误料箱");
                SetRendererColor(bin.zoneRenderer, new Color(1f, 0.25f, 0.18f));
            }

            SchedulePackageRespawn(goal);
        }
    }

    void UpdateRestartButton()
    {
        if (grasp == null || grasp.hand == null || _restartButton == null) return;

        bool click = UpdateButtonClick(
            _restartButton,
            RestartButtonRadius,
            RestartCooldown,
            ref _restartPressArmed,
            ref _restartTapReady,
            ref _restartPressedByFinger,
            ref _restartLastClickPoint,
            ref _restartHasLastClickPoint,
            ref _restartHoverStartTime,
            ref _lastRestartPressTime,
            requireFreeHand: true,
            out bool near,
            out float pressAmount);

        if (click)
            RestartTraining();
        if (near)
            SetButtonGuide(_restartButton.transform.position);

        bool pressing = pressAmount > 0.01f;
        Color color;
        if (click || pressing) color = new Color(1f, 0.74f, 0.24f);
        else if (near) color = new Color(0.96f, 0.42f, 0.25f);
        else color = new Color(0.82f, 0.24f, 0.18f);
        SetRendererColor(_restartButtonRenderer, color);

        float pressScale = Mathf.Lerp(near ? 0.16f : 0.22f, 0.11f, pressAmount);
        _restartButton.transform.localScale = new Vector3(0.54f, pressScale, 0.10f);
    }

    void RestartTraining()
    {
        grasp?.CancelInteraction();
        _startTime = Time.time;
        _dropCount = 0;
        _buttonPressed = false;
        _lastButtonPressTime = -99f;
        _buttonHoverStartTime = -99f;
        _buttonHasLastClickPoint = false;
        _buttonPressArmed = false;
        _buttonTapReady = false;
        _buttonPressedByFinger = false;
        _restartHoverStartTime = Time.time;
        _restartHasLastClickPoint = false;
        _restartPressArmed = true;
        _restartTapReady = false;
        _restartPressedByFinger = true;
        _lastHeld = null;

        foreach (var bin in _bins)
        {
            bin.count = 0;
            UpdateBinCountText(bin);
            SetRendererColor(bin.zoneRenderer, Color.Lerp(bin.color, Color.black, 0.20f));
        }

        for (int i = 0; i < _goals.Count; i++)
        {
            var goal = _goals[i];
            if (goal.block == null || goal.block.Body == null) continue;

            Vector3 start = new Vector3(ConveyorStartX + 0.10f + i * PackageSpawnSpacing, ConveyorY, 0f);
            goal.startPosition = start;
            goal.placed = false;
            goal.onConveyor = true;
            goal.waitingRespawn = false;
            goal.respawnAt = 0f;
            goal.block.gameObject.SetActive(true);
            AssignRandomPackage(goal);
            goal.block.CanGrab = true;
            goal.block.SetHighlight(false);
            goal.block.Body.useGravity = false;
            goal.block.Body.velocity = Vector3.zero;
            goal.block.Body.angularVelocity = Vector3.zero;
            goal.block.Body.position = start;
            goal.block.transform.rotation = Quaternion.identity;
        }

        if (_button != null) _button.transform.localScale = new Vector3(0.52f, 0.24f, 0.10f);
        SetRendererColor(_buttonRenderer, new Color(0.12f, 0.44f, 0.92f));
        if (_restartButton != null) _restartButton.transform.localScale = new Vector3(0.54f, 0.22f, 0.10f);
        SetRendererColor(_restartButtonRenderer, new Color(0.82f, 0.24f, 0.18f));
    }

    void UpdateButton()
    {
        if (grasp == null || grasp.hand == null || _button == null) return;

        bool ready = TotalSortedCount() >= CheckpointTargetCount;
        bool click = UpdateButtonClick(
            _button,
            ButtonRadius,
            ButtonCooldown,
            ref _buttonPressArmed,
            ref _buttonTapReady,
            ref _buttonPressedByFinger,
            ref _buttonLastClickPoint,
            ref _buttonHasLastClickPoint,
            ref _buttonHoverStartTime,
            ref _lastButtonPressTime,
            requireFreeHand: true,
            out bool near,
            out float pressAmount);

        if (click)
        {
            if (ready && !_buttonPressed)
            {
                _buttonPressed = true;
                TrainingFlowController.Active?.RecordSuccess("班次确认完成");
            }
        }
        if (near)
            SetButtonGuide(_button.transform.position);

        Color color;
        if (_buttonPressed) color = new Color(0.12f, 0.82f, 0.34f);
        else if (!ready) color = new Color(0.24f, 0.30f, 0.38f);
        else if (near) color = new Color(0.98f, 0.78f, 0.16f);
        else color = new Color(0.12f, 0.44f, 0.92f);
        SetRendererColor(_buttonRenderer, color);

        float restScale = near ? 0.14f : 0.24f;
        float pressScale = _buttonPressed ? 0.08f : Mathf.Lerp(restScale, 0.10f, pressAmount);
        _button.transform.localScale = new Vector3(0.52f, pressScale, 0.10f);
    }

    // 摄像头平面点击: 进入按钮区域只算悬停;在区域内做一次食指下点动作才触发。
    bool UpdateButtonClick(
        GameObject target,
        float radius,
        float cooldown,
        ref bool armed,
        ref bool readyToTap,
        ref bool pressed,
        ref Vector3 lastPoint,
        ref bool hasLastPoint,
        ref float hoverStartTime,
        ref float lastPressTime,
        bool requireFreeHand,
        out bool near,
        out float pressAmount)
    {
        near = false;
        pressAmount = 0f;
        if (grasp == null || grasp.hand == null || target == null || !grasp.hand.IsActive)
        {
            armed = false;
            readyToTap = false;
            pressed = false;
            hasLastPoint = false;
            hoverStartTime = -99f;
            return false;
        }

        Vector3 clickPoint = ButtonClickPoint();
        Vector3 center = target.transform.position;
        float dx = Mathf.Abs(clickPoint.x - center.x);
        float dy = Mathf.Abs(clickPoint.y - center.y);
        bool inHoverArea = dx <= radius * ButtonHoverWidthFactor
            && dy <= radius * ButtonHoverHeightFactor;
        bool inClickArea = dx <= radius * ButtonClickWidthFactor
            && dy <= radius * ButtonClickHeightFactor;
        near = inHoverArea;

        bool eligible = !requireFreeHand || grasp.Held == null;
        if (!eligible)
        {
            armed = false;
            readyToTap = false;
            pressed = false;
            hasLastPoint = false;
            hoverStartTime = -99f;
            return false;
        }

        if (!inHoverArea)
        {
            armed = false;
            readyToTap = false;
            pressed = false;
            hasLastPoint = false;
            hoverStartTime = -99f;
            return false;
        }

        if (!armed)
        {
            armed = true;
            readyToTap = false;
            pressed = false;
            hasLastPoint = true;
            lastPoint = clickPoint;
            hoverStartTime = Time.time;
            pressAmount = 0.18f;
            return false;
        }

        if (!inClickArea)
        {
            readyToTap = false;
            pressed = false;
            hasLastPoint = true;
            pressAmount = 0.28f;
            lastPoint = clickPoint;
            hoverStartTime = Time.time;
            return false;
        }

        if (!hasLastPoint)
        {
            hasLastPoint = true;
            lastPoint = clickPoint;
            hoverStartTime = Time.time;
            pressAmount = 0.18f;
            return false;
        }

        if (!readyToTap)
        {
            if (Time.time - hoverStartTime < ButtonTapReadySeconds)
            {
                Vector3 settleDelta = clickPoint - lastPoint;
                if (Mathf.Abs(settleDelta.x) > ButtonTapStabilizeMaxDrift || Mathf.Abs(settleDelta.y) > ButtonTapStabilizeMaxDrift)
                {
                    hoverStartTime = Time.time;
                    lastPoint = clickPoint;
                }
                pressAmount = 0.18f;
                return false;
            }

            readyToTap = true;
            lastPoint = clickPoint;
            pressAmount = 0.18f;
            return false;
        }

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 frameDelta = clickPoint - lastPoint;
        if (!pressed && clickPoint.y > lastPoint.y)
            lastPoint = clickPoint;

        float downDistance = lastPoint.y - clickPoint.y;
        float downSpeed = Mathf.Max(0f, -frameDelta.y / dt);
        float sideOffset = Mathf.Abs(clickPoint.x - lastPoint.x);
        pressAmount = Mathf.Clamp01(downDistance / Mathf.Max(ButtonTapMinDownDelta, 1e-4f));

        if (pressed)
        {
            if (downDistance <= ButtonTapMinDownDelta * 0.25f)
                pressed = false;
            else
                return false;
        }

        bool tapDown = (downDistance >= ButtonTapMinDownDelta && downSpeed >= ButtonTapMinDownSpeed)
            || downDistance >= ButtonTapMinDownDelta * 1.6f;
        bool sideStable = sideOffset <= radius * ButtonTapMaxSideOffsetFactor;
        bool canClick = tapDown
            && !pressed
            && sideStable
            && Time.time - lastPressTime >= cooldown;
        hasLastPoint = true;
        if (!canClick) return false;

        lastPressTime = Time.time;
        pressed = true;
        return true;
    }

    Vector3 ButtonClickPoint()
    {
        if (grasp != null && grasp.hand != null && grasp.hand.Points != null && grasp.hand.Points.Length > 8)
            return grasp.hand.Points[8];
        return grasp != null && grasp.hand != null ? grasp.hand.GripPoint : Vector3.zero;
    }

    void SetButtonGuide(Vector3 target)
    {
        _buttonGuideActive = true;
        _buttonGuideTarget = target;
    }

    void UpdateCursor()
    {
        if (grasp == null || grasp.hand == null || _cursor == null) return;
        var hand = grasp.hand;
        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _line.enabled = false;
            return;
        }

        float gripSignal = grasp.GripSignal;
        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.16f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.13f, 0.23f, gripSignal);

        Color color = grasp.Held != null
            ? new Color(0.16f, 0.88f, 0.35f)
            : grasp.Hover != null ? new Color(1f, 0.82f, 0.18f) : new Color(0.18f, 0.68f, 1f);
        SetRendererColor(_cursorRenderer, color);

        bool showLine = grasp.Held == null && (grasp.Hover != null || _buttonGuideActive);
        _line.enabled = showLine;
        if (!showLine) return;

        Vector3 target;
        if (grasp.Hover != null)
        {
            target = grasp.Hover.transform.position;
        }
        else
        {
            target = _buttonGuideTarget;
        }

        target += new Vector3(0f, 0f, -0.16f);
        _line.SetPosition(0, grip);
        _line.SetPosition(1, target);
    }

    void UpdateStatus()
    {
        if (_status == null || grasp == null || grasp.hand == null) return;

        int sorted = TotalSortedCount();
        string phase;
        if (!grasp.hand.IsActive) phase = "等待手势";
        else if (sorted < CheckpointTargetCount) phase = "连续分拣";
        else if (!_buttonPressed) phase = "点击班次确认";
        else phase = "班次确认完成";

        int handled = sorted + _dropCount;
        int score = handled == 0 ? 100 : Mathf.RoundToInt(sorted * 100f / handled);
        float beltSpeed = ConveyorCurrentSpeed();

        _status.text =
            "工位: " + phase +
            "\n累计 " + sorted + "  目标 " + CheckpointTargetCount +
            "\n箱数 " + BinCountSummary() +
            "\n皮带 " + beltSpeed.ToString("0.00") + " m/s" +
            "\n确认 " + (_buttonPressed ? "完成" : "未完成") +
            "\n误投/漏拣 " + _dropCount + "  正确率 " + score +
            "\n抓取 " + grasp.GripSignal.ToString("0.00");
    }

    void ClampBlocksToArea()
    {
        if (!clampBlocksToArea || grasp == null) return;
        float half = blockSize * 0.5f;
        foreach (var g in grasp.grabbables)
        {
            if (g == null || g.Body == null) continue;
            if (!g.gameObject.activeInHierarchy) continue;
            Vector3 p = g.Body.position;
            Vector3 clamped = new Vector3(
                Mathf.Clamp(p.x, areaMin.x + half, areaMax.x - half),
                Mathf.Clamp(p.y, areaMin.y + half, areaMax.y - half),
                0f);

            if ((clamped - p).sqrMagnitude < 0.0001f) continue;
            g.Body.position = clamped;
            g.Body.velocity = Vector3.zero;
            g.Body.angularVelocity = Vector3.zero;
        }
    }

    SortBin FindBinAtPosition(Vector3 position)
    {
        Vector2 p = new Vector2(position.x, position.y);
        foreach (var bin in _bins)
        {
            Vector2 center = new Vector2(bin.zoneCenter.x, bin.zoneCenter.y);
            if (Vector2.Distance(p, center) <= ZoneRadius)
                return bin;
        }
        return null;
    }

    int TotalSortedCount()
    {
        int count = 0;
        foreach (var bin in _bins)
            count += bin.count;
        return count;
    }

    string BinCountSummary()
    {
        if (_bins.Count == 0) return "0";
        string text = "";
        for (int i = 0; i < _bins.Count; i++)
        {
            if (i > 0) text += "  ";
            text += _bins[i].label + ":" + _bins[i].count;
        }
        return text;
    }

    BlockGoal FindGoal(Grabbable block)
    {
        foreach (var goal in _goals)
            if (goal.block == block) return goal;
        return null;
    }

    static void SetColor(GameObject go, Color color)
    {
        SetRendererColor(go.GetComponent<Renderer>(), color);
    }

    static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        return material;
    }
}

