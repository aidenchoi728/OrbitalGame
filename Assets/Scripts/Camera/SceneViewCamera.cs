using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum View{X, Y, Z, XYZ}

public class SceneViewCamera : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 500f;
    [SerializeField] private float defaultDistance = 40f;
    [SerializeField] private float zoomSpeed = 8f;
    [SerializeField] private float minDistance = 1f; // how close you can zoom
    [SerializeField] private float maxDistance = 100f;

    private float yaw;
    private float pitch;
    private float distance;
    
    private bool is2D;

    private float pYaw;
    private float pPitch;
    private float pDistance;

    private void Awake()
    {
        SetPreferred(View.XYZ);
        Recenter();
    }

    private void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    public void Set2D(Plane plane, float customDistance = 0f)
    {
        is2D = true;
        if (customDistance < 0.01f) customDistance = distance;
        SetView(plane, customDistance);
    }

    public void Set2D(bool is2D)
    {
        this.is2D = is2D;
    }

    private void HandleRotation()
    {
        if (is2D) return;
        if (Input.GetMouseButton(0) && !IsPointerOverUI()) // Left mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f); // prevent flipping
            
            UpdateCameraPosition();
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            distance -= scroll * zoomSpeed;
            if(distance < minDistance) distance = minDistance;
            else if(distance > maxDistance) distance = maxDistance;
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = offset;
        transform.LookAt(Vector3.zero);
    }

    /// <summary>
    /// Set rotation and distance based on the Plane enum.
    /// </summary>
    public void SetView(Plane plane, float customDistance = -1f)
    {
        switch (plane)
        {
            case Plane.XY:
                pitch = 0f;
                yaw = 0f;
                break;
            case Plane.XZ:
                pitch = 90f;
                yaw = 0f;
                break;
            case Plane.YZ:
                pitch = 0f;
                yaw = 90f;
                break;
        }

        if (customDistance > 0f)
            distance = customDistance;
        else
            distance = defaultDistance;

        UpdateCameraPosition();
        SetPreferred();
    }

    public void SetPreferred()
    {
        pYaw = yaw;
        pPitch = pitch;
        pDistance = distance;
    }
    
    public void SetPreferred(View view, float customDistance = -1f)
    {
        if (customDistance > 0f)
            pDistance = customDistance;
        else
            pDistance = defaultDistance;

        switch (view)
        {
            case View.X:
                pPitch = 0f;
                pYaw = 90f;
                break;
            case View.Y:
                pPitch = 90f;
                pYaw = 0f;
                break;
            case View.Z:
                pPitch = 0f;
                pYaw = 0f;
                break;
            case View.XYZ:
                pPitch = 45f;
                pYaw = -135f;
                break;
        }
    }

    public void Recenter()
    {
        yaw = pYaw;
        pitch = pPitch;
        distance = pDistance;
        
        UpdateCameraPosition();
    }
    
    public static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
