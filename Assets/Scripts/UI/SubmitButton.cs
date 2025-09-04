using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubmitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image outline;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private Image nextArrow;

    private bool isNext = false;

    public bool IsNext
    {
        set => isNext = value;
    }

    public Color NormalColor
    {
        set => normalColor = value;
    }

    public Color HighlightColor
    {
        set => highlightColor = value;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.color = highlightColor;
        if (isNext)
        {
            nextText.color = highlightColor;
            nextArrow.color = highlightColor;
        }
        else text.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.color = normalColor;
        if (isNext)
        {
            nextText.color = normalColor;
            nextArrow.color = normalColor;
        }
        else text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        outline.color = normalColor;
        if (isNext)
        {
            nextText.color = normalColor;
            nextArrow.color = normalColor;
        }
        else text.color = normalColor;
        
        checkpointManager.Submit();
    }
}
