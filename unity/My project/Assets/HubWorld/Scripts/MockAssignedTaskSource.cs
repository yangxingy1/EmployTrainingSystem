using System.Collections.Generic;

public class MockAssignedTaskSource : IAssignedTaskSource
{
    public List<AssignedTask> GetAssignedTasks()
    {
        return new List<AssignedTask>
        {
            new AssignedTask("rotary_valve", "旋转阀门", "RotaryValve", 0),
            new AssignedTask("electric_switch", "拉杆电闸", "ElectricSwitch", 1),
        };
    }
}
