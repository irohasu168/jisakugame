using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBehaviorProbe : MonoBehaviour
{
    [Header("Assign (or left null = auto find)")]
    [SerializeField] private TurnController turn;
    [SerializeField] private AimInputController aim;
    [SerializeField] private BallController ball;

    private void Awake()
    {
        if (turn == null) turn = FindFirstObjectByType<TurnController>();
        if (aim == null) aim = FindFirstObjectByType<AimInputController>();
        if (ball == null) ball = FindFirstObjectByType<BallController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            var es = EventSystem.current;
            bool overUI = (es != null) && es.IsPointerOverGameObject();

            string turnState = (turn != null) ? turn.State.ToString() : "null";
            string aimState = (aim != null) ? (aim.enabled ? "enabled" : "DISABLED") : "null";
            string ballV = (ball != null) ? ball.GetComponent<Rigidbody>().velocity.magnitude.ToString("F3") : "null";

            Debug.Log($"[Probe] timeScale={Time.timeScale:F2} OverUI={overUI} Turn={turnState} Aim={aimState} BallSpeed={ballV}");
        }
    }
}