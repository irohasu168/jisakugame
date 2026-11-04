using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int hp = 100;

    [Header("References")]
    [SerializeField] private StageClearManager stageClearManager;

    [Header("Options")]
    [SerializeField] private bool resetOnStart = true;

    public int CurrentHP => hp;
    public int MaxHP => maxHp;
    public bool IsDead => hp <= 0;

    public event Action<int, int> OnHPChanged;
    public event Action OnDead;

    private bool dead;

    private void Awake()
    {
        if (stageClearManager == null)
            stageClearManager = FindFirstObjectByType<StageClearManager>();
    }

    private void Start()
    {
        if (resetOnStart)
            ResetHP();
    }

    public void ResetHP()
    {
        hp = maxHp;
        dead = false;

        OnHPChanged?.Invoke(hp, maxHp);

        Debug.Log($"[PlayerHealth] Reset HP={hp}/{maxHp}");
    }

    public void ApplyDamage(int damage, string source = "Unknown")
    {
        if (dead)
            return;

        if (damage <= 0)
            return;

        int before = hp;

        hp = Mathf.Max(0, hp - damage);

        Debug.Log($"[PlayerHealth] Damage={damage}, Source={source}, HP={before}->{hp}/{maxHp}");

        OnHPChanged?.Invoke(hp, maxHp);

        if (hp <= 0)
        {
            dead = true;

            Debug.Log("[PlayerHealth] Dead");

            OnDead?.Invoke();

            if (stageClearManager != null)
                stageClearManager.Failed();
        }
    }

    public void Heal(int amount)
    {
        if (dead)
            return;

        if (amount <= 0)
            return;

        int before = hp;

        hp = Mathf.Clamp(hp + amount, 0, maxHp);

        Debug.Log($"[PlayerHealth] Heal={amount}, HP={before}->{hp}/{maxHp}");

        OnHPChanged?.Invoke(hp, maxHp);
    }
}