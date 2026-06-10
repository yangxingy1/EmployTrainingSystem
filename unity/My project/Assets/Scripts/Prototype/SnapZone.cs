using UnityEngine;

/// <summary>
/// 吸附区:放在"目标槽位"上的通用辅助。搬运中的物体靠近时被磁吸到中心,在区内松手会自动落位。
/// GraspController 负责读取并施加吸附;放多个就能做分拣/码垛。
/// </summary>
public class SnapZone : MonoBehaviour
{
    public float radius = 1.3f;        // 磁吸/落位作用半径
    [Range(0f, 1f)] public float magnetism = 0.85f;  // 搬运时被吸向中心的强度
    public bool active = true;

    public Vector3 Center => transform.position;       // 吸附中心
    public Vector3 SnapPosition => transform.position; // 落位后的静止中心
    public Quaternion SnapRotation => Quaternion.identity;
}
