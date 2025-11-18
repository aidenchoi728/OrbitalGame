using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenChangeUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image[] outlines;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color highlightTextColor;

    private ChangeOrbital changeOrbital;
    
    private bool isOpen = false;

    private void Awake()
    {
        changeOrbital = FindAnyObjectByType<ChangeOrbital>();
    }

    public bool IsOpen { get => isOpen; set => isOpen = value; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpen)
        {
            foreach (Image outline in outlines) outline.color = highlightColor;

            GameObject orbitalInfo = GameObject.FindWithTag("Orbital");
            TextMeshProUGUI[] infoText = orbitalInfo.GetComponentsInChildren<TextMeshProUGUI>();
            orbitalInfo.GetComponent<Image>().color = highlightColor;
            foreach (TextMeshProUGUI text in infoText) text.color = highlightTextColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOpen)
        {
            foreach (Image outline in outlines) outline.color = normalColor;
            
            GameObject orbitalInfo = GameObject.FindWithTag("Orbital");
            TextMeshProUGUI[] infoText = orbitalInfo.GetComponentsInChildren<TextMeshProUGUI>();
            orbitalInfo.GetComponent<Image>().color = normalColor;
            foreach (TextMeshProUGUI text in infoText) text.color = normalTextColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOpen) changeOrbital.OpenChangeOrbital();
    }
}
