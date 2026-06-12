using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局单例：Play 时直接进入化工管道培训场景。
/// 保留旧接口（空实现/兼容）供其他未清理的脚本编译通过。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 当前使用 ──
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── 旧接口（兼容，不再使用）──
    public void LoadTaskScene(string sceneName) { }
    public void ReturnToHub() => ReloadScene();
    public void SetTaskCompleted(string taskId) { }
    public bool IsTaskCompleted(string taskId) => false;
}
