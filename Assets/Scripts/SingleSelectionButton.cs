using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SingleSelectionButton : MonoBehaviour
{
    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Sprite[] buttonSpritesNormal;
    [SerializeField] private Sprite[] buttonSpritesSelected;

    [Serializable] public class IntEvent : UnityEvent<int> {}
    [SerializeField] private IntEvent onClick;

    public void ChangeSelected(int num)
    {
        for (int i = 0; i < buttonImages.Length; i++)
        {
            if(i == num) buttonImages[i].sprite = buttonSpritesSelected[i];
            else buttonImages[i].sprite = buttonSpritesNormal[i];
        }
        
        onClick?.Invoke(num);
    }
}
