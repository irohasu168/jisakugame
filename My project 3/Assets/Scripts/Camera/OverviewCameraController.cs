using UnityEngine;
using UnityEngine.EventSystems;

public class OverviewCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private Transform vcamTransform;

    [Header("Rotation")]
    [SerializeField] private float mouseRotateSpeed = 0.2f;
    [SerializeField] private float touchRotateSpeed = 0.15f;
    [SerializeField] private float pitchMin = 25f;
    [SerializeField] private float pitchMax = 55f;

    [Header("Zoom")]
    [SerializeField] private float mouseZoomSpeed = 5f;
    [SerializeField] private float pinchZoomSpeed = 0.03f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 18f;

    [Header("Default View")]
    [SerializeField] private float defaultPitch = 40f;
    [SerializeField] private float defaultDistance = 9f;

    [Header("Stage Bounds")]
    [SerializeField] private bool limitCameraInsideStage = true;
    [SerializeField] private Vector2 minXZ = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 maxXZ = new Vector2(10f, 10f);
    [SerializeField] private float cameraBoundsPadding = 0.7f;
    [SerializeField] private int distanceSearchSteps = 10;

    [Header("Mobile")]
    [SerializeField] private bool oneFingerRotate = false;
    [SerializeField] private bool twoFingerRotate = true;
    [SerializeField] private bool pinchZoom = true;

    [Header("Debug")]
    [SerializeField] private bool logInput = false;

    private float yaw;
    private float pitch;
    private float distance;

    private void Awake()
    {
        ReadCurrentTransform();
    }

    private void OnEnable()
    {
        ReadCurrentTransform();
    }

    private void Update()
    {
        if (yawPivot == null || pitchPivot == null || vcamTransform == null)
            return;

        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    private void LateUpdate()
    {
        if (yawPivot == null || pitchPivot == null || vcamTransform == null)
            return;

        ApplyTransform();
    }

    private void ReadCurrentTransform()
    {
        if (yawPivot != null)
            yaw = yawPivot.eulerAngles.y;

        if (pitchPivot != null)
            pitch = NormalizeAngle(pitchPivot.localEulerAngles.x);
        else
            pitch = defaultPitch;

        if (vcamTransform != null)
        {
            distance = Mathf.Abs(vcamTransform.localPosition.z);

            if (distance <= 0.001f)
                distance = defaultDistance;
        }
        else
        {
            distance = defaultDistance;
        }

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void HandleMouse()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw += dx * mouseRotateSpeed * 100f;
            pitch -= dy * mouseRotateSpeed * 100f;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            if (logInput)
                Debug.Log($"[OverviewCamera] Mouse Rotate yaw={yaw:F1}, pitch={pitch:F1}");
        }

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * mouseZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (logInput)
                Debug.Log($"[OverviewCamera] Mouse Zoom distance={distance:F1}");
        }
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            if (!oneFingerRotate)
                return;

            Touch t = Input.GetTouch(0);

            if (IsTouchOverUI(t.fingerId))
                return;

            if (t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.deltaPosition;

                yaw += delta.x * touchRotateSpeed;
                pitch -= delta.y * touchRotateSpeed;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }

            return;
        }

        if (Input.touchCount >= 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            if (IsTouchOverUI(t0.fingerId) || IsTouchOverUI(t1.fingerId))
                return;

            if (pinchZoom)
            {
                Vector2 t0Prev = t0.position - t0.deltaPosition;
                Vector2 t1Prev = t1.position - t1.deltaPosition;

                float prevDistance = Vector2.Distance(t0Prev, t1Prev);
                float currentDistance = Vector2.Distance(t0.position, t1.position);

                float pinchDelta = currentDistance - prevDistance;

                distance -= pinchDelta * pinchZoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            if (twoFingerRotate)
            {
                Vector2 averageDelta = (t0.deltaPosition + t1.deltaPosition) * 0.5f;

                yaw += averageDelta.x * touchRotateSpeed;
                pitch -= averageDelta.y * touchRotateSpeed;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }
        }
    }

    private void ApplyTransform()
    {
        yawPivot.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        float finalDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        if (limitCameraInsideStage)
        {
            finalDistance = ClampDistanceInsideStage(finalDistance);
            distance = finalDistance;
        }

        Vector3 localPos = vcamTransform.localPosition;
        localPos.x = 0f;
        localPos.y = 0f;
        localPos.z = -finalDistance;

        vcamTransform.localPosition = localPos;
        vcamTransform.localRotation = Quaternion.identity;
    }

    private float ClampDistanceInsideStage(float desiredDistance)
    {
        if (pitchPivot == null)
            return desiredDistance;

        Vector3 cameraPosAtDesired = GetCameraWorldPosition(desiredDistance);

        if (IsInsideBounds(cameraPosAtDesired))
            return desiredDistance;

        float low = minDistance;
        float high = desiredDistance;

        for (int i = 0; i < distanceSearchSteps; i++)
        {
            float mid = (low + high) * 0.5f;
            Vector3 pos = GetCameraWorldPosition(mid);

            if (IsInsideBounds(pos))
                low = mid;
            else
                high = mid;
        }

        return Mathf.Clamp(low, minDistance, desiredDistance);
    }

    private Vector3 GetCameraWorldPosition(float testDistance)
    {
        return pitchPivot.position +
               pitchPivot.TransformDirection(new Vector3(0f, 0f, -testDistance));
    }

    private bool IsInsideBounds(Vector3 worldPos)
    {
        float minX = minXZ.x + cameraBoundsPadding;
        float maxX = maxXZ.x - cameraBoundsPadding;
        float minZ = minXZ.y + cameraBoundsPadding;
        float maxZ = maxXZ.y - cameraBoundsPadding;

        return worldPos.x >= minX &&
               worldPos.x <= maxX &&
               worldPos.z >= minZ &&
               worldPos.z <= maxZ;
    }

    public void ResetOverviewView()
    {
        ResetOverviewView(defaultPitch, defaultDistance);
    }

    public void ResetOverviewView(float resetPitch, float resetDistance)
    {
        pitch = Mathf.Clamp(resetPitch, pitchMin, pitchMax);
        distance = Mathf.Clamp(resetDistance, minDistance, maxDistance);

        ApplyTransform();

        Debug.Log($"[OverviewCamera] Reset pitch={pitch:F1}, distance={distance:F1}");
    }

    public void SetStageBounds(Vector2 min, Vector2 max)
    {
        minXZ = min;
        maxXZ = max;
    }

    private bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}