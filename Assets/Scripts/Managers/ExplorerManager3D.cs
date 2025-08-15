using UnityEngine;

public class ExplorerManager3D : MonoBehaviour, ExplorerManager
{
    [SerializeField] private OrbitalManager orbitalManager;
    
    private int n, l, ml;
    
    public void ChangeOrbital(int n, int l, int ml)
    {
        this.n = n;
        this.l = l;
        this.ml = ml;
        
        orbitalManager.DestroyOrbital();
        orbitalManager.Orbital(n, l, ml, false);
    }
}
