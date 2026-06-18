using UnityEngine;

public class BreakerStartButtonInteractable : MonoBehaviour
{
    public BreakerShutdownStationRuntime station;

    private void OnMouseDown()
    {
        if (station != null)
        {
            station.PressStartButton();
        }
    }
}
