using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private float dimAmount = 0.85f;
    [SerializeField] private RectTransform textRect;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject previousButton;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private RectTransform[] layouts;
    [SerializeField] private float padding = 10f;
    [SerializeField] private Canvas mainCanvas;

    [Header("Content")] 
    [SerializeField] private string[] textContents;
    [SerializeField] private RectTransform[] spotlightRects;
    [SerializeField] private Rect[] manualSpotlightRects;
    [SerializeField] private bool[] freeze;
    
    private ScreenSpotlightDimController spotlightController;
    private SceneViewCamera cam;
    private GameObject controls;

    private int curr = 0;
    private int close = -1;

    private void Awake()
    {
        spotlightController = GetComponent<ScreenSpotlightDimController>();
        cam = Camera.main.GetComponent<SceneViewCamera>();
        controls = GameObject.FindWithTag("Controls");
        controls.SetActive(false);
        
        StartTutorial();
    }

    private void Update()
    {
        if (close == 0) close++;
        else if (close == 1)
        {
            gameObject.SetActive(false);
            close = -1;
        }
    }

    public void StartTutorial()
    {
        gameObject.SetActive(true);
        spotlightController.dimAmount = dimAmount;
        LoadNext();
    }

    public void LoadNext()
    {
        if(curr == 0) previousButton.SetActive(false);
        else if (curr >= textContents.Length)
        {
            EndTutorial();
            return;
        }
        else previousButton.SetActive(true);
        
        text.text = textContents[curr];
        
        if (spotlightRects[curr] == null)
        {
            spotlightController.targetRectTransform = null;
            spotlightController.manualScreenRectPixels = manualSpotlightRects[curr];
            PositionText(manualSpotlightRects[curr]);
        }
        else
        {
            spotlightController.targetRectTransform = spotlightRects[curr];
            spotlightController.manualScreenRectPixels = new Rect();
            PositionText(GetScreenRect(spotlightRects[curr]));
        }
        cam.Freeze = freeze[curr];
        
        curr++;

        if (curr >= textContents.Length)
        {
            skipButton.SetActive(false);
            nextText.text = "Close";
        }
        
        RefreshLayoutNow();
    }

    public void LoadPrevious()
    {
        skipButton.SetActive(true);
        nextText.text = "Skip";
        curr -= 2;
        LoadNext();
    }

    public void EndTutorial()
    {
        cam.Freeze = false;
        spotlightController.dimAmount = 0f;
        close = 0;
        controls.SetActive(true);
        gameObject.SetActive(true);
    }

    private void PositionText(Rect spotlightRect)
    {
        if (textRect == null) return;

        // Find the parent canvas + camera for proper screen->world conversion
        var canvas = textRect.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRect = canvas.GetComponent<RectTransform>();
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);

        // Decide side by available horizontal space (>= means tie → right)
        float spaceLeft  = spotlightRect.xMin;
        float spaceRight = Screen.width - spotlightRect.xMax;
        bool placeRight  = spaceRight >= spaceLeft;

        // Choose pivot & target screen point
        Vector2 pivot, screenPoint;
        if (placeRight)
        {
            // Align text's TOP-LEFT corner to (spotlight top-right + padding to the right)
            pivot = new Vector2(0f, 1f);
            screenPoint = new Vector2(spotlightRect.xMax + padding, spotlightRect.yMax);
        }
        else
        {
            // Align text's TOP-RIGHT corner to (spotlight top-left - padding to the left)
            pivot = new Vector2(1f, 1f);
            screenPoint = new Vector2(spotlightRect.xMin - padding, spotlightRect.yMax);
        }

        // Set pivot so the chosen corner snaps to the target point
        textRect.pivot = pivot;

        // Convert the desired screen point to the canvas plane and place the rect there
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPoint, cam, out var worldPos))
        {
            textRect.position = worldPos;
        }
    }
    
    private void RefreshLayoutNow()
    {
        foreach (RectTransform layout in layouts)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            Canvas.ForceUpdateCanvases();
        }
    }
    
    public Rect GetScreenRect(RectTransform rt)
    {
        Camera cam = (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : (mainCanvas.worldCamera ? mainCanvas.worldCamera : Camera.main);

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
            corners[i] = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);

        float xMin = corners.Min(c => c.x);
        float xMax = corners.Max(c => c.x);
        float yMin = corners.Min(c => c.y);
        float yMax = corners.Max(c => c.y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}
