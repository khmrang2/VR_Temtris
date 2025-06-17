using UnityEngine;

public class FillBar : MonoBehaviour
{
    [SerializeField] private Color startColor = Color.black;
    [SerializeField] private Color endColor = Color.white;

    [SerializeField] private LineTrigger lineTrigger;
    [SerializeField] private Transform fillBar;
    [SerializeField] private Renderer fillBarRenderer;

    [SerializeField] private float maxHeight = 1.0f;
    private Vector3 initialScale;
    private Vector3 initialPosition;

    private void Start()
    {
        if (fillBar == null)
            fillBar = transform;

        if (fillBarRenderer == null)
            fillBarRenderer = fillBar.GetComponent<Renderer>();

        initialScale = fillBar.localScale;
        initialPosition = fillBar.localPosition;
    }

    private void OnEnable()
    {
        if (lineTrigger != null)
            lineTrigger.OnFillRatioChanged += UpdateFillBar;
    }

    private void OnDisable()
    {
        if (lineTrigger != null)
            lineTrigger.OnFillRatioChanged -= UpdateFillBar;
    }

    private void UpdateFillBar(float ratio)
    {
        float newHeight = maxHeight * Mathf.Clamp01(ratio);
        Vector3 newScale = new Vector3(initialScale.x, newHeight, initialScale.z);
        fillBar.localScale = newScale;

        float yOffset = (newHeight - initialScale.y) * 0.5f;
        fillBar.localPosition = initialPosition + new Vector3(0, yOffset, 0);

        if (fillBarRenderer != null && fillBarRenderer.material.HasProperty("_Color"))
        {
            Color newColor = Color.Lerp(Color.black, Color.white, ratio);
            fillBarRenderer.material.color = newColor;
        }
    }
}
