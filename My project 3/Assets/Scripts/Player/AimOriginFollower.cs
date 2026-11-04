using UnityEngine;

public class AimOriginFollower : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.3f, 0f);

    [Header("Rotation")]
    [SerializeField] private bool followYawRotation = true;

    private void LateUpdate()
    {
        if (target == null)
            return;

        // 現在のターゲット位置にShooterを追従させる
        transform.position = target.position + offset;

        // Yaw方向だけターゲットに合わせる
        if (followYawRotation)
        {
            Vector3 forward = target.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }
}