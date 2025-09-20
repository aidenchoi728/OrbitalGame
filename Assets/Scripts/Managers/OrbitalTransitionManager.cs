using UnityEngine;
using UnityEngine.UI;

public class OrbitalTransitionManager : MonoBehaviour
{
    [SerializeField] private OrbitalManager orbitalManager;
    [SerializeField] private OpenChangeTransButton[] transButtons;
    [SerializeField] private Slider transitionSlider;
    
    private int nBefore, lBefore, mlBefore;
    private int nAfter, lAfter, mlAfter;

    public (int, int, int, int, int, int) GetQuantumNumbers() => (nBefore, nAfter, lBefore, lAfter, mlBefore, mlAfter);
    
    private void Awake()
    {
        nBefore = 1;
        lBefore = 0;
        mlBefore = 0;
        
        ChangeTransition(2, 1, 0, false);
        
        transButtons[0].SetQuantumNumber(1, 0, 0);
        transButtons[1].SetQuantumNumber(2, 1, 0);
    }
    
    public void ChangeTransition(int n, int l, int ml, bool isBefore)
    {
        if (isBefore)
        {
            nBefore = n;
            lBefore = l;
            mlBefore = ml;
        }
        else
        {
            nAfter = n;
            lAfter = l;
            mlAfter = ml;
        }
        
        orbitalManager.TransitionOrbital(nBefore, nAfter, lBefore, lAfter, mlBefore, mlAfter);
        orbitalManager.TransitionAnimation(transitionSlider.value);
    }
}
