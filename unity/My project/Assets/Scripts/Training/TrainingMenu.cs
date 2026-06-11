using System;
using UnityEngine;

/// <summary>
/// 训练大厅: 用食指点击按钮进入不同基础训练模块。
/// </summary>
public class TrainingMenu : MonoBehaviour
{
    public HandInput hand;
    public Action<SceneBootstrap.TrainingTaskType> Selected;

    void Start()
    {
        BuildTitle();
        BuildMenuButtons();
    }

    void BuildTitle()
    {
        var titleGo = new GameObject("MenuTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 1.58f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "慧动手员工培训系统";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var subtitleGo = new GameObject("MenuSubtitle");
        subtitleGo.transform.parent = transform;
        subtitleGo.transform.position = new Vector3(0f, 1.20f, -0.08f);
        var subtitle = subtitleGo.AddComponent<TextMesh>();
        subtitle.text = "食指下点按钮进入训练模块";
        subtitle.anchor = TextAnchor.MiddleCenter;
        subtitle.alignment = TextAlignment.Center;
        subtitle.fontSize = 34;
        subtitle.characterSize = 0.034f;
        subtitle.color = new Color(0.76f, 0.88f, 1f);
    }

    void BuildMenuButtons()
    {
        AddButton("传送带分拣", SceneBootstrap.TrainingTaskType.ConveyorSorting, new Vector3(-1.72f, 0.62f, 0f), new Color(0.10f, 0.44f, 0.88f));
        AddButton("抓取放置", SceneBootstrap.TrainingTaskType.PickPlace, new Vector3(0f, 0.62f, 0f), new Color(0.16f, 0.58f, 0.48f));
        AddButton("按钮点击", SceneBootstrap.TrainingTaskType.ButtonPress, new Vector3(1.72f, 0.62f, 0f), new Color(0.66f, 0.34f, 0.86f));
        AddButton("阀门旋转", SceneBootstrap.TrainingTaskType.ValveRotate, new Vector3(-1.72f, 0.02f, 0f), new Color(0.90f, 0.48f, 0.14f));
        AddButton("推拉开关", SceneBootstrap.TrainingTaskType.ElectricSwitch, new Vector3(0f, 0.02f, 0f), new Color(0.72f, 0.26f, 0.18f));
        AddButton("横向滑块", SceneBootstrap.TrainingTaskType.LinearSlider, new Vector3(1.72f, 0.02f, 0f), new Color(0.20f, 0.56f, 0.92f));
        AddButton("急停复位", SceneBootstrap.TrainingTaskType.EmergencyStop, new Vector3(-1.72f, -0.58f, 0f), new Color(0.88f, 0.12f, 0.08f));
        AddButton("档位旋钮", SceneBootstrap.TrainingTaskType.ModeSelector, new Vector3(0f, -0.58f, 0f), new Color(0.25f, 0.48f, 0.86f));
        AddButton("螺栓拧紧", SceneBootstrap.TrainingTaskType.BoltTightening, new Vector3(1.72f, -0.58f, 0f), new Color(0.46f, 0.50f, 0.58f));
    }

    void AddButton(string label, SceneBootstrap.TrainingTaskType taskType, Vector3 position, Color color)
    {
        var go = new GameObject("MenuButton_" + taskType);
        go.transform.parent = transform;
        var button = go.AddComponent<FingertipTapButton>();
        button.hand = hand;
        button.requireFreeHand = false;
        button.Build(position, new Vector3(1.22f, 0.34f, 0.09f), label, color);
        button.Clicked += () => Selected?.Invoke(taskType);
    }
}
