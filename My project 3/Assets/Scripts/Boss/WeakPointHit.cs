using UnityEngine;

public class WeakPointHit : MonoBehaviour
{
    [SerializeField] private MidBossController boss;

    private void Awake()
    {
        if (boss == null) boss = GetComponentInParent<MidBossController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            boss?.RegisterWeakHit();
        }
    }
}