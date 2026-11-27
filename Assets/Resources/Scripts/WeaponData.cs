using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string weaponName;
    [SerializeField] public int weaponId;
    [SerializeField] public string weaponType;

    [Header("Combat Stats")]
    [SerializeField] public float damage = 10f;
    [SerializeField] public float fireSpeed = 1f;

    [Header("Visuals & Audio")]
    [SerializeField] public Sprite uiSprite;
    [SerializeField] public GameObject model3D;
    [SerializeField] public AudioClip attackSound;
}
