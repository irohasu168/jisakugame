using UnityEngine;
using UnityEngine.UI;

public class ShootButtonUI : MonoBehaviour
{
    [SerializeField] private TurnController turn;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();

        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void Update()
    {
        if (button == null || turn == null) return;

        button.interactable =
            turn.State == TurnController.TurnState.ReadyToShoot;
    }

    private void OnClick()
    {
        if (turn == null) return;

        Debug.Log("[ShootButton] Click");
        turn.Shoot();
    }
}