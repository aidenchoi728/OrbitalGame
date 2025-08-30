using TMPro;
using UnityEngine;

public class TransitionInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;

    public void SetQuantumNumber(int n, int l, int ml)
    {
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
}
