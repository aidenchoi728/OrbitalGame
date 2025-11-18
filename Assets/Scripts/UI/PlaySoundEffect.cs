using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaySoundEffect : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SoundType sound;
    
    AudioManager audioManager;

    private void Awake() => audioManager = FindObjectOfType<AudioManager>();
    
    public void OnPointerClick(PointerEventData eventData) => audioManager.PlaySoundEffect(sound);
}

public enum SoundType
{
    ButtonClick, ButtonOpen, Correct, Wrong, Success
}
