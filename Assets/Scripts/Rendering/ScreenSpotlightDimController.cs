using System.Linq;
using UnityEngine;
using UnityEngine.UI; // Canvas, CanvasScaler

public class ScreenSpotlightDimController : MonoBehaviour
{
    [Header("Target (pick one)")]
    public RectTransform targetRectTransform;    // auto-compute from a UI element
    private Camera uiCameraForWorldToScreen;     // fallback; will auto-pick from Canvas when available
    public Rect manualScreenRectPixels;          // if using manual rect (x,y = left,bottom)

    [Tooltip("If ON, manualScreenRectPixels is authored in the Canvas Scaler's reference resolution (e.g., 1920x1080) and will be scaled to actual screen pixels.")]
    [SerializeField] private bool manualRectIsInReferenceResolution = true;

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

        // Safe first update after UI render (good for WebGL); keep if you use WebGLFrameSync
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
        if (!spotlightMaterial && ScreenSpotlightDimFeature.ActiveMaterial != null)
        {
            spotlightMaterial = ScreenSpotlightDimFeature.ActiveMaterial;
            return;
        }

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

        // Determine the camera based on the target's Canvas.
        Camera cam = uiCameraForWorldToScreen;
        Canvas canvasForScale = null;

        if (targetRectTransform)
        {
            canvasForScale = targetRectTransform.GetComponentInParent<Canvas>();
            if (canvasForScale)
            {
                cam = canvasForScale.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : (canvasForScale.worldCamera ? canvasForScale.worldCamera : uiCameraForWorldToScreen);
            }
        }
        else
        {
            canvasForScale = GetComponentInParent<Canvas>(); // use our own canvas if any
        }

        // Choose source rect
        Rect rectPx = targetRectTransform
            ? GetScreenRect(targetRectTransform, cam)
            : GetManualRectInScreenPixels(manualScreenRectPixels, canvasForScale);

        // Apply padding
        rectPx.xMin -= paddingPixels;
        rectPx.yMin -= paddingPixels;
        rectPx.xMax += paddingPixels;
        rectPx.yMax += paddingPixels;

        // Convert to normalized 0..1 UV
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

    // ---- Scaling helpers ----

    /// <summary>
    /// Converts a manual rect to **screen pixels** using the parent CanvasScaler
    /// when manualRectIsInReferenceResolution is true. If no scaler is found,
    /// returns the input as-is.
    /// </summary>
    Rect GetManualRectInScreenPixels(Rect manualRefRect, Canvas canvasForScale)
    {
        if (!manualRectIsInReferenceResolution) return manualRefRect;

        CanvasScaler scaler = canvasForScale ? canvasForScale.GetComponent<CanvasScaler>() : null;
        if (!scaler || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            return manualRefRect;

        Vector2 refRes = scaler.referenceResolution;
        float match = scaler.matchWidthOrHeight; // 0 = width, 1 = height

        // Unity's Scale With Screen Size formula (same as CanvasScaler)
        float logW   = Mathf.Log(Screen.width  / refRes.x, 2f);
        float logH   = Mathf.Log(Screen.height / refRes.y, 2f);
        float logAvg = Mathf.Lerp(logW, logH, match);
        float scale  = Mathf.Pow(2f, logAvg);

        // Size of the reference canvas once scaled to this screen
        float scaledW = refRes.x * scale;
        float scaledH = refRes.y * scale;

        // ✅ Center the scaled reference area on the screen
        float offsetX = (Screen.width  - scaledW) * 0.5f;
        float offsetY = (Screen.height - scaledH) * 0.5f; // can be negative when content overflows

        // Convert reference-rect (bottom-left origin) -> actual screen pixels
        return new Rect(
            manualRefRect.x      * scale + offsetX,
            manualRefRect.y      * scale + offsetY,
            manualRefRect.width  * scale,
            manualRefRect.height * scale
        );
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
