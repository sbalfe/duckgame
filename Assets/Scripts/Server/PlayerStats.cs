using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    [SerializeField]
    NetworkHealthState m_NetworkHealthState;

    public NetworkHealthState NetworkHealth => m_NetworkHealthState;

    [SerializeField]
    private ClientCharacterVisualization m_CharacterVisualization;

    public ClientCharacterVisualization CharacterVisualization => m_CharacterVisualization;

    private Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Adding CameraController");
            gameObject.AddComponent<CameraController>();
        }

        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        NetworkHealth.IsAlive.OnValueChanged += OnLifeStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        NetworkHealth.IsAlive.OnValueChanged -= OnLifeStateChanged;
    }

    private void OnLifeStateChanged(bool prevLifeState, bool lifeState)
    {
        // If dead
        if (lifeState == false)
        {
            // Kill player
        }
    }

    public void ReceiveHP(int HP)
    {

        if (!IsServer) return;

        // Receive Health
        if (HP > 0)
        {
            NetworkHealth.HitPoints = NetworkHealth.HitPoints + HP;
            Debug.Log("Receiving health");
            // Do a health effect?
        }
        // Take damage
        else
        {
            NetworkHealth.HitPoints = NetworkHealth.HitPoints + HP;
            // Do a damage effect?
            StartCoroutine(CharacterVisualization.TakeDamage());
        }

        // Stop health going below 0 and going above max health
        NetworkHealth.HitPoints = Mathf.Clamp(NetworkHealth.HitPoints, 0, NetworkHealth.BaseHP);
        Debug.Log(NetworkHealth.HitPoints);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Bullet hit");
            ReceiveHP(-5);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            var horizontalInput = Input.GetAxis("Horizontal");
            var verticalInput = Input.GetAxis("Vertical");
            if (horizontalInput != 0)
            {
                // TODO: Change to network transform
                transform.Translate(new Vector3(horizontalInput * Time.deltaTime * moveSpeed, 0, 0));
            }

            if (verticalInput != 0)
            {
                transform.Translate(new Vector3(0, verticalInput * Time.deltaTime * moveSpeed, 0));
            }

            animator.SetFloat("WalkDirectionX", horizontalInput);
            animator.SetFloat("WalkDirectionY", verticalInput);
        }
    }
}
