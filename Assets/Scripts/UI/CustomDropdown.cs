using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomDropdown : MonoBehaviour, CustomInput, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private string name;
    
    [Header("Items")] 
    [SerializeField] private string[] itemLabels;
    [SerializeField] private GameObject itemPrefab;

    [Header("Components")] 
    [SerializeField] private GameObject selectedGo;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject itemParent;
    private Image fieldImage;
    private TextMeshProUGUI selectedText;
    private Image[] itemImages;
    private TextMeshProUGUI[] itemTexts;
    private CustomDropdownItem[] items;
    private RectTransform rect;
    
    [Header("Color")] 
    [SerializeField] private Color fieldNormalColor;
    [SerializeField] private Color fieldHighlightColor;
    [SerializeField] private Color fieldTextColor;
    [SerializeField] private Color itemNormalColor;
    [SerializeField] private Color itemHighlightColor;
    [SerializeField] private Color itemTextNormalColor;
    [SerializeField] private Color itemTextHighlightColor;

    private CheckpointManager checkpointManager;
    private Camera cam;
    private GraphicRaycaster raycaster;
    
    private bool isOpen = false;
    private bool overDropdown = false;
    private bool isSelected = false;
    private int selected;
    private bool isHovering = false;

    private bool correct = false;

    public bool Correct
    {
        set => correct = value;
    }

    public void SetColors(Color fieldNormalColor, Color fieldHighlightColor, Color fieldTextColor, Color itemNormalColor,
        Color itemHighlightColor, Color itemHoverColor, Color itemTextNormalColor, Color itemTextHighlightColor)
    {
        this.fieldNormalColor = fieldNormalColor;
        this.fieldHighlightColor = fieldHighlightColor;
        this.fieldTextColor = fieldTextColor;
        this.itemNormalColor = itemNormalColor;
        this.itemHighlightColor = itemHighlightColor;
        this.itemTextNormalColor = itemTextNormalColor;
        this.itemTextHighlightColor = itemTextHighlightColor;
        
        foreach (CustomDropdownItem item in items) item.SetColors(itemNormalColor, itemHoverColor, itemHighlightColor, itemTextNormalColor, itemTextHighlightColor);

        fieldImage.color = fieldNormalColor;
        selectedText.color = fieldTextColor;
        arrow.GetComponent<Image>().color = fieldTextColor;
    }
    
    private void Start()
    {
        checkpointManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CheckpointManager>();
        cam = Camera.main;
        raycaster = GetComponentInParent<GraphicRaycaster>();
        
        rect = GetComponent<RectTransform>();
        selectedText = GetComponentInChildren<TextMeshProUGUI>();
        itemImages = new Image[itemLabels.Length];
        items = new CustomDropdownItem[itemLabels.Length];
        itemTexts = new TextMeshProUGUI[itemLabels.Length];
        fieldImage = GetComponent<Image>();
        
        for(int i = 0; i < itemLabels.Length; i++)
        {
            GameObject itemGo = Instantiate(itemPrefab, itemParent.transform);
            
            itemGo.GetComponentInChildren<TextMeshProUGUI>().text = itemLabels[i];
            CustomDropdownItem dropdownItem = itemGo.GetComponent<CustomDropdownItem>();
            dropdownItem.Dropdown = this;
            dropdownItem.Num = i;
            
            itemImages[i] = itemGo.GetComponent<Image>();
            itemTexts[i] = itemGo.GetComponentInChildren<TextMeshProUGUI>();
            items[i] = dropdownItem;
        }
        
        selectedGo.SetActive(false);
        arrow.SetActive(true);
        itemParent.SetActive(false);
    }

    void Update()
    {
        if(correct) return;
        
        Vector2 mousePos = Input.mousePosition;
        bool hoverNow = false;
        
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, cam))
        {
            // Step 2: do a raycast and check the topmost result
            var ped = new PointerEventData(EventSystem.current) { position = mousePos };
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(ped, results);

            if (results.Count > 0 && (results[0].gameObject == gameObject || results[1].gameObject == gameObject))
                hoverNow = true;
        }

        // Fire enter/exit
        if (!isHovering && hoverNow)
        {
            isHovering = true;
            fieldImage.color = fieldHighlightColor;
        }
        else if (isHovering && !hoverNow)
        {
            isHovering = false;
            fieldImage.color = fieldNormalColor;
        }

        if (!overDropdown && isOpen && Input.GetMouseButtonDown(0))
        {
            isOpen = false;
            
            fieldImage.color = fieldNormalColor;
            arrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -90);
            itemParent.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        overDropdown = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        overDropdown = false;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (correct) return;
        
        isOpen = !isOpen;
        
        if (isOpen)
        {
            fieldImage.color = fieldNormalColor;
            itemParent.SetActive(true);
            arrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            if(isSelected) fieldImage.color = fieldHighlightColor;
            else
            {
                fieldImage.color = fieldNormalColor;
                arrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -90);
            }
            itemParent.SetActive(false);
        }
    }

    public void ChangeSelected(int num)
    {
        items[selected].Selected = false;
        itemImages[selected].color = itemNormalColor;
        itemTexts[selected].color = itemTextNormalColor;
        
        selected = num;
        
        items[selected].Selected = true;

        itemImages[selected].color = itemHighlightColor;
        itemTexts[selected].color = itemTextHighlightColor;
        
        if (!isSelected)
        {
            isSelected = true;
            arrow.SetActive(false);
            selectedGo.SetActive(true);
        }

        selectedText.text = itemLabels[selected];
        
        checkpointManager.ChangeAnswer(name, selected);
    }
}
