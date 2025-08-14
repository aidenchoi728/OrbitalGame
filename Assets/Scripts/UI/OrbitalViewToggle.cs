using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrbitalViewToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Sprite spriteOff;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Color offColor;
    [SerializeField] private Color onColor;
    [SerializeField] private Color hoverColor;
    
    private Image image;
    private TextMeshProUGUI text;
    
    private bool isOn;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        image.sprite = spriteOff;
        text.color = offColor;
        
        isOn = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = spriteOn;
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOn)
        {
            image.sprite = spriteOff;
            text.color = offColor;
        }
        else
        {
            image.sprite = spriteOn;
            text.color = onColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
    }
}
