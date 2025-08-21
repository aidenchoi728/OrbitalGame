using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle axesToggle;
    [SerializeField] private Toggle nucleusToggle;
    [SerializeField] private SingleSelectionButton resolutionButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private GameObject axes;
    private GameObject nucleus;
    private OrbitalManager orbitalManager;
    private GameManager gameManager;
    
    public void Awake()
    {
        axes = GameObject.Find("Axes");
        nucleus = GameObject.Find("Nucleus Billboard");
        GameObject om = GameObject.Find("Orbital Manager");
        if(om != null) orbitalManager = om.GetComponent<OrbitalManager>();
        GameObject gm = GameObject.FindWithTag("GameManager");
        if(gm != null) gameManager = gm.GetComponent<GameManager>();
    }

    private void Start()
    {
        axesToggle.isOn = Settings.Axes;
        nucleusToggle.isOn = Settings.Nucleus;
        switch (OrbitalSettings.GridSize)
        {
            case 36: 
                resolutionButton.ChangeSelected(0);
                break;
            case 45: 
                resolutionButton.ChangeSelected(1);
                break;
            case 51:
                resolutionButton.ChangeSelected(2);
                break;
        }
        bgmSlider.value = Settings.Bgm;
        sfxSlider.value = Settings.Sfx;
        
        if(axes != null) axes.SetActive(Settings.Axes);
        if(nucleus != null) nucleus.SetActive(Settings.Nucleus);
    }


    public void Axes(bool val)
    {
        Settings.Axes = val;
        if(axes != null) axes.SetActive(val);
    }

    public void Nucleus(bool val)
    {
        Settings.Nucleus = val;
        if(nucleus != null) nucleus.SetActive(val);
    }

    public void Resolution(int val)
    {
        switch (val)
        {
            case 0: 
                OrbitalSettings.GridSize = 36;
                break;
            case 1: 
                OrbitalSettings.GridSize = 45;
                break;
            case 2:
                OrbitalSettings.GridSize = 51;
                break;
        }
        
        if(orbitalManager != null) orbitalManager.GridSize = OrbitalSettings.GridSize;
        if(gameManager != null) gameManager.RefreshResolution();
    }

    public void Bgm(float val)
    {
        Settings.Bgm = val;
    }

    public void Sfx(float val)
    {
        Settings.Sfx = val;
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
