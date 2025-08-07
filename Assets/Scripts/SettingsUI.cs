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

    public void Awake()
    {
        axesToggle.isOn = Settings.Axes;
        nucleusToggle.isOn = Settings.Nucleus;
        resolutionButton.ChangeSelected(Settings.Resolution);
        bgmSlider.value = Settings.Bgm;
        sfxSlider.value = Settings.Sfx;
    }
    
    
    public void Axes(bool val)
    {
        Settings.Axes = val;
    }

    public void Nucleus(bool val)
    {
        Settings.Nucleus = val;
    }

    public void Resolution(int val)
    {
        Settings.Resolution = val;
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
