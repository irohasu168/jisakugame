using UnityEngine;

public class WeakPointVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Colors")]
    [SerializeField] private Color hiddenColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color exposedColor = Color.red;

    [Header("Scale")]
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.35f, 0.35f, 0.35f);
    [SerializeField] private Vector3 exposedScale = new Vector3(0.6f, 0.6f, 0.6f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmount = 0.08f;

    private bool exposed;
    private Vector3 baseScale;
    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            runtimeMaterial = targetRenderer.material;
        }

        SetExposed(false);
    }

    private void Update()
    {
        if (!exposed) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    public void SetExposed(bool value)
    {
        exposed = value;

        if (exposed)
        {
            baseScale = exposedScale;

            if (runtimeMaterial != null)
                runtimeMaterial.color = exposedColor;

            transform.localScale = exposedScale;
        }
        else
        {
            baseScale = hiddenScale;

            if (runtimeMaterial != null)
                runtimeMaterial.color = hiddenColor;

            transform.localScale = hiddenScale;
        }
    }
}