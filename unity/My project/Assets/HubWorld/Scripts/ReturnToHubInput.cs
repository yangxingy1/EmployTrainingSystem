using UnityEngine;

public class ReturnToHubInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
            SceneFlow.EnsureExists().ReturnToHub();
    }

    void OnGUI()
    {
        var rect = new Rect(18f, 18f, 190f, 42f);
        if (GUI.Button(rect, "Return to Hub (R)"))
            SceneFlow.EnsureExists().ReturnToHub();
    }
}
