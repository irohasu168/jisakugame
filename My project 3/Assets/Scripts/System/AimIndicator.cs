using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform directionLine;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private Renderer[] renderers;

    [Header("Visual Settings")]
    [SerializeField] private float minLength = 1.5f;
    [SerializeField] private float maxLength = 6.0f;
    [SerializeField] private float lineThickness = 0.12f;
    [SerializeField] private float yOffset = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color weakColor = Color.cyan;
    [SerializeField] private Color strongColor = Color.red;

    private Material[] runtimeMaterials;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        runtimeMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                runtimeMaterials[i] = renderers[i].material;
        }

        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void UpdateIndicator(Vector3 origin, Vector3 direction, float power01)
    {
        if (direction.sqrMagnitude <= 0.0001f) return;

        direction.Normalize();

        power01 = Mathf.Clamp01(power01);

        float length = Mathf.Lerp(minLength, maxLength, power01);

        Vector3 start = origin + Vector3.up * yOffset;
        Vector3 center = start + direction * (length * 0.5f);
        Vector3 end = start + direction * length;

        transform.position = start;

        Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);

        if (directionLine != null)
        {
            directionLine.position = center;
            directionLine.rotation = lookRot;
            directionLine.localScale = new Vector3(lineThickness, lineThickness, length);
        }

        if (arrowHead != null)
        {
            arrowHead.position = end;
            arrowHead.rotation = lookRot;
        }

        // ‚±‚±‚ÅF‚ð”½‰f
        Color c = Color.Lerp(weakColor, strongColor, power01);
        ApplyColor(c);
    }

    private void ApplyColor(Color color)
    {
        if (runtimeMaterials == null) return;

        foreach (var mat in runtimeMaterials)
        {
            if (mat != null)
                mat.color = color;
        }
    }
}