using UnityEngine;

public abstract class HealthBase : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHp = 10;
    [SerializeField] protected int hp = 10;

    public bool IsAlive => hp > 0;
    public int HP => hp;
    public int MaxHP => maxHp;

    protected virtual void Awake()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
    }

    public virtual void ApplyDamage(DamageInfo info)
    {
        if (!IsAlive) return;

        hp -= Mathf.Max(0, info.Amount);
        hp = Mathf.Max(0, hp);

        // デバッグ
        Debug.Log($"[{gameObject.name}] Damage {info.Type} -{info.Amount}  HP={hp}/{maxHp}");

        if (hp <= 0)
        {
            OnDead(info);
        }
    }

    protected abstract void OnDead(DamageInfo lastHit);
}
