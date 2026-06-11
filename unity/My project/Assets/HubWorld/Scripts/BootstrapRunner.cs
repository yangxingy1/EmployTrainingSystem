using UnityEngine;

public class BootstrapRunner : MonoBehaviour
{
    public string hubWorldSceneName = "HubWorld";

    void Start()
    {
        var session = SessionManager.EnsureExists();
        session.Initialize(new MockAssignedTaskSource());

        SceneFlow.EnsureExists().LoadScene(hubWorldSceneName);
    }
}
