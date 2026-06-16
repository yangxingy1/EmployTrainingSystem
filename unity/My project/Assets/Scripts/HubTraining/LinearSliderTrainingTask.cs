using UnityEngine;

/// <summary>
/// 横向滑块训练: 捏合滑块沿水平轨道移动到目标刻度，训练精细线性控制。
/// </summary>
public class LinearSliderTrainingTask : MonoBehaviour
{
    public HandInput hand;

    GameObject _sliderRoot;
    GameObject _handle;
    GameObject _cursor;
    Renderer _handleRenderer;
    Renderer _cursorRenderer;
    Renderer _targetMarkerRenderer;
    Renderer _readyLamp;
    LineRenderer _guideLine;
    TextMesh _status;
    TextMesh _targetLabel;

    float _currentX;
    float _targetX;
    float _sliderVelocity;
    float _grabOffsetX;
    float _holdTimer;
    float _lastSuccessAt = -99f;
    bool _hovering;
    bool _grabbed;
    bool _wasInTarget;
    int _success;

    readonly float[] _targetOptions = { -0.90f, -0.45f, 0f, 0.45f, 0.90f };

    const float TrackMinX = -1.25f;
    const float TrackMaxX = 1.25f;
    const float HoverRadius = 0.48f;
    const float GrabThreshold = 0.45f;
    const float ReleaseThreshold = 0.24f;
    const float DragSpeed = 8.5f;
    const float DragSmoothTime = 0.060f;
    const float TargetTolerance = 0.10f;
    const float HoldToConfirm = 0.34f;

    void Start()
    {
        BuildTitleAndStatus();
        BuildSlider();
        BuildCursorAndGuide();
        PickTarget();
        ApplySliderPosition();
        UpdateStatus();
    }

    void Update()
    {
        UpdateInteraction();
        UpdateCursorAndGuide();
        UpdateTargetCheck();
        UpdateStatus();
    }

    void BuildTitleAndStatus()
    {
        var titleGo = new GameObject("SliderTaskTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "横向滑块调节训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 56;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("SliderTaskStatus");
        statusGo.transform.parent = transform;
        statusGo.transform.position = new Vector3(3.05f, 1.22f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void BuildSlider()
    {
        _sliderRoot = new GameObject("LinearSliderStation");
        _sliderRoot.transform.parent = transform;
        _sliderRoot.transform.position = new Vector3(0f, -0.18f, 0f);

        CreateBox(_sliderRoot.transform, "SliderPanel", new Vector3(0f, -0.10f, 0.16f), new Vector3(3.35f, 1.38f, 0.16f), new Color(0.18f, 0.20f, 0.24f));
        CreateBox(_sliderRoot.transform, "SliderTrack", new Vector3(0f, 0f, -0.04f), new Vector3(2.76f, 0.16f, 0.08f), new Color(0.06f, 0.07f, 0.08f));
        CreateBox(_sliderRoot.transform, "SliderLeftStop", new Vector3(TrackMinX - 0.12f, 0f, -0.06f), new Vector3(0.10f, 0.52f, 0.10f), new Color(0.65f, 0.70f, 0.76f));
        CreateBox(_sliderRoot.transform, "SliderRightStop", new Vector3(TrackMaxX + 0.12f, 0f, -0.06f), new Vector3(0.10f, 0.52f, 0.10f), new Color(0.65f, 0.70f, 0.76f));

        for (int i = 0; i < _targetOptions.Length; i++)
        {
            float x = _targetOptions[i];
            CreateBox(_sliderRoot.transform, "SliderTick_" + i, new Vector3(x, -0.28f, -0.06f), new Vector3(0.045f, 0.18f, 0.06f), new Color(0.76f, 0.82f, 0.88f));

            var tickTextGo = new GameObject("SliderTickText_" + i);
            tickTextGo.transform.parent = _sliderRoot.transform;
            tickTextGo.transform.localPosition = new Vector3(x, -0.48f, -0.08f);
            var tickText = tickTextGo.AddComponent<TextMesh>();
            tickText.text = Mathf.RoundToInt(Mathf.InverseLerp(TrackMinX, TrackMaxX, x) * 100f).ToString();
            tickText.anchor = TextAnchor.MiddleCenter;
            tickText.alignment = TextAlignment.Center;
            tickText.fontSize = 24;
            tickText.characterSize = 0.026f;
            tickText.color = new Color(0.74f, 0.82f, 0.90f);
        }

        var target = CreateBox(_sliderRoot.transform, "SliderTargetMarker", new Vector3(0f, 0.34f, -0.10f), new Vector3(0.13f, 0.40f, 0.08f), new Color(0.25f, 1f, 0.45f));
        _targetMarkerRenderer = target.GetComponent<Renderer>();

        _handle = CreateBox(_sliderRoot.transform, "SliderHandle", new Vector3(0f, 0f, -0.20f), new Vector3(0.36f, 0.54f, 0.12f), new Color(0.18f, 0.58f, 1f));
        _handleRenderer = _handle.GetComponent<Renderer>();

        var grip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grip.name = "SliderGrip";
        grip.transform.parent = _handle.transform;
        grip.transform.localPosition = new Vector3(0f, 0f, -0.14f);
        grip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        grip.transform.localScale = new Vector3(0.22f, 0.045f, 0.22f);
        Destroy(grip.GetComponent<Collider>());
        SetColor(grip, new Color(0.88f, 0.94f, 1f));

        _readyLamp = CreateSphere(_sliderRoot.transform, "SliderReadyLamp", new Vector3(-1.38f, 0.45f, -0.08f), 0.22f, new Color(0.08f, 0.09f, 0.10f)).GetComponent<Renderer>();

        var targetTextGo = new GameObject("SliderTargetLabel");
        targetTextGo.transform.parent = _sliderRoot.transform;
        targetTextGo.transform.localPosition = new Vector3(0f, 0.70f, -0.08f);
        _targetLabel = targetTextGo.AddComponent<TextMesh>();
        _targetLabel.anchor = TextAnchor.MiddleCenter;
        _targetLabel.alignment = TextAlignment.Center;
        _targetLabel.fontSize = 36;
        _targetLabel.characterSize = 0.036f;
        _targetLabel.color = new Color(0.82f, 1f, 0.86f);
    }

    void BuildCursorAndGuide()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "SliderGripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("SliderGuideLine");
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.026f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void UpdateInteraction()
    {
        if (hand == null || !hand.IsActive || _handle == null)
        {
            _hovering = false;
            _grabbed = false;
            return;
        }

        Vector3 grip = hand.GripPoint;
        _hovering = Vector2.Distance(new Vector2(grip.x, grip.y), new Vector2(_handle.transform.position.x, _handle.transform.position.y)) <= HoverRadius;

        if (!_grabbed && _hovering && hand.PinchOnlyStrength >= GrabThreshold)
        {
            _grabbed = true;
            _grabOffsetX = _currentX - LocalGripX(grip);
        }

        if (_grabbed)
        {
            if (hand.PinchOnlyStrength <= ReleaseThreshold)
            {
                _grabbed = false;
            }
            else
            {
                float targetX = Mathf.Clamp(LocalGripX(grip) + _grabOffsetX, TrackMinX, TrackMaxX);
                _currentX = Mathf.SmoothDamp(_currentX, targetX, ref _sliderVelocity, DragSmoothTime, DragSpeed);
                ApplySliderPosition();
            }
        }
        else
        {
            _sliderVelocity = 0f;
        }

        Color handleColor = _grabbed
            ? new Color(0.20f, 0.90f, 0.40f)
            : _hovering ? new Color(1f, 0.78f, 0.18f) : new Color(0.18f, 0.58f, 1f);
        SetRendererColor(_handleRenderer, handleColor);
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

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.20f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.22f, hand.PinchOnlyStrength);

        Color cursorColor = _grabbed
            ? new Color(0.20f, 0.90f, 0.40f)
            : _hovering ? new Color(1f, 0.78f, 0.18f) : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, cursorColor);

        _guideLine.enabled = _hovering || _grabbed;
        if (!_guideLine.enabled) return;
        _guideLine.SetPosition(0, grip);
        _guideLine.SetPosition(1, _handle.transform.position + new Vector3(0f, 0f, -0.20f));
    }

    void UpdateTargetCheck()
    {
        float error = Mathf.Abs(_currentX - _targetX);
        bool inTarget = error <= TargetTolerance;
        SetRendererColor(_readyLamp, inTarget ? new Color(0.20f, 0.90f, 0.35f) : new Color(0.08f, 0.09f, 0.10f));
        SetRendererColor(_targetMarkerRenderer, inTarget ? new Color(0.20f, 0.90f, 0.35f) : new Color(0.25f, 1f, 0.45f));

        if (inTarget)
            _holdTimer += Time.deltaTime;
        else
            _holdTimer = 0f;

        if (inTarget && !_wasInTarget && _holdTimer >= HoldToConfirm)
        {
            _success++;
            TrainingFlowController.Active?.RecordSuccess("滑块校准达标: " + TargetPercent().ToString("0") + "%");
            _wasInTarget = true;
            _lastSuccessAt = Time.time;
            PickTarget();
        }
        else if (!inTarget)
        {
            _wasInTarget = false;
        }
    }

    void PickTarget()
    {
        float next = _targetOptions[Random.Range(0, _targetOptions.Length)];
        if (_targetOptions.Length > 1)
        {
            int guard = 0;
            while (Mathf.Approximately(next, _targetX) && guard++ < 8)
                next = _targetOptions[Random.Range(0, _targetOptions.Length)];
        }

        _targetX = next;
        _holdTimer = 0f;
        if (_targetMarkerRenderer != null)
            _targetMarkerRenderer.transform.localPosition = new Vector3(_targetX, 0.34f, -0.10f);
        if (_targetLabel != null)
            _targetLabel.text = "目标刻度: " + TargetPercent().ToString("0") + "%";
    }

    void ApplySliderPosition()
    {
        if (_handle == null) return;
        _handle.transform.localPosition = new Vector3(_currentX, 0f, -0.20f);
        _handle.transform.localRotation = Quaternion.identity;
    }

    float LocalGripX(Vector3 grip)
    {
        return _sliderRoot != null
            ? _sliderRoot.transform.InverseTransformPoint(grip).x
            : grip.x;
    }

    float SliderPercent()
    {
        return Mathf.InverseLerp(TrackMinX, TrackMaxX, _currentX) * 100f;
    }

    float TargetPercent()
    {
        return Mathf.InverseLerp(TrackMinX, TrackMaxX, _targetX) * 100f;
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null) return;
        string phase;
        if (!hand.IsActive) phase = "等待手势识别";
        else if (_grabbed) phase = "已抓住滑块, 横向拖动";
        else if (_hovering) phase = "捏合滑块开始调节";
        else phase = "移动光标到滑块";

        float error = Mathf.Abs(SliderPercent() - TargetPercent());
        string done = Time.time - _lastSuccessAt < 1.1f ? "\n目标已达成" : "";
        _status.text =
            "状态: " + phase +
            "\n目标: " + TargetPercent().ToString("0") + "%" +
            "\n当前: " + SliderPercent().ToString("0") + "%" +
            "\n误差: " + error.ToString("0.0") + "%" +
            "\n完成次数: " + _success +
            "\n捏合: " + hand.PinchOnlyStrength.ToString("0.00") +
            done;
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
