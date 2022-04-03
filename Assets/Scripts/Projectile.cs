using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float timeToLive = 10f;

    private ulong shooterID;
    
    public ulong ShooterID
    {
        get => shooterID;
        set => shooterID = value;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity * projectileSpeed;
        Destroy(gameObject, timeToLive);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Destroy(gameObject, 0.2f);
    }
}
