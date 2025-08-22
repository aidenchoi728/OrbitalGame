using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OverlayChangeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color highlightTextColor;
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private GameObject changeOverlayPrefab;
    
    private OrbitalManager orbitalManager;
    private OrbitalCompareManager compareManager;
    
    private RectTransform rightPanelRect;
    private RectTransform orbitalInfoRect;
    private RectTransform orbitalCenterRect;

    private int n = 1, l = 0, ml = 0;

    private void Awake()
    {
        orbitalManager = FindFirstObjectByType<OrbitalManager>();
        compareManager = FindFirstObjectByType<OrbitalCompareManager>();
        
        rightPanelRect = GameObject.Find("Right Panel").GetComponent<RectTransform>();
        orbitalInfoRect = GameObject.Find("Orbital Info").GetComponent<RectTransform>();
        orbitalCenterRect = GameObject.Find("Center").GetComponent<RectTransform>();
    }

    public void DeleteOrbital()
    {
        compareManager.DeleteOrbital((transform.GetSiblingIndex() - 1)/2);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (TextMeshProUGUI text in texts) text.color = highlightTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (TextMeshProUGUI text in texts) text.color = normalTextColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject go = Instantiate(changeOverlayPrefab, transform.parent);
        int index = transform.GetSiblingIndex();
        go.transform.SetSiblingIndex(index + 1);
        orbitalManager.ActiveOrbitalInfo((index - 1) / 2, go);
        ChangeOverlay changeOverlay = go.GetComponent<ChangeOverlay>();
        changeOverlay.SetNum(n, l, ml);
        
        gameObject.SetActive(false);
        Destroy(gameObject);
        
        RefreshLayoutNow(orbitalCenterRect);
        RefreshLayoutNow(orbitalInfoRect);
        RefreshLayoutNow(rightPanelRect);
    }
    
    public void RefreshLayoutNow(RectTransform layoutRoot)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }

    public void SetQuantumNumbers(int n, int l, int ml)
    {
        this.n = n;
        this.l = l;
        this.ml = ml;
    }
}
