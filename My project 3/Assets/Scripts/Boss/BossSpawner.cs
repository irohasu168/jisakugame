using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform stageRoot;

    private GameObject currentBoss;

    public void SpawnBoss()
    {
        if (currentBoss != null) return;
        if (bossPrefab == null || spawnPoint == null) return;

        Transform parent = stageRoot != null ? stageRoot : null;

        currentBoss = Instantiate(
            bossPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            parent
        );

        Debug.Log("[BossSpawner] Boss spawned");
    }

    public void DespawnBoss()
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
        }
    }
}