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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.color = highlightColor;
        text.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.color = normalColor;
        text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        outline.color = normalColor;
        text.color = normalColor;
    }
}
