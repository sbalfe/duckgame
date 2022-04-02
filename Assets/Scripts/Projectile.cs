using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float timeToLive = 5f;
    [SerializeField] private float projectileSpeed = 20f;

    private ulong shooterID;
    
    public ulong ShooterID
    {
        get => shooterID;
        set => shooterID = value;
    }

    public float ProjectileSpeed
    {
        get => projectileSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            DestroyServerRpc();
        }
    }
    
    [ServerRpc]
    public void DestroyServerRpc()
    {
        Destroy(gameObject, timeToLive);
    }
    
    
}
