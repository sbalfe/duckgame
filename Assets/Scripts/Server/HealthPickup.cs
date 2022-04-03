using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthPickup : NetworkBehaviour
{
    private PlayerStats m_PlayerStats;

    [SerializeField]
    private int m_HealthAmount = 20;

    private void Awake()
    {
        if (IsOwner)
        {
            m_PlayerStats = GetComponent<PlayerStats>();
        }
    }

    [ServerRpc]
    void UseHealthServerRpc()
    {
        m_PlayerStats.ReceiveHP(m_HealthAmount);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Health"))
        {
            UseHealthServerRpc();
            Destroy(collision.gameObject);
        }
    }
}
