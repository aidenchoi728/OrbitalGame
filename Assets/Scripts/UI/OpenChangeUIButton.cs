using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenChangeUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image[] outlines;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;

    private bool isOpen;

    public bool IsOpen { get => isOpen; set => isOpen = value; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Image outline in outlines) outline.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isOpen) foreach (Image outline in outlines) outline.color = normalColor;
    }
}
