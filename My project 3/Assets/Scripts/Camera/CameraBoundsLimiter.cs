using UnityEngine;

[DefaultExecutionOrder(200)]
public class CameraBoundsLimiter : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform yawPivot;

    [Header("World Bounds")]
    [SerializeField]
    private Vector2 minXZ =
        new Vector2(-70f, -90f);

    [SerializeField]
    private Vector2 maxXZ =
        new Vector2(70f, 90f);

    [Header("Padding")]
    [SerializeField, Min(0f)]
    private float pivotPadding = 2f;

    [Header("Options")]
    [SerializeField]
    private bool clampYawPivot = true;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLog = false;

    private void LateUpdate()
    {
        if (!clampYawPivot ||
            yawPivot == null)
        {
            return;
        }

        ClampYawPivot();
    }

    private void ClampYawPivot()
    {
        float minimumX =
            minXZ.x + pivotPadding;

        float maximumX =
            maxXZ.x - pivotPadding;

        float minimumZ =
            minXZ.y + pivotPadding;

        float maximumZ =
            maxXZ.y - pivotPadding;

        if (minimumX > maximumX)
        {
            float centerX =
                (minXZ.x + maxXZ.x) * 0.5f;

            minimumX = centerX;
            maximumX = centerX;
        }

        if (minimumZ > maximumZ)
        {
            float centerZ =
                (minXZ.y + maxXZ.y) * 0.5f;

            minimumZ = centerZ;
            maximumZ = centerZ;
        }

        Vector3 before =
            yawPivot.position;

        Vector3 after =
            before;

        after.x =
            Mathf.Clamp(
                after.x,
                minimumX,
                maximumX
            );

        after.z =
            Mathf.Clamp(
                after.z,
                minimumZ,
                maximumZ
            );

        yawPivot.position =
            after;

        if (showDebugLog &&
            Vector3.Distance(before, after) > 0.001f)
        {
            Debug.Log(
                $"[CameraBounds] Clamped. " +
                $"{before} -> {after}"
            );
        }
    }

    public void SetBounds(
        Vector2 minimumXZ,
        Vector2 maximumXZ)
    {
        minXZ = minimumXZ;
        maxXZ = maximumXZ;

        if (yawPivot != null)
        {
            ClampYawPivot();
        }
    }

    public void SetClampEnabled(
        bool enabled)
    {
        clampYawPivot = enabled;
    }
}