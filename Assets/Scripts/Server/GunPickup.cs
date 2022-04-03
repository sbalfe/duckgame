using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunPickup : NetworkBehaviour
{

    private bool m_IsPickup;

    private Gun m_Gun;

    [SerializeField]
    private List<Sprite> m_GunSprites;
    
    [SerializeField] private ClientSoundController m_SoundController;

    private int m_AvailableGunIndex;
    private GameObject m_PickupObject;

    private void Awake()
    {
        m_Gun = gameObject.GetComponentInChildren<Gun>();
    }

    private void OnEnable()
    {
        m_Gun.currentGunIndex.OnValueChanged += OnCurrentGunIndexChanged;
    }

    private void OnDisable()
    {
        m_Gun.currentGunIndex.OnValueChanged -= OnCurrentGunIndexChanged;
    }
    private void OnCurrentGunIndexChanged(int oldValue, int newValue)
    {
        m_Gun.GetComponent<SpriteRenderer>().sprite = m_GunSprites[m_Gun.currentGunIndex.Value];
    }

    [ServerRpc]
    void UpdateCurrentGunIndexServerRpc(int newIndex)
    {
        m_Gun.currentGunIndex.Value = newIndex;
        Destroy(m_PickupObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup"))
        {
            m_IsPickup = true;
            PickupIndex pickupIndex = collision.GetComponent<PickupIndex>();
            m_PickupObject = collision.gameObject;
            m_AvailableGunIndex = pickupIndex.AvailableGunIndex;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup"))
        {
            m_IsPickup = false;
        }
    }

    void Update()
    {
        if (m_IsPickup && IsOwner)
        {
            if (Input.GetKey(KeyCode.E))
            {
                m_SoundController.PlayLocalSound(2);
                UpdateCurrentGunIndexServerRpc(m_AvailableGunIndex);
            }
        }
    }

}
