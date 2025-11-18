using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgMusic;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonOpen;
    [SerializeField] private AudioClip correct;
    [SerializeField] private AudioClip wrong;
    [SerializeField] private AudioClip success;
    
    [Header("Audio Prefabs")]
    [SerializeField] private GameObject bgmPrefab;
    
    private AudioSource bgSource;
    
    private float bgVolume = 0.5f, sfxVolume = 0.5f;

    private void Awake()
    {
        if (Settings.HasRun == true)
        {
            Destroy(gameObject);
            return;
        }

        Settings.HasRun = true;
        
        GameObject bgmObj = Instantiate(bgmPrefab);
        bgSource = bgmObj.GetComponent<AudioSource>();
        DontDestroyOnLoad(bgmObj);
        DontDestroyOnLoad(gameObject);
    }

    public float BgVolume
    {
        set
        {
            bgVolume = value;
            bgSource.volume = bgVolume;
        }
    }

    public float SfxVolume { set => sfxVolume = value; }

    public void PlaySoundEffect(SoundType sound)
    {
        AudioClip clip = null;

        switch (sound)
        {
            case SoundType.ButtonClick:
                clip = buttonClick;
                break;
            case SoundType.ButtonOpen:
                clip = buttonOpen;
                break;
            case SoundType.Correct:
                clip = correct;
                break;
            case SoundType.Wrong:
                clip = wrong;
                break;
            case SoundType.Success:
                clip = success;
                break;
        }
        
        GameObject audioObj = new GameObject(clip.name);
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = sfxVolume;
        audioSource.PlayOneShot(clip, 1f);
        DontDestroyOnLoad(audioObj);
        audioObj.AddComponent<DestroyAfterTime>();
    }
}
