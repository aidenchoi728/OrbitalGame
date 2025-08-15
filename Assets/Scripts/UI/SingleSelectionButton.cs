using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SingleSelectionButton : MonoBehaviour
{
    [SerializeField] private bool isSprite = true;
    
    [SerializeField] private Sprite[] buttonSpritesNormal;
    [SerializeField] private Sprite[] buttonSpritesSelected;
    [SerializeField] private Color buttonColorNormal;
    [SerializeField] private Color buttonColorSelected;
    [SerializeField] private Color textColorNormal;
    [SerializeField] private Color textColorSelected;

    [Serializable] public class IntEvent : UnityEvent<int> {}
    [SerializeField] private IntEvent onClick;
    
    private Image[] buttonImages;
    private TextMeshProUGUI[] buttonTexts;

    private void Awake()
    {
        buttonImages = GetComponentsInChildren<Image>();
        if(!isSprite) buttonTexts = GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void ChangeSelected(int num)
    {
        if (isSprite)
        {
            for (int i = 0; i < buttonImages.Length; i++)
            {
                if(i == num) buttonImages[i].sprite = buttonSpritesSelected[i];
                else buttonImages[i].sprite = buttonSpritesNormal[i];
            }
        }
        
        onClick?.Invoke(num);
    }

    public void UpdateSelected(int num)
    {
        if (isSprite) return;
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == num)
            {
                buttonImages[i].color = buttonColorSelected;
                buttonTexts[i].color = textColorSelected;
            }
            else
            {
                buttonImages[i].color = buttonColorNormal;
                buttonTexts[i].color = textColorNormal;
            }
        }
    }
}
