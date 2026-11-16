using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weapon;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickUpItem();
        }
    }

    public void PickUpItem()
    {
        StartCoroutine(PickUp());
    }

    IEnumerator PickUp()
    {
        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }
}
