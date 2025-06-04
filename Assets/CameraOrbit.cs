using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target & Distances")]
    public Transform target;            // Assign an empty “Pivot” or your model’s center
    public float distance = 5f;         // Starting distance from target
    public float minDistance = 2f;      // How close you can zoom in
    public float maxDistance = 15f;     // How far you can zoom out

    [Header("Speeds")]
    public float rotateSpeed = 200f;    // Sensitivity for orbit (degrees/sec)
    public float zoomSpeed = 10f;       // Sensitivity for zoom (scroll / pinch)
    public float panSpeed = 0.5f;       // Sensitivity for pan (middle-mouse / two-finger)

    [Header("Angle Limits")]
    public float minYAngle = -20f;      // Clamp lowest pitch
    public float maxYAngle = 80f;       // Clamp highest pitch

    private float currentYaw = 0f;      // Horizontal angle
    private float currentPitch = 20f;   // Vertical angle
    private Vector3 currentPivotOffset = Vector3.zero;  // Accumulates pan

    // Touch bookkeeping
    private float lastTouchDist = 0f;
    private Vector2 lastTouchMidpoint = Vector2.zero;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraOrbit: You must assign a target (pivot) in the Inspector.");
            enabled = false;
            return;
        }

        // Initialize yaw/pitch based on current camera position
        Vector3 offset = transform.position - (target.position + currentPivotOffset);
        distance = offset.magnitude;
        currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        currentPitch = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }

        UpdateCameraPosition();
    }

    void HandleMouseInput()
    {
        // 1) ORBIT = Left-mouse drag
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            currentYaw += dx * rotateSpeed * Time.deltaTime;
            currentPitch -= dy * rotateSpeed * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);
        }

        // 2) ZOOM = Scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 3) PAN = Middle-mouse drag
        if (Input.GetMouseButton(2))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            // Convert screen-space delta to world-space pan
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            Vector3 panMovement = (-right * dx + -up * dy) * panSpeed * Time.deltaTime;
            currentPivotOffset += panMovement;
        }
    }

    void HandleTouchInput()
    {
        // ONE-finger → Orbit
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.deltaPosition;
                currentYaw += delta.x * (rotateSpeed * 0.02f);
                currentPitch -= delta.y * (rotateSpeed * 0.02f);
                currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);
            }
            lastTouchDist = 0f; // reset pinch state
        }
        // TWO-finger → Either pinch-zoom or two-finger pan
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Compute current & previous distance between the two touches
            float currDist = Vector2.Distance(t0.position, t1.position);
            float prevDist = Vector2.Distance(
                t0.position - t0.deltaPosition,
                t1.position - t1.deltaPosition);

            float distDelta = currDist - prevDist;

            // If pinch movement is significant → Zoom
            if (Mathf.Abs(distDelta) > 2f)
            {
                distance -= (distDelta * 0.01f) * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
            else
            {
                // Otherwise treat as two-finger pan: track midpoint movement
                Vector2 midCurr = (t0.position + t1.position) * 0.5f;
                Vector2 midPrev = ((t0.position - t0.deltaPosition) + (t1.position - t1.deltaPosition)) * 0.5f;
                Vector2 midDelta = midCurr - midPrev;

                Vector3 right = transform.right;
                Vector3 up = transform.up;
                Vector3 panMovement = (-right * midDelta.x + -up * midDelta.y) * (panSpeed * 0.02f);
                currentPivotOffset += panMovement;
            }

            lastTouchDist = currDist;
            lastTouchMidpoint = (t0.position + t1.position) * 0.5f;
        }
        else
        {
            lastTouchDist = 0f;
        }
    }

    void UpdateCameraPosition()
    {
        // Convert spherical coords (distance, yaw, pitch) → Cartesian offset
        float radPitch = Mathf.Deg2Rad * currentPitch;
        float radYaw = Mathf.Deg2Rad * currentYaw;

        float x = distance * Mathf.Cos(radPitch) * Mathf.Sin(radYaw);
        float y = distance * Mathf.Sin(radPitch);
        float z = distance * Mathf.Cos(radPitch) * Mathf.Cos(radYaw);

        Vector3 offset = new Vector3(x, y, z);
        Vector3 pivot = target.position + currentPivotOffset;

        transform.position = pivot + offset;
        transform.LookAt(pivot);
    }
}
