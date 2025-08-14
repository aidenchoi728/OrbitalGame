using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeOrbital : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] private Image topImage;
    [SerializeField] private Image bottomImage;
    [SerializeField] private Color outlineColorNormal;
    [SerializeField] private Color outlineColorChange;
    [SerializeField] private Button outlineButton;
    [SerializeField] RectTransform layoutRoot;
    
    private int nNum, lNum, mlNum;

    private void Awake()
    {
        topImage.color = outlineColorNormal;
        bottomImage.color = outlineColorNormal;
        gameObject.SetActive(false);
    }

    private void Start()
    {
        topImage.color = outlineColorChange;
        bottomImage.color = outlineColorChange;
        outlineButton.enabled = false;
        
        RefreshLayoutNow();
    }

    public void SetNum(int n, int l, int ml) //TODO
    {
        this.nNum = nNum;
        this.lNum = lNum;
        this.mlNum = mlNum;
    }

    public void SetN(int nNum)
    {
        this.nNum = nNum;
    }

    public void SetL(int lNum)
    {
        this.lNum = lNum;
    }

    public void SetMl(int mlNum)
    {
        this.mlNum = mlNum;
    }
    
    public void OkButton()
    {
        topImage.color = outlineColorNormal;
        bottomImage.color = outlineColorNormal;
        outlineButton.enabled = true;
        
        gameObject.SetActive(false);
        RefreshLayoutNow();
    }
    
    public void RefreshLayoutNow()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }
}
