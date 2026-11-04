using UnityEngine;

public class MineStackManager : MonoBehaviour
{
    [Header("Mine Stack")]
    [SerializeField] private int maxStack = 3;
    [SerializeField] private int stack = 0;

    public int Stack => stack;
    public int MaxStack => maxStack;

    private void Awake()
    {
        stack = Mathf.Clamp(stack, 0, maxStack);
    }

    public bool AddMine(int amount)
    {
        if (amount <= 0)
            return false;

        int before = stack;

        stack = Mathf.Clamp(stack + amount, 0, maxStack);

        Debug.Log($"[MineStack] Add {amount}. {before} -> {stack}/{maxStack}");

        return stack > before;
    }

    public bool ConsumeMine(int amount)
    {
        if (amount <= 0)
            return false;

        if (stack < amount)
        {
            Debug.Log($"[MineStack] Not enough mine. stack={stack}, need={amount}");
            return false;
        }

        int before = stack;

        stack = Mathf.Clamp(stack - amount, 0, maxStack);

        Debug.Log($"[MineStack] Consume {amount}. {before} -> {stack}/{maxStack}");

        return true;
    }

    public void SetStack(int value)
    {
        int before = stack;

        stack = Mathf.Clamp(value, 0, maxStack);

        Debug.Log($"[MineStack] SetStack {before} -> {stack}/{maxStack}");
    }

    public void Clear()
    {
        SetStack(0);
    }

    public bool IsFull()
    {
        return stack >= maxStack;
    }

    public bool HasMine()
    {
        return stack > 0;
    }
}