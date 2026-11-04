using UnityEngine;
using TMPro;

public class MineHUD : MonoBehaviour
{
    [SerializeField] private MineStackManager mineStack;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (mineStack == null)
            mineStack = FindFirstObjectByType<MineStackManager>();

        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (mineStack == null || text == null) return;

        text.text = $"Mine: {mineStack.Stack}/{mineStack.MaxStack}";
    }
}