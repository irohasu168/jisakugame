using UnityEngine;

public class AllyBonusApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RescueRoster rescueRoster;
    [SerializeField] private MineStackManager mineStackManager;

    [Header("Bonus")]
    [SerializeField] private int mineBonusPerAlly = 1;
    [SerializeField] private int maxBonusMine = 3;

    private void Awake()
    {
        if (rescueRoster == null)
            rescueRoster = FindFirstObjectByType<RescueRoster>();

        if (mineStackManager == null)
            mineStackManager = FindFirstObjectByType<MineStackManager>();
    }

    public void ApplyStageStartBonus()
    {
        if (rescueRoster == null || mineStackManager == null)
        {
            Debug.LogWarning("[AllyBonus] Missing reference.");
            return;
        }

        int allyCount = rescueRoster.Allies.Count;
        int bonusMine = Mathf.Clamp(allyCount * mineBonusPerAlly, 0, maxBonusMine);

        mineStackManager.SetStack(bonusMine);

        Debug.Log($"[AllyBonus] Allies={allyCount}, StartMine={bonusMine}");
    }
}