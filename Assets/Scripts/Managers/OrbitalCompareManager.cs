using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

class QuantumNumber
{
    private int n, l, ml;

    public int N
    {
        get => n;
        set => n = value;
    }

    public int L
    {
        get => l;
        set => l = value;
    }

    public int Ml
    {
        get => ml;
        set => ml = value;
    }
}

public class OrbitalCompareManager : MonoBehaviour
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private GameObject orbitalInfoCenter;
    [SerializeField] private LineChart[] charts;

    [SerializeField] private RectTransform[] refreshRects;
    
    private List<QuantumNumber> quantumNumbers = new List<QuantumNumber>();

    private void Awake()
    {
        orbitalManager.IsBillBoard = true;
        foreach (LineChart chart in charts) chart.RemoveAllSerie();
    }

    private void Start()
    {
        foreach(RectTransform refreshRect in refreshRects) RefreshLayoutNow(refreshRect);
    }

    public void NewOrbital(int n, int l, int ml)
    {
        quantumNumbers.Add(new QuantumNumber());
        ChangeOrbital(n, l, ml, quantumNumbers.Count - 1);
    }

    public void ChangeOrbital(int n, int l, int ml, int index = -1)
    {
        if (index == quantumNumbers.Count)
        {
            quantumNumbers.Add(new QuantumNumber());
        } 
        else if (quantumNumbers[index].N == n && quantumNumbers[index].L == l && quantumNumbers[index].Ml == ml)
        {
            orbitalManager.UpdateOrbitalInfo(n, l, ml);
            return;
        }
        
        Destroy(orbitalManager.GetOrbitalInfo(index));
        
        quantumNumbers[index].N = n;
        quantumNumbers[index].L = l;
        quantumNumbers[index].Ml = ml;
        
        orbitalManager.Orbital(n, l, ml, true, index);

        if (l == 0)
        {
            orbitalManager.Psi(n, l, ml, true, true, index);
            orbitalManager.PsiSquared(n, l, ml, true, true, index);
        }
        orbitalManager.PsiSquaredRSquared(n, l, ml, true, true, index);
    }

    public void DeleteOrbital(int index)
    {
        orbitalManager.DestroyOverlap(index);
        quantumNumbers.RemoveAt(index);

        if (index > 0) Destroy(orbitalInfoCenter.transform.GetChild(index * 2));
        if (index == 0)
        {
            if (quantumNumbers.Count == 0)
            {
                Debug.Log("No orbital left"); //TODO
            }
            else Destroy(orbitalInfoCenter.transform.GetChild(index * 2 + 1));
        }
    }

    public void RefreshResolution() //TODO
    {
        orbitalManager.DestroyOrbital();
        
        foreach(QuantumNumber q in quantumNumbers) orbitalManager.Orbital(q.N, q.L, q.Ml, true);
    }
    
    public void RefreshLayoutNow(RectTransform layoutRoot)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }
}
