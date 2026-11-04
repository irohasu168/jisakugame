using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    private const int StageCount = 10;

    public enum DungeonRunState
    {
        NotStarted,
        Exploring,
        StageClear,
        DungeonClear,
        Failed
    }

    [Header("References")]
    [SerializeField] private StageGenerator stageGenerator;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private TurnController turnController;
    [SerializeField] private BossStageGimmickController bossStageGimmickController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Dungeon Settings")]
    [SerializeField] private DungeonDifficulty currentDungeonDifficulty = DungeonDifficulty.Normal;
    [SerializeField] private int dungeonSeed = 1000;
    [SerializeField] private bool startDungeonOnStart = true;
    [SerializeField] private BlockClearTracker blockClearTracker;

    [Header("Difficulty Plans")]
    [SerializeField] private DungeonDifficultyPlan[] plans;

    [SerializeField] private AllyBonusApplier allyBonusApplier;
    [SerializeField] private BossStageController bossStageController;

    [Header("Seed Options")]
    [SerializeField] private bool avoidClearedDungeonSeeds = true;
    [SerializeField] private int randomSeedMin = 10000;
    [SerializeField] private int randomSeedMax = 999999;
    [SerializeField] private int maxSeedSearchTries = 100;

    private int currentStageIndex = -1;
    private DungeonRunState state = DungeonRunState.NotStarted;

    public int CurrentStageNumber => currentStageIndex + 1;
    public int MaxStageCount => StageCount;
    public DungeonDifficulty CurrentDungeonDifficulty => currentDungeonDifficulty;
    public DungeonRunState State => state;

    public bool IsCurrentBossStage
    {
        get
        {
            if (currentStageIndex < 0)
                return false;

            if (currentStageIndex >= StageCount)
                return false;

            DungeonStageData data = GetCurrentStageData();
            return data != null && data.isBossStage;
        }
    }

    private void Awake()
    {
        if (stageGenerator == null)
            stageGenerator = FindFirstObjectByType<StageGenerator>();

        if (difficultyManager == null)
            difficultyManager = FindFirstObjectByType<DifficultyManager>();

        if (turnController == null)
            turnController = FindFirstObjectByType<TurnController>();

        if (allyBonusApplier == null)
            allyBonusApplier = FindFirstObjectByType<AllyBonusApplier>();

        if (bossStageController == null)
            bossStageController = FindFirstObjectByType<BossStageController>();

        if (bossStageGimmickController == null)
            bossStageGimmickController = FindFirstObjectByType<BossStageGimmickController>();

        if (blockClearTracker == null)
            blockClearTracker = FindFirstObjectByType<BlockClearTracker>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

    }

    private void Start()
    {
        if (startDungeonOnStart)
            StartDungeon(currentDungeonDifficulty);
    }

    public void StartDungeon(DungeonDifficulty difficulty)
    {
        currentDungeonDifficulty = difficulty;
        currentStageIndex = -1;
        state = DungeonRunState.Exploring;

        if (avoidClearedDungeonSeeds)
        {
            dungeonSeed = GenerateUnclearedDungeonSeed(currentDungeonDifficulty);
        }

        // ダンジョン開始時だけHP全回復
        if (playerHealth != null)
            playerHealth.ResetHP();

        Debug.Log($"[Dungeon] Start Dungeon difficulty={currentDungeonDifficulty}, dungeonSeed={dungeonSeed}");

        NextStage();
    }

    public void NextStage()
    {
        currentStageIndex++;

        if (currentStageIndex >= StageCount)
        {
            DungeonClear();
            return;
        }

        DungeonStageData stageData = GetCurrentStageData();

        if (stageData == null)
        {
            Debug.LogError("[Dungeon] StageData is null.");
            return;
        }

        if (difficultyManager != null)
            difficultyManager.SetDifficulty(stageData.stageDifficulty);

        int stageSeed = CreateStageSeed(stageData.stageNumber);

        Debug.Log(
            $"[Dungeon] Generate Stage {stageData.stageNumber}/{StageCount}, " +
            $"DungeonDifficulty={currentDungeonDifficulty}, " +
            $"StageDifficulty={stageData.stageDifficulty}, " +
            $"BossStage={stageData.isBossStage}, " +
            $"Seed={stageSeed}"
        );

        if (stageGenerator != null)
            stageGenerator.Generate(stageSeed);

        if (bossStageController != null)
            bossStageController.RefreshVisibility();

        if (bossStageGimmickController != null)
            bossStageGimmickController.RefreshGimmicks();


        if (blockClearTracker != null)
            blockClearTracker.RefreshBlockCount();


        if (allyBonusApplier != null)
            allyBonusApplier.ApplyStageStartBonus();

        if (turnController != null)
            turnController.CancelToAim();

        state = DungeonRunState.Exploring;
    }

    public void OnStageCleared()
    {
        if (state != DungeonRunState.Exploring)
            return;

        state = DungeonRunState.StageClear;

        Debug.Log($"[Dungeon] Stage {CurrentStageNumber}/{StageCount} Clear!");
    }

    public void GoNextStageFromResult()
    {
        if (state != DungeonRunState.StageClear)
            return;

        NextStage();
    }

    public void RetryCurrentStage()
    {
        if (currentStageIndex < 0)
            return;

        DungeonStageData stageData = GetCurrentStageData();

        if (stageData == null)
            return;

        if (difficultyManager != null)
            difficultyManager.SetDifficulty(stageData.stageDifficulty);

        int stageSeed = CreateStageSeed(stageData.stageNumber);

        Debug.Log($"[Dungeon] Retry Stage {stageData.stageNumber}, seed={stageSeed}");

        if (stageGenerator != null)
            stageGenerator.Generate(stageSeed);

        if (turnController != null)
            turnController.CancelToAim();

        state = DungeonRunState.Exploring;
    }

    private void DungeonClear()
    {
        state = DungeonRunState.DungeonClear;

        DungeonSeedHistory.AddCleared(currentDungeonDifficulty, dungeonSeed);

        Debug.Log($"[Dungeon] Dungeon Clear! difficulty={currentDungeonDifficulty}, seed={dungeonSeed}");
    }

    private DungeonStageData GetCurrentStageData()
    {
        if (currentStageIndex < 0)
            return null;

        if (currentStageIndex >= StageCount)
            return null;

        DungeonDifficultyPlan plan = GetCurrentPlan();

        if (plan == null)
        {
            Debug.LogWarning("[Dungeon] Plan not found. Using fallback stage data.");
            return CreateFallbackStageData(currentStageIndex);
        }

        if (plan.stages == null || plan.stages.Length <= currentStageIndex)
        {
            Debug.LogWarning("[Dungeon] Plan stages is invalid. Using fallback stage data.");
            return CreateFallbackStageData(currentStageIndex);
        }

        if (plan.stages[currentStageIndex] == null)
        {
            Debug.LogWarning("[Dungeon] Stage element is null. Using fallback stage data.");
            return CreateFallbackStageData(currentStageIndex);
        }

        return plan.stages[currentStageIndex];
    }


    private DungeonDifficultyPlan GetCurrentPlan()
    {
        if (plans == null)
            return null;

        foreach (var plan in plans)
        {
            if (plan != null && plan.dungeonDifficulty == currentDungeonDifficulty)
                return plan;
        }

        return null;
    }

    private int CreateStageSeed(int stageNumber)
    {
        int dungeonOffset = (int)currentDungeonDifficulty * 10000;
        return dungeonSeed + dungeonOffset + stageNumber;
    }

    private DungeonStageData CreateFallbackStageData(int index)
    {
        DungeonStageData data = new DungeonStageData();
        data.stageNumber = index + 1;
        data.stageDifficulty = CreateFallbackDifficulty(index);
        data.isBossStage = index == StageCount - 1;
        return data;
    }

    private GameDifficulty CreateFallbackDifficulty(int index)
    {
        switch (currentDungeonDifficulty)
        {
            case DungeonDifficulty.Beginner:
                if (index <= 3) return GameDifficulty.Beginner;
                if (index <= 7) return GameDifficulty.Easy;
                return GameDifficulty.Normal;

            case DungeonDifficulty.Easy:
                if (index <= 2) return GameDifficulty.Easy;
                if (index <= 7) return GameDifficulty.Normal;
                return GameDifficulty.Hard;

            case DungeonDifficulty.Normal:
                if (index <= 1) return GameDifficulty.Easy;
                if (index <= 6) return GameDifficulty.Normal;
                return GameDifficulty.Hard;

            case DungeonDifficulty.Hard:
                if (index <= 1) return GameDifficulty.Normal;
                return GameDifficulty.Hard;
        }

        return GameDifficulty.Normal;
    }
    public void RetryDungeon()
    {
        Debug.Log("[Dungeon] Retry Dungeon");

        currentStageIndex = -1;
        state = DungeonRunState.NotStarted;

        StartDungeon(currentDungeonDifficulty);
    }

    private int GenerateUnclearedDungeonSeed(DungeonDifficulty difficulty)
    {
        for (int i = 0; i < maxSeedSearchTries; i++)
        {
            int candidate = Random.Range(randomSeedMin, randomSeedMax + 1);

            if (!DungeonSeedHistory.HasCleared(difficulty, candidate))
            {
                Debug.Log($"[Dungeon] New uncleared seed selected: {candidate}");
                return candidate;
            }
        }

        int fallback = Random.Range(randomSeedMin, randomSeedMax + 1);

        Debug.LogWarning($"[Dungeon] Could not find uncleared seed after {maxSeedSearchTries} tries. Using fallback seed={fallback}");

        return fallback;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (plans == null)
            return;

        foreach (var plan in plans)
        {
            if (plan == null)
                continue;

            if (plan.stages == null || plan.stages.Length != StageCount)
            {
                DungeonStageData[] newStages = new DungeonStageData[StageCount];

                for (int i = 0; i < StageCount; i++)
                {
                    if (plan.stages != null && i < plan.stages.Length && plan.stages[i] != null)
                    {
                        newStages[i] = plan.stages[i];
                    }
                    else
                    {
                        newStages[i] = new DungeonStageData();
                        newStages[i].stageNumber = i + 1;
                        newStages[i].stageDifficulty = GameDifficulty.Normal;
                        newStages[i].isBossStage = i == StageCount - 1;
                    }
                }

                plan.stages = newStages;
            }

            for (int i = 0; i < plan.stages.Length; i++)
            {
                if (plan.stages[i] == null)
                    plan.stages[i] = new DungeonStageData();

                plan.stages[i].stageNumber = i + 1;

                if (i == StageCount - 1)
                    plan.stages[i].isBossStage = true;
            }
        }
    }
#endif
}
