using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float projectileSpeed = 20f;

    private ulong shooterID;
    
    public ulong ShooterID
    {
        get => shooterID;
        set => shooterID = value;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity * projectileSpeed;
    }
}
