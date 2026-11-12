using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private float angle = 60;
    [SerializeField] private float distance = 10;
    [SerializeField] private Transform target;
    void Update()
    {
        Quaternion rotation = Quaternion.Euler(angle, 0, 0);
        Vector3 targetPosition = target.position - (rotation * Vector3.forward * distance);
        transform.position = targetPosition;
        transform.rotation = rotation;
    }
}
