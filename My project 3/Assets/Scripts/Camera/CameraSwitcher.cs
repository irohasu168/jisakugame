using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Assign Cinemachine Virtual Camera components")]
    [SerializeField] private MonoBehaviour overviewCam;
    [SerializeField] private MonoBehaviour playerCam;

    [Header("Overview Controller")]
    [SerializeField] private OverviewCameraController overviewController;
    [SerializeField] private OverviewPivotFollower overviewPivotFollower;
    [SerializeField] private PlayerCameraLookController playerCameraLookController;

    [Header("Priorities")]
    [SerializeField] private int overviewPriority = 20;
    [SerializeField] private int playerPriority = 10;

    private System.Reflection.PropertyInfo overviewPriorityProp;
    private System.Reflection.PropertyInfo playerPriorityProp;

    private void Awake()
    {
        overviewPriorityProp = overviewCam != null
            ? overviewCam.GetType().GetProperty("Priority")
            : null;

        playerPriorityProp = playerCam != null
            ? playerCam.GetType().GetProperty("Priority")
            : null;

        if (overviewPivotFollower == null)
            overviewPivotFollower = FindFirstObjectByType<OverviewPivotFollower>();

    }

    private void Start()
    {
        SwitchToOverview();
    }

    public void SwitchToOverview()
    {
        if (playerCameraLookController != null)
            playerCameraLookController.enabled = false;

        if (overviewPivotFollower != null)
            overviewPivotFollower.SnapToTarget();

        if (overviewController != null)
        {
            overviewController.enabled = true;
            overviewController.ResetOverviewView();
        }

        SetPriority(overviewCam, overviewPriorityProp, overviewPriority);
        SetPriority(playerCam, playerPriorityProp, playerPriority);

        Debug.Log("[CameraSwitcher] SwitchToOverview");
    }

    public void SwitchToPlayer()
    {
        if (playerCameraLookController != null)
        {
            playerCameraLookController.enabled = true;
            playerCameraLookController.SnapToTarget();
            playerCameraLookController.ResetPlayerView();
        }

        if (overviewController != null)
            overviewController.enabled = false;

        SetPriority(playerCam, playerPriorityProp, overviewPriority);
        SetPriority(overviewCam, overviewPriorityProp, playerPriority);

        Debug.Log("[CameraSwitcher] SwitchToPlayer");
    }

    private void SetPriority(MonoBehaviour cam, System.Reflection.PropertyInfo prop, int value)
    {
        if (cam == null || prop == null)
        {
            Debug.LogWarning("[CameraSwitcher] Priority設定に失敗。参照を確認してください。", this);
            return;
        }

        prop.SetValue(cam, value);
    }
}