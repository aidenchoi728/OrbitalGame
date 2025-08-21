using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerManager2D : MonoBehaviour, ExplorerManager
{
    [SerializeField] private OrbitalManager orbitalManager;
    
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI radialNodeText;
    [SerializeField] private TextMeshProUGUI angularNodeText;
    
    [Header("Charts")]
    [SerializeField] private GameObject PsiChart;
    [SerializeField] private GameObject PsiSqChart;

    [Header("Plane")] [SerializeField] private SingleSelectionButton planeSelect;

    [Header("Layout")] 
    [SerializeField] private RectTransform rightPanelRect;
    [SerializeField] private RectTransform orbitalInfoPanelRect;
    
    private int n, l, ml;
    private bool radialNode;
    private bool angularNode;
    private bool changeOpen;
    private Plane plane = Plane.XZ;

    private void Awake()
    {
        orbitalManager.IsBillBoard = false;
    }

    private void Start()
    {
        planeSelect.UpdateSelected(1);
        ChangeOrbital(1, 0, 0);
    }

    public void ChangeOrbital(int n, int l, int ml)
    {
        if (this.n == n && this.l == l && this.ml == ml)
        {
            orbitalManager.CreateOrbitalInfo(n, l, ml);
            return;
        }
        
        this.n = n;
        this.l = l;
        this.ml = ml;
        
        orbitalManager.DestroyCrossSection(true);
        orbitalManager.CrossSection(n, l, ml, plane);

        if (radialNode)
        {
            orbitalManager.DestroyCSRadialNode(true);
            orbitalManager.CrossSectionRadialNode(n, l, ml, plane);
        }

        if (angularNode)
        {
            orbitalManager.DestroyCSAngularNode(true);
            orbitalManager.CrossSectionAngularNode(n, l, ml, plane);
        }

        if (l > 1) angularNodeText.text = $"Angular Nodes [{l}]";
        else angularNodeText.text = $"Angular Node [{l}]";
        
        if(n - l - 1 > 1) radialNodeText.text = $"Radial Nodes [{n - l - 1}]";
        else radialNodeText.text = $"Radial Node [{n - l - 1}]";

        if (l != 0)
        {
            PsiChart.SetActive(false);
            PsiSqChart.SetActive(false);
        }
        else
        {
            PsiChart.SetActive(true);
            PsiSqChart.SetActive(true);
            
            orbitalManager.Psi(n, l, ml, false, true);
            orbitalManager.PsiSquared(n, l, ml, false, true);
        }
        orbitalManager.PsiSquaredRSquared(n, l, ml, false, true);
    }

    public void ChangePlane(int planeNum)
    { 
        switch (planeNum)
        {
            case 0:
                if (plane == Plane.XY) return;
                plane = Plane.XY;
                break;
            case 1:
                if (plane == Plane.XZ) return;
                plane = Plane.XZ;
                break;
            case 2:
                if (plane == Plane.YZ) return;
                plane = Plane.YZ;
                break;
        }

        planeSelect.UpdateSelected(planeNum);
        
        if (!changeOpen)
        {
            orbitalManager.DestroyCrossSection(true);
            orbitalManager.CrossSection(n, l, ml, plane);

            if (radialNode)
            {
                orbitalManager.DestroyCSRadialNode(true);
                orbitalManager.CrossSectionRadialNode(n, l, ml, plane);
            }

            if (angularNode)
            {
                orbitalManager.DestroyCSAngularNode(true);
                orbitalManager.CrossSectionAngularNode(n, l, ml, plane);
            }
        }
        
        RefreshLayoutNow(rightPanelRect);
    }

    public void RadialNode(bool val)
    {
        radialNode = val;
        
        if (val) orbitalManager.CrossSectionRadialNode(n, l, ml, plane);
        else orbitalManager.DestroyCSRadialNode(true);
    }

    public void AngularNode(bool val)
    {
        angularNode = val;
        
        if (val) orbitalManager.CrossSectionAngularNode(n, l, ml, plane);
        else orbitalManager.DestroyCSAngularNode(true);
    }

    public void RefreshResolution() {}
    
    public void RefreshLayoutNow(RectTransform layoutRoot)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }

    public bool ChangeOpen
    {
        get => changeOpen;
        set => changeOpen = value;
    }
}
