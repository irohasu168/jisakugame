using UnityEngine;

[DefaultExecutionOrder(100)]
public class OverviewCameraCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform pitchPivot;

    [SerializeField]
    private Transform vcamTransform;

    [SerializeField]
    private OverviewCameraController overviewController;

    [Header("Collision")]
    [SerializeField]
    private LayerMask wallLayers;

    [SerializeField, Min(0.01f)]
    private float cameraRadius = 0.35f;

    [SerializeField, Min(0f)]
    private float wallPadding = 0.15f;

    [SerializeField, Min(0.1f)]
    private float minDistance = 3f;

    [SerializeField, Min(0f)]
    private float smoothSpeed = 20f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLog = false;

    private float currentDistance;

    private void Awake()
    {
        if (overviewController == null)
        {
            overviewController =
                GetComponent<
                    OverviewCameraController
                >();
        }

        currentDistance =
            GetDesiredDistance();
    }

    private void OnEnable()
    {
        currentDistance =
            GetDesiredDistance();
    }

    private void LateUpdate()
    {
        if (pitchPivot == null ||
            vcamTransform == null)
        {
            return;
        }

        float desiredDistance =
            GetDesiredDistance();

        desiredDistance =
            Mathf.Max(
                minDistance,
                desiredDistance
            );

        Vector3 origin =
            pitchPivot.position;

        Vector3 direction =
            pitchPivot.TransformDirection(
                Vector3.back
            );

        float allowedDistance =
            desiredDistance;

        if (Physics.SphereCast(
                origin,
                cameraRadius,
                direction,
                out RaycastHit hit,
                desiredDistance,
                wallLayers,
                QueryTriggerInteraction.Ignore))
        {
            allowedDistance =
                Mathf.Max(
                    minDistance,
                    hit.distance - wallPadding
                );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[OverviewCollision] Hit={hit.collider.name}, " +
                    $"AllowedDistance={allowedDistance:F2}"
                );
            }
        }

        if (smoothSpeed <= 0f)
        {
            currentDistance =
                allowedDistance;
        }
        else
        {
            float interpolation =
                Mathf.Clamp01(
                    Time.unscaledDeltaTime *
                    smoothSpeed
                );

            currentDistance =
                Mathf.Lerp(
                    currentDistance,
                    allowedDistance,
                    interpolation
                );
        }

        Vector3 localPosition =
            vcamTransform.localPosition;

        localPosition.x = 0f;
        localPosition.y = 0f;
        localPosition.z =
            -currentDistance;

        vcamTransform.localPosition =
            localPosition;
    }

    private float GetDesiredDistance()
    {
        if (overviewController != null)
        {
            return overviewController.BaseDistance;
        }

        if (vcamTransform != null)
        {
            return Mathf.Abs(
                vcamTransform.localPosition.z
            );
        }

        return minDistance;
    }
}