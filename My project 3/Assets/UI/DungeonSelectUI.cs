using UnityEngine;

public class DungeonSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject turnInputRoot;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject debugRoot;
    [SerializeField] private TurnUIVisibility turnUIVisibility;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (selectPanel == null)
            selectPanel = gameObject;

        if (turnUIVisibility == null)
            turnUIVisibility = FindFirstObjectByType<TurnUIVisibility>();
    }

    private void Start()
    {
        ShowSelectFromResult();
    }

    public void ShowSelectFromResult()
    {
        if (selectPanel != null)
            selectPanel.SetActive(true);

        if (hudRoot != null)
            hudRoot.SetActive(false);

        if (turnInputRoot != null)
            turnInputRoot.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (debugRoot != null)
            debugRoot.SetActive(false);

        if (turnUIVisibility != null)
            turnUIVisibility.SetUILocked(true);

        Debug.Log("[DungeonSelectUI] Back To Dungeon Select");
    }

    public void StartBeginnerDungeon()
    {
        StartDungeon(DungeonDifficulty.Beginner);
    }

    public void StartEasyDungeon()
    {
        StartDungeon(DungeonDifficulty.Easy);
    }

    public void StartNormalDungeon()
    {
        StartDungeon(DungeonDifficulty.Normal);
    }

    public void StartHardDungeon()
    {
        StartDungeon(DungeonDifficulty.Hard);
    }

    private void StartDungeon(DungeonDifficulty difficulty)
    {
        if (dungeonManager == null)
        {
            Debug.LogError("[DungeonSelectUI] DungeonManager is null.");
            return;
        }

        if (turnUIVisibility != null)
            turnUIVisibility.SetUILocked(false);

        dungeonManager.StartDungeon(difficulty);

        if (selectPanel != null)
            selectPanel.SetActive(false);

        if (hudRoot != null)
            hudRoot.SetActive(true);

        if (debugRoot != null)
            debugRoot.SetActive(false);

        Debug.Log($"[DungeonSelectUI] Start {difficulty} Dungeon");
    }
}