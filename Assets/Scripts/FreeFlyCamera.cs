using UnityEngine;

public class FreeFlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 2.5f;
    public float verticalSpeed = 6f;

    [Header("Look")]
    public float lookSensitivity = 2.0f;
    public bool holdRightMouseToLook = true;

    [Header("Speed Tuning")]
    public float scrollSpeedStep = 2f;
    public float minSpeed = 1f;
    public float maxSpeed = 40f;

    [Header("Smoothing (optional)")]
    public bool useSmoothing = true;
    public float positionLerp = 12f;
    public float rotationLerp = 20f;

    [Header("Reset (optional)")]
    public KeyCode resetKey = KeyCode.F;
    public Vector3 resetPosition = new Vector3(0f, 10f, 0f);
    public Vector3 resetEulerAngles = new Vector3(20f, 0f, 0f);

    float yaw;
    float pitch;

    Vector3 targetPos;
    Quaternion targetRot;

    void Awake()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        // Change move speed with mouse wheel
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            moveSpeed = Mathf.Clamp(moveSpeed + scroll * scrollSpeedStep, minSpeed, maxSpeed);
        }

        // Reset
        if (Input.GetKeyDown(resetKey))
        {
            targetPos = resetPosition;
            yaw = resetEulerAngles.y;
            pitch = resetEulerAngles.x;
            targetRot = Quaternion.Euler(pitch, yaw, 0f);
        }

        bool looking = !holdRightMouseToLook || Input.GetMouseButton(1);

        // Look
        if (looking)
        {
            // Lock cursor while looking
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mx = Input.GetAxis("Mouse X") * lookSensitivity;
            float my = Input.GetAxis("Mouse Y") * lookSensitivity;

            yaw += mx;
            pitch -= my;
            pitch = Mathf.Clamp(pitch, -85f, 85f);

            targetRot = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Movement
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(KeyCode.Q)) up -= 1f;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // Move relative to camera direction (ignoring roll)
        Vector3 fwdDir = targetRot * Vector3.forward;
        Vector3 rightDir = targetRot * Vector3.right;

        Vector3 forward = new Vector3(fwdDir.x, 0f, fwdDir.z).normalized;
        Vector3 right   = new Vector3(rightDir.x, 0f, rightDir.z).normalized;

        Vector3 move = (forward * v + right * h) * speed + Vector3.up * (up * verticalSpeed);
        targetPos += move * Time.deltaTime;

        // Apply
        if (useSmoothing)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
        }
        else
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }
    }
}
