using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DirectionsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject panel;
    
    private Transform textTransform;
    private bool isShow = true;

    private void Awake()
    {
        textTransform = GetComponentInChildren<Transform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = new Color(0.8353f, 0.9490f, 1, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = new Color(0.1529f, 0.6313f, 0.8431f, 1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isShow = !isShow;

        if (isShow)
        {
            transform.localScale = new Vector3(1, -1, 1);
            textTransform.localScale = new Vector3(1, -1, 1);
            text.text = "Hide Directions";
            panel.SetActive(true);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            textTransform.localScale = new Vector3(1, -1, 1);
            text.text = "Show Directions";
            panel.SetActive(false);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
}
