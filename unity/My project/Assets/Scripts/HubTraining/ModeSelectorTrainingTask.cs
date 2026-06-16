using UnityEngine;

/// <summary>
/// 档位选择旋钮训练: 捏合旋钮并旋转到指定离散档位。
/// </summary>
public class ModeSelectorTrainingTask : MonoBehaviour
{
    class ModeSlot
    {
        public string label;
        public float angle;
        public Renderer marker;
        public TextMesh text;
    }

    public HandInput hand;

    readonly ModeSlot[] _slots =
    {
        new ModeSlot { label = "OFF", angle = 225f },
        new ModeSlot { label = "手动", angle = 315f },
        new ModeSlot { label = "自动", angle = 45f },
        new ModeSlot { label = "复位", angle = 135f },
    };

    GameObject _knobRoot;
    GameObject _pointer;
    GameObject _cursor;
    Renderer _knobRenderer;
    Renderer _pointerRenderer;
    Renderer _cursorRenderer;
    Renderer _readyLamp;
    LineRenderer _guideLine;
    TextMesh _status;
    TextMesh _targetText;

    float _angle = 225f;
    float _lastHandAngle;
    float _holdTimer;
    float _lastSuccessAt = -99f;
    bool _near;
    bool _rotating;
    int _targetIndex = 2;
    int _success;

    readonly Color _panelColor = new Color(0.18f, 0.20f, 0.23f);
    readonly Color _metalColor = new Color(0.56f, 0.61f, 0.68f);
    readonly Color _blue = new Color(0.22f, 0.58f, 1f);
    readonly Color _green = new Color(0.20f, 0.86f, 0.34f);
    readonly Color _yellow = new Color(1f, 0.78f, 0.16f);
    readonly Color _lampDim = new Color(0.08f, 0.09f, 0.10f);

    const float KnobRadius = 0.88f;
    const float GrabRadius = 1.08f;
    const float GripThreshold = 0.44f;
    const float ReleaseThreshold = 0.24f;
    const float AngleDeadZone = 0.22f;
    const float MaxStepDegrees = 7.5f;
    const float SnapSpeed = 8f;
    const float TargetTolerance = 13f;
    const float HoldToConfirm = 0.38f;

    void Start()
    {
        BuildPanel();
        BuildCursorAndGuide();
        BuildText();
        PickTarget();
        ApplyKnobRotation();
        UpdateStatus();
    }

    void Update()
    {
        UpdateInteraction();
        UpdateSnapAndTarget();
        UpdateCursorAndGuide();
        UpdateVisuals();
        UpdateStatus();
    }

    void BuildPanel()
    {
        var root = new GameObject("ModeSelectorStation");
        root.transform.parent = transform;
        root.transform.position = Vector3.zero;

        CreateBox(root.transform, "ModePanel", new Vector3(0f, 0f, 0.16f), new Vector3(3.05f, 2.75f, 0.16f), _panelColor);

        var ringGo = new GameObject("ModeSelectorRing");
        ringGo.transform.parent = root.transform;
        var ring = ringGo.AddComponent<LineRenderer>();
        ring.positionCount = 73;
        ring.startWidth = 0.025f;
        ring.endWidth = 0.025f;
        ring.material = MakeMaterial(new Color(0.74f, 0.80f, 0.86f));
        for (int i = 0; i < ring.positionCount; i++)
        {
            float a = i / 72f * Mathf.PI * 2f;
            ring.SetPosition(i, new Vector3(Mathf.Cos(a) * KnobRadius, Mathf.Sin(a) * KnobRadius, -0.08f));
        }

        for (int i = 0; i < _slots.Length; i++)
            BuildSlotMarker(root.transform, _slots[i], i);

        _knobRoot = new GameObject("ModeSelectorKnob");
        _knobRoot.transform.parent = root.transform;
        _knobRoot.transform.localPosition = new Vector3(0f, 0f, -0.16f);

        var knob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        knob.name = "ModeKnobBody";
        knob.transform.parent = _knobRoot.transform;
        knob.transform.localPosition = Vector3.zero;
        knob.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        knob.transform.localScale = new Vector3(0.48f, 0.08f, 0.48f);
        Destroy(knob.GetComponent<Collider>());
        _knobRenderer = knob.GetComponent<Renderer>();
        SetRendererColor(_knobRenderer, _blue);

        _pointer = CreateBox(_knobRoot.transform, "ModeKnobPointer", new Vector3(0.29f, 0f, -0.08f), new Vector3(0.58f, 0.07f, 0.07f), _yellow);
        _pointerRenderer = _pointer.GetComponent<Renderer>();

        _readyLamp = CreateSphere(root.transform, "ModeReadyLamp", new Vector3(-1.12f, -1.03f, -0.08f), 0.24f, _lampDim).GetComponent<Renderer>();
    }

    void BuildSlotMarker(Transform parent, ModeSlot slot, int index)
    {
        Vector3 direction = AngleToDirection(slot.angle);
        Vector3 markerPos = direction * KnobRadius;
        var marker = CreateBox(parent, "ModeSlot_" + index, markerPos + new Vector3(0f, 0f, -0.08f), new Vector3(0.18f, 0.12f, 0.06f), _metalColor);
        marker.transform.localRotation = Quaternion.Euler(0f, 0f, slot.angle);
        slot.marker = marker.GetComponent<Renderer>();

        var labelGo = new GameObject("ModeSlotLabel_" + index);
        labelGo.transform.parent = parent;
        labelGo.transform.localPosition = direction * 1.18f + new Vector3(0f, 0f, -0.08f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = slot.label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 32;
        text.characterSize = 0.034f;
        text.color = new Color(0.82f, 0.90f, 1f);
        slot.text = text;
    }

    void BuildCursorAndGuide()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "ModeSelectorCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("ModeSelectorGuideLine");
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.026f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void BuildText()
    {
        var titleGo = new GameObject("ModeSelectorTitle");
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "档位选择旋钮训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 56;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var targetGo = new GameObject("ModeSelectorTargetText");
        targetGo.transform.position = new Vector3(0f, -1.46f, -0.08f);
        _targetText = targetGo.AddComponent<TextMesh>();
        _targetText.anchor = TextAnchor.MiddleCenter;
        _targetText.alignment = TextAlignment.Center;
        _targetText.fontSize = 40;
        _targetText.characterSize = 0.040f;
        _targetText.color = new Color(0.82f, 1f, 0.86f);

        var statusGo = new GameObject("ModeSelectorStatus");
        statusGo.transform.position = new Vector3(3.05f, 1.24f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateInteraction()
    {
        if (hand == null || !hand.IsActive)
        {
            _near = false;
            _rotating = false;
            return;
        }

        Vector3 grip = hand.GripPoint;
        Vector3 offset = grip - _knobRoot.transform.position;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        _near = distance <= GrabRadius;
        bool gripping = hand.PinchOnlyStrength >= GripThreshold;

        if (_near && gripping)
        {
            float handAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (!_rotating)
            {
                _rotating = true;
                _lastHandAngle = handAngle;
            }
            else
            {
                float step = Mathf.DeltaAngle(_lastHandAngle, handAngle);
                if (Mathf.Abs(step) >= AngleDeadZone)
                    _angle = Mathf.Repeat(_angle + Mathf.Clamp(step, -MaxStepDegrees, MaxStepDegrees), 360f);
                _lastHandAngle = handAngle;
            }
        }
        else
        {
            _rotating = false;
            if (hand.PinchOnlyStrength <= ReleaseThreshold)
                _lastHandAngle = 0f;
        }

        ApplyKnobRotation();
    }

    void UpdateSnapAndTarget()
    {
        int nearest = NearestSlotIndex(_angle);
        float nearestAngle = _slots[nearest].angle;
        if (!_rotating)
        {
            float delta = Mathf.DeltaAngle(_angle, nearestAngle);
            _angle = Mathf.Repeat(_angle + Mathf.Clamp(delta, -SnapSpeed * Time.deltaTime * 60f, SnapSpeed * Time.deltaTime * 60f), 360f);
            ApplyKnobRotation();
        }

        bool correct = nearest == _targetIndex && Mathf.Abs(Mathf.DeltaAngle(_angle, _slots[_targetIndex].angle)) <= TargetTolerance;
        if (correct)
            _holdTimer += Time.deltaTime;
        else
            _holdTimer = 0f;

        if (correct && _holdTimer >= HoldToConfirm)
        {
            _success++;
            TrainingFlowController.Active?.RecordSuccess("档位选择正确: " + _slots[_targetIndex].label);
            _lastSuccessAt = Time.time;
            PickTarget();
        }
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

        Color cursorColor = _rotating
            ? _green
            : _near ? _yellow : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, cursorColor);

        _guideLine.enabled = _near || _rotating;
        if (!_guideLine.enabled) return;
        _guideLine.SetPosition(0, grip);
        _guideLine.SetPosition(1, _knobRoot.transform.position + new Vector3(0f, 0f, -0.18f));
    }

    void UpdateVisuals()
    {
        int nearest = NearestSlotIndex(_angle);
        bool correct = nearest == _targetIndex && Mathf.Abs(Mathf.DeltaAngle(_angle, _slots[_targetIndex].angle)) <= TargetTolerance;

        SetRendererColor(_knobRenderer, _rotating ? _green : _near ? _yellow : _blue);
        SetRendererColor(_pointerRenderer, correct ? _green : _yellow);
        SetRendererColor(_readyLamp, correct ? _green : _lampDim);

        for (int i = 0; i < _slots.Length; i++)
        {
            Color markerColor = i == _targetIndex
                ? new Color(0.25f, 1f, 0.45f)
                : i == nearest ? _yellow : _metalColor;
            SetRendererColor(_slots[i].marker, markerColor);
            if (_slots[i].text != null)
                _slots[i].text.color = i == _targetIndex ? new Color(0.82f, 1f, 0.86f) : new Color(0.82f, 0.90f, 1f);
        }
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null) return;

        int nearest = NearestSlotIndex(_angle);
        string phase;
        if (!hand.IsActive) phase = "等待手势识别";
        else if (_rotating) phase = "正在旋转档位旋钮";
        else if (_near) phase = "捏合旋钮并旋转";
        else phase = "移动光标到旋钮";

        string done = Time.time - _lastSuccessAt < 1.1f ? "\n档位选择正确" : "";
        _status.text =
            "目标: " + _slots[_targetIndex].label +
            "\n当前: " + _slots[nearest].label +
            "\n状态: " + phase +
            "\n角度: " + _angle.ToString("0.0") +
            "\n完成次数: " + _success +
            "\n捏合: " + hand.PinchOnlyStrength.ToString("0.00") +
            done;
    }

    void PickTarget()
    {
        int next = Random.Range(0, _slots.Length);
        if (_slots.Length > 1)
        {
            int guard = 0;
            while (next == _targetIndex && guard++ < 8)
                next = Random.Range(0, _slots.Length);
        }

        _targetIndex = next;
        _holdTimer = 0f;
        if (_targetText != null)
            _targetText.text = "目标档位: " + _slots[_targetIndex].label;
    }

    int NearestSlotIndex(float angle)
    {
        int best = 0;
        float bestDelta = float.MaxValue;
        for (int i = 0; i < _slots.Length; i++)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(angle, _slots[i].angle));
            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = i;
            }
        }
        return best;
    }

    void ApplyKnobRotation()
    {
        if (_knobRoot != null)
            _knobRoot.transform.localRotation = Quaternion.Euler(0f, 0f, _angle);
    }

    static Vector3 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
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

    GameObject CreateSphere(Transform parent, string name, Vector3 position, float size, Color color)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.parent = parent;
        sphere.transform.localPosition = position;
        sphere.transform.localRotation = Quaternion.identity;
        sphere.transform.localScale = Vector3.one * size;
        Destroy(sphere.GetComponent<Collider>());
        SetColor(sphere, color);
        return sphere;
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
