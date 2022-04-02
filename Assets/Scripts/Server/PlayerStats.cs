using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    private NetworkVariable<int> health = new(100);

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

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetAxis("Horizontal") != 0)
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

            animator.SetFloat("DirectionX", horizontalInput);
            animator.SetFloat("DirectionY", verticalInput);
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        health.Value -= damage;
        if (health.Value <= 0)
        {
            // Die
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}