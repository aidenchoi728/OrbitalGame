using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomInputField : MonoBehaviour, CustomInput, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private string name;
    [SerializeField] private float blinkTime = 0.3f;
    
    [Header("Color")] 
    [SerializeField] private Color fieldNormalColor;
    [SerializeField] private Color fieldHighlightColor;
    [SerializeField] private Color fieldTextColor;
    
    private Image fieldImage;
    private GameObject highlight;
    private Image highlightImage;
    private TextMeshProUGUI selectedText;
    private CheckpointManager checkpointManager;

    private int isSelected;
    private bool correct;

    private float blink = 0f;
    private bool isBlinking;

    private int selected = -1;

    private void Start()
    {
        checkpointManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CheckpointManager>();
        
        selectedText = GetComponentInChildren<TextMeshProUGUI>();
        fieldImage = GetComponent<Image>();
        highlightImage = transform.GetChild(0).GetComponent<Image>();
        highlight = highlightImage.gameObject;
        highlight.SetActive(false);

        selectedText.text = "";
    }

    private void Update()
    {
        if (isSelected == -1) NotSelected();
        if (Input.GetMouseButtonDown(0)) isSelected = -1;

        if (isSelected == 1)
        {
            blink -= Time.deltaTime;
            if (blink <= 0f)
            {
                blink = blinkTime;
                isBlinking = !isBlinking;
                if (selected == -1)
                {
                    if (isBlinking) selectedText.text = "|";
                    else selectedText.text = "";
                }
                else
                {
                    if (isBlinking)
                    {
                        highlight.SetActive(true);
                        selectedText.color = fieldNormalColor;
                    }
                    else
                    {
                        highlight.SetActive(false);
                        selectedText.color = fieldTextColor;
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha0)) ChangeSelected(0);
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelected(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelected(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelected(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelected(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeSelected(5);
            if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeSelected(6);
            if (Input.GetKeyDown(KeyCode.Alpha7)) ChangeSelected(7);
            if (Input.GetKeyDown(KeyCode.Alpha8)) ChangeSelected(8);
            if (Input.GetKeyDown(KeyCode.Alpha9)) ChangeSelected(9);
            if (Input.GetKeyDown(KeyCode.Backspace)) ChangeSelected(-1);
        }
    }

    private void NotSelected()
    {
        isSelected = 0;
        if (selected == -1) selectedText.text = "";
        else
        {
            selectedText.color = fieldTextColor;
            highlight.SetActive(false);
        }
    }

    public void SetColors(Color fieldNormalColor, Color fieldHighlightColor, Color fieldTextColor,
        Color itemNormalColor,
        Color itemHighlightColor, Color itemHoverColor, Color itemTextNormalColor, Color itemTextHighlightColor)
    {
        this.fieldNormalColor = fieldNormalColor;
        this.fieldHighlightColor = fieldHighlightColor;
        this.fieldTextColor = fieldTextColor;
        
        fieldImage.color = fieldNormalColor;
        highlightImage.color = fieldTextColor;
        selectedText.color = fieldTextColor;
    }

    public void ChangeSelected(int num)
    {
        selected = num;
        
        if (num == -1)
        {
            highlight.SetActive(false);
            if (isBlinking) selectedText.text = "|";
            else selectedText.text = "";
            return;
        }
        
        selectedText.text = num.ToString();
        checkpointManager.ChangeAnswer(name, selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (correct || isSelected == 1) return;
        fieldImage.color = fieldHighlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (correct || isSelected == 1) return;
        fieldImage.color = fieldNormalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (correct|| isSelected == 1) return;
        
        isSelected = 1;
        fieldImage.color = fieldNormalColor;
        
        if (selected == -1) selectedText.text = "|";
        else
        {
            highlight.SetActive(true);
            selectedText.color = fieldNormalColor;
        }
        blink = blinkTime;
        isBlinking = true;
    }

    public bool Correct
    {
        set
        {
            NotSelected();
            correct = value;
        }
    }
}