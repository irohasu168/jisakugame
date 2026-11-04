using UnityEngine;
using TMPro;

public class DungeonHUD : MonoBehaviour
{
    [SerializeField] private DungeonManager dungeon;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (dungeon == null)
            dungeon = FindFirstObjectByType<DungeonManager>();

        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (dungeon == null || text == null)
            return;

        text.text =
            $"Dungeon: {dungeon.CurrentDungeonDifficulty}\n" +
            $"Stage: {dungeon.CurrentStageNumber}/{dungeon.MaxStageCount}";
    }
}