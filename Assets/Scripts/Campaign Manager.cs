using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private GameObject infoArrow;
    [SerializeField] private TextMeshProUGUI infoNum;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoDesc;
    [SerializeField] private TextMeshProUGUI infoAlign;

    [Header("Scroll Effect")] 
    [SerializeField] private RectTransform scrollRect;
    [SerializeField] private float scrollMargin;
    [SerializeField] private GameObject scroll;
    [SerializeField] private float scrollSpeed = 0.1f; // Tweak this to make scrolling faster/slower
    [SerializeField] private float scrollMin = 1f;
    
    private string[] dataLines;
    private List<string[]> data;
    private int currentCampaign;

    private Scrollbar scrollbar;
    private float prevScreenHeight;
    private float scrollHeight;
    private float prevScroll;
    
    public LevelButton currentLevelButton;
    private bool moduleCheckpoint;
    private int levelMCNum;

    private void Awake()
    {
        currentCampaign = 1;
        ReadData();
        scrollbar = scroll.GetComponent<Scrollbar>();
    }

    private void Start()
    {
        infoPanel.SetActive(false);
        infoArrow.SetActive(false);
        GenerateLevels();
        
        prevScreenHeight = Screen.height;
        scrollHeight = 203 + scrollMargin + 104 * (data.Count - 2);
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
        
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (!IsPointerOnNonInteractiveUI())
            {
                infoPanel.SetActive(false);
                infoArrow.SetActive(false);
        
                if (currentLevelButton != null)
                {
                    currentLevelButton.isPressed = false;
                    currentLevelButton.OnPointerExit(new PointerEventData(EventSystem.current));
                }
            }
        }
        
        float currScroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Math.Abs(currScroll) >= scrollMin)
        {
            float newVal = scrollbar.value - currScroll * scrollSpeed;
            if(newVal > 1) newVal = 1;
            if(newVal < 0) newVal = 0;
            
            scrollbar.value = newVal;
        }
    }

    public void ScrollEffect(float val)
    {
        scrollRect.anchoredPosition = new Vector2(0, val * (scrollHeight - prevScreenHeight));
    }

    public void LevelButton(bool isModule, int moduleCheckpointNum, int dataNum, float y)
    {
        if (currentLevelButton != null)
        {
            currentLevelButton.isPressed = false;
            currentLevelButton.OnPointerExit(new PointerEventData(EventSystem.current));
        }
        
        infoPanel.SetActive(true);
        infoArrow.SetActive(true);
        infoArrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y - 33);

        if (isModule) infoNum.text = $"MODULE {moduleCheckpointNum}";
        else infoNum.text = $"CHECKPOINT {moduleCheckpointNum}";
        
        infoTitle.text = data[dataNum][3];
        infoDesc.text = data[dataNum][4];
        infoAlign.text = data[dataNum][5];

        moduleCheckpoint = isModule;
        levelMCNum = moduleCheckpointNum;
    }

    public void GoLevelButton()
    {
        if (moduleCheckpoint)
        {
            ModuleInfo.campaignNum = currentCampaign;
            ModuleInfo.moduleNum =  levelMCNum;
            SceneManager.LoadScene("Module");
        }
        else
        {
            CheckpointInfo.campaignNum = currentCampaign;
            CheckpointInfo.checkpointNum = levelMCNum;
            SceneManager.LoadScene("Checkpoint");
        }
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
                module.dataNum = i;
                int.TryParse(data[i][2], out module.moduleCheckpointNum);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -104 * (i - 2));
            }
            else
            {
                GameObject go = Instantiate(checkpointPrefab, levelPanel.transform);
                LevelButton checkpoint = go.GetComponent<LevelButton>();
                checkpoint.num.text = $"CHECKPOINT {data[i][2]}";
                checkpoint.name.text = data[i][3];
                checkpoint.dataNum = i;
                int.TryParse(data[i][2], out checkpoint.moduleCheckpointNum);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -104 * (i - 2));
            }
        }
    }
    
    private void ReadData()
    {
        data = new List<string[]>();
        dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"Campaign Data - {currentCampaign}.csv"));
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
    
    bool IsPointerOnNonInteractiveUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            var go = result.gameObject;

            if (go.GetComponent<Button>() != null) return true;
            if (go.GetComponent<Toggle>() != null) return true;
            if (go.GetComponent<Slider>() != null) return true;
            if (go.GetComponent<Scrollbar>() != null) return true;
            if (go.GetComponent<InputField>() != null) return true;
            if (go.GetComponent<Dropdown>() != null) return true;
            if (go.GetComponent<ScrollRect>() != null) return true;
            if (go.GetComponent<TMP_InputField>() != null) return true; // if you're using TextMeshPro
            if (go.GetComponent<Image>() != null) return true;
        }

        return false;
    }
}
