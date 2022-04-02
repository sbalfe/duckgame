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

    private int m_AvailableGunIndex = 0;

    private void Awake()
    {
        m_Gun = gameObject.AddComponent<Gun>();
    }

    void Start()
    {
        Debug.Log("Hello");
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
        if (!IsClient) return;

        //GetComponent<SpriteRenderer>().sprite = m_GunSprites[m_Gun.currentGunIndex.Value];
        if (m_AvailableGunIndex == 0) GetComponent<SpriteRenderer>().color = Color.red;
        if (m_AvailableGunIndex == 1) GetComponent<SpriteRenderer>().color = Color.green;

    }

    [ServerRpc]
    void UpdateCurrentGunIndexServerRpc(int newIndex)
    {
        m_Gun.currentGunIndex.Value = newIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision Enter");
        if (collision.gameObject.CompareTag("Pickup"))
        {
            m_AvailableGunIndex = 1;
            m_IsPickup = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Collision Exit");
        if (collision.gameObject.CompareTag("Pickup"))
        {
            m_AvailableGunIndex = 0;
            UpdateCurrentGunIndexServerRpc(m_AvailableGunIndex);
            m_IsPickup = true;
        }
    }

    void Update()
    {
        if (m_IsPickup)
        {
            if (Input.GetKey(KeyCode.E))
            {
                UpdateCurrentGunIndexServerRpc(m_AvailableGunIndex);
            }
        }
    }

}
