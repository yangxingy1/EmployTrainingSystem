using UnityEngine;

public class TaskStation : MonoBehaviour
{
    public string taskId = "";
    public string displayName = "";
    public string sceneName = "";
    public int anchorId;
    public TaskStationState state = TaskStationState.Locked;

    static readonly Color ActiveBaseColor = new Color(0.12f, 0.42f, 0.68f);
    static readonly Color ActiveMarkerColor = new Color(0.18f, 0.82f, 1.0f);
    static readonly Color LockedBaseColor = new Color(0.30f, 0.31f, 0.33f);
    static readonly Color LockedMarkerColor = new Color(0.48f, 0.49f, 0.52f);

    Transform _labelTransform;

    void LateUpdate()
    {
        if (_labelTransform == null || Camera.main == null) return;

        var toCamera = Camera.main.transform.position - _labelTransform.position;
        if (toCamera.sqrMagnitude > 0.001f)
            _labelTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    public void Configure(AssignedTask task, int anchorId)
    {
        this.anchorId = anchorId;
        taskId = task.taskId;
        displayName = task.displayName;
        sceneName = task.sceneName;
        state = TaskStationState.Active;

        BuildVisual();
    }

    public void ConfigureLocked(int anchorId)
    {
        this.anchorId = anchorId;
        taskId = "";
        displayName = "Locked";
        sceneName = "";
        state = TaskStationState.Locked;

        BuildVisual();
    }

    void BuildVisual()
    {
        ClearChildren();

        var active = state == TaskStationState.Active;
        var baseColor = active ? ActiveBaseColor : LockedBaseColor;
        var markerColor = active ? ActiveMarkerColor : LockedMarkerColor;

        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseGo.name = "Station Base";
        baseGo.transform.SetParent(transform, false);
        baseGo.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        baseGo.transform.localScale = new Vector3(1.25f, 0.08f, 1.25f);
        SetColor(baseGo, baseColor);

        var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = active ? "Active Marker" : "Locked Marker";
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = new Vector3(0f, 0.82f, 0f);
        marker.transform.localScale = active ? new Vector3(0.42f, 1.35f, 0.42f) : new Vector3(0.52f, 0.95f, 0.52f);
        SetColor(marker, markerColor);

        var trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.75f, 0f);
        trigger.size = new Vector3(2.4f, 1.8f, 2.4f);

        CreateLabel(active ? displayName : "Locked", active ? Color.white : new Color(0.82f, 0.84f, 0.86f));
    }

    void CreateLabel(string text, Color color)
    {
        var labelGo = new GameObject("Station Label");
        labelGo.transform.SetParent(transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        _labelTransform = labelGo.transform;

        var label = labelGo.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.24f;
        label.fontSize = 54;
        label.color = color;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        var existingTrigger = GetComponent<BoxCollider>();
        if (existingTrigger != null) Destroy(existingTrigger);
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
