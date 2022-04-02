using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private List<GunSO> gunList;
    public NetworkVariable<int> currentGunIndex = new(0);

    private Camera camera;

    private bool isFiring = false;
    // Start is called before the first frame update

    void Start()
    {
        camera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && gunList.Count > 0)
        {
            currentGunIndex.Value = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isFiring = true;
                StartCoroutine("Fire");
            }

            if (Input.GetMouseButtonUp(0))
            {
                isFiring = false;
                StopCoroutine("Fire");
            }
        }
    }

    IEnumerator Fire()
    {
        while (isFiring)
        {
            FireServerRpc();
            yield return new WaitForSeconds(gunList[currentGunIndex.Value].FireRate);
        }
    }

    [ServerRpc]
    void FireServerRpc()
    {
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.

        var aimDirection = camera.ScreenToWorldPoint(Input.mousePosition) -
                           transform.position;
        aimDirection = aimDirection.normalized;

        // Get the projectile from the currently equipped gun
        var projectile = gunList[currentGunIndex.Value].Projectile;
        var obj = Instantiate(projectile, transform.position, transform.rotation);
        // Set direction (velocity multiplied in projectile script)
        obj.GetComponent<Rigidbody2D>().velocity = aimDirection;
        // Set ID of shooter
        obj.GetComponent<Projectile>().ShooterID = OwnerClientId;
        obj.GetComponent<NetworkObject>().Spawn();
    }
}