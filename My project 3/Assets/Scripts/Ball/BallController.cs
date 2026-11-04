using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public event Action OnShotStopped;

    [Header("Stop Conditions")]
    [SerializeField] private float minShotTime = 0.5f;
    [SerializeField] private float maxShotTime = 5.0f;
    [SerializeField] private float stopSpeedThreshold = 0.2f;
    [SerializeField] private int maxBounces = 30;

    [Header("Debug")]
    [SerializeField] private KeyCode forceEndKey = KeyCode.F8;
    [SerializeField] private bool logState = true;
    [SerializeField] private BallVisibility ballVisibility;
    private Rigidbody rb;
    private bool running;
    private float timer;
    private int bounceCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (ballVisibility == null)
            ballVisibility = GetComponent<BallVisibility>();

        SetIdle();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (running && Input.GetKeyDown(forceEndKey))
        {
            Debug.Log("[Ball] Force stop by F8");
            StopShot();
        }
#endif
    }

    public void Launch(Vector3 dir, float power)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (ballVisibility != null)
            ballVisibility.Show();

        dir.Normalize();

        // 物理を有効化
        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.WakeUp();

        // 速度初期化
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 状態初期化
        timer = 0f;
        bounceCount = 0;
        running = true;

        // 速度を直接設定
        rb.velocity = dir * power;

        Debug.Log(
            $"[Ball] Launch AFTER velocity dir={dir} power={power} " +
            $"isKinematic={rb.isKinematic} constraints={rb.constraints} velocity={rb.velocity}"
        );
    }

    private void FixedUpdate()
    {
        if (!running) return;

        timer += Time.fixedDeltaTime;

        float speed = rb.velocity.magnitude;

        bool canCheckSpeed = timer >= minShotTime;
        bool bySpeed = canCheckSpeed && speed < stopSpeedThreshold;
        bool byTime = timer >= maxShotTime;
        bool byBounce = bounceCount >= maxBounces;

        if (logState && Mathf.FloorToInt(timer * 10f) % 10 == 0)
        {
            // ログが多すぎる場合は logState をOFFにする
            Debug.Log($"[Ball] timer={timer:F2} speed={speed:F2} bounces={bounceCount}");
        }

        if (bySpeed || byTime || byBounce)
        {
            string reason =
                bySpeed ? "Speed" :
                byTime ? "Time" :
                "Bounce";

            Debug.Log($"[Ball] Shot stopped by {reason}. timer={timer:F2}, speed={speed:F2}, bounces={bounceCount}");
            StopShot();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!running) return;

        bounceCount++;

        if (logState)
            Debug.Log($"[Ball] Bounce {bounceCount} with {collision.collider.name}");
    }

    private void StopShot()
    {
        if (!running) return;

        running = false;

        SetIdle();

        OnShotStopped?.Invoke();
    }

    private void SetIdle()
    {
        if (rb == null) return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 待機中は絶対に動かさない
        rb.isKinematic = true;
        rb.Sleep();
    }
}
