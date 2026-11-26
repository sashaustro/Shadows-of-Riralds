using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Pickup")]
    [SerializeField] public WeaponData weapon;
    [SerializeField] private float pickUpDelay = 0.05f;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PickUpItem();
        }
    }

    public void PickUpItem()
    {
        StartCoroutine(PickUp());
    }

    private IEnumerator PickUp()
    {
        yield return new WaitForSeconds(pickUpDelay);
        Destroy(gameObject);
    }
}
