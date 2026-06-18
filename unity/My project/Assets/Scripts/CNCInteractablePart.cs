using UnityEngine;

public enum CNCInteractionType
{
    TogglePower,
    ToggleDoor,
    ToggleClamp,
    CycleStart,
    EmergencyStop,
    Reset,
    ToggleMode
}

public class CNCInteractablePart : MonoBehaviour
{
    public CNCTrainingMachineRuntime station;
    public CNCInteractionType interactionType;
}
