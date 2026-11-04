using UnityEngine;

public class CameraBoundsLimiter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform vcamTransform;

    [Header("World Bounds")]
    [SerializeField] private Vector2 minXZ = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 maxXZ = new Vector2(10f, 10f);

    [Header("Padding")]
    [SerializeField] private float pivotPadding = 0.5f;
    [SerializeField] private float cameraPadding = 0.5f;

    [Header("Options")]
    [SerializeField] private bool clampYawPivot = true;
    [SerializeField] private bool clampCameraPosition = true;

    private void LateUpdate()
    {
        if (clampYawPivot && yawPivot != null)
        {
            ClampTransformXZ(yawPivot, pivotPadding);
        }

        if (clampCameraPosition && vcamTransform != null)
        {
            ClampTransformXZ(vcamTransform, cameraPadding);
        }
    }

    private void ClampTransformXZ(Transform target, float padding)
    {
        Vector3 pos = target.position;

        pos.x = Mathf.Clamp(pos.x, minXZ.x + padding, maxXZ.x - padding);
        pos.z = Mathf.Clamp(pos.z, minXZ.y + padding, maxXZ.y - padding);

        target.position = pos;
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minXZ = min;
        maxXZ = max;
    }
}
