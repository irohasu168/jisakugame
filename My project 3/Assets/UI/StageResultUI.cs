using UnityEngine;
using TMPro;

public class StageResultUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private RescueRoster rescueRoster;
    [SerializeField] private MineStackManager mineStackManager;

    [Header("Texts")]
    [SerializeField] private TMP_Text clearText;
    [SerializeField] private TMP_Text resultText;

    [Header("Buttons")]
    [SerializeField] private GameObject nextStageButton;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject retryDungeonButton;
    [SerializeField] private GameObject backToSelectButton;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (rescueRoster == null)
            rescueRoster = FindFirstObjectByType<RescueRoster>();

        if (mineStackManager == null)
            mineStackManager = FindFirstObjectByType<MineStackManager>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (dungeonManager == null)
            return;

        bool isFinalStage =
            dungeonManager.CurrentStageNumber >= dungeonManager.MaxStageCount;

        if (clearText != null)
        {
            clearText.text = isFinalStage
                ? "DUNGEON CLEAR!"
                : "STAGE CLEAR!";
        }

        int allyCount = rescueRoster != null ? rescueRoster.Allies.Count : 0;
        int maxAllies = rescueRoster != null ? rescueRoster.MaxAllies : 3;

        int mineCount = mineStackManager != null ? mineStackManager.Stack : 0;
        int maxMine = mineStackManager != null ? mineStackManager.MaxStack : 3;

        if (resultText != null)
        {
            resultText.text =
                $"Stage {dungeonManager.CurrentStageNumber} / {dungeonManager.MaxStageCount} Clear\n" +
                $"Dungeon: {dungeonManager.CurrentDungeonDifficulty}\n" +
                $"Allies: {allyCount} / {maxAllies}\n" +
                $"Mine: {mineCount} / {maxMine}";
        }

        if (nextStageButton != null)
            nextStageButton.SetActive(!isFinalStage);

        if (retryButton != null)
            retryButton.SetActive(true);


        if (retryDungeonButton != null)
            retryDungeonButton.SetActive(isFinalStage);

        if (backToSelectButton != null)
            backToSelectButton.SetActive(isFinalStage);

    }
}