using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.3f;

    private CharacterController controller;
    private Animator _anim;

    float fixedYPosition = 0f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float InputX = Input.GetAxisRaw("Horizontal");
        float InputY = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(InputX, 0, InputY);
        move = Vector3.ClampMagnitude(move, 1f);

        // Поворот персонажа за курсором
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(
            Input.mousePosition
        );

        Plane ground = new Plane(Vector3.up, transform.position);

        if (ground.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);

            Vector3 direction = point - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        // Перерахунок руху відносно повороту персонажа
        Vector3 localMove = transform.InverseTransformDirection(move);

        // Запуск Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            !isDashing &&
            move.sqrMagnitude > 0.01f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = move;

            _anim.SetFloat("dashX", localMove.x);
            _anim.SetFloat("dashY", localMove.z);
            _anim.SetTrigger("dash");
        }

        // Рух персонажа
        if (isDashing)
        {
            float step = Mathf.Min(Time.deltaTime, dashTimer);

            controller.Move(
                dashDirection * (dashDistance / dashDuration) * step
            );

            dashTimer -= step;

            if (dashTimer <= 0f)
            {
                isDashing = false;

                _anim.ResetTrigger("dash");
                _anim.Play("Movement", 0, 0f);
            }
        }
        else
        {
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        // Фіксація висоти
        Vector3 position = transform.position;

        if (position.y != fixedYPosition)
        {
            position.y = fixedYPosition;
            transform.position = position;
        }

        // Анімація відносно повороту персонажа
        _anim.SetFloat("moveX", isDashing ? 0 : localMove.x);
        _anim.SetFloat("moveY", isDashing ? 0 : localMove.z);
    }
}