using UnityEngine;

public class BallVisibility : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private bool hideOnStart = true;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        if (hideOnStart)
            Hide();
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }
}