using UnityEngine;

public class OverviewPivotFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform yawPivot;

    [Header("Follow")]
    [SerializeField] private bool followTarget = true;
    [SerializeField] private bool useFixedY = true;
    [SerializeField] private float fixedY = 0f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float followSpeed = 20f;

    [Header("Options")]
    [SerializeField] private bool snapOnEnable = true;

    private void OnEnable()
    {
        if (snapOnEnable)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (!followTarget)
        {
            return;
        }

        if (target == null || yawPivot == null)
        {
            return;
        }

        Vector3 targetPos = target.position + offset;

        if (useFixedY)
        {
            targetPos.y = fixedY;
        }

        yawPivot.position = Vector3.Lerp(
            yawPivot.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }

    public void SnapToTarget()
    {
        if (target == null || yawPivot == null)
        {
            return;
        }

        Vector3 targetPos = target.position + offset;

        if (useFixedY)
        {
            targetPos.y = fixedY;
        }

        yawPivot.position = targetPos;
    }
}
