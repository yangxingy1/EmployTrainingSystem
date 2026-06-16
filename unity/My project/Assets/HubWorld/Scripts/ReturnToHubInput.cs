using UnityEngine;

public class ReturnToHubInput : MonoBehaviour
{
    GUIStyle _buttonStyle;
    GUIStyle _hintStyle;
    Texture2D _buttonTexture;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
            SceneFlow.EnsureExists().ReturnToHub();
    }

    void OnGUI()
    {
        EnsureStyles();

        var label = ResolveReturnLabel();

        var buttonRect = new Rect(18f, 18f, 186f, 42f);
        if (GUI.Button(buttonRect, label, _buttonStyle))
            SceneFlow.EnsureExists().ReturnToHub();

        GUI.Label(new Rect(214f, 27f, 260f, 24f), "快捷键 R / Esc", _hintStyle);
    }

    static string ResolveReturnLabel()
    {
        var session = SessionManager.Instance;
        var sceneName = session != null ? session.returnSceneName : "";
        if (string.IsNullOrEmpty(sceneName) || sceneName == "freshTrain")
            return "返回自由练习区";
        if (sceneName == "entry")
            return "返回游戏大厅";
        if (sceneName == "train1")
            return "返回常规训练区";
        if (sceneName == "HubWorld")
            return "返回大厅";

        return "返回" + sceneName;
    }

    void EnsureStyles()
    {
        if (_buttonStyle != null) return;

        _buttonTexture = MakeTexture(new Color(0.08f, 0.28f, 0.36f, 0.92f));

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.normal.background = _buttonTexture;
        _buttonStyle.hover.background = _buttonTexture;
        _buttonStyle.active.background = _buttonTexture;
        _buttonStyle.normal.textColor = Color.white;
        _buttonStyle.hover.textColor = Color.white;
        _buttonStyle.active.textColor = Color.white;
        _buttonStyle.fontSize = 16;
        _buttonStyle.fontStyle = FontStyle.Bold;
        _buttonStyle.alignment = TextAnchor.MiddleCenter;

        _hintStyle = new GUIStyle(GUI.skin.label);
        _hintStyle.normal.textColor = new Color(0.82f, 0.90f, 0.94f, 0.92f);
        _hintStyle.fontSize = 14;
        _hintStyle.alignment = TextAnchor.MiddleLeft;
    }

    static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
