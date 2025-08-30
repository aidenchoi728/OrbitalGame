using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlaygroundManager : MonoBehaviour
{
    [SerializeField] private GameObject playgroundPrefab;
    [SerializeField] private Transform mainPanel;
    
    [SerializeField] private string[] names;
    [SerializeField] private string[] sceneNames;
    [SerializeField] private Sprite[] icons;
    [SerializeField] private string[] descriptions;

    private void Awake()
    {
        for (int i = 0; i < names.Length; i++)
        {
            Transform playground = Instantiate(playgroundPrefab, mainPanel).transform;
            
            playground.Find("Top/Title").GetComponent<TextMeshProUGUI>().text = names[i];
            playground.Find("Top/Go").GetComponent<PlaygroundButton>().SceneName = sceneNames[i];
            playground.Find("Bottom/Image").GetComponent<Image>().sprite = icons[i];
            playground.Find("Bottom/Description").GetComponent<TextMeshProUGUI>().text = descriptions[i];
            
        }
        
    }
}
