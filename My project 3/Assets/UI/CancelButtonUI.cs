using UnityEngine;
using UnityEngine.UI;

public class CancelButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TurnController turn;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        turn?.CancelToAim();
    }
}