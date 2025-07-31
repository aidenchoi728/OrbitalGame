using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public static class ModuleInfo
{
    public static int campaignNum = 0;
    public static int moduleNum = 0;
}

public class ModuleManager : MonoBehaviour
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject chartObject;

    private string[] dialogues;
    private int[] funcIndex;
    private Functions[] functions;
    private int[] destroyIndex;
    private Functions[] destroyFunctions;
    private int[] orbitalIndex;
    private int[] n;
    private int[] l;
    private int[] ml;
    private Plane[] planes;

    private int curr = -1;
    private int currFunc = 0;
    private int currDestroy = 0;
    private int currOrbital = 0;
    private int currPlane = 0;

    private void Awake()
    {
        #if UNITY_EDITOR
            LoadData(Path.Combine(Application.streamingAssetsPath, "Campaign 1 Module 1.csv"));
        #else
            LoadData(Path.Combine(Application.streamingAssetsPath, $"Campaign {ModuleInfo.campaignNum} Module {ModuleInfo.moduleNum}.csv"));
        #endif
    }

    private void Start()
    {
        Next();
    }
    
    public void LoadData(string filePath)
    {
        List<string> dialogueList = new();
        List<int> destroyIndexList = new();
        List<Functions> destroyList = new();
        List<int> funcIndexList = new();
        List<Functions> functionList = new();
        List<int> orbitalIndexList = new();
        List<int> nList = new();
        List<int> lList = new();
        List<int> mlList = new();
        List<Plane> planeList = new();

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 1; i < lines.Length; i++) // Skip header row
        {
            string[] values = SplitCsvLine(lines[i]);

            if (values.Length < 12) continue; // Skip malformed rows

            // Dialogue
            if (!string.IsNullOrWhiteSpace(values[1]))
                dialogueList.Add(values[1]);

            // Destroy Index
            if (int.TryParse(values[2], out int dIndex))
                destroyIndexList.Add(dIndex);

            // Destroy Function Enum
            if (Enum.TryParse(values[3], out Functions destroy))
                destroyList.Add(destroy);

            // Function Index
            if (int.TryParse(values[5], out int fIndex))
                funcIndexList.Add(fIndex);

            // Function Enum
            if (Enum.TryParse(values[6], out Functions func))
                functionList.Add(func);

            // Orbital Index
            if (int.TryParse(values[7], out int orbIndex))
                orbitalIndexList.Add(orbIndex);

            // n, l, ml
            if (int.TryParse(values[8], out int nVal))
                nList.Add(nVal);

            if (int.TryParse(values[9], out int lVal))
                lList.Add(lVal);

            if (int.TryParse(values[10], out int mlVal))
                mlList.Add(mlVal);

            // Plane Enum
            if (Enum.TryParse(values[11], out Plane plane))
                planeList.Add(plane);
        }

        dialogues = dialogueList.ToArray();
        destroyIndex = destroyIndexList.ToArray();
        destroyFunctions = destroyList.ToArray();
        funcIndex = funcIndexList.ToArray();
        functions = functionList.ToArray();
        orbitalIndex = orbitalIndexList.ToArray();
        n = nList.ToArray();
        l = lList.ToArray();
        ml = mlList.ToArray();
        planes = planeList.ToArray();
    }


    public void Next()
    {
        curr++;
        if (curr >= dialogues.Length) return;
        
        dialogueText.text = dialogues[curr];

        while (currDestroy < destroyIndex.Length && destroyIndex[currDestroy] == curr)
        {
            switch (destroyFunctions[currDestroy])
            {
                case Functions.ORB:
                    orbitalManager.DestroyOrbital();
                    break;
                case Functions.OV:
                    orbitalManager.DestroyOrbital();
                    break;
                case Functions.TRAN:
                    orbitalManager.DestroyOrbital();
                    break;
                case Functions.AN:
                    orbitalManager.DestroyAngularNode();
                    break;
                case Functions.RN:
                    orbitalManager.DestroyRadialNode();
                    break;
                case Functions.CS:
                    orbitalManager.DestroyCrossSection(true);
                    break;
                case Functions.CS90B:
                    orbitalManager.DestroyCSBoundary(true);
                    break;
                case Functions.CSAN:
                    orbitalManager.DestroyCSAngularNode(true);
                    break;
                case Functions.CSRN:
                    orbitalManager.DestroyCSRadialNode(true);
                    break;
                case Functions.PS:
                    chartObject.SetActive(false);
                    orbitalManager.isChart = false;
                    break;
                case Functions.PR:
                    chartObject.SetActive(false);
                    orbitalManager.isChart = false;
                    break;
            }

            currDestroy++;
        }

        while (currFunc < funcIndex.Length && funcIndex[currFunc] == curr)
        {
            switch (functions[currFunc])
            {
                case Functions.ORB:
                    orbitalManager.Orbital(n[currOrbital], l[currOrbital], ml[currOrbital], false);
                    currOrbital++;
                    break;
                case Functions.OV:
                    while (currOrbital < orbitalIndex.Length && orbitalIndex[currOrbital] == currFunc)
                    {
                        orbitalManager.Orbital(n[currOrbital], l[currOrbital], ml[currOrbital], true);
                        currOrbital++;
                    }
                    break;
                case Functions.TRAN:
                    orbitalManager.TransitionOrbital(n[currOrbital], n[currOrbital + 1], l[currOrbital], l[currOrbital + 1], ml[currOrbital], ml[currOrbital + 1]);
                    currOrbital += 2;
                    break;
                case Functions.AN:
                    orbitalManager.AngularNode(n[currOrbital], l[currOrbital], ml[currOrbital]);
                    currOrbital++;
                    break;
                case Functions.RN:
                    orbitalManager.RadialNode(n[currOrbital], l[currOrbital], ml[currOrbital]);
                    currOrbital++;
                    break;
                case Functions.CS:
                    orbitalManager.CrossSection(n[currOrbital],  l[currOrbital], ml[currOrbital], planes[currPlane]);
                    currPlane++;
                    currOrbital++;
                    break;
                case Functions.CS90B:
                    orbitalManager.CrossSectionBoundary(n[currOrbital], l[currOrbital], ml[currOrbital], planes[currPlane]);
                    currPlane++;
                    currOrbital++;
                    break;
                case Functions.CSAN:
                    orbitalManager.CrossSectionAngularNode(n[currOrbital], l[currOrbital], ml[currOrbital], planes[currPlane]);
                    currPlane++;
                    currOrbital++;
                    break;
                case Functions.CSRN:
                    orbitalManager.CrossSectionRadialNode(n[currOrbital], l[currOrbital], ml[currOrbital], planes[currPlane]);
                    currPlane++;
                    currOrbital++;
                    break;
                case Functions.PS:
                    orbitalManager.PsiSquared(n[currOrbital], l[currOrbital], ml[currOrbital]);
                    currOrbital++;
                    break;
                case Functions.PR:
                    orbitalManager.PsiSquaredRSquared(n[currOrbital], l[currOrbital], ml[currOrbital]);
                    currOrbital++;
                    break;
            }

            currFunc++;
        }
        
    }

    public void Previous()
    {
        curr--;
    }
    
    public static string[] SplitCsvLine(string line)
    {
        var values = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        values.Add(current); // add last value
        return values.ToArray();
    }

}
