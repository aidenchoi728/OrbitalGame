using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private GameObject transControlPanel;
    [SerializeField] private GameObject chart;
    
    [Header("Orbital Information")]
    [SerializeField] private int n, l, ml;

    private int nPrev, lPrev, mlPrev;

    private bool isOrbital = false,
        isTransition = false,
        isSliceXY = false,
        isSliceXZ = false,
        isSliceYZ = false,
        isSliceBoundary = false,
        isSliceRadialNode = false,
        isSliceAngularNode = false; 
    
    private void Start()
    {
        transControlPanel.SetActive(false);
        chart.SetActive(false);
    }

    public void OrbitalToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else
            {
                orbitalManager.Orbital(n, l, ml, false);
                nPrev = n;
                lPrev = l;
                mlPrev = ml;
                isOrbital = true;
            }
        }
        else
        {
            orbitalManager.DestroyOrbital();
            orbitalManager.DestroyRadialNode();
            orbitalManager.DestroyAngularNode();
            isOrbital = false;
        }
    }

    public void OrbitalOverlapButtom()
    {
        if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
        else
        {
            orbitalManager.Orbital(n, l, ml, true);
            nPrev = n;
            lPrev = l;
            mlPrev = ml;
        }
    }

    public void OrbitalOverlapOffButton()
    {
        orbitalManager.DestroyOrbital();
    }

    public void OrbitalTransitionToggle(bool isOn)
    {
        if (isOn)
        {
            if(!isOrbital) Debug.Log("No orbital");
            else if(nPrev == n && lPrev == l && mlPrev == ml) Debug.Log("Same orbital");
            else if(lPrev - l != 1 && lPrev - l != -1) Debug.Log("Invalid Transition");
            else
            {
                orbitalManager.TransitionOrbital(nPrev, n, lPrev, l, mlPrev, ml);
                transControlPanel.SetActive(true);
                isTransition = true;
            }
            
        }
        else
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            if(!isTransition) Debug.Log("Not in transition");
            else
            {
                orbitalManager.Orbital(n, l, ml, false);
                nPrev = n;
                lPrev = l;
                mlPrev = ml;
                transControlPanel.SetActive(false);
                isTransition = false;
            }
        }
    }

    public void RadialNodeToggle(bool isOn)
    {
        if (isOn)
        {
            if(!isOrbital) Debug.Log("No orbital");
            else orbitalManager.RadialNode(nPrev, lPrev, mlPrev);
        }
        else orbitalManager.DestroyRadialNode();
    }

    public void AngularNodeToggle(bool isOn)
    {
        if (isOn)
        {
            if(!isOrbital) Debug.Log("No orbital");
            else orbitalManager.AngularNode(nPrev, lPrev, mlPrev);
        }
        else orbitalManager.DestroyAngularNode();
    }

    public void XYSliceToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else
            {
                orbitalManager.CrossSection(n, l, ml, Plane.XY);
                if(isSliceBoundary) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.XY);
                if(isSliceRadialNode) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.XY);
                if(isSliceAngularNode) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.XY);
                isSliceXY = true;
                nPrev = n;
                lPrev = l;
                mlPrev = ml;
            }
        }
        else
        {
            orbitalManager.DestroyCrossSection(false);
            orbitalManager.DestroyCSBoundary(false, 0);
            orbitalManager.DestroyCSRadialNode(false, 0);
            orbitalManager.DestroyCSAngularNode(false, 0);
            isSliceXY = false;
        }
    }
    
    public void XZSliceToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else
            {
                orbitalManager.CrossSection(n, l, ml, Plane.XZ);
                if(isSliceBoundary) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.XZ);
                if(isSliceRadialNode) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.XZ);
                if(isSliceAngularNode) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.XZ);
                isSliceXZ = true;
                nPrev = n;
                lPrev = l;
                mlPrev = ml;
            }
        }
        else
        {
            orbitalManager.DestroyCrossSection(false, 1);
            orbitalManager.DestroyCSBoundary(false, 1);
            orbitalManager.DestroyCSRadialNode(false, 1);
            orbitalManager.DestroyCSAngularNode(false, 1);
            isSliceXZ = false;
        }
    }
    
    public void YZSliceToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else
            {
                orbitalManager.CrossSection(n, l, ml, Plane.YZ);
                if(isSliceBoundary) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.YZ);
                if(isSliceRadialNode) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.YZ);
                if(isSliceAngularNode) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.YZ);
                isSliceYZ = true;
                nPrev = n;
                lPrev = l;
                mlPrev = ml;
            }
        }
        else
        {
            orbitalManager.DestroyCrossSection(false, 2);
            orbitalManager.DestroyCSBoundary(false, 2);
            orbitalManager.DestroyCSRadialNode(false, 2);
            orbitalManager.DestroyCSAngularNode(false, 2);
            isSliceYZ = false;
        }
    }

    public void SliceBoundaryToggle(bool isOn)
    {
        if (isOn)
        {
            if (!isSliceXY && !isSliceXZ && !isSliceYZ) Debug.Log("No slice");
            else
            {
                if(isSliceXY) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.XY);
                if(isSliceXZ) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.XZ);
                if(isSliceYZ) orbitalManager.CrossSectionBoundary(nPrev, lPrev, mlPrev, Plane.YZ);
                isSliceBoundary = true;
            }
        }
        else
        {
            orbitalManager.DestroyCSBoundary(true);
            isSliceBoundary = false;
        }
    }
    
    public void SliceRadialNodeToggle(bool isOn)
    {
        if (isOn)
        {
            if (!isSliceXY && !isSliceXZ && !isSliceYZ) Debug.Log("No slice");
            else
            {
                if(isSliceXY) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.XY);
                if(isSliceXZ) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.XZ);
                if(isSliceYZ) orbitalManager.CrossSectionRadialNode(nPrev, lPrev, mlPrev, Plane.YZ);
                isSliceRadialNode = true;
            }
        }
        else
        {
            orbitalManager.DestroyCSRadialNode(true);
            isSliceRadialNode = false;
        }
    }

    public void SliceAngularNodeToggle(bool isOn)
    {
        if (isOn)
        {
            if (!isSliceXY && !isSliceXZ && !isSliceYZ) Debug.Log("No slice");
            else
            {
                if(isSliceXY) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.XY);
                if(isSliceXZ) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.XZ);
                if(isSliceYZ) orbitalManager.CrossSectionAngularNode(nPrev, lPrev, mlPrev, Plane.YZ);
                isSliceAngularNode = true;
            }
        }
        else
        {
            orbitalManager.DestroyCSAngularNode(true);
            isSliceAngularNode = false;
        }
    }

    public void PsiToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else if(l != 0) Debug.Log("Not radially symmetric");
            else
            {
                orbitalManager.Psi(n, l, ml);
                chart.SetActive(true);
            }
        }
        else
        {
            chart.SetActive(false);
            orbitalManager.isChart = false;
        }
    }
    
    public void PsiSquaredToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else if(l != 0) Debug.Log("Not radially symmetric");
            else
            {
                orbitalManager.PsiSquared(n, l, ml);
                chart.SetActive(true);
            }
        }
        else
        {
            chart.SetActive(false);
            orbitalManager.isChart = false;
        }
    }
    
    public void PsiSquaredRSquaredToggle(bool isOn)
    {
        if (isOn)
        {
            if(n < 0 || n > 3 || l < 0 || l >= n || ml < -1 * l || ml > l) Debug.Log("No valid orbital");
            else
            {
                orbitalManager.PsiSquaredRSquared(n, l, ml);
                chart.SetActive(true);
            }
        }
        else
        {
            chart.SetActive(false);
            orbitalManager.isChart = false;
        }
    }
}
