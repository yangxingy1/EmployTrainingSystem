using UnityEngine;

/// <summary>
/// 螺栓拧紧训练: 依次捏合目标螺栓并顺时针旋转，达到目标圈数后判定扭矩达标。
/// </summary>
public class BoltTighteningTrainingTask : MonoBehaviour
{
    struct TrackedRotation
    {
        float _baseAngle;
        float _currentOffset;
        float _accumulatedAngle;

        public float TotalOffset => _accumulatedAngle + _currentOffset;

        public void Reset()
        {
            _baseAngle = 0f;
            _currentOffset = 0f;
            _accumulatedAngle = 0f;
        }

        public void SetBaseFromVector(Vector3 direction)
        {
            _accumulatedAngle += _currentOffset;
            _baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _currentOffset = 0f;
        }

        public void SetTargetFromVector(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _currentOffset = ShortestAngleDistance(_baseAngle, targetAngle);
            if (Mathf.Abs(_currentOffset) > 90f)
            {
                _baseAngle = targetAngle;
                _accumulatedAngle += _currentOffset;
                _currentOffset = 0f;
            }
        }

        static float ShortestAngleDistance(float start, float end)
        {
            float delta = end - start;
            float sign = Mathf.Sign(delta);
            delta = Mathf.Abs(delta) % 360f;
            if (delta > 180f) delta = -(360f - delta);
            return delta * sign;
        }
    }

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
    LineRenderer _guideLine;
    TextMesh _status;
    TextMesh _targetLabel;

    int _targetIndex;
    int _completedSets;
    int _wrongDirectionCount;
    TrackedRotation _toolRotation;
    TrackedRotation _orbitRotation;
    float _lastTrackedRotation;
    float _reverseAccumulator;
    float _lastSuccessAt = -99f;
    bool _nearTarget;
    bool _tightening;
    bool _toolDriven;
    bool _orbitDriven;

    readonly Color _steel = new Color(0.64f, 0.68f, 0.72f);
    readonly Color _darkSteel = new Color(0.30f, 0.33f, 0.36f);
    readonly Color _blue = new Color(0.22f, 0.58f, 1f);
    readonly Color _yellow = new Color(1f, 0.78f, 0.16f);
    readonly Color _green = new Color(0.20f, 0.86f, 0.34f);
    readonly Color _red = new Color(0.95f, 0.18f, 0.10f);

    const float GrabRadius = 0.72f;
    const float GripThreshold = 0.42f;
    const float ReleaseThreshold = 0.24f;
    const float TightenDegrees = 320f;
    const float ToolTwistSensitivity = 1.25f;
    const float OrbitTwistSensitivity = 0.45f;
    const float TightenDirection = -1f;
    const float PositionTrackedRadius = 0.20f;
    const float MinToolVectorLength = 0.16f;
    const float MinStepDegrees = 0.16f;
    const float MaxStepDegrees = 18f;
    const float ReverseWarnDegrees = 28f;

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
            StopTightening();
            return;
        }

        BoltUnit bolt = CurrentBolt();
        Vector3 grip = hand.GripPoint;
        Vector3 offset = grip - bolt.center;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        _nearTarget = distance <= GrabRadius;
        bool gripping = hand.PinchOnlyStrength >= GripThreshold;

        if (_nearTarget && gripping)
        {
            if (!_tightening)
            {
                BeginTightening(bolt);
            }
            else
            {
                TrackBoltRotation(bolt);
            }
        }
        else
        {
            if (hand.PinchOnlyStrength <= ReleaseThreshold || !_nearTarget)
                StopTightening();
        }

        if (bolt.progress < TightenDegrees || bolt.done) return;

        bolt.done = true;
        _lastSuccessAt = Time.time;
        SelectNextBolt();
    }

    void BeginTightening(BoltUnit bolt)
    {
        _tightening = true;
        _toolRotation.Reset();
        _orbitRotation.Reset();
        _toolDriven = false;
        _orbitDriven = false;
        _reverseAccumulator = 0f;

        Vector3 toolVector = ToolTwistVector();
        if (toolVector.sqrMagnitude >= MinToolVectorLength * MinToolVectorLength)
        {
            _toolRotation.SetBaseFromVector(toolVector.normalized);
            _toolDriven = true;
        }

        Vector3 orbitVector = OrbitVector(bolt);
        if (orbitVector.sqrMagnitude >= PositionTrackedRadius * PositionTrackedRadius)
        {
            _orbitRotation.SetBaseFromVector(orbitVector.normalized);
            _orbitDriven = true;
        }

        _lastTrackedRotation = 0f;
    }

    void TrackBoltRotation(BoltUnit bolt)
    {
        Vector3 toolVector = ToolTwistVector();
        if (toolVector.sqrMagnitude >= MinToolVectorLength * MinToolVectorLength)
        {
            if (!_toolDriven)
            {
                _toolRotation.SetBaseFromVector(toolVector.normalized);
                _toolDriven = true;
            }
            _toolRotation.SetTargetFromVector(toolVector.normalized);
        }
        else
        {
            _toolDriven = false;
        }

        Vector3 orbitVector = OrbitVector(bolt);
        if (orbitVector.sqrMagnitude >= PositionTrackedRadius * PositionTrackedRadius)
        {
            if (!_orbitDriven)
            {
                _orbitRotation.SetBaseFromVector(orbitVector.normalized);
                _orbitDriven = true;
            }
            _orbitRotation.SetTargetFromVector(orbitVector.normalized);
        }
        else
        {
            _orbitDriven = false;
        }

        float tracked = TightenDirection * (_toolRotation.TotalOffset * ToolTwistSensitivity + _orbitRotation.TotalOffset * OrbitTwistSensitivity);
        float rawStep = tracked - _lastTrackedRotation;
        _lastTrackedRotation = tracked;

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
                TrainingFlowController.Active?.RecordMistake("螺栓出现反向旋转");
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
        _toolDriven = false;
        _orbitDriven = false;
        _lastTrackedRotation = 0f;
        _reverseAccumulator = 0f;
        _toolRotation.Reset();
        _orbitRotation.Reset();
    }

    Vector3 ToolTwistVector()
    {
        if (hand == null || hand.Points == null || hand.Points.Length <= 17)
            return Vector3.zero;

        Vector3 thumb = hand.Points[4];
        Vector3 index = hand.Points[8];
        Vector3 vector = index - thumb;
        vector.z = 0f;
        if (vector.sqrMagnitude >= MinToolVectorLength * MinToolVectorLength)
            return vector;

        vector = hand.Points[17] - hand.Points[5];
        vector.z = 0f;
        return vector;
    }

    Vector3 OrbitVector(BoltUnit bolt)
    {
        if (hand == null || bolt == null) return Vector3.zero;
        Vector3 vector = hand.GripPoint - bolt.center;
        vector.z = 0f;
        return vector;
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
            : _nearTarget ? _yellow : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, cursorColor);

        BoltUnit bolt = CurrentBolt();
        _guideLine.enabled = bolt != null && (_nearTarget || _tightening);
        if (!_guideLine.enabled) return;
        _guideLine.SetPosition(0, grip);
        _guideLine.SetPosition(1, bolt.center + new Vector3(0f, 0f, -0.18f));
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
        else if (_tightening) phase = (_toolDriven ? "拧动手指/手腕推进" : "围绕螺栓小幅旋转");
        else if (_nearTarget) phase = "捏合螺栓头开始拧紧";
        else phase = "移动光标到高亮螺栓";

        float pct = bolt == null ? 100f : Mathf.Clamp01(bolt.progress / TightenDegrees) * 100f;
        string done = Time.time - _lastSuccessAt < 1.1f ? "\n扭矩达标" : "";
        _status.text =
            "状态: " + phase +
            "\n当前目标: " + (bolt == null ? "完成" : (_targetIndex + 1).ToString()) +
            "\n扭矩进度: " + pct.ToString("0") + "%" +
            "\n完成轮次: " + _completedSets +
            "\n反向提示: " + _wrongDirectionCount +
            "\n捏合: " + hand.PinchOnlyStrength.ToString("0.00") +
            done;

        if (_targetLabel != null)
            _targetLabel.text = bolt == null ? "全部螺栓扭矩达标" : "目标: 拧紧 " + (_targetIndex + 1) + " 号螺栓";
    }

    void SelectNextBolt()
    {
        for (int i = 0; i < _bolts.Length; i++)
        {
            if (_bolts[i] == null || _bolts[i].done) continue;
            _targetIndex = i;
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
