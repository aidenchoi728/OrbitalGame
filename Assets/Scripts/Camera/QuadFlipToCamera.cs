using UnityEngine;

public class QuadFlipToCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (!targetCamera)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!targetCamera) return;

        // Vector from quad to camera, and the quad's forward vector
        Vector3 toCamera = targetCamera.transform.position - transform.position;
        float dot = Vector3.Dot(transform.forward, toCamera);

        Vector3 scale = transform.localScale;
        if (dot < 0)
        {
            // Camera is "behind" the quad, so flip Z
            scale.z = Mathf.Abs(scale.z);
        }
        else
        {
            // Camera is "in front", normal orientation
            scale.z = -Mathf.Abs(scale.z);
        }
        transform.localScale = scale;
    }
}