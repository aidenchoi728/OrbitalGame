using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject chart;
    [SerializeField] private Transform progressBar;
    [SerializeField] private Transform scoreLine;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI questionNumText;
    [SerializeField] private TextMeshProUGUI checkpointNumText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreMaxText;
    [SerializeField] private TextMeshProUGUI suggestedReviewText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answerParent;
    [SerializeField] private Image top;
    [SerializeField] private Image[] centers;
    [SerializeField] private Image bottom, submitButtonImage;
    [SerializeField] private TextMeshProUGUI submitButtonText;
    [SerializeField] private SubmitButton submitButton;
    [SerializeField] private GameObject submitNextText;
    [SerializeField] private GameObject submitNextArrow;
    [SerializeField] private GameObject hintButton, seeAnswerButton;
    [SerializeField] private TextMeshProUGUI hintText, wrongText;

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
    private bool correct = false, wrong = false, usedHint = false;
    private bool haveTried = false;
    private Image[] progressImages;
    private Image[] scoreImages;
    private int score = 0;

    private int[] quantumNumbers;
    private int[] correctAnswers;
    private int[] answers = {-1, -1, -1};

    private string[] qnTypes = new string[3] { "principal", "azimuthal", "magnetic" };
    
    private void Awake()
    {
        dataLines = Resources.Load<TextAsset>($"Data/LevelData{ModuleInfo.campaignNum}C{ModuleInfo.moduleNum}").text.Split(
            new[] { '\n', '\r' },
            System.StringSplitOptions.RemoveEmptyEntries
        );
        
        //dataLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, $"LevelData{CheckpointInfo.campaignNum}C{CheckpointInfo.checkpointNum}.csv"));
        foreach (string line in dataLines) data.Add(SplitCsvLine(line));
        progressImages = new Image[dataLines.Length];
        scoreImages = new Image[dataLines.Length];
        progressText.text = "0%";

        for (int i = 0; i < data.Count - 1; i++)
        {
            progressImages[i] = Instantiate(progressPrefab, progressBar).GetComponent<Image>();
            scoreImages[i] = Instantiate(progressPrefab, scoreLine).GetComponent<Image>();
        }
        
        chart.SetActive(false);
        hintText.gameObject.SetActive(false);
        wrongText.gameObject.SetActive(false);
        resultsPanel.SetActive(false);
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
                answerType = UnityEngine.Random.Range(0, 2);
                currAnswer = Instantiate(quantum1Prefab[answerType], answerParent);
                correctAnswers = new int[] {changeToNum(answerType)};
                quantumName = new string[1] {qnTypes[answerType]};
                break;
            case "QN2":
                answerType = 0;
                currAnswer = Instantiate(quantum2Prefab[answerType], answerParent);
                correctAnswers = new int[] {changeToNum(answerType), changeToNum((answerType + 1) % 3)};
                quantumName = new String[2] {qnTypes[answerType], qnTypes[(answerType + 1) % 3]};
                answerType += 3;
                break;
            case "QN3":
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
        
        currAnswer.transform.SetSiblingIndex(1);
        
        questionText.text = ProcessString(data[curr][1], quantumName);
        
        Debug.Log($"Quantum Numbers: {quantumNumbers[0]} {quantumNumbers[1]} {quantumNumbers[2]}, Answer: {correctAnswers[0]}");

        RefreshLayoutNow();

    }

    public void Submit()
    {
        if (correct || wrong)
        {
            if (curr == data.Count - 1)
            {
                resultsPanel.SetActive(true);
                mainPanel.SetActive(false);
                checkpointNumText.text = $"Checkpoint {CheckpointInfo.checkpointNum}";
                scoreText.text = score.ToString();
                scoreMaxText.text = $"/ {fullCredit * (data.Count - 1)}";
                suggestedReviewText.text = data[0][0];
                return;
            }
            
            for (int i = 3; i < 5; i++)
            {
                switch (data[curr][i])
                {
                    case "AN":
                        orbitalManager.DestroyAngularNode();
                        break;
                    case "RN":
                        orbitalManager.DestroyRadialNode();
                        break;
                }
            }
            
            Flow();
            ChangeColor(0);
            hintButton.SetActive(true);
            hintText.gameObject.SetActive(false);
            answers = new int[] {-1, -1, -1};
            submitButton.IsNext = false;
            submitNextText.SetActive(false);
            submitNextArrow.SetActive(false);
            wrongText.gameObject.SetActive(false);
            submitButtonText.gameObject.SetActive(true);
            correct = false;
            wrong = false;
            usedHint = false;
            haveTried = false;
            RefreshLayoutNow();
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
            progressText.text = $"{Mathf.Round((float) curr / (dataLines.Length - 1) * 100)}%";
            if (haveTried || usedHint)
            {
                score += partialCredit;
                progressImages[curr - 1].color = progressColors[1];
                scoreImages[curr - 1].color = progressColors[1];
            }
            else
            {
                score += fullCredit;
                progressImages[curr - 1].color = progressColors[0];
                scoreImages[curr - 1].color = progressColors[0];
            }
            hintText.gameObject.SetActive(false);
            wrongText.gameObject.SetActive(false);
        }
        else
        {
            WrongText();
            if (haveTried) SeeAnswer();
            else
            {
                ChangeColor(2);
                seeAnswerButton.SetActive(true);
                haveTried = true;
            }
        }
        UpdateTextWidth();
        RefreshLayoutNow();
    }

    public void Hint()
    {
        usedHint = true;
        hintButton.SetActive(false);
        hintText.gameObject.SetActive(true);

        List<string> hints = new List<string>();
        
        int n = quantumNumbers[0];
        int l = quantumNumbers[1];
        int ml = quantumNumbers[2];
        
        for (int i = 3; i < 5; i++)
        {
            switch (data[curr][i])
            {
                case "AN":
                    if (l > 0)
                    {
                        orbitalManager.AngularNode(n, l, ml);
                        hints.Add("AN");
                    }
                    else hints.Add("ANX");
                    break;
                case "RN":
                    if (n - l > 1)
                    {
                        orbitalManager.RadialNode(n, l, ml);
                        hints.Add("RN");
                    }
                    else hints.Add("RNX");
                    break;
            }
        }

        string message = "";

        if (hints.Contains("ANX"))
        {
            if (hints.Contains("RNX")) message += "There are no nodes. ";
            else message += "There are no angular nodes. ";
        }
        else if (hints.Contains("RNX"))
        {
            message += "There are no radial nodes. ";
        }

        if (hints.Contains("AN"))
        {
            if (hints.Contains("RN")) message += "Look at the nodes. ";
            else message += "Look at the angular node(s). ";
        }
        else if (hints.Contains("RN"))
        {
            message += "Look at the radial node(s). ";
        }

        hintText.text = "Hint: " + message;
        
        RefreshLayoutNow();
    }

    public void NextButton()
    {
        SceneManager.LoadScene("Campaign Mode");
    }

    public void RedoButton()
    {
        SceneManager.LoadScene("Checkpoint");
    }

    public void WrongText()
    {
        wrongText.gameObject.SetActive(true);
        bool blank = false;
        
        string message = "";
        
        for (int i = 0; i < correctAnswers.Length; i++)
        {
            if (answers[i] == -1)
            {
                message += "Looks like you left something blank. ";
                blank = true;
                break;
            }
        }
        
        switch (data[curr][2])
        {
            case "N":
                message += "Remember, nodes are any planes or radii where the wave function is zero. ";
                break;
            case "AN":
                message += "Remember, angular nodes are any planes where the wave function is zero. ";
                if (quantumNumbers[1] == 2 && quantumNumbers[2] == 0 && correctAnswers[0] > answers[0]) message += "Cones also count. ";
                break;
            case "RN":
                message += "Remember, radial nodes are any radii where the wave function is zero. Look for places where the boundary is broken. ";
                break;
            case "QN1":
                switch (answerType)
                {
                    case 0:
                        if (!blank)
                        {
                            switch (quantumNumbers[1])
                            {
                                case 1:
                                    message += $"n={answers[0] + 1} doesn't have p orbitals. ";
                                    break;
                                case 2:
                                    if (answers[0] < 2) message += $"n={answers[0] + 1} doesn't have d orbitals. ";
                                    break;
                            }
                        }
                        message += "Remember, n is equal to the total number of nodes, plus 1. ";
                        break;
                    case 1:
                        if (answers[0] >= quantumNumbers[0])
                            message += $"n={quantumNumbers[0]} can't have this azimuthal quantum number. ";
                        
                        message += "Remember, l is equal to the total number of angular nodes. ";
                        break;
                }
                break;
            case "QN2":
                if (!blank && answers[1] > answers[0])
                    message += $"n={answers[0] + 1} can't have this azimuthal quantum number. ";
                if (correctAnswers[0] != answers[0])
                {
                    message += "Remember, n is equal to the total number of nodes, plus 1. ";
                    if(correctAnswers[1] != answers[1]) message += "Also, l is equal to the total number of angular nodes. ";
                }
                else if (correctAnswers[1] != answers[1])
                    message += "Remember, l is equal to the total number of angular nodes. ";
                break;
            case "QN3": break;
            case "ORB1":
                switch (answerType)
                {
                    case 0:
                        if (!blank)
                        {
                            switch (quantumNumbers[1])
                            {
                                case 1:
                                    message += $"n={answers[0] + 1} doesn't have p orbitals. ";
                                    break;
                                case 2:
                                    if (answers[0] < 2) message += $"n={answers[0] + 1} doesn't have d orbitals. ";
                                    break;
                            }
                        }
                        message += "Remember, n is equal to the total number of nodes, plus 1. ";
                        break;
                    case 1:
                        if (answers[0] >= quantumNumbers[0])
                            message += $"n={quantumNumbers[0]} can't have this azimuthal quantum number. ";
                        
                        message += "Remember, l is equal to the total number of angular nodes. ";
                        break;
                    case 2:
                        message += "Remember, the magnetic quantum number represents the spatial orientation of the orbital. ";
                        break;
                }
                break;
            case "ORB2":
                switch (answerType)
                {
                    case 0:
                        if (!blank && answers[1] > answers[0])
                            message += $"n={answers[0] + 1} can't have this azimuthal quantum number. ";
                        if (correctAnswers[0] != answers[0])
                        {
                            message += "Remember, n is equal to the total number of nodes, plus 1. ";
                            if(correctAnswers[1] != answers[1]) message += "Also, l is equal to the total number of angular nodes. ";
                        }
                        else if(correctAnswers[1] != answers[1]) message += "Remember, l is equal to the total number of angular nodes. ";
                        break;
                    case 1:
                        if (answers[0] != correctAnswers[0])
                        {
                            if (answers[0] != -1)
                            {
                                switch (quantumNumbers[1])
                                {
                                    case 1:
                                        message += $"n={answers[0] + 1} doesn't have p orbitals. ";
                                        break;
                                    case 2:
                                        if (answers[0] < 2) message += $"n={answers[0] + 1} doesn't have d orbitals. ";
                                        break;
                                }
                            }
                            message += "Remember, n is equal to the total number of nodes, plus 1. ";
                        }

                        if (answers[1] != correctAnswers[1])
                                message += "The magnetic quantum number represents the spatial orientation of the orbital. ";
                        break;
                    case 2:
                        if (answers[0] != correctAnswers[0])
                        {
                            if (answers[0] >= quantumNumbers[0])
                                message += $"n={quantumNumbers[0]} can't have this azimuthal quantum number. ";
                        
                            message += "Remember, l is equal to the total number of angular nodes. ";
                        }

                        if (answers[1] != correctAnswers[1])
                            message += "The magnetic quantum number represents the spatial orientation of the orbital. ";

                        break;
                }
                break;
            case "ORB3":
                if (!blank && answers[1] > answers[0])
                    message += $"n={answers[0] + 1} can't have this azimuthal quantum number. ";
                if (correctAnswers[0] != answers[0])
                {
                    message += "Remember, n is equal to the total number of nodes, plus 1. ";
                    if(correctAnswers[1] != answers[1]) message += "Also, l is equal to the total number of angular nodes. ";
                }
                else if(correctAnswers[1] != answers[1]) message += "Remember, l is equal to the total number of angular nodes. ";
                if (answers[2] != correctAnswers[2])
                    message += "The magnetic quantum number represents the spatial orientation of the orbital. ";
                
                break;
        }

        wrongText.text = message;
    }
    
    public void SeeAnswer()
    {
        progressImages[curr - 1].color = progressColors[2];
        scoreImages[curr - 1].color = progressColors[2];
        progressText.text = $"{Mathf.Round((float) curr / (dataLines.Length - 1) * 100)}%";
        wrong = true;
        seeAnswerButton.SetActive(false);
        submitButtonText.gameObject.SetActive(false);
        submitNextText.SetActive(true);
        submitNextArrow.SetActive(true);
        submitButton.IsNext = true;
        
        CustomDropdown[] dropdowns = currAnswer.GetComponentsInChildren<CustomDropdown>();
        for (int i = 0; i < dropdowns.Length; i++) dropdowns[i].ChangeSelected(correctAnswers[i]);
        
        UpdateTextWidth();
        RefreshLayoutNow();
    }

    public void RefreshResolution() //TODO
    {
        
    }

    public void ChangeColor(int mode)
    {
        top.sprite = topSprite[mode];
        foreach(Image center in centers) center.sprite = centerSprite[mode];
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
        
        ColorBlock cb = hintButton.GetComponent<Button>().colors;
        cb.normalColor = submitButtonNormalColor[mode];
        cb.highlightedColor = submitButtonHighlightColor[mode];
        cb.pressedColor = submitButtonNormalColor[mode];
        hintButton.GetComponent<Button>().colors = cb;
        
        submitNextText.GetComponent<TextMeshProUGUI>().color = submitButtonNormalColor[mode];
        submitNextArrow.GetComponent<Image>().color = submitButtonNormalColor[mode];
        
        hintText.color = submitButtonNormalColor[mode];
        wrongText.color = submitButtonNormalColor[mode];
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

    private void UpdateTextWidth()
    {
        int width = 780;
        if (hintButton.activeSelf) width -= 24 + 40;
        if(seeAnswerButton.activeSelf) width -= 24 + 140;

        Debug.Log(width);
        
        wrongText.rectTransform.sizeDelta = new Vector2(width, 0);
        hintText.rectTransform.sizeDelta = new Vector2(width, 0);
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
