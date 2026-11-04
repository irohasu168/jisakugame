using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private MidBossController boss;

    [Header("UI Root")]
    [SerializeField] private GameObject viewRoot;

    [Header("UI Parts")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    [Header("Options")]
    [SerializeField] private bool hideWhenBossDefeated = true;

    private void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (boss == null)
            boss = FindFirstObjectByType<MidBossController>();

        if (viewRoot == null)
            viewRoot = gameObject;
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isBossStage =
            dungeonManager != null &&
            dungeonManager.IsCurrentBossStage;

        bool hasBoss = boss != null;

        bool bossAlive =
            hasBoss &&
            !boss.IsDefeated &&
            boss.CurrentHP > 0;

        bool shouldShow =
            isBossStage &&
            hasBoss &&
            (!hideWhenBossDefeated || bossAlive);

        if (viewRoot != null && viewRoot.activeSelf != shouldShow)
            viewRoot.SetActive(shouldShow);

        if (!shouldShow)
            return;

        int currentHp = Mathf.Max(0, boss.CurrentHP);
        int maxHp = Mathf.Max(1, boss.MaxHP);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"Boss HP: {currentHp} / {maxHp}";
        }
    }
}
