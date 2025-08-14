using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;

    [Header("Settings")] 
    [SerializeField] private GameObject axes;
    [SerializeField] private GameObject nucleus;

    private int curr = 0;
    private bool isWait = false;
    private float waitTime = 0f;
    private bool isNext = false;

    private string[] dataLines;
    private List<string[]> data = new List<string[]>();
    
    private List<string[]> vData = new List<string[]>();
    private int nextNum = 1;
    private int vNext = 0;
    
    private void Awake()
    {
        dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"Level Data - {ModuleInfo.campaignNum}-M{ModuleInfo.moduleNum}.csv"));
        foreach (string line in dataLines)
        {
            data.Add(SplitCsvLine(line));
        }
        
        chartObject.SetActive(false);
        previousButton.SetActive(false);
    }

    private void Start()
    {
        Flow();
    }

    private void Update()
    {
        if(isNext) Flow();
        if (isWait)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0f)
            {
                isWait = false;
                Flow();
            }
        }
        
        if(Input.GetKeyDown(KeyCode.LeftArrow) && previousButton.activeSelf == true) Previous();
        if(Input.GetKeyDown(KeyCode.RightArrow) && nextButton.activeSelf == true) Next();
    }

    private void Flow()
    {
        curr++;
        if (curr >= data.Count)
        {
            nextButton.SetActive(false);
            return;
        }

        switch (data[curr][1])
        {
            case "Dialogue":
                dialogueText.text = data[curr][2];
                break;
            case "Destroy":
                switch (data[curr][3])
                {
                    case "ORB":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "OV":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "TRAN":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "AN":
                        orbitalManager.DestroyAngularNode();
                        break;
                    case "RN":
                        orbitalManager.DestroyRadialNode();
                        break;
                    case "CS":
                        orbitalManager.DestroyCrossSection(true);
                        break;
                    case "CS90B":
                        orbitalManager.DestroyCSBoundary(true);
                        break;
                    case "CSAN":
                        orbitalManager.DestroyCSAngularNode(true);
                        break;
                    case "CSRN":
                        orbitalManager.DestroyCSRadialNode(true);
                        break;
                    case "PSI":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PS":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PR":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "W1D":
                        orbitalManager.DestroyWave1D();
                        break;
                    case "W2D":
                        orbitalManager.DestroyWave2D();
                        break;
                    case "W3D":
                        orbitalManager.DestroyWave3D();
                        break;
                }
                break;
            case "Function":
                int.TryParse(data[curr][4], out int n);
                int.TryParse(data[curr][5], out int l);
                int.TryParse(data[curr][6], out int ml);
                
                switch (data[curr][3])
                {
                    case "ORB":
                        orbitalManager.Orbital(n, l, ml, false);
                        break;
                    case "OV":
                        orbitalManager.Orbital(n, l, ml, true);
                        break;
                    case "TRAN":
                        int.TryParse(data[curr][7], out int n2);
                        int.TryParse(data[curr][8], out int l2);
                        int.TryParse(data[curr][9], out int ml2);
                        orbitalManager.TransitionOrbital(n, n2, l, l2, ml, ml2);
                        break;
                    case "AN":
                        orbitalManager.AngularNode(n, l, ml);
                        break;
                    case "RN":
                        orbitalManager.RadialNode(n, l, ml);
                        break;
                    case "CS":
                        Enum.TryParse(data[curr][10], out Plane planeCS);
                        orbitalManager.CrossSection(n, l, ml, planeCS);
                        break;
                    case "CS90B":
                        Enum.TryParse(data[curr][10], out Plane plane90B);
                        orbitalManager.CrossSectionBoundary(n, l, ml, plane90B);
                        break;
                    case "CSAN":
                        Enum.TryParse(data[curr][10], out Plane planeAN);
                        orbitalManager.CrossSectionAngularNode(n, l, ml, planeAN);
                        break;
                    case "CSRN":
                        Enum.TryParse(data[curr][10], out Plane planeRN);
                        orbitalManager.CrossSectionRadialNode(n, l, ml, planeRN);
                        break;
                    case "PSI":
                        chartObject.SetActive(true);
                        orbitalManager.Psi(n, l, ml);
                        break;
                    case "PS":
                        chartObject.SetActive(true);
                        orbitalManager.PsiSquared(n, l, ml);
                        break;
                    case "PR":
                        chartObject.SetActive(true);
                        orbitalManager.PsiSquaredRSquared(n, l, ml);
                        break;
                    case "W1D":
                        orbitalManager.Wave1D();
                        break;
                    case "W2D":
                        orbitalManager.Wave2D();
                        break;
                    case "W3D":
                        orbitalManager.Wave3D();
                        break;
                }
                break;
            case "Wait":
                isWait = true;
                float.TryParse(data[curr][11], out waitTime);
                return;
            case "Next":
                if (isNext)
                {
                    isNext = false;
                    break;
                }
                return;
        }
        
        Flow();
    }

    private void VirtualFlow()
    {
        if (vNext >= nextNum) return;
        curr++;
        if (curr >= data.Count) return;
        
        switch (data[curr][1])
        {
            case "Dialogue":
                RemoveData(1, "Dialogue");
                break;
            case "Destroy":
                RemoveData(3, data[curr][3]);
                vData[curr][1] = ".";
                break;
            case "Function": break;
            case "Wait": 
                vData[curr][1] = ".";
                break;
            case "Next":
                vNext++;
                break;
        }
        
        VirtualFlow();
    }

    private void LoadPrev()
    {
        if(vNext >= nextNum) return;
        curr++;
        if (curr >= data.Count)
        {
            nextButton.SetActive(false);
            return;
        }
        
        switch (vData[curr][1])
        {
            case "Dialogue":
                dialogueText.text = vData[curr][2];
                break;
            case "Destroy":
                switch (vData[curr][3])
                {
                    case "ORB":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "OV":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "TRAN":
                        orbitalManager.DestroyOrbital();
                        break;
                    case "AN":
                        orbitalManager.DestroyAngularNode();
                        break;
                    case "RN":
                        orbitalManager.DestroyRadialNode();
                        break;
                    case "CS":
                        orbitalManager.DestroyCrossSection(true);
                        break;
                    case "CS90B":
                        orbitalManager.DestroyCSBoundary(true);
                        break;
                    case "CSAN":
                        orbitalManager.DestroyCSAngularNode(true);
                        break;
                    case "CSRN":
                        orbitalManager.DestroyCSRadialNode(true);
                        break;
                    case "PSI":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PS":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PR":
                        orbitalManager.isChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "W1D":
                        orbitalManager.DestroyWave1D();
                        break;
                    case "W2D":
                        orbitalManager.DestroyWave2D();
                        break;
                    case "W3D":
                        orbitalManager.DestroyWave3D();
                        break;
                }
                break;
            case "Function":
                int.TryParse(vData[curr][4], out int n);
                int.TryParse(vData[curr][5], out int l);
                int.TryParse(vData[curr][6], out int ml);
                
                switch (vData[curr][3])
                {
                    case "ORB":
                        orbitalManager.Orbital(n, l, ml, false);
                        break;
                    case "OV":
                        orbitalManager.Orbital(n, l, ml, true);
                        break;
                    case "TRAN":
                        int.TryParse(vData[curr][7], out int n2);
                        int.TryParse(vData[curr][8], out int l2);
                        int.TryParse(vData[curr][9], out int ml2);
                        orbitalManager.TransitionOrbital(n, n2, l, l2, ml, ml2);
                        break;
                    case "AN":
                        orbitalManager.AngularNode(n, l, ml);
                        break;
                    case "RN":
                        orbitalManager.RadialNode(n, l, ml);
                        break;
                    case "CS":
                        Enum.TryParse(vData[curr][10], out Plane planeCS);
                        orbitalManager.CrossSection(n, l, ml, planeCS);
                        break;
                    case "CS90B":
                        Enum.TryParse(vData[curr][10], out Plane plane90B);
                        orbitalManager.CrossSectionBoundary(n, l, ml, plane90B);
                        break;
                    case "CSAN":
                        Enum.TryParse(vData[curr][10], out Plane planeAN);
                        orbitalManager.CrossSectionAngularNode(n, l, ml, planeAN);
                        break;
                    case "CSRN":
                        Enum.TryParse(vData[curr][10], out Plane planeRN);
                        orbitalManager.CrossSectionRadialNode(n, l, ml, planeRN);
                        break;
                    case "PSI":
                        chartObject.SetActive(true);
                        orbitalManager.Psi(n, l, ml);
                        break;
                    case "PS":
                        chartObject.SetActive(true);
                        orbitalManager.PsiSquared(n, l, ml);
                        break;
                    case "PR":
                        chartObject.SetActive(true);
                        orbitalManager.PsiSquaredRSquared(n, l, ml);
                        break;
                    case "W1D":
                        orbitalManager.Wave1D();
                        break;
                    case "W2D":
                        orbitalManager.Wave2D();
                        break;
                    case "W3D":
                        orbitalManager.Wave3D();
                        break;
                }
                break;
            case "Wait":
                isWait = true;
                float.TryParse(vData[curr][11], out waitTime);
                return;
            case "Next":
                vNext++;
                break;
            case ".": break;
        }
        
        LoadPrev();
    }

    public void Next()
    {
        if (isWait)
        {
            isWait = false;
            isNext = true;
        }
        else Flow();
        
        nextNum++;
        previousButton.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }

    public void Previous()
    {
        nextButton.SetActive(true);
        
        orbitalManager.DestroyAll();
        chartObject.SetActive(false);
        isWait = false;
        
        nextNum -= 2;
        
        curr = 0;
        vNext = 0;
        
        vData = new List<string[]>();
        foreach (var line in data)
        {
            var newLine = new string[line.Length];
            Array.Copy(line, newLine, line.Length);
            vData.Add(newLine);
        }

        
        VirtualFlow();

        nextNum++;
        
        curr = 0;
        vNext = 0;
        LoadPrev();
        
        if(nextNum == 1) previousButton.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);

    }
    
    private static string[] SplitCsvLine(string line)
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

    private void RemoveData(int pos, string str)
    {
        for (int i = 1; i < curr; i++)
            if (vData[i][pos] == str)
            {
                Debug.Log(vData[i][1]);
                vData[i][1] = ".";
            }
    }
}
