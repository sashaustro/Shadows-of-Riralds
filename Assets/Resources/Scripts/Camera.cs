using UnityEngine;

public class Camera : MonoBehaviour
{
    public static bool IsOrbiting { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private float followSmoothSpeed = 10f;

    [Header("Orbit Rotation (Right Mouse Button)")]
    [SerializeField] private float yawAngle = 0f;
    [SerializeField] private float defaultYawAngle = 0f;
    [SerializeField] private float orbitSensitivity = 3.2f;
    [Tooltip("Плавність обертання камери мишкою (більше = плавніше)")]
    [SerializeField] private float orbitSmoothTime = 0.14f;
    [SerializeField] private float orbitDragThreshold = 0.03f;
    [SerializeField] private bool hideCursorWhileOrbiting = true;

    [Header("Camera Recenter")]
    [Tooltip("Клавіша швидкої центрації камери за спиною персонажа")]
    [SerializeField] private KeyCode centerBehindPlayerKey = KeyCode.C;
    [Tooltip("Час між двома кліками ПКМ для повернення камери у стандартний ракурс")]
    [SerializeField] private float doubleClickThreshold = 0.28f;
    [Tooltip("Плавність повернення камери при центрації (збалансовано плавно, але не занадто повільно)")]
    [SerializeField] private float recenterSmoothTime = 0.26f;
    [SerializeField] private float maxRecenterSpeed = 420f;

    [Header("Mouse Look-Ahead (2.5D Feel)")]
    [Tooltip("Легке зміщення камери в бік курсора, щоб краще бачити напрямок погляду")]
    [SerializeField] private float mouseLeadAmount = 0.16f;
    [SerializeField] private float maxMouseLeadDistance = 1.4f;
    [SerializeField] private float mouseLeadSmoothSpeed = 6f;

    [Header("2.5D Angle & Zoom (Scroll Wheel)")]
    [SerializeField] private float angle = 44f;
    [SerializeField] private float distance = 10f;

    [SerializeField] private float minAngle = 32f;
    [SerializeField] private float maxAngle = 56f;
    [SerializeField] private float minDistance = 6.5f;
    [SerializeField] private float maxDistance = 13.5f;

    [SerializeField] private float scrollSensitivity = 0.2f;
    [SerializeField] private float scrollMultiplier = 5f;
    [SerializeField] private float zoomSmoothSpeed = 8f;

    private Player targetPlayer;
    private float zoomT;
    private float defaultZoomT;
    private float currentAngle;
    private float currentDistance;

    private float targetYaw;
    private float currentYaw;
    private float yawVelocity;

    private Vector3 currentLeadOffset;
    private float lastRightClickTime = -10f;
    private bool draggedDuringRightHold;
    private bool isRecentering;

    void Start()
    {
        if (target != null)
        {
            targetPlayer = target.GetComponent<Player>();
        }

        zoomT = Mathf.InverseLerp(minAngle, maxAngle, angle);
        defaultZoomT = zoomT;
        currentAngle = Mathf.Lerp(minAngle, maxAngle, zoomT);
        currentDistance = Mathf.Lerp(minDistance, maxDistance, zoomT);

        targetYaw = yawAngle;
        currentYaw = yawAngle;
        IsOrbiting = false;
    }

    void OnDisable()
    {
        SetOrbitCursorState(false);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            SetOrbitCursorState(false);
            return;
        }

        if (targetPlayer == null)
        {
            targetPlayer = target.GetComponent<Player>();
        }

        HandleOrbitAndRecenter();
        HandleZoom();
        UpdateCameraTransform();
    }

    private void HandleOrbitAndRecenter()
    {
        // Початок натискання ПКМ + перевірка подвійного кліку ПКМ для скидання ракурсу
        if (Input.GetMouseButtonDown(1))
        {
            draggedDuringRightHold = false;
            float timeSinceLastClick = Time.unscaledTime - lastRightClickTime;
            if (timeSinceLastClick <= doubleClickThreshold)
            {
                RecenterToDefault();
                lastRightClickTime = -10f;
            }
            else
            {
                lastRightClickTime = Time.unscaledTime;
            }
        }

        // Обертання камери затиснутою ПКМ
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseX) > orbitDragThreshold)
            {
                draggedDuringRightHold = true;
            }

            if (draggedDuringRightHold)
            {
                isRecentering = false;
                SetOrbitCursorState(true);
                targetYaw += mouseX * orbitSensitivity;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            SetOrbitCursorState(false);
        }

        // СКМ (натискання на колесико) — повернення в стандартний ракурс і зум
        if (Input.GetMouseButtonDown(2))
        {
            RecenterToDefault();
        }

        // Клавіша C — центрація камери рівно за спиною персонажа (куди він зараз дивиться)
        if (Input.GetKeyDown(centerBehindPlayerKey))
        {
            SetShortestTargetYaw(target.eulerAngles.y);
            isRecentering = true;
        }

        float activeSmoothTime = isRecentering ? recenterSmoothTime : orbitSmoothTime;
        float activeMaxSpeed = isRecentering ? maxRecenterSpeed : Mathf.Infinity;

        currentYaw = Mathf.SmoothDamp(
            currentYaw,
            targetYaw,
            ref yawVelocity,
            activeSmoothTime,
            activeMaxSpeed,
            Time.deltaTime
        );

        if (isRecentering && Mathf.Abs(targetYaw - currentYaw) < 0.25f)
        {
            isRecentering = false;
        }

        yawAngle = Mathf.Repeat(currentYaw, 360f);
    }

    private void RecenterToDefault()
    {
        SetShortestTargetYaw(defaultYawAngle);
        zoomT = defaultZoomT;
        isRecentering = true;
    }

    private void SetShortestTargetYaw(float desiredAngle)
    {
        targetYaw = currentYaw + Mathf.DeltaAngle(currentYaw, desiredAngle);
    }

    private void SetOrbitCursorState(bool orbiting)
    {
        if (IsOrbiting == orbiting)
        {
            return;
        }

        IsOrbiting = orbiting;
        if (!hideCursorWhileOrbiting)
        {
            return;
        }

        Cursor.lockState = orbiting ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !orbiting;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            zoomT = Mathf.Clamp01(zoomT - scroll * scrollSensitivity * scrollMultiplier);
        }

        float targetAngle = Mathf.Lerp(minAngle, maxAngle, zoomT);
        float targetDistance = Mathf.Lerp(minDistance, maxDistance, zoomT);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, zoomSmoothSpeed * Time.deltaTime);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothSpeed * Time.deltaTime);

        angle = currentAngle;
        distance = currentDistance;
    }

    private void UpdateCameraTransform()
    {
        // Під час обертання камери ПКМ центруємо фокус строго на персонажі без зміщення за курсором
        Vector3 desiredLead = Vector3.zero;
        if (!IsOrbiting && targetPlayer != null && targetPlayer.HasValidAimPoint)
        {
            Vector3 aimDelta = targetPlayer.AimPoint - target.position;
            aimDelta.y = 0f;
            desiredLead = Vector3.ClampMagnitude(aimDelta * mouseLeadAmount, maxMouseLeadDistance);
        }

        currentLeadOffset = Vector3.Lerp(currentLeadOffset, desiredLead, mouseLeadSmoothSpeed * Time.deltaTime);

        Quaternion rotation = Quaternion.Euler(currentAngle, currentYaw, 0f);
        Vector3 focusPoint = target.position + targetOffset + currentLeadOffset;
        Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * currentDistance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
