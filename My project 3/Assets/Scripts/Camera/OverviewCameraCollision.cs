using UnityEngine;

public class OverviewCameraCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private Transform vcamTransform;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private float cameraRadius = 0.35f;
    [SerializeField] private float wallPadding = 0.15f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float smoothSpeed = 20f;

    private float currentDistance;

    private void Awake()
    {
        if (pitchPivot != null && vcamTransform != null)
        {
            currentDistance = Mathf.Abs(vcamTransform.localPosition.z);
        }
    }

    private void LateUpdate()
    {
        if (pitchPivot == null || vcamTransform == null)
            return;

        Vector3 desiredWorldPos = vcamTransform.position;
        Vector3 origin = pitchPivot.position;

        Vector3 toCamera = desiredWorldPos - origin;
        float desiredDistance = toCamera.magnitude;

        if (desiredDistance <= 0.001f)
            return;

        Vector3 direction = toCamera.normalized;
        float allowedDistance = desiredDistance;

        if (Physics.SphereCast(
            origin,
            cameraRadius,
            direction,
            out RaycastHit hit,
            desiredDistance,
            wallLayers,
            QueryTriggerInteraction.Ignore))
        {
            allowedDistance = Mathf.Max(minDistance, hit.distance - wallPadding);
        }

        currentDistance = Mathf.Lerp(
            Mathf.Abs(vcamTransform.localPosition.z),
            allowedDistance,
            Time.deltaTime * smoothSpeed
        );

        Vector3 localPos = vcamTransform.localPosition;
        localPos.z = -currentDistance;
        vcamTransform.localPosition = localPos;
    }
}
