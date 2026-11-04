using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BallMineTrigger : MonoBehaviour
{
    [SerializeField] private MineStackManager stackManager;

    [Header("Safety (same target explosion cooldown)")]
    [SerializeField] private float sameTargetCooldown = 0.2f;

    // “同一対象”判定用：Colliderごとに最後に爆発した時刻を記録
    private Dictionary<Collider, float> lastExplodeTime = new Dictionary<Collider, float>();

    private void Awake()
    {
        if (stackManager == null)
            stackManager = FindFirstObjectByType<MineStackManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stackManager == null) return;
        if (stackManager.Stack <= 0) return;

        Collider target = collision.collider;

        // 安全弁：同一対象への爆発CD
        float now = Time.time;
        if (lastExplodeTime.TryGetValue(target, out float last))
        {
            if (now - last < sameTargetCooldown)
                return;
        }
        lastExplodeTime[target] = now;

        // Mineを1消費して爆発扱い
        if (stackManager.ConsumeMine(1))
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            Debug.Log($"[Mine] Explode at {hitPoint} on {target.name}");

            // TODO（次の段階）：
            // ここで爆風ダメージ・エフェクト・SEを呼ぶ
        }
    }
}