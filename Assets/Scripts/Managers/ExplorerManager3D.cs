using System;
using TMPro;
using UnityEngine;

public class ExplorerManager3D : MonoBehaviour, ExplorerManager
{
    [SerializeField] private OrbitalManager orbitalManager;
    
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI radialNodeText;
    [SerializeField] private TextMeshProUGUI angularNodeText;
    
    [Header("Charts")]
    [SerializeField] private GameObject PsiChart;
    [SerializeField] private GameObject PsiSqChart;
    
    private int n, l, ml;
    private bool radialNode;
    private bool angularNode;

    private void Awake()
    {
        orbitalManager.IsBillBoard = true;
    }

    private void Start()
    {
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
        
        orbitalManager.DestroyOrbital();
        orbitalManager.Orbital(n, l, ml, false);

        if (radialNode)
        {
            orbitalManager.DestroyRadialNode();
            orbitalManager.RadialNode(n, l, ml);
        }

        if (angularNode)
        {
            orbitalManager.DestroyAngularNode();
            orbitalManager.AngularNode(n, l, ml);
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

    public void RadialNode(bool val)
    {
        radialNode = val;
        
        if (val) orbitalManager.RadialNode(n, l, ml);
        else orbitalManager.DestroyRadialNode();
    }

    public void AngularNode(bool val)
    {
        angularNode = val;
        
        if (val) orbitalManager.AngularNode(n, l, ml);
        else orbitalManager.DestroyAngularNode();
    }

    public void RefreshResolution() //TODO
    {
        orbitalManager.DestroyOrbital();
        orbitalManager.Orbital(n, l, ml, false);
    }
}
