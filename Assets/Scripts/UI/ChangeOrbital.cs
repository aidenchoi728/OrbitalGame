using System;
using TMPro;
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
    [SerializeField] private OpenChangeUIButton outlineButtonScript;

    [Header("Content")]
    [SerializeField] private GameObject[] azimuthalButtons;
    [SerializeField] private GameObject[] magneticTopButtons;
    [SerializeField] private GameObject magneticBottom;
    
    [Header("Magnetic Text")]
    [SerializeField] private TextMeshProUGUI[] magneticTopTexts;

    [Header("Selection Buttons")] 
    [SerializeField] private SingleSelectionButton principalSelect;
    [SerializeField] private SingleSelectionButton azimuthalSelect;
    [SerializeField] private SingleSelectionButton magneticSelect;

    [Header("Layout")] 
    [SerializeField] private RectTransform rightPanelRect;
    [SerializeField] private RectTransform orbitalInfoRect;
    [SerializeField] private RectTransform changeOrbitalRect;
    [SerializeField] private RectTransform magneticRect;
    [SerializeField] private RectTransform magneticSelectRect;
    
    private ExplorerManager explorerManager;
    
    private int nNum, lNum, mlNum;

    private void Awake()
    {
        explorerManager = GameObject.FindWithTag("GameManager").GetComponent<ExplorerManager>();
        
        topImage.color = outlineColorNormal;
        bottomImage.color = outlineColorNormal;
        gameObject.SetActive(false);
        
        SetNum(1, 0, 0);
    }
    
    public void OpenChangeOrbital()
    {
        GameObject orbitalInfo = GameObject.FindWithTag("Orbital");
        orbitalInfo.SetActive(false);
        Destroy(orbitalInfo);
        gameObject.SetActive(true);
        
        outlineButtonScript.IsOpen = true;
        
        topImage.color = outlineColorChange;
        bottomImage.color = outlineColorChange;
        outlineButton.enabled = false;
        
        RefreshLayoutNow(rightPanelRect);
    }

    public void SetNum(int n, int l, int ml)
    {
        nNum = n - 1;
        lNum = l;
        switch (l)
        {
            case 0:
                mlNum = 0;
                break;
            case 1:
                mlNum = ml + 1;
                break;
            case 2:
                if (ml == -2) mlNum = 3;
                else if (ml == 2) mlNum = 4;
                else mlNum = ml + 1;
                break;
        }
        
        SetN(nNum);
        SetL(lNum);
        SetMl(mlNum);
    }

    public void SetN(int nNum)
    {
        this.nNum = nNum;
        principalSelect.UpdateSelected(nNum);

        switch (nNum)
        {
            case 0:
                azimuthalButtons[1].SetActive(false);
                azimuthalButtons[2].SetActive(false);
                
                SetL(0);
                
                break;
            case 1:
                azimuthalButtons[1].SetActive(true);
                azimuthalButtons[2].SetActive(false);

                if (lNum == 2) SetL(0);
                
                break;
            case 2:
                azimuthalButtons[1].SetActive(true);
                azimuthalButtons[2].SetActive(true);

                break;
        }
    }

    public void SetL(int lNum)
    {
        this.lNum = lNum;
        azimuthalSelect.UpdateSelected(lNum);

        switch (lNum)
        {
            case 0:
                magneticTopButtons[1].SetActive(false);
                magneticTopButtons[2].SetActive(false);
                magneticBottom.SetActive(false);
                
                mlNum = 0;
                magneticSelect.UpdateSelected(0);

                magneticTopTexts[0].text = "0";
                
                break;
            case 1:
                magneticTopButtons[1].SetActive(true);
                magneticTopButtons[2].SetActive(true);
                magneticBottom.SetActive(false);

                if (mlNum > 2)
                {
                    mlNum = 0;
                    magneticSelect.UpdateSelected(0);
                    Debug.Log(mlNum);
                }

                magneticTopTexts[0].text = "-1 [y]";
                magneticTopTexts[1].text = "0 [z]";
                magneticTopTexts[2].text = "1 [x]";
                
                break;
            case 2:
                magneticTopButtons[1].SetActive(true);
                magneticTopButtons[2].SetActive(true);
                magneticBottom.SetActive(true);

                magneticTopTexts[0].text = "-1 [yz]";
                magneticTopTexts[1].text = "0 [z<sup>2</sup>]";
                magneticTopTexts[2].text = "1 [xz]";
                
                break;
        }
        
        RefreshLayoutNow(magneticSelectRect);
        RefreshLayoutNow(magneticRect);
        RefreshLayoutNow(changeOrbitalRect);
        RefreshLayoutNow(orbitalInfoRect);
        RefreshLayoutNow(rightPanelRect);
    }

    public void SetMl(int mlNum)
    {
        this.mlNum = mlNum;
        magneticSelect.UpdateSelected(mlNum);
    }
    
    public void OkButton()
    {
        topImage.color = outlineColorNormal;
        bottomImage.color = outlineColorNormal;
        outlineButton.enabled = true;
        
        gameObject.SetActive(false);
        
        outlineButtonScript.IsOpen = false;

        int n, l, ml = 0;
        
        n = nNum + 1;
        l = lNum;
        switch (l)
        {
            case 0:
                ml = 0;
                break;
            case 1:
                ml = mlNum - 1;
                break;
            case 2:
                if (mlNum == 3) ml = -2;
                else if (mlNum == 4) ml = 2;
                else ml = mlNum - 1;
                break;
        }
        
        explorerManager.ChangeOrbital(n, l, ml);
        
        RefreshLayoutNow(rightPanelRect);
    }
    
    public void RefreshLayoutNow(RectTransform layoutRoot)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }
}
