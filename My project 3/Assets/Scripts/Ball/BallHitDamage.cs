using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BallHitDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int hitDamage = 1;
    [SerializeField] private LayerMask damageableLayers = ~0;

    [Header("Anti Multi-Hit")]
    [SerializeField] private float sameTargetCooldown = 0.1f;

    private GameObject lastHitObject;
    private float lastHitTime;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject targetObject = collision.collider.gameObject;

        if (!IsInLayerMask(targetObject.layer, damageableLayers))
        {
            return;
        }

        if (targetObject == lastHitObject && Time.time - lastHitTime < sameTargetCooldown)
        {
            return;
        }

        IDamageable damageable =
            collision.collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        Vector3 hitPoint = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : collision.transform.position;

        DamageInfo info = new DamageInfo(
            hitDamage,
            DamageType.BallHit,
            gameObject,
            hitPoint
        );

        if (collision.contacts.Length > 0)
        {
            info.Normal = collision.contacts[0].normal;
        }

        damageable.ApplyDamage(info);

        lastHitObject = targetObject;
        lastHitTime = Time.time;

        Debug.Log($"[BallHitDamage] Hit {targetObject.name}, damage={hitDamage}");
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}