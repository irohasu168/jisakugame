using UnityEngine;

public class StageClearManager : MonoBehaviour
{
    public enum StagePlayState
    {
        Playing,
        Clear,
        Failed
    }

    [Header("References")]
    [SerializeField] private TurnController turn;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private MidBossController boss;
    [SerializeField] private GameObject resultUI;

    [Header("Debug Clear")]
    [SerializeField] private bool allowDebugClear = true;
    [SerializeField] private KeyCode debugClearKey = KeyCode.F7;
    [SerializeField] private DungeonSelectUI dungeonSelectUI;
    private StagePlayState state = StagePlayState.Playing;

    public StagePlayState State => state;

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();

        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (boss == null)
            boss = FindFirstObjectByType<MidBossController>();

        if (resultUI != null)
            resultUI.SetActive(false);

        if (dungeonSelectUI == null)
            dungeonSelectUI = FindFirstObjectByType<DungeonSelectUI>();
    }

    private void OnEnable()
    {
        if (boss != null)
            boss.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        if (boss != null)
            boss.OnBossDefeated -= HandleBossDefeated;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (allowDebugClear && Input.GetKeyDown(debugClearKey))
        {
            Clear();
        }
#endif
    }

    private void HandleBossDefeated()
    {
        bool isBossStage =
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage;

        if (!isBossStage)
        {
            Debug.Log("[StageClear] Boss defeated, but current stage is not boss stage.");
            return;
        }

        Debug.Log("[StageClear] Boss stage clear by boss defeated.");

        Clear();
    }

    public void Clear()
    {
        if (state != StagePlayState.Playing)
            return;

        state = StagePlayState.Clear;

        Debug.Log("[StageClear] CLEAR!");

        if (dungeonManager != null)
            dungeonManager.OnStageCleared();

        if (resultUI != null)
            resultUI.SetActive(true);
    }

    public void Failed()
    {
        if (state != StagePlayState.Playing)
            return;

        state = StagePlayState.Failed;

        Debug.Log("[StageClear] FAILED!");

        if (resultUI != null)
            resultUI.SetActive(true);
    }

    public void ResetStageState()
    {
        state = StagePlayState.Playing;

        if (resultUI != null)
            resultUI.SetActive(false);

        Debug.Log("[StageClear] Reset -> Playing");
    }

    public void OnClickNextStage()
    {
        if (dungeonManager != null)
            dungeonManager.GoNextStageFromResult();

        ResetStageState();
    }

    public void OnClickRetryStage()
    {
        if (dungeonManager != null)
            dungeonManager.RetryCurrentStage();

        ResetStageState();
    }
    public void OnClickRetryDungeon()
    {
        if (dungeonManager != null)
            dungeonManager.RetryDungeon();

        ResetStageState();
    }


    public void OnClickBackToSelect()
    {
        ResetStageState();

        if (dungeonSelectUI != null)
            dungeonSelectUI.ShowSelectFromResult();
    }

}