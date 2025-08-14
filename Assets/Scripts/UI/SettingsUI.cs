using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class Settings
{
    private static bool axes = true;
    private static bool nucleus = true;
    private static int resolution = 1; // 0: low, 1: mid, 2: high
    private static float bgm = 0.5f;
    private static float sfx = 0.5f;
    
    public static bool Axes { get => axes; set => axes = value; }
    public static bool Nucleus { get => nucleus; set => nucleus = value; }
    public static int Resolution { get => resolution; set => resolution = value; }
    public static float Bgm { get => bgm; set => bgm = value; }
    public static float Sfx { get => sfx; set => sfx = value; }
}

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
    
    public void Awake()
    {
        axesToggle.isOn = Settings.Axes;
        nucleusToggle.isOn = Settings.Nucleus;
        resolutionButton.ChangeSelected(Settings.Resolution);
        bgmSlider.value = Settings.Bgm;
        sfxSlider.value = Settings.Sfx;
        
        axes = GameObject.Find("Axes");
        nucleus = GameObject.Find("Nucleus Billboard");
        GameObject om = GameObject.Find("Orbital Manager");
        if(om != null) orbitalManager = om.GetComponent<OrbitalManager>();
        
        if(axes != null) axes.SetActive(Settings.Axes);
        if(nucleus != null) nucleus.SetActive(Settings.Nucleus);
        if(orbitalManager != null)
            switch (Settings.Resolution)
            {
                case 0:
                    orbitalManager.gridSize = 36;
                    break;
                case 1:
                    orbitalManager.gridSize = 45;
                    break;
                case 2:
                    orbitalManager.gridSize = 51;
                    break;
            }
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
        Settings.Resolution = val;
        if(orbitalManager != null)
            switch (val)
            {
                case 0:
                    orbitalManager.gridSize = 36;
                    break;
                case 1:
                    orbitalManager.gridSize = 45;
                    break;
                case 2:
                    orbitalManager.gridSize = 51;
                    break;
            }
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
