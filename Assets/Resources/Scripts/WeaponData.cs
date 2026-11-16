using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] public string weaponName;
    [SerializeField] public int weaponId;
    [SerializeField] public string weaponType;
    [SerializeField] public float damage;
    [SerializeField] public float fireSpeed;
    [SerializeField] public Sprite uiSprite;
    [SerializeField] public GameObject model3D;
    [SerializeField] public AudioClip attackSound;
}