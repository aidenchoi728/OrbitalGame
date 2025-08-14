using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class ScrollbarScript : MonoBehaviour
{
    [Header("Scroll Effect")] 
    [SerializeField] private RectTransform scrollRect;
    [SerializeField] private RectTransform pushRect;
    [SerializeField] private float push;
    [SerializeField] private float topMargin = 203f;
    [SerializeField] private float bottomMargin = 149f;
    [SerializeField] private GameObject scroll;
    [SerializeField] private float scrollSpeed = 2f; // Tweak this to make scrolling faster/slower
    [SerializeField] private float scrollMin = 0.005f;
    [SerializeField] private bool onlyOnPanel = true;

    private float prevRectHeight = 0f;
    
    private Scrollbar scrollbar;
    private float prevScreenHeight;
    private float scrollHeight;
    private float prevScroll;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        scrollbar = scroll.GetComponent<Scrollbar>();
    }

    // Update is called once per frame
    void Start()
    {
        prevScreenHeight = 1080;
        scrollHeight = topMargin + bottomMargin + scrollRect.rect.height;
        if(prevScreenHeight >= scrollHeight) scroll.SetActive(false);
        else
        {
            if(pushRect!= null) pushRect.anchoredPosition = new Vector2(-push, pushRect.anchoredPosition.y);
            scroll.SetActive(true);
            scrollbar.size = prevScreenHeight / scrollHeight;
        }
    }
    
    private void Update()
    {
        if (Math.Abs(prevScreenHeight - 1080) > 1f || Math.Abs(scrollRect.rect.height - prevRectHeight) > 1f)
        {
            prevScreenHeight = 1080;
            prevRectHeight = scrollRect.rect.height;
            
            scrollHeight = topMargin + bottomMargin + prevRectHeight;
            
            Debug.Log(scrollHeight);
            
            if (prevScreenHeight >= scrollHeight)
            {
                scroll.SetActive(false);
                scrollRect.anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                if(pushRect!= null) pushRect.anchoredPosition = new Vector2(-push, pushRect.anchoredPosition.y);
                scroll.SetActive(true);
                scrollbar.size = prevScreenHeight / scrollHeight;
            }
            
        }
        
        float currScroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Math.Abs(currScroll) >= scrollMin && scroll.activeInHierarchy && (!onlyOnPanel || IsPointerOnNonInteractiveUI()))
        {
            float newVal = scrollbar.value - currScroll * scrollSpeed;
            if(newVal > 1) newVal = 1;
            if(newVal < 0) newVal = 0;
            
            scrollbar.value = newVal;
        }
    }
    
    public void ScrollEffect(float val)
    {
        scrollRect.anchoredPosition = new Vector2(-push, val * (scrollHeight - prevScreenHeight));
    }
    
    bool IsPointerOnNonInteractiveUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            var go = result.gameObject;

            if (go.GetComponent<Button>() != null) return true;
            if (go.GetComponent<Toggle>() != null) return true;
            if (go.GetComponent<Slider>() != null) return true;
            if (go.GetComponent<Scrollbar>() != null) return true;
            if (go.GetComponent<InputField>() != null) return true;
            if (go.GetComponent<Dropdown>() != null) return true;
            if (go.GetComponent<ScrollRect>() != null) return true;
            if (go.GetComponent<TMP_InputField>() != null) return true; // if you're using TextMeshPro
            if (go.GetComponent<Image>() != null) return true;
        }

        return false;
    }
}
