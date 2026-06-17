using UnityEngine;

/// <summary>
/// 螺栓拧紧训练: 依次套住目标螺栓，捏住扭矩扳手手柄并顺时针拉动，达到目标扭矩后判定达标。
/// </summary>
public class BoltTighteningTrainingTask : MonoBehaviour
{
    class BoltUnit
    {
        public GameObject root;
        public GameObject head;
        public Renderer headRenderer;
        public Renderer ringRenderer;
        public TextMesh label;
        public Vector3 center;
        public float progress;
        public float visualAngle;
        public bool done;
    }

    public HandInput hand;

    readonly BoltUnit[] _bolts = new BoltUnit[3];

    GameObject _cursor;
    Renderer _cursorRenderer;
    Renderer _torqueFillRenderer;
    Renderer _wrenchHandleRenderer;
    Renderer _wrenchSocketRenderer;
    Renderer _wrenchGripRenderer;
    GameObject _wrenchRoot;
    LineRenderer _guideLine;
    TextMesh _status;
    TextMesh _targetLabel;

    int _targetIndex;
    int _completedSets;
    int _wrongDirectionCount;
    float _lastWrenchAngle;
    float _reverseAccumulator;
    float _lastSuccessAt = -99f;
    float _wrenchAngle = WrenchStartAngle;
    bool _nearTarget;
    bool _handleNear;
    bool _tightening;

    readonly Color _steel = new Color(0.64f, 0.68f, 0.72f);
    readonly Color _darkSteel = new Color(0.30f, 0.33f, 0.36f);
    readonly Color _blue = new Color(0.22f, 0.58f, 1f);
    readonly Color _yellow = new Color(1f, 0.78f, 0.16f);
    readonly Color _green = new Color(0.20f, 0.86f, 0.34f);

    const float BoltGuideRadius = 1.55f;
    const float GripThreshold = 0.46f;
    const float ReleaseThreshold = 0.28f;
    const float TightenDegrees = 240f;
    const float TightenDirection = -1f;
    const float WrenchStartAngle = 42f;
    const float WrenchHandleLength = 1.22f;
    const float WrenchGripDistance = 1.18f;
    const float WrenchMinPullRadius = 0.52f;
    const float WrenchMaxPullRadius = 1.48f;
    const float HandleGrabRadius = 0.34f;
    const float MinStepDegrees = 0.18f;
    const float MaxStepDegrees = 22f;
    const float ReverseWarnDegrees = 55f;

    void Start()
    {
        BuildWorkpiece();
        BuildCursorAndGuide();
        BuildText();
        SelectNextBolt();
        UpdateStatus();
    }

    void Update()
    {
        UpdateInteraction();
        UpdateCursorAndGuide();
        UpdateVisuals();
        UpdateStatus();
    }

    void BuildWorkpiece()
    {
        var root = new GameObject("BoltTighteningStation");
        root.transform.parent = transform;
        root.transform.position = Vector3.zero;

        CreateBox(root.transform, "AssemblyPlate", new Vector3(0f, -0.05f, 0.14f), new Vector3(3.35f, 1.55f, 0.16f), new Color(0.18f, 0.20f, 0.23f));
        CreateBox(root.transform, "PlateTopEdge", new Vector3(0f, 0.82f, 0.02f), new Vector3(3.48f, 0.08f, 0.12f), _darkSteel);
        CreateBox(root.transform, "PlateBottomEdge", new Vector3(0f, -0.92f, 0.02f), new Vector3(3.48f, 0.08f, 0.12f), _darkSteel);

        Vector3[] centers =
        {
            new Vector3(-1.05f, 0.02f, -0.08f),
            new Vector3(0f, 0.02f, -0.08f),
            new Vector3(1.05f, 0.02f, -0.08f),
        };

        for (int i = 0; i < _bolts.Length; i++)
            _bolts[i] = BuildBolt(root.transform, i, centers[i]);

        CreateBox(root.transform, "TorqueRail", new Vector3(0f, -0.66f, -0.08f), new Vector3(2.20f, 0.06f, 0.06f), new Color(0.26f, 0.29f, 0.33f));
        var fill = CreateBox(root.transform, "TorqueFill", new Vector3(-1.08f, -0.66f, -0.11f), new Vector3(0.04f, 0.11f, 0.06f), _green);
        _torqueFillRenderer = fill.GetComponent<Renderer>();

        BuildWrench(root.transform);
    }

    void BuildWrench(Transform parent)
    {
        _wrenchRoot = new GameObject("TorqueWrench");
        _wrenchRoot.transform.parent = parent;

        var socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        socket.name = "TorqueWrenchSocket";
        socket.transform.parent = _wrenchRoot.transform;
        socket.transform.localPosition = new Vector3(0f, 0f, -0.15f);
        socket.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        socket.transform.localScale = new Vector3(0.36f, 0.035f, 0.36f);
        Destroy(socket.GetComponent<Collider>());
        _wrenchSocketRenderer = socket.GetComponent<Renderer>();
        SetRendererColor(_wrenchSocketRenderer, new Color(0.72f, 0.77f, 0.82f));

        var handle = CreateBox(_wrenchRoot.transform, "TorqueWrenchHandle",
            new Vector3(WrenchHandleLength * 0.52f, 0f, -0.15f),
            new Vector3(WrenchHandleLength, 0.105f, 0.055f),
            new Color(0.45f, 0.50f, 0.56f));
        _wrenchHandleRenderer = handle.GetComponent<Renderer>();

        var grip = CreateBox(_wrenchRoot.transform, "TorqueWrenchGrip",
            new Vector3(WrenchGripDistance, 0f, -0.17f),
            new Vector3(0.28f, 0.18f, 0.075f),
            new Color(0.10f, 0.14f, 0.18f));
        _wrenchGripRenderer = grip.GetComponent<Renderer>();

        CreateBox(_wrenchRoot.transform, "TorqueWrenchJawMark",
            new Vector3(0.16f, 0.115f, -0.16f),
            new Vector3(0.22f, 0.045f, 0.045f),
            new Color(0.90f, 0.95f, 1f));
        CreateBox(_wrenchRoot.transform, "TorqueWrenchJawMarkMirror",
            new Vector3(0.16f, -0.115f, -0.16f),
            new Vector3(0.22f, 0.045f, 0.045f),
            new Color(0.90f, 0.95f, 1f));

        UpdateWrenchPose(CurrentBolt(), true);
    }

    BoltUnit BuildBolt(Transform parent, int index, Vector3 center)
    {
        var boltRoot = new GameObject("Bolt_" + (index + 1));
        boltRoot.transform.parent = parent;
        boltRoot.transform.localPosition = center;

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "BoltSocket_" + (index + 1);
        ring.transform.parent = boltRoot.transform;
        ring.transform.localPosition = new Vector3(0f, 0f, 0.04f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = new Vector3(0.40f, 0.025f, 0.40f);
        Destroy(ring.GetComponent<Collider>());
        var ringRenderer = ring.GetComponent<Renderer>();
        SetRendererColor(ringRenderer, _darkSteel);

        var head = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        head.name = "BoltHead_" + (index + 1);
        head.transform.parent = boltRoot.transform;
        head.transform.localPosition = new Vector3(0f, 0f, -0.03f);
        head.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        head.transform.localScale = new Vector3(0.28f, 0.09f, 0.28f);
        Destroy(head.GetComponent<Collider>());
        var headRenderer = head.GetComponent<Renderer>();
        SetRendererColor(headRenderer, _steel);

        CreateBox(head.transform, "BoltSlot_" + (index + 1), new Vector3(0f, 0f, -0.11f), new Vector3(0.42f, 0.045f, 0.04f), new Color(0.12f, 0.13f, 0.15f));

        var labelGo = new GameObject("BoltLabel_" + (index + 1));
        labelGo.transform.parent = parent;
        labelGo.transform.localPosition = center + new Vector3(0f, -0.46f, -0.08f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = (index + 1).ToString();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 34;
        label.characterSize = 0.036f;
        label.color = new Color(0.82f, 0.90f, 1f);

        return new BoltUnit
        {
            root = boltRoot,
            head = head,
            headRenderer = headRenderer,
            ringRenderer = ringRenderer,
            label = label,
            center = boltRoot.transform.position,
        };
    }

    void BuildCursorAndGuide()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "BoltTighteningCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("BoltTighteningGuideLine");
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.026f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void BuildText()
    {
        var titleGo = new GameObject("BoltTaskTitle");
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "螺栓拧紧训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var targetGo = new GameObject("BoltTargetLabel");
        targetGo.transform.position = new Vector3(0f, 1.28f, -0.08f);
        _targetLabel = targetGo.AddComponent<TextMesh>();
        _targetLabel.anchor = TextAnchor.MiddleCenter;
        _targetLabel.alignment = TextAlignment.Center;
        _targetLabel.fontSize = 40;
        _targetLabel.characterSize = 0.040f;
        _targetLabel.color = new Color(0.82f, 1f, 0.86f);

        var statusGo = new GameObject("BoltTaskStatus");
        statusGo.transform.position = new Vector3(3.02f, 1.22f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateInteraction()
    {
        if (hand == null || !hand.IsActive || CurrentBolt() == null)
        {
            _nearTarget = false;
            _handleNear = false;
            StopTightening();
            return;
        }

        BoltUnit bolt = CurrentBolt();
        Vector3 grip = hand.GripPoint;
        Vector3 offset = grip - bolt.center;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        _nearTarget = distance <= BoltGuideRadius;
        _handleNear = DistanceXY(grip, WrenchGripPoint(bolt)) <= HandleGrabRadius;
        bool gripping = hand.PinchOnlyStrength >= GripThreshold;
        bool usablePullRadius = distance >= WrenchMinPullRadius && distance <= WrenchMaxPullRadius;

        if (_handleNear && gripping)
        {
            if (!_tightening)
            {
                BeginTightening(bolt);
            }
            else
            {
                TrackWrenchPull(bolt);
            }
        }
        else
        {
            if (_tightening && gripping && usablePullRadius)
            {
                TrackWrenchPull(bolt);
            }
            else if (hand.PinchOnlyStrength <= ReleaseThreshold || !usablePullRadius)
            {
                StopTightening();
            }
        }

        if (bolt.progress < TightenDegrees || bolt.done) return;

        bolt.done = true;
        _lastSuccessAt = Time.time;
        StopTightening();
        SelectNextBolt();
    }

    void BeginTightening(BoltUnit bolt)
    {
        _tightening = true;
        _reverseAccumulator = 0f;
        _wrenchAngle = AngleFromBolt(bolt, hand.GripPoint);
        _lastWrenchAngle = _wrenchAngle;
        UpdateWrenchPose(bolt, false);
    }

    void TrackWrenchPull(BoltUnit bolt)
    {
        float angle = AngleFromBolt(bolt, hand.GripPoint);
        float angleDelta = Mathf.DeltaAngle(_lastWrenchAngle, angle);
        _lastWrenchAngle = angle;
        _wrenchAngle = angle;
        UpdateWrenchPose(bolt, false);

        float rawStep = TightenDirection * angleDelta;

        if (Mathf.Abs(rawStep) < MinStepDegrees || Mathf.Abs(rawStep) > MaxStepDegrees)
            return;

        if (rawStep > 0f)
        {
            ApplyTightenStep(bolt, rawStep);
            _reverseAccumulator = Mathf.Max(0f, _reverseAccumulator - rawStep);
        }
        else
        {
            _reverseAccumulator += -rawStep;
            if (_reverseAccumulator >= ReverseWarnDegrees)
            {
                _wrongDirectionCount++;
                TrainingFlowController.Active?.RecordMistake("扳手反向回程过大");
                _reverseAccumulator = 0f;
            }
        }
    }

    void ApplyTightenStep(BoltUnit bolt, float step)
    {
        bolt.progress = Mathf.Min(TightenDegrees, bolt.progress + step);
        bolt.visualAngle += step;
        float pct = bolt.progress / TightenDegrees;
        float sink = Mathf.Lerp(0f, 0.12f, pct);
        bolt.head.transform.localPosition = new Vector3(0f, 0f, -0.03f + sink);
        bolt.head.transform.localRotation = Quaternion.Euler(90f, 0f, -bolt.visualAngle);
    }

    void StopTightening()
    {
        _tightening = false;
        _lastWrenchAngle = 0f;
        _reverseAccumulator = 0f;
        _wrenchAngle = WrenchStartAngle;
        UpdateWrenchPose(CurrentBolt(), true);
    }

    float AngleFromBolt(BoltUnit bolt, Vector3 point)
    {
        if (bolt == null) return WrenchStartAngle;
        Vector3 vector = point - bolt.center;
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }

    Vector3 WrenchGripPoint(BoltUnit bolt)
    {
        if (bolt == null) return Vector3.zero;
        float angle = _tightening ? _wrenchAngle : WrenchStartAngle;
        float radians = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
        return bolt.center + direction * WrenchGripDistance;
    }

    void UpdateWrenchPose(BoltUnit bolt, bool resetToStart)
    {
        if (_wrenchRoot == null) return;

        if (bolt == null)
        {
            _wrenchRoot.SetActive(false);
            return;
        }

        _wrenchRoot.SetActive(true);
        if (resetToStart) _wrenchAngle = WrenchStartAngle;
        _wrenchRoot.transform.position = bolt.center + new Vector3(0f, 0f, -0.04f);
        _wrenchRoot.transform.rotation = Quaternion.Euler(0f, 0f, _wrenchAngle);

        Color handle = _tightening
            ? _green
            : _handleNear ? _yellow : new Color(0.45f, 0.50f, 0.56f);
        SetRendererColor(_wrenchHandleRenderer, handle);
        SetRendererColor(_wrenchGripRenderer, _handleNear || _tightening ? _yellow : new Color(0.10f, 0.14f, 0.18f));
        SetRendererColor(_wrenchSocketRenderer, _tightening ? _green : new Color(0.72f, 0.77f, 0.82f));
    }

    static float DistanceXY(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
    }

    void UpdateCursorAndGuide()
    {
        if (_cursor == null || _guideLine == null || hand == null) return;

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _guideLine.enabled = false;
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.18f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.22f, hand.PinchOnlyStrength);

        Color cursorColor = _tightening
            ? _green
            : _handleNear ? _yellow : _nearTarget ? new Color(0.62f, 0.82f, 1f) : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, cursorColor);

        BoltUnit bolt = CurrentBolt();
        _guideLine.enabled = bolt != null && (_nearTarget || _handleNear || _tightening);
        if (!_guideLine.enabled) return;
        _guideLine.SetPosition(0, grip);
        _guideLine.SetPosition(1, WrenchGripPoint(bolt) + new Vector3(0f, 0f, -0.18f));
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < _bolts.Length; i++)
        {
            BoltUnit bolt = _bolts[i];
            if (bolt == null) continue;

            bool current = i == _targetIndex && !bolt.done;
            Color headColor = bolt.done
                ? _green
                : current ? (_tightening ? _green : _yellow) : _steel;
            SetRendererColor(bolt.headRenderer, headColor);
            SetRendererColor(bolt.ringRenderer, current ? _yellow : bolt.done ? _green : _darkSteel);
            if (bolt.label != null)
                bolt.label.color = current ? _yellow : bolt.done ? _green : new Color(0.82f, 0.90f, 1f);
        }

        BoltUnit currentBolt = CurrentBolt();
        UpdateWrenchPose(currentBolt, false);
        float pct = currentBolt == null ? 1f : Mathf.Clamp01(currentBolt.progress / TightenDegrees);
        if (_torqueFillRenderer != null)
        {
            float width = Mathf.Max(0.035f, 2.16f * pct);
            _torqueFillRenderer.transform.localScale = new Vector3(width, 0.11f, 0.06f);
            _torqueFillRenderer.transform.localPosition = new Vector3(-1.08f + width * 0.5f, -0.66f, -0.11f);
            SetRendererColor(_torqueFillRenderer, pct >= 1f ? _green : _blue);
        }
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null) return;

        BoltUnit bolt = CurrentBolt();
        string phase;
        if (!hand.IsActive) phase = "等待手势识别";
        else if (AllDone()) phase = "本轮螺栓已全部达标";
        else if (_tightening) phase = "顺时针拉动扭矩扳手";
        else if (_handleNear) phase = "捏合扳手手柄开始拧紧";
        else if (_nearTarget) phase = "移动到高亮扳手手柄";
        else phase = "移动光标到目标螺栓旁";

        float pct = bolt == null ? 100f : Mathf.Clamp01(bolt.progress / TightenDegrees) * 100f;
        string done = Time.time - _lastSuccessAt < 1.1f ? "\n扭矩达标" : "";
        _status.text =
            "状态: " + phase +
            "\n当前目标: " + (bolt == null ? "完成" : (_targetIndex + 1).ToString()) +
            "\n扭矩进度: " + pct.ToString("0") + "%" +
            "\n完成轮次: " + _completedSets +
            "\n反向/空回程: " + _wrongDirectionCount +
            "\n捏合: " + hand.PinchOnlyStrength.ToString("0.00") +
            done;

        if (_targetLabel != null)
            _targetLabel.text = bolt == null ? "全部螺栓扭矩达标" : "目标: 用扭矩扳手拧紧 " + (_targetIndex + 1) + " 号螺栓";
    }

    void SelectNextBolt()
    {
        for (int i = 0; i < _bolts.Length; i++)
        {
            if (_bolts[i] == null || _bolts[i].done) continue;
            _targetIndex = i;
            _wrenchAngle = WrenchStartAngle;
            UpdateWrenchPose(_bolts[i], true);
            return;
        }

        _completedSets++;
        TrainingFlowController.Active?.RecordSuccess("一组螺栓扭矩达标");
        Invoke(nameof(ResetBolts), 0.85f);
    }

    void ResetBolts()
    {
        for (int i = 0; i < _bolts.Length; i++)
        {
            BoltUnit bolt = _bolts[i];
            if (bolt == null) continue;
            bolt.progress = 0f;
            bolt.visualAngle = 0f;
            bolt.done = false;
            bolt.head.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            bolt.head.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        _targetIndex = 0;
        StopTightening();
        _nearTarget = false;
    }

    BoltUnit CurrentBolt()
    {
        if (_targetIndex < 0 || _targetIndex >= _bolts.Length) return null;
        BoltUnit bolt = _bolts[_targetIndex];
        return bolt != null && !bolt.done ? bolt : null;
    }

    bool AllDone()
    {
        foreach (BoltUnit bolt in _bolts)
            if (bolt != null && !bolt.done) return false;
        return true;
    }

    GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.localPosition = position;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;
        Destroy(box.GetComponent<Collider>());
        SetColor(box, color);
        return box;
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
