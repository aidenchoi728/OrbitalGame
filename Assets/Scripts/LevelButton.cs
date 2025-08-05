using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool isModule;
    public int moduleCheckpointNum = 1;
    
    public TextMeshProUGUI num;
    public TextMeshProUGUI name;
    
    private Image bg;
    private Image line;
    private Image icon;
    

    private void Awake()
    {
        bg = GetComponent<Image>();
        line = transform.Find("Line").GetComponent<Image>();
        icon = transform.Find("Icon").GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bg.color = new Color(0.0627f, 0.0627f, 0.1922f, 0.52f);
        line.color = new Color(0.6235f, 0.8f, 0.9137f, 1f);
        icon.color = new Color(1f, 0.3804f, 0.7882f, 1f);
        num.color = new Color(0.9412f, 0.3569f, 0.7412f, 1f);
        name.color = new Color(0.8353f, 0.9490f, 1f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bg.color = new Color(0.1255f, 0.1216f, 0.2902f, 0.52f);
        line.color = new Color(0.0902f, 0.2745f, 0.3569f, 1f);
        icon.color = new Color(0.1529f, 0.6314f, 0.8431f, 1f);
        num.color = new Color(0.1529f, 0.6314f, 0.8431f, 1f);
        name.color = new Color(0.1529f, 0.6314f, 0.8431f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        line.color = new Color(0.9412f, 0.3569f, 0.7412f, 1f);
        icon.color = new Color(1f, 0.3804f, 0.7882f, 1f);
        num.color = new Color(0.9412f, 0.3569f, 0.7412f, 1f);
        name.color = new Color(0.9412f, 0.3569f, 0.7412f, 1f);
        
        
    }
}