
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

        // Запуск Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            !isDashing &&
            move.sqrMagnitude > 0.01f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = move;

            _anim.SetFloat("dashX", move.x);
            _anim.SetFloat("dashY", move.z);
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

        // Фіксуємо висоту
        Vector3 position = transform.position;

        if (position.y != fixedYPosition)
        {
            position.y = fixedYPosition;
            transform.position = position;
        }

        // Анімація руху
        _anim.SetFloat("moveX", isDashing ? 0 : move.x);
        _anim.SetFloat("moveY", isDashing ? 0 : move.z);
    }
}
