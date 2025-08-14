using UnityEngine;

public class ScreenSpaceBillboard : MonoBehaviour
{
    [SerializeField] private float screenHeight = 50f; // Desired on-screen height in pixels.
    [SerializeField] private float desiredWorldHeight = 1f; // Real-world height of your quad/sprite.
    
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!cam) return;

        // 1. Face the camera (billboard effect)
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        // 2. Use projected distance along the camera's forward vector for consistent scale
        Vector3 camToObj = transform.position - cam.transform.position;
        float projectedDistance = Vector3.Dot(camToObj, cam.transform.forward);

        // 3. Calculate world units per pixel at this distance
        float pixelPerWorldUnit = Screen.height / (2.0f * projectedDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));

        // 4. Compute and apply scale
        float scale = screenHeight / (pixelPerWorldUnit * desiredWorldHeight);
        transform.localScale = Vector3.one * scale;
    }
}