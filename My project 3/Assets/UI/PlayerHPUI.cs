using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHPChanged += Refresh;

        ForceRefresh();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHPChanged -= Refresh;
    }

    private void ForceRefresh()
    {
        if (playerHealth == null)
            return;

        Refresh(playerHealth.CurrentHP, playerHealth.MaxHP);
    }

    private void Refresh(int currentHp, int maxHp)
    {
        currentHp = Mathf.Max(0, currentHp);
        maxHp = Mathf.Max(1, maxHp);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"HP: {currentHp} / {maxHp}";
        }
    }
}