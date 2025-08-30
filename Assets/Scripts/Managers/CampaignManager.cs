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
    [SerializeField] private ScrollbarScript scrollbar;
    [SerializeField] private RectTransform scrollRect;
    
    [Header("Information")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject infoArrow;
    [SerializeField] private TextMeshProUGUI infoNum;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoDesc;
    [SerializeField] private TextMeshProUGUI infoAlign;

    private string[] dataLines;
    private List<string[]> data;
    private int currentCampaign;

    public LevelButton currentLevelButton;
    private bool moduleCheckpoint;
    private int levelMCNum;

    private void Awake()
    {
        currentCampaign = 1;
        ReadData();
    }

    private void Start()
    {
        infoPanel.SetActive(false);
        infoArrow.SetActive(false);
        GenerateLevels();
        
    }

    private void Update()
    {
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
            }
            else
            {
                GameObject go = Instantiate(checkpointPrefab, levelPanel.transform);
                LevelButton checkpoint = go.GetComponent<LevelButton>();
                checkpoint.num.text = $"CHECKPOINT {data[i][2]}";
                checkpoint.name.text = data[i][3];
                checkpoint.dataNum = i;
                int.TryParse(data[i][2], out checkpoint.moduleCheckpointNum);
            }
        }

        scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, 103 * (data.Count - 2) - 8);
        
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
