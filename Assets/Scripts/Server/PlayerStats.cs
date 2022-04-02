using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    private NetworkVariable<int> health = new(100);


    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Adding CameraController");
            gameObject.AddComponent<CameraController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                //Debug.Log("press w");
                // TODO: Change to network transform
                transform.Translate(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, 0, 0));
            }

            if (Input.GetAxis("Vertical") != 0)
            {
                transform.Translate(new Vector3(0, Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed, 0));
            }
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