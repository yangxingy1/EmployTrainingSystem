using System;
using UnityEngine;

/// <summary>
/// 训练任务公共导航条: 提供返回主菜单按钮。
/// </summary>
public class TaskNavigationBar : MonoBehaviour
{
    public HandInput hand;
    public GraspController grasp;
    public Action BackToMenu;

    void Start()
    {
        var buttonGo = new GameObject("BackToMenuButton");
        buttonGo.transform.parent = transform;
        var button = buttonGo.AddComponent<FingertipTapButton>();
        button.hand = hand;
        button.grasp = grasp;
        button.requireFreeHand = true;
        button.Build(new Vector3(-2.45f, 1.55f, 0f), new Vector3(0.92f, 0.30f, 0.08f), "返回菜单", new Color(0.42f, 0.48f, 0.58f));
        button.Clicked += () => BackToMenu?.Invoke();
    }
}
