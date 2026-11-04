using UnityEngine;

public class BossStageController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private MidBossController bossController;

    [Header("Boss Objects")]
    [SerializeField] private Transform bossRoot;
    [SerializeField] private GameObject bossVisualRoot;

    [Header("Spawn")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Vector3 fallbackBossSpawnPosition = new Vector3(0f, -3f, 5f);

    [Header("Auto Collected Parts")]
    [SerializeField] private Renderer[] bossRenderers;
    [SerializeField] private Collider[] bossColliders;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (bossController == null)
            bossController = FindFirstObjectByType<MidBossController>();

        if (bossRoot == null && bossController != null)
            bossRoot = bossController.transform;

        CollectBossParts();
    }

    private void Start()
    {
        SetBossStageActive(false);
    }

    private void CollectBossParts()
    {
        Transform searchRoot = bossRoot;

        if (bossVisualRoot != null)
            searchRoot = bossVisualRoot.transform;

        if (searchRoot == null)
            return;

        bossRenderers = searchRoot.GetComponentsInChildren<Renderer>(true);
        bossColliders = searchRoot.GetComponentsInChildren<Collider>(true);
    }

    public void RefreshVisibility()
    {
        bool isBossStage =
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage;

        if (isBossStage)
        {
            MoveBossToSpawnPosition();

            if (bossController != null)
                bossController.ResetBoss();

            SetBossStageActive(true);

            Debug.Log("[BossStageController] Boss enabled for boss stage.");
        }
        else
        {
            SetBossStageActive(false);

            Debug.Log("[BossStageController] Boss disabled for normal stage.");
        }
    }

    private void MoveBossToSpawnPosition()
    {
        if (bossRoot == null)
            return;

        Vector3 spawnPos =
            bossSpawnPoint != null
                ? bossSpawnPoint.position
                : fallbackBossSpawnPosition;

        bossRoot.position = spawnPos;
        bossRoot.rotation = Quaternion.identity;
    }

    private void SetBossStageActive(bool active)
    {
        if (bossVisualRoot != null)
            bossVisualRoot.SetActive(active);

        if (bossRenderers != null)
        {
            foreach (Renderer r in bossRenderers)
            {
                if (r != null)
                    r.enabled = active;
            }
        }

        if (bossColliders != null)
        {
            foreach (Collider c in bossColliders)
            {
                if (c != null)
                    c.enabled = active;
            }
        }

        // MidBossController自体は残しておく。
        // StageClearManagerのイベント購読を壊さないため。
        if (bossController != null)
            bossController.enabled = true;
    }
}