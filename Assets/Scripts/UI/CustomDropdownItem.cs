using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomDropdownItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color highlightTextColor;
    [SerializeField] private float blink = 0.2f;
    private Image image;
    private TextMeshProUGUI text;
    private bool selected = false;
    private bool hover = false;
    private CustomDropdown dropdown;
    private int num;
    private float blinkCount = -1;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (hover)
        {
            if (blinkCount > 0)
            {
                blinkCount -= Time.deltaTime;
                if(blinkCount <= 0 && hover) image.color = highlightColor;
            }
        }
        else blinkCount = -1;
    }

    public bool Selected
    {
        set => selected = value;
    }

    public CustomDropdown Dropdown
    {
        set => dropdown = value;
    }

    public int Num
    {
        set => num = value;
    }

    public void SetColors(Color normalColor, Color highlightColor, Color selectedColor, Color normalTextColor,
        Color highlightTextColor)
    {
        this.normalColor = normalColor;
        this.highlightColor = highlightColor;
        this.selectedColor = selectedColor;
        this.normalTextColor = normalTextColor;
        this.highlightTextColor = highlightTextColor;

        if (selected)
        {
            image.color = selectedColor;
            text.color = highlightTextColor;
        }
        else
        {
            image.color = normalColor;
            text.color = normalTextColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover = true;
        image.color = highlightColor;
        text.color = highlightTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover = false;
        if (!selected)
        {
            image.color = normalColor;
            text.color = normalTextColor;
        }
        else
        {
            image.color = selectedColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected = true;
        dropdown.ChangeSelected(num);
        blinkCount = blink;
    }
}
