using System;
using System.Linq;
using UnityEngine;

public class ScreenSpotlightDimController : MonoBehaviour
{
    [Header("Target (pick one)")]
    public RectTransform targetRectTransform;    // if you want this to auto-compute from a UI element
    private Camera uiCameraForWorldToScreen;      // set if Canvas is Screen Space - Camera or World Space
    public Rect manualScreenRectPixels;          // or set this yourself in pixels (x,y = left,bottom)

    [Header("Spotlight Settings")]
    [Range(0f, 1f)] public float dimAmount = 0.7f; // how dark the outside is
    [SerializeField] private float paddingPixels = 16f;               // expands the rect on all sides
    [SerializeField] private float featherPixels = 8f;                // soft edge width

    [Header("Material (must be the SAME one used by the Renderer Feature)")]
    [SerializeField] private Material spotlightMaterial;

    private void Awake()
    {
        uiCameraForWorldToScreen = Camera.main;
    }

    void Reset()      { TryWireMaterial(); }
    void OnValidate() { if (!spotlightMaterial) TryWireMaterial(); }

    void TryWireMaterial()
    {
        // 1) Preferred: the feature exposes what it's using this frame
        if (!spotlightMaterial && ScreenSpotlightDimFeature.ActiveMaterial != null)
        {
            spotlightMaterial = ScreenSpotlightDimFeature.ActiveMaterial;
            return;
        }

        // 2) Fallback: find any loaded feature asset and grab its material
        if (!spotlightMaterial)
        {
            var feat = Resources.FindObjectsOfTypeAll<ScreenSpotlightDimFeature>()
                .FirstOrDefault(f =>
                    f != null &&
                    f.settings != null &&
                    f.settings.material != null);

            if (feat != null)
                spotlightMaterial = feat.settings.material;
        }
    }

    void LateUpdate()
    {
        if (!spotlightMaterial) return;

        // Choose source of the rect
        Rect rectPx = targetRectTransform
            ? GetScreenRect(targetRectTransform, uiCameraForWorldToScreen)
            : manualScreenRectPixels;

        // Padding
        rectPx.xMin -= paddingPixels;
        rectPx.yMin -= paddingPixels;
        rectPx.xMax += paddingPixels;
        rectPx.yMax += paddingPixels;

        // Convert to normalized UV
        float sw = Mathf.Max(1, (float)Screen.width);
        float sh = Mathf.Max(1, (float)Screen.height);
        Vector4 spot = new Vector4(
            (rectPx.xMin + rectPx.xMax) * 0.5f / sw,
            (rectPx.yMin + rectPx.yMax) * 0.5f / sh,
            Mathf.Abs(rectPx.width) * 0.5f / sw,
            Mathf.Abs(rectPx.height) * 0.5f / sh
        );

        float featherNorm = featherPixels / Mathf.Min(sw, sh);

        // Feed the shared material instance
        spotlightMaterial.SetVector("_SpotRect", spot);
        spotlightMaterial.SetFloat("_SpotFeather", featherNorm);
        spotlightMaterial.SetFloat("_DimAmount", dimAmount);
    }

    // Helper: screen rect (pixels) for a RectTransform
    public static Rect GetScreenRect(RectTransform rt, Camera uiCam = null)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++)
            corners[i] = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);

        float xMin = corners.Min(c => c.x);
        float xMax = corners.Max(c => c.x);
        float yMin = corners.Min(c => c.y);
        float yMax = corners.Max(c => c.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // If you're supplying it yourself:
    public void SetManualScreenRect(float xMin, float yMin, float xMax, float yMax)
        => manualScreenRectPixels = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
}
