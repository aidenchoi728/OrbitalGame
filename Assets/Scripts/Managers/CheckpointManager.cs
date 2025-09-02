using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public static class CheckpointInfo
{
    public static int campaignNum = 0;
    public static int checkpointNum = 0;
}

public class CheckpointManager : MonoBehaviour, GameManager
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private RectTransform[] refreshLayout;

    [Header("Scoring")]
    [SerializeField] private int fullCredit;
    [SerializeField] private int partialCredit;
    
    [Header("Fields")]
    [SerializeField] private GameObject chart;
    [SerializeField] private Transform progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI questionNumText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answerParent;
    [SerializeField] private Image top, center, bottom, submitButtonImage;
    [SerializeField] private TextMeshProUGUI submitButtonText;
    [SerializeField] private SubmitButton submitButton;
    [SerializeField] private GameObject submitNextText;
    [SerializeField] private GameObject submitNextArrow;
    [SerializeField] private GameObject hintButton, seeAnswerButton;

    [Header("Prefabs")] 
    [SerializeField] private GameObject progressPrefab;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject[] quantum1Prefab;
    [SerializeField] private GameObject[] quantum2Prefab;
    [SerializeField] private GameObject quantum3Prefab;
    [SerializeField] private GameObject[] orbital1Prefab;
    [SerializeField] private GameObject[] orbital2Prefab;
    [SerializeField] private GameObject orbital3Prefab;

    [Header("Colors")] 
    [SerializeField] private Color[] progressColors;
    [SerializeField] private Sprite[] topSprite;
    [SerializeField] private Sprite[] centerSprite;
    [SerializeField] private Sprite[] bottomSprite;
    [SerializeField] private Color[] questionNumTextColor;
    [SerializeField] private Color[] questionTextColor;
    [SerializeField] private Color[] answerTextColor;
    [SerializeField] private Color[] submitButtonNormalColor;
    [SerializeField] private Color[] submitButtonHighlightColor;
    [SerializeField] private Color[] fieldNormalColor;
    [SerializeField] private Color[] fieldHighlightColor;
    [SerializeField] private Color[] fieldTextColor;
    [SerializeField] private Color[] itemNormalColor;
    [SerializeField] private Color[] itemHighlightColor;
    [SerializeField] private Color[] itemHoverColor;
    [SerializeField] private Color[] itemTextNormalColor;
    [SerializeField] private Color[] itemTextHighlightColor;
    
    private string[] dataLines;
    private List<string[]> data = new List<string[]>();

    private int curr = 0;
    private GameObject currAnswer;
    private int answerType;
    private bool correct = false;
    private bool haveTried = false;
    private Image[] progressImages;
    private int score = 0;

    private int[] quantumNumbers;
    private int[] correctAnswers;
    private int[] answers = {-1, -1, -1};

    private string[] qnTypes = new string[3] { "principal", "azimuthal", "magnetic" };

    private void Awake()
    {
        dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"Level Data - {CheckpointInfo.campaignNum}-C{CheckpointInfo.checkpointNum}.csv"));
        
        progressImages = new Image[dataLines.Length];
        progressText.text = "0%";
        for (int i = 0; i < dataLines.Length; i++)
        {
            data.Add(SplitCsvLine(dataLines[i]));
            progressImages[i] = Instantiate(progressPrefab, progressBar).GetComponent<Image>();
        }
        
        chart.SetActive(false);
    }

    private void Start()
    {
        Flow();
    }

    private void Flow()
    {
        correct = false;
        curr++;
        RandomOrbital();

        if (currAnswer != null)
        {
            currAnswer.SetActive(false);
            Destroy(currAnswer);
        }
        
        questionNumText.text = $"Question {curr}";
        string[] quantumName = null;

        switch (data[curr][2])
        {
            case "N":
                currAnswer = Instantiate(nodePrefab, answerParent);
                correctAnswers = new int[] {quantumNumbers[0] - 1};
                break;
            case "AN":
                currAnswer = Instantiate(nodePrefab, answerParent);
                correctAnswers = new int[] {quantumNumbers[1]};
                break;
            case "RN":
                currAnswer = Instantiate(nodePrefab, answerParent);
                correctAnswers = new int[] {quantumNumbers[0] - quantumNumbers[1] - 1};
                break;
            case "QN1":
                answerType = UnityEngine.Random.Range(0, 3);
                currAnswer = Instantiate(quantum1Prefab[answerType], answerParent);
                correctAnswers = new int[] {changeToNum(answerType)};
                quantumName = new string[1] {qnTypes[answerType]};
                break;
            case "QN2":
                answerType = UnityEngine.Random.Range(0, 3);
                currAnswer = Instantiate(quantum2Prefab[answerType], answerParent);
                correctAnswers = new int[] {changeToNum(answerType), changeToNum((answerType + 1) % 3)};
                quantumName = new String[2] {qnTypes[answerType], qnTypes[(answerType + 1) % 3]};
                answerType += 3;
                break;
            case "QN3":
                answerType = 6;
                currAnswer = Instantiate(quantum3Prefab, answerParent);
                correctAnswers = new int[] {changeToNum(0), changeToNum(1), changeToNum(2)};
                break;
            case "ORB1":
                answerType = UnityEngine.Random.Range(0, 3);
                currAnswer = Instantiate(orbital1Prefab[answerType], answerParent);
                correctAnswers = new int[] { changeToNum(answerType) };

                GameObject[] labelGos = GameObject.FindGameObjectsWithTag("Label");
                TextMeshProUGUI[] labels = new TextMeshProUGUI[labelGos.Length];
                for (int i = 0; i < labels.Length; i++) labels[i] = labelGos[i].GetComponent<TextMeshProUGUI>();
                
                string[] orbitalName = OrbitalToString();
                Debug.Log(orbitalName[0] + orbitalName[1]);
                switch (answerType)
                {
                    case 0:
                        labels[0].text = orbitalName[0] + orbitalName[1];
                        break;
                    case 1:
                        labels[1].text = quantumNumbers[0].ToString();
                        labels[0].text = orbitalName[1];
                        break;
                    case 2:
                        labels[0].text = quantumNumbers[0].ToString() + orbitalName[0];
                        break;
                }
                
                break;
            case "ORB2":
                answerType = UnityEngine.Random.Range(0, 3);
                currAnswer = Instantiate(orbital2Prefab[answerType], answerParent);
                correctAnswers = new int[] { changeToNum(answerType), changeToNum((answerType + 1) % 3) };
                
                TextMeshProUGUI label = GameObject.FindGameObjectWithTag("Label").GetComponent<TextMeshProUGUI>();
                switch (answerType)
                {
                    case 0:
                        label.text = OrbitalToString()[1];
                        break;
                    case 1:
                        label.text = OrbitalToString()[0];
                        break;
                    case 2:
                        label.text = quantumNumbers[0].ToString();
                        break;
                }
                
                answerType += 3;
                break;
            case "ORB3":
                answerType = 6;
                currAnswer = Instantiate(orbital3Prefab, answerParent);
                correctAnswers = new int[] { changeToNum(0), changeToNum(1), changeToNum(2) };
                break;
        }
        
        questionText.text = ProcessString(data[curr][1], quantumName);
        
        Debug.Log($"Quantum Numbers: {quantumNumbers[0]} {quantumNumbers[1]} {quantumNumbers[2]}, Answer: {correctAnswers[0]}");

        RefreshLayoutNow();

    }

    public void CheckAnswer() //TODO
    {
        if (correct)
        {
            Flow();
            ChangeColor(0);
            hintButton.SetActive(true);
            answers = new int[] {-1, -1, -1};
            submitButton.IsNext = false;
            submitNextText.SetActive(false);
            submitNextArrow.SetActive(false);
            submitButtonText.gameObject.SetActive(true);
            progressText.text = $"{Mathf.Round((float) (curr - 1) / dataLines.Length * 100)}%";
            correct = false;
            haveTried = false;
            return;
        }
        
        correct = true;
        for(int i = 0; i < correctAnswers.Length; i++) if(correctAnswers[i] != answers[i]) correct = false;
        
        if (correct)
        {
            ChangeColor(1);
            hintButton.SetActive(false);
            seeAnswerButton.SetActive(false);
            submitButtonText.gameObject.SetActive(false);
            submitNextText.SetActive(true);
            submitNextArrow.SetActive(true);
            submitButton.IsNext = true;
            CustomDropdown[] dropdowns = currAnswer.GetComponentsInChildren<CustomDropdown>();
            foreach (CustomDropdown dropdown in dropdowns) dropdown.Correct = true;
            if (haveTried)
            {
                score += partialCredit;
                progressImages[curr - 1].color = progressColors[1];
            }
            else
            {
                score += fullCredit;
                progressImages[curr - 1].color = progressColors[0];
            }
        }
        else
        {
            if (haveTried)
            {
                progressImages[curr - 1].color = progressColors[2];
            }
            ChangeColor(2);
            hintButton.SetActive(false);
            seeAnswerButton.SetActive(true);
            haveTried = true;
        }
    }

    public void RefreshResolution() //TODO
    {
        
    }

    public void ChangeColor(int mode)
    {
        top.sprite = topSprite[mode];
        center.sprite = centerSprite[mode];
        bottom.sprite = bottomSprite[mode];
        questionNumText.color = questionNumTextColor[mode];
        questionText.color = questionTextColor[mode];
        foreach(GameObject label in GameObject.FindGameObjectsWithTag("Label")) label.GetComponent<TextMeshProUGUI>().color = answerTextColor[mode];
        submitButtonImage.color = submitButtonNormalColor[mode];
        submitButtonText.color = submitButtonNormalColor[mode];
        submitButton.HighlightColor = submitButtonHighlightColor[mode];
        submitButton.NormalColor = submitButtonNormalColor[mode];

        if (mode != 0)
        {
            CustomDropdown[] dropdowns = currAnswer.GetComponentsInChildren<CustomDropdown>();
            foreach (CustomDropdown dropdown in dropdowns) dropdown.SetColors(fieldNormalColor[mode], fieldHighlightColor[mode], fieldTextColor[mode], 
                itemNormalColor[mode], itemHighlightColor[mode], itemHoverColor[mode], itemTextNormalColor[mode], itemTextHighlightColor[mode]);
        }
    }

    public void ChangeAnswer(string name, int value)
    {
        switch (name)
        {
            case "l":
                if(answerType == 1 || answerType == 5) answers[0] = value;
                else answers[1] = value;
                break;
            case "ml":
                if (answerType == 2) answers[0] = value;
                else if(answerType == 4 || answerType == 5) answers[1] = value;
                else answers[2] = value;
                break;
            default:
                answers[0] = value;
                break;
        }
    }

    private int changeToNum(int type)
    {
        switch (type)
        {
            case 0: return quantumNumbers[0] - 1;
            case 1: return quantumNumbers[1];
            case 2:
                switch (quantumNumbers[1])
                {
                    case 0: return 0;
                    case 1: return (quantumNumbers[2] + 2) % 3 + 1;
                    case 2: return (quantumNumbers[2] + 2) * 7 % 5 + 4;
                }
                break;
        }
        return 0;
    }

    private void RandomOrbital()
    {
        int n = 0, l = 0;
        int rand = UnityEngine.Random.Range(0, 6);
        switch (rand)
        {
            case 0:
                n = 1;
                l = 0;
                break;
            case 1:
                n = 2;
                l = 0;
                break;
            case 2:
                n = 2;
                l = 1;
                break;
            case 3:
                n = 3;
                l = 0;
                break;
            case 4:
                n = 3;
                l = 1;
                break;
            case 5:
                n = 3;
                l = 2;
                break;
        }
        int ml = UnityEngine.Random.Range(-l, l + 1);

        orbitalManager.Orbital(n, l, ml, false);
        quantumNumbers = new int[3] { n, l, ml };
    }

    private string[] OrbitalToString()
    {
        char lName = new char();
        string mlName = new string("");
        
        switch (quantumNumbers[1])
        {
            case 0:
                lName = 's';
                mlName = "";
                break;
            case 1:
                lName = 'p';
                switch (quantumNumbers[2])
                {
                    case -1:
                        mlName = "<sub>y";
                        break;
                    case 0:
                        mlName = "<sub>z";
                        break;
                    case 1:
                        mlName = "<sub>x";
                        break;
                }
                break;
            case 2:
                lName = 'd';
                switch (quantumNumbers[2])
                {
                    case -2:
                        mlName = "<sub>xy";
                        break;
                    case -1:
                        mlName = "<sub>yz";
                        break;
                    case 0:
                        mlName = "<sub>z<sup>2</sup>";
                        break;
                    case 1:
                        mlName = "<sub>xz";
                        break;
                    case 2:
                        mlName = "<sub>x<sup>2</sup>-y<sup>2</sup>";
                        break;
                }
                break;
        }
        
        return new string[] { lName.ToString(), mlName };
    }
    
    private static string ProcessString(string s, string[] addition = null)
    {
        if(addition == null) return s;
        
        List<char> chars = new List<char>();
        bool included = true;
        int num = 0;

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i].Equals('{'))
            {
                included = false;
                for(int j = 0; j < addition[num].Length; j++) chars.Add(addition[num][j]);
                num++;
            }
            if(included) chars.Add(s[i]);
            else if (s[i].Equals('}')) included = true;
        }
        
        return string.Join("", chars);
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
    
    public void RefreshLayoutNow()
    {
        foreach (RectTransform layoutRoot in refreshLayout)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
            Canvas.ForceUpdateCanvases();
        }
    }
}
