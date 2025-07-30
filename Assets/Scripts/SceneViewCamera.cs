using UnityEngine;

public class SceneViewCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float scrollSpeed = 10f;

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

        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        // Normalize only if we're moving
        if (dir.sqrMagnitude > 0.01f)
            dir = dir.normalized;

        Vector3 upDown = Vector3.zero;
        if (Input.GetKey(KeyCode.Space)) upDown += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift)) upDown += Vector3.down;

        Vector3 velocity = (transform.TransformDirection(dir) + upDown) * moveSpeed * Time.deltaTime;
        transform.position += velocity;

        transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
    }

}