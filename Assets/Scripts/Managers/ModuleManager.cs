using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Networking;

public static class ModuleInfo
{
    public static int campaignNum = 0;
    public static int moduleNum = 0;
}

public class ModuleManager : MonoBehaviour, GameManager
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject chartObject;
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private TransitionInfo[] transitionInfo;

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
    
    private List<int[]> currOrbitals = new List<int[]>();

    private SceneViewCamera cam;
    
    private void Awake()
    {
        dataLines = Resources.Load<TextAsset>($"Data/LevelData{ModuleInfo.campaignNum}M{ModuleInfo.moduleNum}").text.Split(
            new[] { '\n', '\r' },
            System.StringSplitOptions.RemoveEmptyEntries
        );
        
        //dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"LevelData{ModuleInfo.campaignNum}M{ModuleInfo.moduleNum}.csv"));
        foreach (string line in dataLines) data.Add(SplitCsvLine(line));
        
        chartObject.SetActive(false);
        previousButton.SetActive(false);
        transitionPanel.SetActive(false);
        
        cam = Camera.main.GetComponent<SceneViewCamera>();
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
                        currOrbitals = new List<int[]>();
                        break;
                    case "OV":
                        orbitalManager.DestroyOrbital();
                        currOrbitals = new List<int[]>();
                        break;
                    case "TRAN":
                        orbitalManager.DestroyOrbital();
                        transitionPanel.SetActive(false);
                        currOrbitals = new List<int[]>();
                        break;
                    case "AN":
                        orbitalManager.DestroyAngularNode();
                        break;
                    case "RN":
                        orbitalManager.DestroyRadialNode();
                        break;
                    case "CS":
                        orbitalManager.DestroyCrossSection();
                        break;
                    case "CS90B":
                        orbitalManager.DestroyCSBoundary();
                        break;
                    case "CSAN":
                        orbitalManager.DestroyCSAngularNode();
                        break;
                    case "CSRN":
                        orbitalManager.DestroyCSRadialNode();
                        break;
                    case "PSI":
                        orbitalManager.IsChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PS":
                        orbitalManager.IsChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PR":
                        orbitalManager.IsChart = false;
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
                        int[] orbital = { n, l, ml };
                        currOrbitals.Add(orbital);
                        break;
                    case "OV":
                        orbitalManager.Orbital(n, l, ml, true);
                        int[] overlap = { n, l, ml };
                        currOrbitals.Add(overlap);
                        break;
                    case "TRAN":
                        transitionPanel.SetActive(true);
                        int.TryParse(data[curr][7], out int n2);
                        int.TryParse(data[curr][8], out int l2);
                        int.TryParse(data[curr][9], out int ml2);
                        orbitalManager.TransitionOrbital(n, n2, l, l2, ml, ml2);
                        transitionInfo[0].SetQuantumNumber(n, l, ml);
                        transitionInfo[1].SetQuantumNumber(n2, l2, ml2);
                        int[] trans1 = { n, l, ml };
                        int[] trans2 = { n2, l2, ml2 };
                        currOrbitals.Add(trans1);
                        currOrbitals.Add(trans2);
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
            case "Recenter":
                cam.Recenter();
                break;
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
                        currOrbitals = new List<int[]>();
                        break;
                    case "OV":
                        orbitalManager.DestroyOrbital();
                        currOrbitals = new List<int[]>();
                        break;
                    case "TRAN":
                        orbitalManager.DestroyOrbital();
                        transitionPanel.SetActive(false);
                        currOrbitals = new List<int[]>();
                        break;
                    case "AN":
                        orbitalManager.DestroyAngularNode();
                        break;
                    case "RN":
                        orbitalManager.DestroyRadialNode();
                        break;
                    case "CS":
                        orbitalManager.DestroyCrossSection();
                        break;
                    case "CS90B":
                        orbitalManager.DestroyCSBoundary();
                        break;
                    case "CSAN":
                        orbitalManager.DestroyCSAngularNode();
                        break;
                    case "CSRN":
                        orbitalManager.DestroyCSRadialNode();
                        break;
                    case "PSI":
                        orbitalManager.IsChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PS":
                        orbitalManager.IsChart = false;
                        chartObject.SetActive(false);
                        break;
                    case "PR":
                        orbitalManager.IsChart = false;
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
                        int[] orbital = { n, l, ml };
                        currOrbitals.Add(orbital);
                        break;
                    case "OV":
                        orbitalManager.Orbital(n, l, ml, true);
                        int[] overlap = { n, l, ml };
                        currOrbitals.Add(overlap);
                        break;
                    case "TRAN":
                        transitionPanel.SetActive(true);
                        int.TryParse(data[curr][7], out int n2);
                        int.TryParse(data[curr][8], out int l2);
                        int.TryParse(data[curr][9], out int ml2);
                        orbitalManager.TransitionOrbital(n, n2, l, l2, ml, ml2);
                        transitionInfo[0].SetQuantumNumber(n, l, ml);
                        transitionInfo[1].SetQuantumNumber(n2, l2, ml2);
                        int[] trans1 = { n, l, ml };
                        int[] trans2 = { n2, l2, ml2 };
                        currOrbitals.Add(trans1);
                        currOrbitals.Add(trans2);
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
            case "Recenter":
                cam.Recenter();
                break;
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
    
    public void RefreshResolution() //TODO
    {
        if (currOrbitals.Count == 1)
        {
            orbitalManager.DestroyOrbital();
            orbitalManager.Orbital(currOrbitals[0][0], currOrbitals[0][1], currOrbitals[0][2], false);
        }
        else if (currOrbitals.Count > 1)
        {
            orbitalManager.DestroyOrbital();
            foreach(int[] currOrbital in currOrbitals) orbitalManager.Orbital(currOrbital[0], currOrbital[1], currOrbital[2], true);
        }
    }
}
