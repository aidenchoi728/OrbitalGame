using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransitionPanelUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image[] outlines;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;

    private void Awake()
    {
        foreach (Image outline in outlines) outline.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Image outline in outlines) outline.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Image outline in outlines) outline.color = normalColor;
    }
}
