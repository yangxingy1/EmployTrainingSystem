using System.Collections.Generic;

public class MockAssignedTaskSource : IAssignedTaskSource
{
    public List<AssignedTask> GetAssignedTasks()
    {
        return new List<AssignedTask>
        {
            new AssignedTask("rotary_valve", "旋转阀门", "SampleScene", 0),
            new AssignedTask("electric_switch", "拉杆电闸", "ElectricSwitch", 1),
            new AssignedTask("sort_line", "传送分拣", "SampleScene", 2),
            new AssignedTask("emergency_stop", "急停复位", "SampleScene", 3),
            new AssignedTask("button_press", "按钮点击", "SampleScene", 4),
            new AssignedTask("slider_calibration", "滑块校准", "SampleScene", 5),
            new AssignedTask("bolt_tightening", "螺栓拧紧", "SampleScene", 6),
            new AssignedTask("material_transfer", "物料搬运", "SampleScene", 7),
            new AssignedTask("mode_selector", "档位选择", "SampleScene", 8),
            new AssignedTask("integrated_exam", "综合考核", "SampleScene", 9),
        };
    }
}
