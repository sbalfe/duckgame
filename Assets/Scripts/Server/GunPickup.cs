using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunPickup : NetworkBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup"))
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}
