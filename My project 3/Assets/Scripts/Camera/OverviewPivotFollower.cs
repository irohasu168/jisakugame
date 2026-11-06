using UnityEngine;

[DefaultExecutionOrder(0)]
public class OverviewPivotFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform yawPivot;

    [SerializeField]
    private OverviewCameraPanController panController;

    [Header("Follow")]
    [SerializeField] private bool followTarget = false;
    [SerializeField] private bool useFixedY = true;
    [SerializeField] private float fixedY = 0f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [SerializeField, Min(0f)]
    private float followSpeed = 20f;

    [Header("Pan Compatibility")]
    [SerializeField]
    private bool pauseFollowWhilePanning = true;

    [SerializeField]
    private bool resumeFollowAfterPan = false;

    [Header("Options")]
    [SerializeField] private bool snapOnEnable = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private bool wasPanning;

    public bool FollowTarget
    {
        get
        {
            return followTarget;
        }
    }

    private void Awake()
    {
        FindPanController();
    }

    private void OnEnable()
    {
        wasPanning = false;

        if (snapOnEnable)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        bool isPanning =
            panController != null &&
            panController.IsDragging;

        if (isPanning)
        {
            wasPanning = true;

            if (pauseFollowWhilePanning)
            {
                return;
            }
        }

        if (wasPanning && !isPanning)
        {
            wasPanning = false;

            if (!resumeFollowAfterPan)
            {
                followTarget = false;
            }
        }

        if (!followTarget)
        {
            return;
        }

        if (target == null ||
            yawPivot == null)
        {
            return;
        }

        Vector3 targetPosition =
            GetTargetPosition();

        if (followSpeed <= 0f)
        {
            yawPivot.position =
                targetPosition;

            return;
        }

        float interpolation =
            Mathf.Clamp01(
                Time.unscaledDeltaTime *
                followSpeed
            );

        yawPivot.position =
            Vector3.Lerp(
                yawPivot.position,
                targetPosition,
                interpolation
            );
    }

    private void FindPanController()
    {
        if (panController != null)
        {
            return;
        }

        if (yawPivot == null)
        {
            return;
        }

        panController =
            yawPivot.GetComponent<
                OverviewCameraPanController
            >();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition =
            target.position + offset;

        if (useFixedY)
        {
            targetPosition.y = fixedY;
        }

        return targetPosition;
    }

    public void SnapToTarget()
    {
        if (target == null ||
            yawPivot == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "[OverviewFollower] " +
                    "TargetÇ‹ÇΩÇÕYaw PivotÇ™ñ¢ê›íËÇ≈Ç∑ÅB"
                );
            }

            return;
        }

        yawPivot.position =
            GetTargetPosition();

        if (showDebugLog)
        {
            Debug.Log(
                $"[OverviewFollower] Snapped. " +
                $"Target={target.name}, " +
                $"Position={yawPivot.position}"
            );
        }
    }

    public void SetFollowEnabled(
        bool enabled)
    {
        followTarget = enabled;

        if (showDebugLog)
        {
            Debug.Log(
                $"[OverviewFollower] " +
                $"Follow={enabled}"
            );
        }
    }

    public void SetTarget(
        Transform newTarget)
    {
        target = newTarget;
    }

    public void SetYawPivot(
        Transform newYawPivot)
    {
        yawPivot = newYawPivot;
        panController = null;

        FindPanController();
    }
}