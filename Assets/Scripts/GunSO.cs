using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GunScriptableObject", order = 1)]
public class GunSO : ScriptableObject
{
    [SerializeField] private string gunName;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Sprite gunSprite;
    [SerializeField] private float fireRate;
    [SerializeField] private int soundIndex;
    
    public string GunName { get => gunName; }
    public GameObject Projectile { get => projectile; }
    public Sprite GunSprite { get => gunSprite; }
    public float FireRate { get => fireRate; }
    public int SoundIndex
    {
        get => soundIndex;
    }
}
