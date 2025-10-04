using System.Linq;
using UnityEngine;
using UnityEngine.UI; // for Canvas & layout helpers

public class ScreenSpotlightDimController : MonoBehaviour
{
    [Header("Target (pick one)")]
    public RectTransform targetRectTransform;    // auto-compute from a UI element
    private Camera uiCameraForWorldToScreen;     // fallback; will auto-pick from Canvas when available
    public Rect manualScreenRectPixels;          // or provide screen-space rect yourself (pixels, bottom-left origin)

    [Header("Spotlight Settings")]
    [Range(0f, 1f)] public float dimAmount = 0.7f; // how dark the outside is
    [SerializeField] private float paddingPixels = 16f;  // expands the rect on all sides
    [SerializeField] private float featherPixels = 8f;   // soft edge width

    [Header("Material (must be the SAME one used by the Renderer Feature)")]
    [SerializeField] private Material spotlightMaterial;

    [Header("Update Mode")]
    [Tooltip("If true, updates every frame (keeps up with moving UI). If false, only updates on enable/RefreshNow().")]
    [SerializeField] private bool continuousUpdate = true;

    void Awake()
    {
        uiCameraForWorldToScreen = Camera.main;
    }

    void OnEnable()
    {
        TryWireMaterial();

        // ✅ First, do a WebGL-safe update AFTER the next real UI render/layout
        if (targetRectTransform)
        {
            WebGLFrameSync.AfterLayoutReady(this, targetRectTransform, UpdateSpotlight);
        }
        else
        {
            WebGLFrameSync.AfterNextRenderedFrame(this, UpdateSpotlight);
        }
    }

    void Reset()      { TryWireMaterial(); }
    void OnValidate() { if (!spotlightMaterial) TryWireMaterial(); }

    void TryWireMaterial()
    {
        // 1) Preferred: material exposed by the feature
        if (!spotlightMaterial && ScreenSpotlightDimFeature.ActiveMaterial != null)
        {
            spotlightMaterial = ScreenSpotlightDimFeature.ActiveMaterial;
            return;
        }

        // 2) Fallback: find any loaded feature asset and grab its material
        if (!spotlightMaterial)
        {
            var feat = Resources.FindObjectsOfTypeAll<ScreenSpotlightDimFeature>()
                .FirstOrDefault(f => f != null && f.settings != null && f.settings.material != null);
            if (feat != null) spotlightMaterial = feat.settings.material;
        }
    }

    void LateUpdate()
    {
        if (continuousUpdate) UpdateSpotlight();
    }

    /// <summary>For one-off recalculation (e.g., after content/size changes).</summary>
    public void RefreshNow(bool forceLayout = false)
    {
        if (forceLayout && targetRectTransform)
            WebGLFrameSync.AfterLayoutReady(this, targetRectTransform, UpdateSpotlight);
        else
            WebGLFrameSync.AfterNextRenderedFrame(this, UpdateSpotlight);
    }

    // ---- Core update ----
    void UpdateSpotlight()
    {
        if (!spotlightMaterial) return;

        // Determine the camera to use for screen conversion, based on the target's Canvas.
        Camera cam = uiCameraForWorldToScreen;
        if (targetRectTransform)
        {
            var canvas = targetRectTransform.GetComponentInParent<Canvas>();
            if (canvas)
            {
                cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : (canvas.worldCamera ? canvas.worldCamera : uiCameraForWorldToScreen);
            }
        }

        // Choose source rect
        Rect rectPx = targetRectTransform
            ? GetScreenRect(targetRectTransform, cam)
            : manualScreenRectPixels;

        // Apply padding
        rectPx.xMin -= paddingPixels;
        rectPx.yMin -= paddingPixels;
        rectPx.xMax += paddingPixels;
        rectPx.yMax += paddingPixels;

        // Normalize to 0..1 UV space
        float sw = Mathf.Max(1, (float)Screen.width);
        float sh = Mathf.Max(1, (float)Screen.height);
        Vector4 spot = new Vector4(
            (rectPx.xMin + rectPx.xMax) * 0.5f / sw,
            (rectPx.yMin + rectPx.yMax) * 0.5f / sh,
            Mathf.Abs(rectPx.width) * 0.5f / sw,
            Mathf.Abs(rectPx.height) * 0.5f / sh
        );

        float featherNorm = featherPixels / Mathf.Min(sw, sh);

        // Feed material
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

        float xMin = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float xMax = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float yMin = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float yMax = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // If you're supplying it yourself:
    public void SetManualScreenRect(float xMin, float yMin, float xMax, float yMax)
        => manualScreenRectPixels = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
}
