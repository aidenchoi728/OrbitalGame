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

public class OrbitalCompareManager : MonoBehaviour, GameManager
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
        
        RefreshLayoutNow();
    }

    public void ChangeOrbital(int n, int l, int ml, int index = -1)
    {
        Debug.Log(index);
        
        if (index == quantumNumbers.Count)
        {
            quantumNumbers.Add(new QuantumNumber());
        } 
        else if (quantumNumbers[index].N == n && quantumNumbers[index].L == l && quantumNumbers[index].Ml == ml)
        {
            orbitalManager.UpdateOrbitalInfo(n, l, ml);
            return;
        }
        else{
            orbitalManager.DestroyOverlay(index);
        }
        
        quantumNumbers[index].N = n;
        quantumNumbers[index].L = l;
        quantumNumbers[index].Ml = ml;
        
        orbitalManager.Orbital(n, l, ml, true, index);

        orbitalManager.Psi(n, l, ml, true, true, index);
        orbitalManager.PsiSquared(n, l, ml, true, true, index);
        orbitalManager.PsiSquaredRSquared(n, l, ml, true, true, index);
        
        RefreshLayoutNow();
    }

    public void DeleteOrbital(int index)
    {
        orbitalManager.DestroyOverlay(index);
        quantumNumbers.RemoveAt(index);
        
        RefreshLayoutNow();
        
        orbitalManager.RefreshChart();
    }

    public void RefreshResolution()
    {
        orbitalManager.DestroyOrbital();
        
        foreach(QuantumNumber q in quantumNumbers) orbitalManager.Orbital(q.N, q.L, q.Ml, true);
    }
    
    public void RefreshLayoutNow()
    {
        foreach (RectTransform refreshRect in refreshRects)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(refreshRect);
            Canvas.ForceUpdateCanvases();
        }
        
    }
}
