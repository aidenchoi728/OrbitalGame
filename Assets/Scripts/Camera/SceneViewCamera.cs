using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneViewCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float scrollSpeed = 15f;

    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 90f;

    void Start()
    {
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if ((Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl)) || Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Handle movement ONLY for WASD
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;

        Vector3 dir = new Vector3(moveX, 0, moveZ);

        // Normalize only if moving
        if (dir.sqrMagnitude > 0.01f)
            dir = dir.normalized;

        // Up/down movement
        Vector3 upDown = Vector3.zero;
        if (Input.GetKey(KeyCode.Space)) upDown += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift)) upDown += Vector3.down;

        Vector3 velocity = (transform.TransformDirection(dir) + upDown) * moveSpeed * Time.deltaTime;
        transform.position += velocity;

        // Scroll zoom only if not on UI
        if (!IsPointerOnNonInteractiveUI())
            transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
    }


    bool IsPointerOnNonInteractiveUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            var go = result.gameObject;

            if (go.GetComponent<Button>() != null) return true;
            if (go.GetComponent<Toggle>() != null) return true;
            if (go.GetComponent<Slider>() != null) return true;
            if (go.GetComponent<Scrollbar>() != null) return true;
            if (go.GetComponent<InputField>() != null) return true;
            if (go.GetComponent<Dropdown>() != null) return true;
            if (go.GetComponent<ScrollRect>() != null) return true;
            if (go.GetComponent<TMP_InputField>() != null) return true; // if you're using TextMeshPro
            if (go.GetComponent<Image>() != null) return true;
        }

        return false;
    }
    
}