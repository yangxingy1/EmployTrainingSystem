using UnityEngine;

/// <summary>
/// 极简组件：在训练任务场景中按 Escape 返回 HubRoom。
/// 由各 Bootstrap 在 Start() 中通过 AddComponent 挂载，
/// 确保训练场景可独立运行（GameManager 不存在时按 Escape 无操作）。
/// </summary>
public class TaskReturnHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance?.ReturnToHub();
    }
}
