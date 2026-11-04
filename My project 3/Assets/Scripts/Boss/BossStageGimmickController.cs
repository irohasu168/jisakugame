using UnityEngine;

public class BossStageGimmickController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;

    [Header("Gimmick Roots")]
    [SerializeField] private GameObject normalGimmickRoot;
    [SerializeField] private GameObject hardGimmickRoot;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();
    }

    private void Start()
    {
        RefreshGimmicks();
    }

    public void RefreshGimmicks()
    {
        bool isBossStage =
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage;

        DungeonDifficulty difficulty =
            dungeonManager != null
                ? dungeonManager.CurrentDungeonDifficulty
                : DungeonDifficulty.Normal;

        bool useNormalGimmick =
            isBossStage &&
            (difficulty == DungeonDifficulty.Normal ||
             difficulty == DungeonDifficulty.Hard);

        bool useHardGimmick =
            isBossStage &&
            difficulty == DungeonDifficulty.Hard;

        if (normalGimmickRoot != null)
            normalGimmickRoot.SetActive(useNormalGimmick);

        if (hardGimmickRoot != null)
            hardGimmickRoot.SetActive(useHardGimmick);

        Debug.Log(
            $"[BossGimmick] BossStage={isBossStage}, Difficulty={difficulty}, " +
            $"NormalGimmick={useNormalGimmick}, HardGimmick={useHardGimmick}"
        );
    }
}
