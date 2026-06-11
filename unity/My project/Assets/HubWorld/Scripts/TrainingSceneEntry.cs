using UnityEngine;

public class TrainingSceneEntry : MonoBehaviour
{
    public TrainingSceneKind sceneKind = TrainingSceneKind.RotaryValve;

    void Start()
    {
        var bootstrapGo = new GameObject("SceneBootstrap");
        var bootstrap = bootstrapGo.AddComponent<SceneBootstrap>();
        bootstrap.sceneKind = sceneKind;

        gameObject.AddComponent<ReturnToHubInput>();
    }
}
