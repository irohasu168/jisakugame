using UnityEngine;
using System;
public class MidBossController : MonoBehaviour, IDamageable
{
    [Header("HP")]
    [SerializeField] private int maxHp = 200;
    [SerializeField] private int hp = 200;

    [Header("Shield")]
    [Range(0f, 1f)]
    [SerializeField] private float shieldDamageRate = 0.35f; // シールド中35% 

    [Header("Weakness")]
    [SerializeField] private Transform weakPoint;            // 弱点Transform
    [SerializeField] private int weakHitToExpose = 3;        // 直撃3回 
    [SerializeField] private int turnsToExpose = 2;          // 2T経過 
    [SerializeField] private float weakMultiplier = 3f;      // 弱点×3 
    [SerializeField] private TurnController turn;
    [SerializeField] private WeaknessCinematic cinematic;
    [SerializeField] private WeakPointVisual weakPointVisual;
    private bool weakExposed = false;
    private int weakHitCount = 0;
    private int turnCount = 0;
    private bool isWeakExposed = false;
    public event Action OnBossDefeated;
    public bool IsAlive => hp > 0;
    private bool defeated;
    public int CurrentHP => hp;
    public int MaxHP => maxHp;
    public bool IsDefeated => defeated;
    private void Awake()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
        if (turn == null) turn = FindFirstObjectByType<TurnController>();
        if (weakPointVisual != null)
            weakPointVisual.SetExposed(false);
    }

    private void OnEnable()
    {
        if (turn != null) turn.OnTurnEnded += OnTurnEnded;
    }

    private void OnDisable()
    {
        if (turn != null) turn.OnTurnEnded -= OnTurnEnded;
    }

    private void OnTurnEnded()
    {
        turnCount++;

        // 2T経過で露出（ハイブリッド） 
        if (!isWeakExposed && turnCount >= turnsToExpose)
        {
            ExposeWeakness();
        }

        // 露出が「1ターンだけ」ならここで戻す（モードA寄り）
        // 今回はまず露出維持 or 演出だけにしておく（後でモードA/B-1を切替可能）
    }

    public void RegisterWeakHit()
    {
        if (isWeakExposed) return;

        weakHitCount++;
        if (weakHitCount >= weakHitToExpose)
        {
            ExposeWeakness(); // 直撃3回で露出 
        }
    }

    private void ExposeWeakness()
    {
        if (weakExposed) return;

        weakExposed = true;

        Debug.Log("[Boss] Weakness Exposed -> call cinematic");

        if (weakPointVisual != null)
            weakPointVisual.SetExposed(true);

        if (cinematic != null && weakPoint != null)
            cinematic.Play(weakPoint.position);
    }

    public void ApplyDamage(DamageInfo info)
    {
        if (defeated)
            return;

        int baseDamage = Mathf.Max(0, info.Amount);
        int finalDamage = baseDamage;

        if (weakExposed)
        {
            finalDamage = Mathf.CeilToInt(baseDamage * weakMultiplier);
            Debug.Log($"[Boss] Weak damage! base={baseDamage}, x{weakMultiplier}, final={finalDamage}");
        }
        else
        {
            finalDamage = Mathf.CeilToInt(baseDamage * shieldDamageRate);
            finalDamage = Mathf.Max(1, finalDamage);
            Debug.Log($"[Boss] Shield damage. base={baseDamage}, rate={shieldDamageRate}, final={finalDamage}");
        }

        hp -= finalDamage;
        hp = Mathf.Max(0, hp);

        Debug.Log($"[Boss] HP={hp}/{maxHp}");

        if (hp <= 0)
        {
            defeated = true;

            Debug.Log("[Boss] Defeated!");

            OnBossDefeated?.Invoke();
        }
    }

    public void ResetBoss()
    {
        hp = maxHp;
        weakExposed = false;
        defeated = false;

        if (weakPointVisual != null)
            weakPointVisual.SetExposed(false);

        Debug.Log($"[Boss] Reset HP={hp}/{maxHp}");
    }
}