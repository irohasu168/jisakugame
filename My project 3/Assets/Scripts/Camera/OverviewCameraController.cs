using UnityEngine;

[DefaultExecutionOrder(-100)]
public class OverviewCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private Transform vcamTransform;

    [Header("Fixed Overview View")]
    [SerializeField] private float defaultPitch = 40f;
    [SerializeField] private float defaultDistance = 9f;

    [SerializeField]
    private float pitchMin = 25f;

    [SerializeField]
    private float pitchMax = 55f;

    [SerializeField, Min(0.1f)]
    private float minimumDistance = 3f;

    [Header("Options")]
    [SerializeField]
    private bool lockYawRotation = true;

    [SerializeField]
    private float fixedYaw = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private float pitch;
    private float baseDistance;

    public float BaseDistance
    {
        get
        {
            return baseDistance;
        }
    }

    private void Awake()
    {
        ReadCurrentTransform();
    }

    private void OnEnable()
    {
        ReadCurrentTransform();
        ApplyBaseTransform();
    }

    private void LateUpdate()
    {
        ApplyBaseTransform();
    }

    private void ReadCurrentTransform()
    {
        pitch = defaultPitch;
        baseDistance = defaultDistance;

        if (pitchPivot != null)
        {
            float currentPitch =
                NormalizeAngle(
                    pitchPivot.localEulerAngles.x
                );

            if (Mathf.Abs(currentPitch) > 0.001f)
            {
                pitch = currentPitch;
            }
        }

        if (vcamTransform != null)
        {
            float currentDistance =
                Mathf.Abs(
                    vcamTransform.localPosition.z
                );

            if (currentDistance > 0.001f)
            {
                baseDistance = currentDistance;
            }
        }

        pitch = Mathf.Clamp(
            pitch,
            pitchMin,
            pitchMax
        );

        baseDistance =
            Mathf.Max(
                minimumDistance,
                baseDistance
            );
    }

    private void ApplyBaseTransform()
    {
        if (yawPivot == null ||
            pitchPivot == null ||
            vcamTransform == null)
        {
            return;
        }

        if (lockYawRotation)
        {
            yawPivot.rotation =
                Quaternion.Euler(
                    0f,
                    fixedYaw,
                    0f
                );
        }

        pitchPivot.localRotation =
            Quaternion.Euler(
                pitch,
                0f,
                0f
            );

        Vector3 localPosition =
            vcamTransform.localPosition;

        localPosition.x = 0f;
        localPosition.y = 0f;
        localPosition.z = -baseDistance;

        vcamTransform.localPosition =
            localPosition;

        vcamTransform.localRotation =
            Quaternion.identity;
    }

    public void ResetOverviewView()
    {
        ResetOverviewView(
            defaultPitch,
            defaultDistance
        );
    }

    public void ResetOverviewView(
        float resetPitch,
        float resetDistance)
    {
        pitch = Mathf.Clamp(
            resetPitch,
            pitchMin,
            pitchMax
        );

        baseDistance =
            Mathf.Max(
                minimumDistance,
                resetDistance
            );

        ApplyBaseTransform();

        if (showDebugLog)
        {
            Debug.Log(
                $"[OverviewCamera] Reset. " +
                $"Pitch={pitch:F1}, " +
                $"Distance={baseDistance:F1}"
            );
        }
    }

    public void SetBaseDistance(
        float newDistance)
    {
        baseDistance =
            Mathf.Max(
                minimumDistance,
                newDistance
            );
    }

    public void SetStageBounds(
        Vector2 minimumXZ,
        Vector2 maximumXZ)
    {
        // 既存コードとの互換用です。
        // 実際のXZ制限はCameraBoundsLimiterが担当します。
    }

    private float NormalizeAngle(
        float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}