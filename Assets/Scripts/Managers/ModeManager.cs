using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour
{
    public void SelectMode(bool isCampaign)
    {
        if(isCampaign) SceneManager.LoadScene("Campaign Mode");
        else SceneManager.LoadScene("Playground Mode");
    }
}
