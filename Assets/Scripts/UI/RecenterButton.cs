using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecenterButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightedColor;

    private SceneViewCamera cam;
    private TextMeshProUGUI text;
    private Image image;

    private void Awake()
    {
        cam = Camera.main.GetComponent<SceneViewCamera>();
        text = transform.GetComponentInChildren<TextMeshProUGUI>();
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = highlightedSprite;
        text.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = normalSprite;
        text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        cam.Recenter();
    }
}