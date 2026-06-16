using UnityEngine;

/// <summary>
/// 基础按钮点击训练: 根据提示点击正确的工位按钮。
/// </summary>
public class ButtonPressTrainingTask : MonoBehaviour
{
    public HandInput hand;
    public GraspController grasp;

    readonly string[] _labels = { "START", "RESET", "CONFIRM", "STOP" };
    readonly FingertipTapButton[] _buttons = new FingertipTapButton[4];
    TextMesh _status;
    int _targetIndex;
    int _success;
    int _mistake;

    void Start()
    {
        BuildTitle();
        BuildButtons();
        PickNextTarget();
        UpdateStatus("点击高亮提示的按钮");
    }

    void BuildTitle()
    {
        var titleGo = new GameObject("ButtonTaskTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 1.72f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "按钮点击训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 54;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("ButtonTaskStatus");
        statusGo.transform.parent = transform;
        statusGo.transform.position = new Vector3(2.85f, 1.24f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void BuildButtons()
    {
        Vector3[] positions =
        {
            new Vector3(-1.25f, 0.55f, 0f),
            new Vector3(1.25f, 0.55f, 0f),
            new Vector3(-1.25f, -0.35f, 0f),
            new Vector3(1.25f, -0.35f, 0f),
        };

        Color[] colors =
        {
            new Color(0.12f, 0.48f, 0.92f),
            new Color(0.86f, 0.35f, 0.24f),
            new Color(0.18f, 0.70f, 0.38f),
            new Color(0.82f, 0.58f, 0.12f),
        };

        for (int i = 0; i < _buttons.Length; i++)
        {
            int index = i;
            var go = new GameObject("TrainingButton_" + _labels[i]);
            go.transform.parent = transform;
            var button = go.AddComponent<FingertipTapButton>();
            button.hand = hand;
            button.grasp = grasp;
            button.Build(positions[i], new Vector3(1.25f, 0.42f, 0.09f), _labels[i], colors[i]);
            button.Clicked += () => HandleButton(index);
            _buttons[i] = button;
        }
    }

    void PickNextTarget()
    {
        _targetIndex = Random.Range(0, _labels.Length);
        for (int i = 0; i < _buttons.Length; i++)
            if (_buttons[i] != null)
                _buttons[i].idleColor = i == _targetIndex
                    ? new Color(0.12f, 0.66f, 0.95f)
                    : new Color(0.28f, 0.32f, 0.40f);
    }

    void HandleButton(int index)
    {
        if (index == _targetIndex)
        {
            _success++;
            TrainingFlowController.Active?.RecordSuccess("正确点击: " + _labels[index]);
            PickNextTarget();
            UpdateStatus("正确, 继续点击下一按钮");
        }
        else
        {
            _mistake++;
            TrainingFlowController.Active?.RecordMistake("误触按钮: " + _labels[index]);
            UpdateStatus("误触: 请看清目标按钮");
        }
    }

    void UpdateStatus(string message)
    {
        if (_status == null) return;
        int total = _success + _mistake;
        int accuracy = total == 0 ? 100 : Mathf.RoundToInt(_success * 100f / total);
        _status.text =
            "任务: " + message +
            "\n目标: " + _labels[_targetIndex] +
            "\n正确: " + _success +
            "\n误触: " + _mistake +
            "\n正确率: " + accuracy;
    }
}

/// <summary>
/// 摄像头平面按钮: 食指进入按钮只算悬停,稳定后向下点按才触发一次。
/// </summary>
public class FingertipTapButton : MonoBehaviour
{
    public HandInput hand;
    public GraspController grasp;
    public bool requireFreeHand = true;
    public bool interactable = true;
    public System.Action Clicked;

    public Color idleColor = new Color(0.16f, 0.36f, 0.76f);
    public Color hoverColor = new Color(0.98f, 0.72f, 0.18f);
    public Color pressedColor = new Color(0.20f, 0.82f, 0.38f);
    public Color disabledColor = new Color(0.24f, 0.28f, 0.34f);

    GameObject _button;
    Renderer _renderer;
    LineRenderer _guideLine;
    Vector3 _size = new Vector3(1.18f, 0.34f, 0.08f);
    Vector3 _lastPoint;
    bool _armed;
    bool _ready;
    bool _pressed;
    bool _hasLastPoint;
    float _visualPressAmount;
    float _hoverStartTime = -99f;
    float _lastPressTime = -99f;

    const float HoverWidthFactor = 1.12f;
    const float HoverHeightFactor = 1.18f;
    const float ClickWidthFactor = 0.90f;
    const float ClickHeightFactor = 0.88f;
    const float TapReadySeconds = 0.11f;
    const float TapMinDownDelta = 0.028f;
    const float TapMinDownSpeed = 0.18f;
    const float TapMaxSideOffsetFactor = 0.62f;
    const float TapStabilizeMaxDrift = 0.060f;
    const float CooldownSeconds = 0.45f;

    public void Build(Vector3 center, Vector3 size, string label, Color color)
    {
        transform.position = center;
        _size = size;
        idleColor = color;

        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = label + "_Base";
        baseGo.transform.parent = transform;
        baseGo.transform.localPosition = new Vector3(0f, -size.y * 0.32f, 0.04f);
        baseGo.transform.localScale = new Vector3(size.x * 1.08f, size.y * 0.32f, size.z);
        Destroy(baseGo.GetComponent<Collider>());
        SetColor(baseGo.GetComponent<Renderer>(), Color.Lerp(color, Color.black, 0.35f));

        _button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _button.name = label + "_Button";
        _button.transform.parent = transform;
        _button.transform.localPosition = Vector3.zero;
        _button.transform.localScale = size;
        Destroy(_button.GetComponent<Collider>());
        _renderer = _button.GetComponent<Renderer>();
        SetColor(_renderer, idleColor);

        var labelGo = new GameObject(label + "_Label");
        labelGo.transform.parent = transform;
        labelGo.transform.localPosition = new Vector3(0f, 0f, -0.08f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 38;
        text.characterSize = Mathf.Min(0.040f, size.x / Mathf.Max(label.Length, 1) * 0.42f);
        text.color = Color.white;

        var lineGo = new GameObject(label + "_GuideLine");
        lineGo.transform.parent = transform;
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.020f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void Update()
    {
        if (_button == null || _renderer == null) return;

        bool click = UpdateTap(out bool near, out float pressAmount);
        if (!interactable)
        {
            SetColor(_renderer, disabledColor);
            _button.transform.localScale = _size;
            UpdateGuide(false);
            return;
        }

        if (click) Clicked?.Invoke();

        _visualPressAmount = Mathf.Lerp(_visualPressAmount, pressAmount, 1f - Mathf.Exp(-Time.deltaTime * 16f));

        Color color = click || _visualPressAmount > 0.01f
            ? pressedColor
            : near ? hoverColor : idleColor;
        SetColor(_renderer, color);

        float pressY = Mathf.Lerp(near ? _size.y * 0.72f : _size.y, _size.y * 0.48f, _visualPressAmount);
        _button.transform.localScale = new Vector3(_size.x, pressY, _size.z);
        UpdateGuide(near);
    }

    bool UpdateTap(out bool near, out float pressAmount)
    {
        near = false;
        pressAmount = 0f;
        if (!interactable || hand == null || !hand.IsActive)
        {
            ResetTap();
            return false;
        }

        Vector3 point = ButtonPoint();
        Vector3 center = transform.position;
        float halfW = _size.x * 0.5f;
        float halfH = _size.y * 0.5f;
        float dx = Mathf.Abs(point.x - center.x);
        float dy = Mathf.Abs(point.y - center.y);
        bool inHover = dx <= halfW * HoverWidthFactor && dy <= halfH * HoverHeightFactor;
        bool inClick = dx <= halfW * ClickWidthFactor && dy <= halfH * ClickHeightFactor;
        near = inHover;

        bool eligible = !requireFreeHand || grasp == null || grasp.Held == null;
        if (!eligible || !inHover)
        {
            ResetTap();
            return false;
        }

        if (!_armed)
        {
            _armed = true;
            _ready = false;
            _pressed = false;
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.16f;
            return false;
        }

        if (!inClick)
        {
            _ready = false;
            _pressed = false;
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.22f;
            return false;
        }

        if (!_hasLastPoint)
        {
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.16f;
            return false;
        }

        if (!_ready)
        {
            if (Time.time - _hoverStartTime < TapReadySeconds)
            {
                Vector3 settleDelta = point - _lastPoint;
                if (Mathf.Abs(settleDelta.x) > TapStabilizeMaxDrift || Mathf.Abs(settleDelta.y) > TapStabilizeMaxDrift)
                {
                    _hoverStartTime = Time.time;
                    _lastPoint = point;
                }
                pressAmount = 0.16f;
                return false;
            }

            _ready = true;
            _lastPoint = point;
            pressAmount = 0.16f;
            return false;
        }

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 frameDelta = point - _lastPoint;
        if (!_pressed && point.y > _lastPoint.y)
            _lastPoint = point;

        float downDistance = _lastPoint.y - point.y;
        float downSpeed = Mathf.Max(0f, -frameDelta.y / dt);
        float sideOffset = Mathf.Abs(point.x - _lastPoint.x);
        pressAmount = Mathf.Clamp01(downDistance / Mathf.Max(TapMinDownDelta, 1e-4f));

        if (_pressed)
        {
            if (downDistance <= TapMinDownDelta * 0.25f)
                _pressed = false;
            else
                return false;
        }

        bool tapDown = (downDistance >= TapMinDownDelta && downSpeed >= TapMinDownSpeed)
            || downDistance >= TapMinDownDelta * 1.6f;
        bool sideStable = sideOffset <= halfW * TapMaxSideOffsetFactor;
        bool canClick = tapDown
            && !_pressed
            && sideStable
            && Time.time - _lastPressTime >= CooldownSeconds;
        _hasLastPoint = true;
        if (!canClick) return false;

        _lastPressTime = Time.time;
        _pressed = true;
        return true;
    }

    Vector3 ButtonPoint()
    {
        if (hand.Points != null && hand.Points.Length > 8)
            return hand.Points[8];
        return hand.GripPoint;
    }

    void UpdateGuide(bool near)
    {
        if (_guideLine == null) return;
        bool show = near && interactable && hand != null && hand.IsActive;
        _guideLine.enabled = show;
        if (!show) return;

        Vector3 point = ButtonPoint() + new Vector3(0f, 0f, -0.14f);
        Vector3 target = transform.position + new Vector3(0f, 0f, -0.14f);
        _guideLine.SetPosition(0, point);
        _guideLine.SetPosition(1, target);
    }

    void ResetTap()
    {
        _armed = false;
        _ready = false;
        _pressed = false;
        _hasLastPoint = false;
        _visualPressAmount = Mathf.Lerp(_visualPressAmount, 0f, 1f - Mathf.Exp(-Time.deltaTime * 14f));
        _hoverStartTime = -99f;
    }

    static void SetColor(Renderer renderer, Color color)
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
