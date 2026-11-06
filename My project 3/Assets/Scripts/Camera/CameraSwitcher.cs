using System.Reflection;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Virtual Cameras")]
    [SerializeField] private MonoBehaviour overviewCam;
    [SerializeField] private MonoBehaviour playerCam;

    [Header("Overview")]
    [SerializeField]
    private OverviewCameraController overviewController;

    [SerializeField]
    private OverviewCameraPanController overviewPanController;

    [SerializeField]
    private OverviewPivotFollower overviewPivotFollower;

    [Header("Player")]
    [SerializeField]
    private PlayerCameraLookController playerCameraLookController;

    [Header("Priorities")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;

    [Header("Start")]
    [SerializeField] private bool startWithOverview = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private PropertyInfo overviewPriorityProperty;
    private PropertyInfo playerPriorityProperty;

    private bool isOverviewActive;

    public bool IsOverviewActive
    {
        get
        {
            return isOverviewActive;
        }
    }

    private void Awake()
    {
        overviewPriorityProperty =
            GetPriorityProperty(overviewCam);

        playerPriorityProperty =
            GetPriorityProperty(playerCam);

        if (overviewPivotFollower == null)
        {
            overviewPivotFollower =
                FindFirstObjectByType<OverviewPivotFollower>();
        }

        if (overviewPanController == null)
        {
            overviewPanController =
                FindFirstObjectByType<OverviewCameraPanController>();
        }
    }

    private void Start()
    {
        if (startWithOverview)
        {
            SwitchToOverview();
        }
        else
        {
            SwitchToPlayer();
        }
    }

    public void SwitchToOverview()
    {
        isOverviewActive = true;

        if (playerCameraLookController != null)
        {
            playerCameraLookController.enabled = false;
        }

        if (overviewPivotFollower != null)
        {
            overviewPivotFollower.SnapToTarget();
            overviewPivotFollower.SetFollowEnabled(false);
        }

        if (overviewController != null)
        {
            overviewController.enabled = true;
            overviewController.ResetOverviewView();
        }

        if (overviewPanController != null)
        {
            overviewPanController.enabled = true;
            overviewPanController.StopMobileMovement();
        }

        SetPriority(
            overviewCam,
            overviewPriorityProperty,
            activePriority
        );

        SetPriority(
            playerCam,
            playerPriorityProperty,
            inactivePriority
        );

        if (showDebugLog)
        {
            Debug.Log(
                "[CameraSwitcher] Switched to Overview."
            );
        }
    }

    public void SwitchToPlayer()
    {
        isOverviewActive = false;

        if (overviewPanController != null)
        {
            overviewPanController.StopMobileMovement();
            overviewPanController.enabled = false;
        }

        if (overviewController != null)
        {
            overviewController.enabled = false;
        }

        if (playerCameraLookController != null)
        {
            playerCameraLookController.enabled = true;
            playerCameraLookController.SnapToTarget();
            playerCameraLookController.ResetPlayerView();
        }

        SetPriority(
            playerCam,
            playerPriorityProperty,
            activePriority
        );

        SetPriority(
            overviewCam,
            overviewPriorityProperty,
            inactivePriority
        );

        if (showDebugLog)
        {
            Debug.Log(
                "[CameraSwitcher] Switched to Player."
            );
        }
    }

    public void ToggleCamera()
    {
        if (isOverviewActive)
        {
            SwitchToPlayer();
        }
        else
        {
            SwitchToOverview();
        }
    }

    private PropertyInfo GetPriorityProperty(
        MonoBehaviour cameraComponent)
    {
        if (cameraComponent == null)
        {
            return null;
        }

        return cameraComponent
            .GetType()
            .GetProperty("Priority");
    }

    private void SetPriority(
        MonoBehaviour cameraComponent,
        PropertyInfo priorityProperty,
        int value)
    {
        if (cameraComponent == null ||
            priorityProperty == null)
        {
            Debug.LogWarning(
                "[CameraSwitcher] Priority設定に失敗しました。" +
                "Virtual Camera参照を確認してください。",
                this
            );

            return;
        }

        priorityProperty.SetValue(
            cameraComponent,
            value
        );
    }
}