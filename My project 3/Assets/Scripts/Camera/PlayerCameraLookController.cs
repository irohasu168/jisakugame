using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraLookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private Transform vcamTransform;

    [Header("Follow")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float followSpeed = 25f;

    [Header("Rotation")]
    [SerializeField] private float mouseRotateSpeed = 0.25f;
    [SerializeField] private float touchRotateSpeed = 0.15f;
    [SerializeField] private float pitchMin = -10f;
    [SerializeField] private float pitchMax = 55f;

    [Header("Zoom")]
    [SerializeField] private float mouseZoomSpeed = 3f;
    [SerializeField] private float pinchZoomSpeed = 0.025f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 10f;

    [Header("Default View")]
    [SerializeField] private float defaultPitch = 20f;
    [SerializeField] private float defaultDistance = 6f;

    [Header("Mobile")]
    [SerializeField] private bool oneFingerLook = false;
    [SerializeField] private bool twoFingerLook = true;
    [SerializeField] private bool pinchZoom = true;

    [Header("Options")]
    [SerializeField] private bool snapOnEnable = true;

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

        if (snapOnEnable)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null || yawPivot == null || pitchPivot == null || vcamTransform == null)
            return;

        FollowTarget();

        if (Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();

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

    private void FollowTarget()
    {
        Vector3 targetPos = target.position + targetOffset;

        yawPivot.position = Vector3.Lerp(
            yawPivot.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }

    public void SnapToTarget()
    {
        if (target == null || yawPivot == null)
            return;

        yawPivot.position = target.position + targetOffset;
    }

    private void HandleMouse()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // PC: 右ドラッグで周りを見る
        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw += dx * mouseRotateSpeed * 100f;
            pitch -= dy * mouseRotateSpeed * 100f;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        // PC: ホイールで距離調整
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * mouseZoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            if (!oneFingerLook)
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

            if (twoFingerLook)
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

        Vector3 localPos = vcamTransform.localPosition;
        localPos.x = 0f;
        localPos.y = 0f;
        localPos.z = -distance;

        vcamTransform.localPosition = localPos;
        vcamTransform.localRotation = Quaternion.identity;
    }

    public void ResetPlayerView()
    {
        pitch = Mathf.Clamp(defaultPitch, pitchMin, pitchMax);
        distance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);

        ApplyTransform();
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