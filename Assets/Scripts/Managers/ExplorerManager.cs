public interface ExplorerManager : GameManager
{
    public void ChangeOrbital(int n, int l, int ml);

    public void RadialNode(bool val);

    public void AngularNode(bool val);
    
    public void RefreshResolution();
}