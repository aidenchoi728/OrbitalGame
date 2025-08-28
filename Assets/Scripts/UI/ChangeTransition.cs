using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeTransition : MonoBehaviour
{
    [Header("Transition Info")]
    [SerializeField] private OpenChangeTransButton[] transitionButtons;
    
    [Header("Visuals")] 
    [SerializeField] private Image[] outlines;
    [SerializeField] private TextMeshProUGUI azimuthalText;
    [SerializeField] private TextMeshProUGUI magneticText;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color errorColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color errorTextColor;
    [SerializeField] private Color buttonColorNormal;
    [SerializeField] private Color buttonColorSelected;
    [SerializeField] private Color buttonTextNormal;
    [SerializeField] private Color buttonTextSelected;
    [SerializeField] private Color buttonColorNormalError;
    [SerializeField] private Color buttonColorSelectedError;
    [SerializeField] private Color buttonTextNormalError;
    [SerializeField] private Color buttonTextSelectedError;

    [Header("Content")]
    [SerializeField] private GameObject[] azimuthalButtons;
    [SerializeField] private GameObject azimuthalError;
    [SerializeField] private GameObject[] magneticTopButtons;
    [SerializeField] private GameObject magneticBottom;
    [SerializeField] private GameObject magneticError;
    
    [Header("Magnetic Text")]
    [SerializeField] private TextMeshProUGUI[] magneticTopTexts;

    [Header("Selection Buttons")] 
    [SerializeField] private SingleSelectionButton principalSelect;
    [SerializeField] private SingleSelectionButton azimuthalSelect;
    [SerializeField] private SingleSelectionButton magneticSelect;

    [Header("Layout")] 
    [SerializeField] private RectTransform orbitalInfoRect;
    [SerializeField] private RectTransform changeOrbitalRect;
    [SerializeField] private RectTransform azimuthalRect;
    [SerializeField] private RectTransform magneticRect;
    [SerializeField] private RectTransform magneticSelectRect;
    
    private OrbitalTransitionManager transitionManager;
    
    private int nNum, lNum, mlNum;
    private int nBefore, lBefore, mlBefore;
    private int nAfter, lAfter, mlAfter;
    private bool isBefore;

    private bool error;

    public bool IsBefore
    {
        get => isBefore;
        set => isBefore = value;
    }

    public void SetQuantumNumber(int nBefore, int nAfter, int lBefore, int lAfter, int mlBefore, int mlAfter)
    {
        this.nBefore = nBefore;
        this.nAfter = nAfter;
        this.lBefore = lBefore;
        this.lAfter = lAfter;
        this.mlBefore = mlBefore;
        this.mlAfter = mlAfter;
    }

    private void Awake()
    {
        transitionManager = GameObject.FindWithTag("GameManager").GetComponent<OrbitalTransitionManager>();
        
        gameObject.SetActive(false);

        nBefore = 1;
        lBefore = 0;
        mlBefore = 0;
        nAfter = 2;
        lAfter = 1;
        mlAfter = 0;
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
        
        CheckValidity();
    }

    public void SetMl(int mlNum)
    {
        this.mlNum = mlNum;
        magneticSelect.UpdateSelected(mlNum);
        
        CheckValidity();
    }
    
    public void OkButton()
    {
        if (CheckValidity())
        {
            gameObject.SetActive(false);
            
            if (isBefore)
            {
                transitionManager.ChangeTransition(nBefore, lBefore, mlBefore, true);
                        
                if (isBefore) transitionButtons[0].SetQuantumNumber(nBefore, lBefore, mlBefore);
                else transitionButtons[1].SetQuantumNumber(nBefore, lBefore, mlBefore);
            }
            else
            {
                transitionManager.ChangeTransition(nAfter, lAfter, mlAfter, false);
                        
                if (isBefore) transitionButtons[0].SetQuantumNumber(nAfter, lAfter, mlAfter);
                else transitionButtons[1].SetQuantumNumber(nAfter, lAfter, mlAfter);
            }
        }
    }

    private void UpdateQuantumNumbers()
    {
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

        if (isBefore)
        {
            nBefore = n;
            lBefore = l;
            mlBefore = ml;
        }
        else
        {
            nAfter = n;
            lAfter = l;
            mlAfter = ml;
        }
    }

    private bool CheckValidity()
    {
        UpdateQuantumNumbers();

        bool valid = true;
        
        int deltaL = lAfter - lBefore;
        int deltaMl = mlAfter - mlBefore;
        
        if (!(deltaL == 1 || deltaL == -1))
        {
            valid = false;
            azimuthalText.color = errorTextColor;
            azimuthalError.SetActive(true);
            azimuthalSelect.ChangeColor(buttonColorSelectedError, buttonColorNormalError, buttonTextSelectedError, buttonTextNormalError);
        }
        else
        {
            azimuthalText.color = normalTextColor;
            azimuthalError.SetActive(false);
            azimuthalSelect.ChangeColor(buttonColorSelected, buttonColorNormal, buttonTextSelected, buttonTextNormal);
        }

        if (deltaMl > 1 || deltaMl < -1)
        {
            valid = false;
            magneticText.color = errorTextColor;
            magneticError.SetActive(true);
            magneticSelect.ChangeColor(buttonColorSelectedError, buttonColorNormalError, buttonTextSelectedError, buttonTextNormalError);
        }
        else
        {
            magneticText.color = normalTextColor;
            magneticError.SetActive(false);
            magneticSelect.ChangeColor(buttonColorSelected, buttonColorNormal, buttonTextSelected, buttonTextNormal);
        }

        if (!valid)
        {
            foreach (Image outline in outlines) outline.color = errorColor;
        }
        else
        {
            foreach (Image outline in outlines) outline.color = normalColor;
        }
        
        RefreshLayoutNow(azimuthalRect);
        RefreshLayoutNow(magneticRect);
        RefreshLayoutNow(changeOrbitalRect);
        RefreshLayoutNow(orbitalInfoRect);

        error = !valid;
        return valid;
    }
    
    public void RefreshLayoutNow(RectTransform layoutRoot)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }
}
