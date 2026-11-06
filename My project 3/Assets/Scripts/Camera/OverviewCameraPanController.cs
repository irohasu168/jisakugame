using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-200)]
public class OverviewCameraPanController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Camera referenceCamera;
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("PC Right Mouse Pan")]
    [SerializeField] private bool enableRightMousePan = true;

    [SerializeField, Min(0.001f)]
    private float mousePanSensitivity = 0.08f;

    [SerializeField]
    private bool blockMouseOverUI = true;

    [Header("Mobile Button Movement")]
    [SerializeField]
    private bool enableMobileButtonMove = true;

    [SerializeField, Min(0f)]
    private float mobileMoveSpeed = 25f;

    [Header("Movement Direction")]
    [SerializeField] private bool useCameraDirection = true;
    [SerializeField] private bool invertMouseHorizontal = false;
    [SerializeField] private bool invertMouseVertical = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private bool isRightMouseDragging;

    private Vector2 previousMousePosition;

    private bool mobileUpPressed;
    private bool mobileDownPressed;
    private bool mobileLeftPressed;
    private bool mobileRightPressed;

    public bool IsDragging
    {
        get
        {
            return isRightMouseDragging ||
                   mobileUpPressed ||
                   mobileDownPressed ||
                   mobileLeftPressed ||
                   mobileRightPressed;
        }
    }

    public bool IsRightMouseDragging
    {
        get
        {
            return isRightMouseDragging;
        }
    }

    private void Awake()
    {
        if (yawPivot == null)
        {
            yawPivot = transform;
        }

        if (referenceCamera == null)
        {
            referenceCamera = Camera.main;
        }
    }

    private void OnDisable()
    {
        ResetInputState();
    }

    private void Update()
    {
        if (!CanControlOverviewCamera())
        {
            ResetInputState();
            return;
        }

        HandleRightMousePan();
        HandleMobileButtonMove();
    }

    private bool CanControlOverviewCamera()
    {
        if (yawPivot == null)
        {
            return false;
        }

        if (cameraSwitcher == null)
        {
            return true;
        }

        return cameraSwitcher.IsOverviewActive;
    }

    private void HandleRightMousePan()
    {
        if (!enableRightMousePan)
        {
            return;
        }

        const int rightMouseButton = 1;

        if (Input.GetMouseButtonDown(rightMouseButton))
        {
            if (blockMouseOverUI && IsMouseOverUI())
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[OverviewPan] Input blocked by UI."
                    );
                }

                return;
            }

            isRightMouseDragging = true;
            previousMousePosition = Input.mousePosition;

            if (showDebugLog)
            {
                Debug.Log(
                    "[OverviewPan] Right mouse pan started."
                );
            }
        }

        if (Input.GetMouseButton(rightMouseButton) &&
            isRightMouseDragging)
        {
            Vector2 currentMousePosition =
                Input.mousePosition;

            Vector2 mouseDelta =
                currentMousePosition -
                previousMousePosition;

            previousMousePosition =
                currentMousePosition;

            if (mouseDelta.sqrMagnitude > 0.001f)
            {
                MoveByMouseDelta(mouseDelta);
            }
        }

        if (Input.GetMouseButtonUp(rightMouseButton))
        {
            if (isRightMouseDragging && showDebugLog)
            {
                Debug.Log(
                    $"[OverviewPan] Right mouse pan ended. " +
                    $"Position={yawPivot.position}"
                );
            }

            isRightMouseDragging = false;
        }
    }

    private void MoveByMouseDelta(
        Vector2 mouseDelta)
    {
        float horizontalSign =
            invertMouseHorizontal ? 1f : -1f;

        float verticalSign =
            invertMouseVertical ? 1f : -1f;

        Vector2 input = new Vector2(
            mouseDelta.x * horizontalSign,
            mouseDelta.y * verticalSign
        );

        MovePivot(
            input,
            mousePanSensitivity
        );
    }

    private void HandleMobileButtonMove()
    {
        if (!enableMobileButtonMove)
        {
            return;
        }

        Vector2 input = Vector2.zero;

        if (mobileUpPressed)
        {
            input.y += 1f;
        }

        if (mobileDownPressed)
        {
            input.y -= 1f;
        }

        if (mobileLeftPressed)
        {
            input.x -= 1f;
        }

        if (mobileRightPressed)
        {
            input.x += 1f;
        }

        if (input.sqrMagnitude <= 0.001f)
        {
            return;
        }

        MovePivot(
            input.normalized,
            mobileMoveSpeed *
            Time.unscaledDeltaTime
        );
    }

    private void MovePivot(
        Vector2 input,
        float amount)
    {
        if (yawPivot == null)
        {
            return;
        }

        GetHorizontalDirections(
            out Vector3 right,
            out Vector3 forward
        );

        Vector3 movement =
            right * input.x +
            forward * input.y;

        Vector3 previousPosition =
            yawPivot.position;

        Vector3 nextPosition =
            previousPosition +
            movement * amount;

        nextPosition.y =
            previousPosition.y;

        yawPivot.position =
            nextPosition;

        if (showDebugLog &&
            Vector3.Distance(
                previousPosition,
                nextPosition
            ) > 0.001f)
        {
            Debug.Log(
                $"[OverviewPan] Pivot moved. " +
                $"{previousPosition} -> {nextPosition}"
            );
        }
    }

    private void GetHorizontalDirections(
        out Vector3 right,
        out Vector3 forward)
    {
        if (useCameraDirection &&
            referenceCamera != null)
        {
            right =
                referenceCamera.transform.right;

            forward =
                referenceCamera.transform.forward;
        }
        else
        {
            right = yawPivot.right;
            forward = yawPivot.forward;
        }

        right.y = 0f;
        forward.y = 0f;

        if (right.sqrMagnitude > 0.001f)
        {
            right.Normalize();
        }
        else
        {
            right = Vector3.right;
        }

        if (forward.sqrMagnitude > 0.001f)
        {
            forward.Normalize();
        }
        else
        {
            forward = Vector3.forward;
        }
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current
                   .IsPointerOverGameObject();
    }

    public void SetMobileDirection(
        OverviewCameraMoveDirection direction,
        bool pressed)
    {
        switch (direction)
        {
            case OverviewCameraMoveDirection.Up:
                mobileUpPressed = pressed;
                break;

            case OverviewCameraMoveDirection.Down:
                mobileDownPressed = pressed;
                break;

            case OverviewCameraMoveDirection.Left:
                mobileLeftPressed = pressed;
                break;

            case OverviewCameraMoveDirection.Right:
                mobileRightPressed = pressed;
                break;
        }

        if (showDebugLog)
        {
            Debug.Log(
                $"[OverviewPan] Mobile {direction} " +
                $"pressed={pressed}"
            );
        }
    }

    public void StopMobileMovement()
    {
        mobileUpPressed = false;
        mobileDownPressed = false;
        mobileLeftPressed = false;
        mobileRightPressed = false;
    }

    private void ResetInputState()
    {
        isRightMouseDragging = false;
        StopMobileMovement();
    }

    public void SnapToTarget(
        Transform target)
    {
        if (target == null ||
            yawPivot == null)
        {
            return;
        }

        Vector3 position =
            yawPivot.position;

        position.x =
            target.position.x;

        position.z =
            target.position.z;

        yawPivot.position =
            position;

        if (showDebugLog)
        {
            Debug.Log(
                $"[OverviewPan] Snapped. " +
                $"Position={position}"
            );
        }
    }
}

public enum OverviewCameraMoveDirection
{
    Up,
    Down,
    Left,
    Right
}