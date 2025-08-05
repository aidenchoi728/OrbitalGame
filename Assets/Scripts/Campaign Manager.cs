using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampaignManager : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private TextMeshProUGUI campaignName;
    
    [Header("Level")]
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject modulePrefab;
    [SerializeField] private GameObject checkpointPrefab;
    
    [Header("Information")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoNum;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoDesc;
    [SerializeField] private TextMeshProUGUI infoAlign;

    [Header("Scroll Effect")] 
    [SerializeField] private RectTransform scrollRect;
    [SerializeField] private float scrollMargin;
    [SerializeField] private GameObject scroll;
    
    private string[] dataLines;
    private List<string[]> data;

    private Scrollbar scrollbar;
    private float prevScreenHeight;
    private float scrollHeight;

    private void Awake()
    {
        ReadData(1);
        scrollbar = scroll.GetComponent<Scrollbar>();
    }

    private void Start()
    {
        infoPanel.SetActive(false);
        GenerateLevels();
        
        prevScreenHeight = Screen.height;
        scrollHeight = 203 + scrollMargin + 104 * (data.Count - 2);
        Debug.Log($"{scrollHeight} {prevScreenHeight}");
        if(prevScreenHeight >= scrollHeight) scroll.SetActive(false);
        else
        {
            
            scroll.SetActive(true);
            scrollbar.size = prevScreenHeight / scrollHeight;
        }
    }

    private void Update()
    {
        if (Math.Abs(prevScreenHeight - Screen.height) < 1f)
        {
            prevScreenHeight = Screen.height;
            if(prevScreenHeight >= scrollHeight) scroll.SetActive(false);
            else
            {
                scrollHeight = 203 + scrollMargin + 104 * (data.Count - 2);
                scroll.SetActive(true);
                scrollbar.size = prevScreenHeight / scrollHeight;
            }
            
        }
    }

    public void ScrollEffect(float val)
    {
        scrollRect.anchoredPosition = new Vector2(0, val * (scrollHeight - prevScreenHeight));
    }

    private void GenerateLevels()
    {
        for (int i = 2; i < data.Count; i++)
        {
            if (data[i][1] == "Module")
            {
                GameObject go = Instantiate(modulePrefab, levelPanel.transform);
                LevelButton module = go.GetComponent<LevelButton>();
                module.num.text = $"MODULE {data[i][2]}";
                module.name.text = data[i][3];
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -104 * (i - 2));
            }
            else
            {
                GameObject go = Instantiate(checkpointPrefab, levelPanel.transform);
                LevelButton checkpoint = go.GetComponent<LevelButton>();
                checkpoint.num.text = $"CHECKPOINT {data[i][2]}";
                checkpoint.name.text = data[i][3];
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -104 * (i - 2));
            }
        }
    }
    
    private void ReadData(int num)
    {
        data = new List<string[]>();
        dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"Campaign Data - {num}.csv"));
        foreach (string line in dataLines)
        {
            data.Add(SplitCsvLine(line));
        }
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
}
