using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlaygroundButton : MonoBehaviour, IPointerClickHandler
{
    private string sceneName;

    public string SceneName
    {
        set => sceneName = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene(sceneName);
    }
}