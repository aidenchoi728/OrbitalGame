using TMPro;
using UnityEngine;

public class MouseHoverSign : MonoBehaviour
{
    [SerializeField] private GameObject uiPrefab; // Assign your UI prefab in the inspector
    [SerializeField] private string signText;
    [SerializeField] private float yDisplacement = 60f;
    
    private GameObject currentUIInstance;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            Vector3 pos = Input.mousePosition + Vector3.up * yDisplacement;
            if (currentUIInstance == null && uiPrefab != null)
            {
                currentUIInstance = Instantiate(uiPrefab, pos, Quaternion.identity, FindFirstObjectByType<Canvas>().transform);
                currentUIInstance.GetComponentInChildren<TextMeshProUGUI>().SetText(signText);
            }
            else if (currentUIInstance != null)
            {
                currentUIInstance.transform.position = pos;
            }
        }
        else if (currentUIInstance != null)
        {
            Destroy(currentUIInstance);
        }
    }
}