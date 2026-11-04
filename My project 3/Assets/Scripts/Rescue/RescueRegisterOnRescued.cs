using UnityEngine;

public class RescueRegisterOnRescued : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RescueHealth rescueHealth;
    [SerializeField] private RescueRoster rescueRoster;
    [SerializeField] private StageClearManager stageClearManager;
    [SerializeField] private DungeonManager dungeonManager;

    [Header("Clear Rule")]
    [SerializeField] private bool clearNormalStageOnRescue = true;

    private void Awake()
    {
        if (rescueHealth == null)
            rescueHealth = GetComponent<RescueHealth>();

        if (rescueRoster == null)
            rescueRoster = FindFirstObjectByType<RescueRoster>();

        if (stageClearManager == null)
            stageClearManager = FindFirstObjectByType<StageClearManager>();

        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();
    }

    private void OnEnable()
    {
        if (rescueHealth != null)
            rescueHealth.OnRescued += HandleRescued;
    }

    private void OnDisable()
    {
        if (rescueHealth != null)
            rescueHealth.OnRescued -= HandleRescued;
    }

    private void HandleRescued(RescueHealth rescue)
    {
        if (rescueRoster != null)
            rescueRoster.AddRescued(rescue);

        Debug.Log($"[RescueRegister] Registered rescue: {rescue.gameObject.name}");

        bool isBossStage =
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage;

        if (clearNormalStageOnRescue && !isBossStage)
        {
            if (stageClearManager != null)
            {
                Debug.Log("[RescueRegister] Normal stage clear by rescue.");
                stageClearManager.Clear();
            }
        }
    }
}
