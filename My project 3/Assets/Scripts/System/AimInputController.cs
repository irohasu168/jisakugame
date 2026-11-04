using UnityEngine;
using UnityEngine.EventSystems;

public class AimInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shooter;
    [SerializeField] private AimIndicator aimIndicator;
    [SerializeField] private TurnController turn;

    [Header("Tuning")]
    [SerializeField] private float maxDragPixels = 300f;
    [SerializeField] private float minDragPixels = 15f;
    [SerializeField] private float maxPower = 20f;

    [Header("Aim Direction")]
    [Tooltip("ONなら、引っ張った方向とは逆方向に撃つ。OFFならドラッグ方向へ撃つ。")]
    [SerializeField] private bool invertDrag = true;

    private bool dragging;
    private Vector2 startPos;
    private Vector2 currentPos;
    private int moveLogCounter;

    private void Awake()
    {
        dragging = false;

        if (shooter == null)
        {
            Debug.LogError("[Aim] Shooter が未設定です。Player/Shooter を Inspector に入れてください。", this);
            enabled = false;
            return;
        }

        if (turn == null)
        {
            Debug.LogError("[Aim] TurnController が未設定です。", this);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool overUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            Debug.Log($"[Aim] MouseDown overUI={overUI}");

            if (overUI)
            {
                Debug.Log("[Aim] Blocked by UI");
                dragging = false;
                return;
            }

            Begin(Input.mousePosition);
        }

        if (dragging)
        {
            currentPos = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                Move(currentPos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log($"[Aim] MouseUp detected dragging={dragging}");
                End(currentPos);
                return;
            }

            // MouseUpを取り逃した時の保険
            if (!Input.GetMouseButton(0))
            {
                Debug.Log("[Aim] Mouse released fallback");
                End(currentPos);
                return;
            }
        }
    }

    private void HandleTouch()
    {
        if (Input.touchCount != 1)
        {
            if (dragging)
            {
                dragging = false;
                aimIndicator?.SetVisible(false);
            }

            return;
        }

        Touch t = Input.GetTouch(0);

        // ここから下は既存処理
    }

    private void Begin(Vector2 pos)
    {
        Debug.Log($"[Aim] Begin from object={gameObject.name}");

        if (turn.State != TurnController.TurnState.InputAim)
        {
            Debug.Log($"[Aim] Blocked by TurnState: {turn.State}");
            dragging = false;
            return;
        }

        dragging = true;
        startPos = pos;
        currentPos = pos;

        aimIndicator?.SetVisible(true);

        Vector3 forward = shooter.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
            forward = Vector3.forward;

        forward.Normalize();

        aimIndicator?.UpdateIndicator(shooter.position, forward, 0f);
    }

    private void Move(Vector2 pos)
    {
        currentPos = pos;

        if ((moveLogCounter++ % 10) == 0)
            Debug.Log("[Aim] Move");

        Vector2 drag = currentPos - startPos;

        float dragLen = Mathf.Clamp(drag.magnitude, 0f, maxDragPixels);
        float power01 = dragLen / maxDragPixels;

        // ★横方向360度：ドラッグベクトル全体から狙い方向を作る
        Vector3 dirXZ = ComputeAimDirectionFromDrag(drag);

        aimIndicator?.UpdateIndicator(shooter.position, dirXZ, power01);
    }

    private void End(Vector2 releasePos)
    {
        Debug.Log("[Aim] End");

        currentPos = releasePos;
        dragging = false;
        aimIndicator?.SetVisible(false);

        Vector2 drag = currentPos - startPos;
        float dragMagnitude = drag.magnitude;

        Debug.Log($"[Aim] start={startPos} current={currentPos}");
        Debug.Log($"[Aim] Drag magnitude = {dragMagnitude}");

        if (dragMagnitude < minDragPixels)
        {
            Debug.Log("[Aim] Drag too short. Aim canceled.");
            return;
        }

        float dragLen = Mathf.Clamp(dragMagnitude, 0f, maxDragPixels);
        float power01 = dragLen / maxDragPixels;
        float power = power01 * maxPower;

        // ★横方向360度：End時もMoveと同じ計算を使う
        Vector3 dirXZ = ComputeAimDirectionFromDrag(drag);

        if (turn == null)
        {
            Debug.LogError("[Aim] TurnController is null.");
            return;
        }

        turn.SetAim(dirXZ, power);
        turn.ConfirmAim();

        Debug.Log($"Aim Confirmed: power={power:F2}, dir={dirXZ}");
    }

    /// <summary>
    /// 画面上のドラッグ方向から、カメラ基準のワールド水平360度方向を作る。
    /// </summary>
    private Vector3 ComputeAimDirectionFromDrag(Vector2 drag)
    {
        // ドラッグがほぼ無い時は、Shooterの前方向を使う
        if (drag.sqrMagnitude <= 0.0001f)
        {
            Vector3 fallback = shooter.forward;
            fallback.y = 0f;

            if (fallback.sqrMagnitude <= 0.0001f)
                fallback = Vector3.forward;

            fallback.Normalize();
            return fallback;
        }

        // invertDragがONなら「引っ張った方向と逆へ撃つ」
        Vector2 aimDrag = invertDrag ? -drag : drag;

        // プレイヤー/シューター基準の前方向
        Vector3 forward = shooter.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
            forward = Vector3.forward;

        forward.Normalize();

        // プレイヤー/シューター基準の右方向
        Vector3 right = shooter.right;
        right.y = 0f;

        if (right.sqrMagnitude <= 0.0001f)
            right = Vector3.right;

        right.Normalize();

        // 画面上のドラッグ方向を、プレイヤー基準の水平ワールド方向へ変換
        Vector3 dir =
            right * aimDrag.x +
            forward * aimDrag.y;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = forward;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.forward;

        dir.Normalize();

        return dir;
    }

}