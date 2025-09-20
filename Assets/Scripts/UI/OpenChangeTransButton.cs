using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenChangeTransButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private OrbitalTransitionManager transitionManager;
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private ChangeTransition changeTransition;
    [SerializeField] private bool isBefore;

    private int n, l, ml;

    public void SetQuantumNumber(int n, int l, int ml)
    {
        this.n = n;
        this.l = l;
        this.ml = ml;
        
        char lName = new char();
        string mlName = new string("");
        
        switch (l)
        {
            case 0:
                lName = 's';
                mlName = "";
                break;
            case 1:
                lName = 'p';
                switch (ml)
                {
                    case -1:
                        mlName = "y";
                        break;
                    case 0:
                        mlName = "z";
                        break;
                    case 1:
                        mlName = "x";
                        break;
                }
                break;
            case 2:
                lName = 'd';
                switch (ml)
                {
                    case -2:
                        mlName = "xy";
                        break;
                    case -1:
                        mlName = "yz";
                        break;
                    case 0:
                        mlName = "z<sup>2</sup>";
                        break;
                    case 1:
                        mlName = "xz";
                        break;
                    case 2:
                        mlName = "x<sup>2</sup>-y<sup>2</sup>";
                        break;
                }
                break;
        }

        texts[0].text = $"{n}{lName}<sub>{mlName}</sub>";
        texts[1].text = $"n = {n}";
        texts[2].text = $"l = {l}";
        texts[3].text = $"m<sub>l</sub> = {ml}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (TextMeshProUGUI text in texts) text.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (TextMeshProUGUI text in texts) text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        changeTransition.gameObject.SetActive(true);
        changeTransition.IsBefore = isBefore;
        changeTransition.SetNum(n, l, ml);

        (int nBefore, int nAfter, int lBefore, int lAfter, int mlBefore, int mlAfter) = transitionManager.GetQuantumNumbers();
        changeTransition.SetQuantumNumber(nBefore, nAfter, lBefore, lAfter, mlBefore, mlAfter);
    }
}
