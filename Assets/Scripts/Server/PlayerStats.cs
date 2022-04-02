using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// TODO:Change to network behaviour
public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int health = 100;

    public override void OnNetworkSpawn()
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
        // if is owner
        if (Input.GetAxis("Horizontal") != 0)
        {
            // TODO: Change to network transform
            transform.Translate(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, 0, 0));
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.Translate(new Vector3(0, Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed, 0));
        }
    }
}
