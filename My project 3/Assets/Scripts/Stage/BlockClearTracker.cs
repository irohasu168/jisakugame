using UnityEngine;

public class BlockClearTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private StageClearManager stageClearManager;

    [Header("Options")]
    [SerializeField] private bool clearWhenAllBreakableBlocksDestroyed = true;
    [SerializeField] private bool ignoreBossStage = true;

    private int totalBreakableBlocks;
    private int destroyedBreakableBlocks;
    private bool tracking;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (stageClearManager == null)
            stageClearManager = FindFirstObjectByType<StageClearManager>();
    }

    private void OnEnable()
    {
        BlockHealth.OnAnyBlockDestroyed += HandleBlockDestroyed;
    }

    private void OnDisable()
    {
        BlockHealth.OnAnyBlockDestroyed -= HandleBlockDestroyed;
    }

    public void RefreshBlockCount()
    {
        BlockHealth[] blocks = FindObjectsByType<BlockHealth>(FindObjectsSortMode.None);

        totalBreakableBlocks = 0;
        destroyedBreakableBlocks = 0;

        foreach (BlockHealth block in blocks)
        {
            if (block == null)
                continue;

            if (block.IsAlive)
                totalBreakableBlocks++;
        }

        tracking = true;

        Debug.Log($"[BlockClearTracker] BreakableBlocks={totalBreakableBlocks}");
    }

    private void HandleBlockDestroyed(BlockHealth block)
    {
        if (!tracking)
            return;

        if (!clearWhenAllBreakableBlocksDestroyed)
            return;

        if (ignoreBossStage &&
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage)
        {
            return;
        }

        destroyedBreakableBlocks++;

        Debug.Log($"[BlockClearTracker] Destroyed={destroyedBreakableBlocks}/{totalBreakableBlocks}");

        if (totalBreakableBlocks <= 0)
            return;

        if (destroyedBreakableBlocks >= totalBreakableBlocks)
        {
            Debug.Log("[BlockClearTracker] All breakable blocks destroyed. Stage Clear!");

            tracking = false;

            if (stageClearManager != null)
                stageClearManager.Clear();
        }
    }
}