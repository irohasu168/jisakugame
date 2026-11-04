using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BallMineExplosion : MonoBehaviour
{
    [SerializeField] private MineStackManager stackManager;

    [Header("Explosion")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private LayerMask damageableLayers = ~0;

    [Tooltip("ターゲット最大HPに掛ける割合。例: 0.07 = 7%")]
    [SerializeField] private float rate = 0.07f;

    [Tooltip("最低保証ダメージ")]
    [SerializeField] private int minDamage = 1;

    [Header("Safety")]
    [SerializeField] private float sameTargetCooldown = 0.2f;

    private readonly Dictionary<int, float> lastExplodeTime = new Dictionary<int, float>();

    private void Awake()
    {
        if (stackManager == null)
            stackManager = FindFirstObjectByType<MineStackManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stackManager == null)
            return;

        if (stackManager.Stack <= 0)
            return;

        IDamageable damageable =
            collision.collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            Debug.Log($"[MineExplosion] Not consumed. Hit non-damageable: {collision.collider.name}");
            return;
        }

        if (!IsInLayerMask(collision.collider.gameObject.layer, damageableLayers))
        {
            Debug.Log($"[MineExplosion] Not consumed. Layer not damageable: {collision.collider.name}");
            return;
        }

        if (!stackManager.ConsumeMine(1))
            return;

        ContactPoint cp = collision.GetContact(0);

        Explode(cp.point, cp.normal);
    }

    private void Explode(Vector3 center, Vector3 normal)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            radius,
            damageableLayers
        );

        int damagedCount = 0;

        foreach (Collider hit in hits)
        {
            int id = hit.GetInstanceID();

            if (lastExplodeTime.TryGetValue(id, out float lastTime))
            {
                if (Time.time - lastTime < sameTargetCooldown)
                    continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            int maxHp = TryGetMaxHP(damageable);
            int damage = Mathf.Max(minDamage, Mathf.CeilToInt(maxHp * rate));

            DamageInfo info = new DamageInfo(
                damage,
                DamageType.MineExplosion,
                gameObject,
                hit.ClosestPoint(center)
            );

            info.Normal = normal;

            damageable.ApplyDamage(info);

            lastExplodeTime[id] = Time.time;
            damagedCount++;
        }

        Debug.Log($"[MineExplosion] Exploded. center={center}, hits={hits.Length}, damaged={damagedCount}");

        // Ball本体に付けている場合はDestroyしない方が安全。
        // Mineオブジェクト自体に付ける場合だけDestroyする。
        // Destroy(gameObject);
    }

    private int TryGetMaxHP(IDamageable dmg)
    {
        Component component = dmg as Component;

        if (component == null)
            return 1;

        HealthBase hb = component.GetComponent<HealthBase>();

        if (hb != null)

            hb = component.GetComponentInParent<HealthBase>();

        if (hb != null)
            return hb.MaxHP;

        return 1;
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
