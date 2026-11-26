using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 18f;

    [Header("Mouse Aim & Rotation")]
    [Tooltip("Час згладжування повороту (менше = різкіше, більше = плавніше)")]
    [SerializeField] private float turnSmoothTime = 0.055f;
    [SerializeField] private float maxTurnSpeed = 900f;
    [Tooltip("Висота площини прицілювання відносно ніг персонажа (усуває зміщення кута в 2.5D)")]
    [SerializeField] private float aimHeightOffset = 0.9f;
    [Tooltip("Мертва зона біля центру персонажа, щоб він не смикався коли курсор прямо на ньому")]
    [SerializeField] private float minAimDistance = 0.35f;

    [Header("Aim Direction Indicator")]
    [SerializeField] private bool showAimIndicator = true;
    [SerializeField] private float aimIndicatorRadius = 0.85f;
    [SerializeField] private float aimIndicatorHeight = 0.06f;
    [SerializeField] private float aimIndicatorWidth = 0.055f;
    [SerializeField] private Color aimIndicatorColor = new Color(0.95f, 0.85f, 0.45f, 0.65f);

    [Header("Dash")]
    [SerializeField] private float dashDistance = 3.5f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 0.4f;

    [Header("Physics")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float fixedYPosition = 0f;

    private CharacterController controller;
    private Animator _anim;
    private UnityEngine.Camera mainCamera;
    private LineRenderer aimLine;

    private Vector3 currentHorizontalVelocity;
    private float verticalVelocity;
    private float turnAngularVelocity;
    private float targetYawAngle;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    public Vector3 AimPoint { get; private set; }
    public bool HasValidAimPoint { get; private set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();
        mainCamera = UnityEngine.Camera.main;
        targetYawAngle = transform.eulerAngles.y;

        SetupAimIndicator();
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = UnityEngine.Camera.main;
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        Vector3 moveInput = GetCameraRelativeInput();

        HandleRotationTowardsMouse(moveInput);
        HandleDashInput(moveInput);
        HandleMovement(moveInput);
        UpdateAnimator(moveInput);

        if (aimLine != null && aimLine.enabled != showAimIndicator)
        {
            aimLine.enabled = showAimIndicator;
        }
    }

    void OnValidate()
    {
        if (aimLine != null)
        {
            RefreshAimIndicatorVisuals();
        }
    }

    private Vector3 GetCameraRelativeInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (mainCamera != null)
        {
            forward = mainCamera.transform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;

            right = mainCamera.transform.right;
            right.y = 0f;
            right.Normalize();
        }

        Vector3 move = right * inputX + forward * inputZ;
        return Vector3.ClampMagnitude(move, 1f);
    }

    private void HandleRotationTowardsMouse(Vector3 moveInput)
    {
        HasValidAimPoint = false;

        // Поки гравець крутить камеру затиснутою ПКМ — не крутимо персонажа за курсором
        if (Camera.IsOrbiting)
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                targetYawAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
            }
        }
        else if (mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // Площина на рівні корпусу персонажа (а не підлоги), щоб у 2.5D курсор точно збігався з поглядом
            Vector3 planeOrigin = transform.position + Vector3.up * aimHeightOffset;
            Plane aimPlane = new Plane(Vector3.up, planeOrigin);

            if (aimPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                AimPoint = hitPoint;
                HasValidAimPoint = true;

                Vector3 lookOffset = hitPoint - planeOrigin;
                lookOffset.y = 0f;

                if (lookOffset.magnitude >= minAimDistance)
                {
                    targetYawAngle = Mathf.Atan2(lookOffset.x, lookOffset.z) * Mathf.Rad2Deg;
                }
                else if (moveInput.sqrMagnitude > 0.01f)
                {
                    // Якщо курсор прямо на персонажі — орієнтуємося за напрямком руху
                    targetYawAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
                }
            }
        }

        float smoothYaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetYawAngle,
            ref turnAngularVelocity,
            turnSmoothTime,
            maxTurnSpeed,
            Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
    }

    private void HandleDashInput(Vector3 moveInput)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : transform.forward;
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (_anim != null)
            {
                Vector3 localDash = transform.InverseTransformDirection(dashDirection);
                _anim.SetFloat("dashX", localDash.x);
                _anim.SetFloat("dashY", localDash.z);
                _anim.SetTrigger("dash");
            }
        }
    }

    private void HandleMovement(Vector3 moveInput)
    {
        Vector3 horizontalDisplacement;

        if (isDashing)
        {
            float step = Mathf.Min(Time.deltaTime, dashTimer);
            float dashSpeed = dashDuration > 0f ? dashDistance / dashDuration : 0f;
            horizontalDisplacement = dashDirection * (dashSpeed * step);
            currentHorizontalVelocity = dashDirection * dashSpeed;

            dashTimer -= step;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (_anim != null)
                {
                    _anim.ResetTrigger("dash");
                    _anim.Play("Movement", 0, 0f);
                }
            }
        }
        else
        {
            Vector3 targetVelocity = moveInput * moveSpeed;
            currentHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration * moveSpeed * Time.deltaTime
            );
            horizontalDisplacement = currentHorizontalVelocity * Time.deltaTime;
        }

        if (useGravity)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedGravity;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 totalMove = horizontalDisplacement + Vector3.up * (verticalVelocity * Time.deltaTime);
            controller.Move(totalMove);
        }
        else
        {
            controller.Move(horizontalDisplacement);
            Vector3 pos = transform.position;
            if (!Mathf.Approximately(pos.y, fixedYPosition))
            {
                pos.y = fixedYPosition;
                transform.position = pos;
            }
        }
    }

    private void UpdateAnimator(Vector3 moveInput)
    {
        if (_anim == null)
        {
            return;
        }

        Vector3 localMove = isDashing ? Vector3.zero : transform.InverseTransformDirection(moveInput);
        _anim.SetFloat("moveX", localMove.x);
        _anim.SetFloat("moveY", localMove.z);
    }

    private void SetupAimIndicator()
    {
        GameObject indicatorObj = new GameObject("AimIndicator");
        indicatorObj.transform.SetParent(transform, false);
        indicatorObj.transform.localPosition = new Vector3(0f, aimIndicatorHeight, 0f);

        aimLine = indicatorObj.AddComponent<LineRenderer>();
        aimLine.useWorldSpace = false;
        aimLine.loop = false;
        aimLine.positionCount = 3;
        aimLine.startWidth = aimIndicatorWidth;
        aimLine.endWidth = aimIndicatorWidth;
        aimLine.numCapVertices = 4;
        aimLine.numCornerVertices = 4;
        aimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        aimLine.receiveShadows = false;

        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null)
        {
            aimLine.material = new Material(lineShader);
        }

        RefreshAimIndicatorVisuals();
    }

    private void RefreshAimIndicatorVisuals()
    {
        if (aimLine == null)
        {
            return;
        }

        aimLine.enabled = showAimIndicator;
        aimLine.transform.localPosition = new Vector3(0f, aimIndicatorHeight, 0f);
        aimLine.startWidth = aimIndicatorWidth;
        aimLine.endWidth = aimIndicatorWidth;

        Color edgeColor = new Color(aimIndicatorColor.r, aimIndicatorColor.g, aimIndicatorColor.b, aimIndicatorColor.a * 0.35f);
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(aimIndicatorColor, 0f),
                new GradientColorKey(aimIndicatorColor, 0.5f),
                new GradientColorKey(aimIndicatorColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(edgeColor.a, 0f),
                new GradientAlphaKey(aimIndicatorColor.a, 0.5f),
                new GradientAlphaKey(edgeColor.a, 1f)
            }
        );
        aimLine.colorGradient = gradient;

        // Невелика стрілка-шеврон перед персонажем, яка чітко показує напрямок погляду
        float r = aimIndicatorRadius;
        aimLine.SetPosition(0, new Vector3(-0.26f * r, 0f, 0.78f * r));
        aimLine.SetPosition(1, new Vector3(0f, 0f, 1.05f * r));
        aimLine.SetPosition(2, new Vector3(0.26f * r, 0f, 0.78f * r));
    }
}
