using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private CharacterController controller;
    float fixedZPosition = 0f;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float InputX = Input.GetAxisRaw("Horizontal");
        float InputY = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(InputX, 0, InputY);
        move = Vector3.ClampMagnitude(move, 1f);
        controller.Move(move * moveSpeed * Time.deltaTime);



        Vector3 position = transform.position;
        if (position.y != fixedZPosition)
        {
            position.y = fixedZPosition;
            transform.position = position;
        }
    }
}
