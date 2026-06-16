using UnityEngine;

/// <summary>
/// 基础阀门旋转训练: 捏合阀门并旋转到目标角度。
/// </summary>
public class ValveRotateTrainingTask : MonoBehaviour
{
    public GraspController grasp;

    GameObject _valveRoot;
    Renderer _valveRenderer;
    LineRenderer _guideLine;
    TextMesh _status;
    float _angle;
    float _visualAngle;
    float _angleVelocity;
    float _targetAngle = 90f;
    float _lastPointerAngle;
    Vector3 _lastGripPoint;
    bool _rotating;
    bool _wasInTarget;
    int _success;

    const float ValveRadius = 0.95f;
    const float OuterInputRadius = 0.24f;
    const float GripThreshold = 0.25f;
    const float DragDegreesPerUnit = 130f;
    const float InputDeadZone = 0.004f;
    const float AngleDeadZone = 0.20f;
    const float MaxStepDegrees = 6f;
    const float VisualSmoothTime = 0.055f;
    const float Tolerance = 10f;

    void Start()
    {
        BuildTitle();
        BuildValve(Vector3.zero);
        PickTarget();
    }

    void Update()
    {
        UpdateValve();
        UpdateGuideLine();
        UpdateStatus();
    }

    void BuildTitle()
    {
        var titleGo = new GameObject("ValveTaskTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 1.72f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "阀门旋转训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 54;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("ValveTaskStatus");
        statusGo.transform.parent = transform;
        statusGo.transform.position = new Vector3(2.85f, 1.20f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void BuildValve(Vector3 center)
    {
        _valveRoot = new GameObject("ValveTrainingWheel");
        _valveRoot.transform.parent = transform;
        _valveRoot.transform.position = center;

        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "ValveWheel";
        wheel.transform.parent = _valveRoot.transform;
        wheel.transform.localPosition = Vector3.zero;
        wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        wheel.transform.localScale = new Vector3(0.56f, 0.035f, 0.56f);
        Destroy(wheel.GetComponent<Collider>());
        _valveRenderer = wheel.GetComponent<Renderer>();
        SetColor(_valveRenderer, new Color(0.96f, 0.52f, 0.16f));

        var pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.name = "ValvePointer";
        pointer.transform.parent = _valveRoot.transform;
        pointer.transform.localPosition = new Vector3(0.26f, 0f, -0.08f);
        pointer.transform.localScale = new Vector3(0.52f, 0.055f, 0.07f);
        Destroy(pointer.GetComponent<Collider>());
        SetColor(pointer.GetComponent<Renderer>(), new Color(1f, 0.90f, 0.22f));

        var ringGo = new GameObject("ValveInputRing");
        ringGo.transform.parent = transform;
        var ring = ringGo.AddComponent<LineRenderer>();
        ring.positionCount = 65;
        ring.startWidth = 0.018f;
        ring.endWidth = 0.018f;
        ring.material = MakeMaterial(new Color(1f, 0.82f, 0.35f));
        for (int i = 0; i < ring.positionCount; i++)
        {
            float a = i / 64f * Mathf.PI * 2f;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(a) * ValveRadius, Mathf.Sin(a) * ValveRadius, -0.09f));
        }

        var guideGo = new GameObject("ValveGuideLine");
        guideGo.transform.parent = transform;
        _guideLine = guideGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.026f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void PickTarget()
    {
        float[] targets = { 45f, 90f, 135f, 180f, 225f, 270f };
        _targetAngle = targets[Random.Range(0, targets.Length)];
    }

    void UpdateValve()
    {
        if (grasp == null || grasp.hand == null || _valveRoot == null) return;

        Vector3 center = _valveRoot.transform.position;
        Vector3 offset = grasp.hand.GripPoint - center;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        bool near = grasp.hand.IsActive && distance <= ValveRadius;
        bool active = near && grasp.Held == null && grasp.GripSignal >= GripThreshold;

        if (active)
        {
            if (!_rotating)
            {
                _rotating = true;
                _lastGripPoint = grasp.hand.GripPoint;
                _lastPointerAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            }
            else
            {
                float step;
                if (distance >= OuterInputRadius)
                {
                    float pointerAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                    step = Mathf.DeltaAngle(_lastPointerAngle, pointerAngle);
                    _lastPointerAngle = pointerAngle;
                }
                else
                {
                    Vector3 delta = grasp.hand.GripPoint - _lastGripPoint;
                    float input = delta.x + delta.y;
                    step = Mathf.Abs(input) >= InputDeadZone ? input * DragDegreesPerUnit : 0f;
                }

                if (Mathf.Abs(step) >= AngleDeadZone)
                    _angle = Mathf.Repeat(_angle + Mathf.Clamp(step, -MaxStepDegrees, MaxStepDegrees), 360f);
                _lastGripPoint = grasp.hand.GripPoint;
            }
        }
        else
        {
            _rotating = false;
        }

        _visualAngle = Mathf.SmoothDampAngle(_visualAngle, _angle, ref _angleVelocity, VisualSmoothTime);
        _valveRoot.transform.rotation = Quaternion.Euler(0f, 0f, _visualAngle);
        bool ok = Mathf.Abs(Mathf.DeltaAngle(_angle, _targetAngle)) <= Tolerance;
        if (ok && !_wasInTarget)
        {
            _success++;
            TrainingFlowController.Active?.RecordSuccess("阀门角度达标: " + _targetAngle.ToString("0") + "°");
            _wasInTarget = true;
            PickTarget();
        }
        else if (!ok)
        {
            _wasInTarget = false;
        }

        Color color = _rotating
            ? new Color(1f, 0.86f, 0.18f)
            : ok ? new Color(0.20f, 0.88f, 0.38f) : new Color(0.96f, 0.52f, 0.16f);
        SetColor(_valveRenderer, color);
    }

    void UpdateGuideLine()
    {
        if (_guideLine == null) return;
        if (grasp == null || grasp.hand == null || _valveRoot == null || !grasp.hand.IsActive)
        {
            _guideLine.enabled = false;
            return;
        }

        Vector3 center = _valveRoot.transform.position;
        Vector3 handPoint = ValveGuidePoint();
        Vector3 offset = handPoint - center;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        bool show = _rotating || distance <= ValveRadius * 1.75f;
        _guideLine.enabled = show;
        if (!show) return;

        Vector3 direction = distance > 0.001f
            ? new Vector3(offset.x, offset.y, 0f).normalized
            : Vector3.right;
        Vector3 target = center + direction * Mathf.Min(ValveRadius * 0.72f, Mathf.Max(distance, ValveRadius * 0.35f));
        _guideLine.SetPosition(0, handPoint + new Vector3(0f, 0f, -0.16f));
        _guideLine.SetPosition(1, target + new Vector3(0f, 0f, -0.16f));
    }

    Vector3 ValveGuidePoint()
    {
        if (grasp != null && grasp.hand != null && grasp.hand.Points != null && grasp.hand.Points.Length > 8)
            return grasp.hand.Points[8];
        return grasp != null && grasp.hand != null ? grasp.hand.GripPoint : Vector3.zero;
    }

    void UpdateStatus()
    {
        if (_status == null) return;
        float error = Mathf.Abs(Mathf.DeltaAngle(_angle, _targetAngle));
        _status.text =
            "目标角度: " + _targetAngle.ToString("0") +
            "\n当前角度: " + _angle.ToString("0.0") +
            "\n误差: " + error.ToString("0.0") +
            "\n完成次数: " + _success +
            "\n状态: " + (_rotating ? "调节中" : "等待捏合旋转");
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
