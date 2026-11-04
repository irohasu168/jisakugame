using System.Collections;
using UnityEngine;
using Cinemachine;

public class WeaknessCinematic : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float panMaxOffset = 2.0f;

    [Header("Priority Override")]
    [SerializeField] private int boostPriority = 10000;

    [Header("Timing（企画書確定）")]
    [SerializeField] private float slowTimeScale = 0.15f;
    [SerializeField] private float slowDuration = 0.15f;
    [SerializeField] private float zoomMul = 1.05f;
    [SerializeField] private float recoverDuration = 0.10f;

    private int originalPriority;
    private float defaultFov;
    private float defaultTimeScale;
    private Coroutine playing;

    private void Awake()
    {
        defaultTimeScale = Time.timeScale;

        if (vcam != null)
        {
            originalPriority = vcam.Priority;
            defaultFov = vcam.m_Lens.FieldOfView;
        }
    }

    public void Play(Vector3 weakWorldPos)
    {
        Debug.Log($"[CINE] Play called. weakPos={weakWorldPos}");

        if (vcam == null)
        {
            Debug.LogError("[CINE] vcam is null.");
            return;
        }

        if (followTarget == null)
        {
            Debug.LogError("[CINE] followTarget is null.");
            return;
        }

        if (playing != null)
        {
            StopCoroutine(playing);
            Restore();
        }

        playing = StartCoroutine(CoPlay(weakWorldPos));
    }

    private IEnumerator CoPlay(Vector3 weakWorldPos)
    {
        originalPriority = vcam.Priority;
        defaultFov = vcam.m_Lens.FieldOfView;
        defaultTimeScale = Time.timeScale;

        Debug.Log($"[CINE] Before boost priority={vcam.Priority}");

        // Player VCamを確実にライブ化する
        vcam.Priority = boostPriority;

        Debug.Log($"[CINE] After boost priority={vcam.Priority}");

        // 弱点位置へフォーカスターゲットを寄せる
        Vector3 delta = weakWorldPos - followTarget.position;
        delta = Vector3.ClampMagnitude(delta, panMaxOffset);
        followTarget.position += delta;

        // テストで完全に弱点中心へ寄せたい場合は上の2行の代わりにこれでもOK
        // followTarget.position = weakWorldPos;

        float zoomFov = defaultFov / zoomMul;

        // ズームイン
        yield return CoEaseFov(defaultFov, zoomFov, slowDuration);

        // スロー
        Time.timeScale = slowTimeScale;
        yield return new WaitForSecondsRealtime(slowDuration);

        // スロー解除
        Time.timeScale = defaultTimeScale;

        // ズーム戻し
        yield return CoEaseFov(zoomFov, defaultFov, recoverDuration);

        Restore();
    }

    private IEnumerator CoEaseFov(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Clamp01(t / duration);

            // SmoothStep：EaseOut→EaseInに近い補間
            float eased = x * x * (3f - 2f * x);

            if (vcam != null)
                vcam.m_Lens.FieldOfView = Mathf.Lerp(from, to, eased);

            yield return null;
        }

        if (vcam != null)
            vcam.m_Lens.FieldOfView = to;
    }

    private void Restore()
    {
        if (vcam != null)
        {
            vcam.m_Lens.FieldOfView = defaultFov;
            vcam.Priority = originalPriority;
        }

        Time.timeScale = defaultTimeScale;
        playing = null;

        Debug.Log("[CINE] Restore");
    }

    private void OnDisable()
    {
        Restore();
    }
}