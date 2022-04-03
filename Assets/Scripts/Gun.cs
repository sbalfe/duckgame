using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private float projectileSpawnOffset = 1f;
    [SerializeField] private List<GunSO> gunList;
    public NetworkVariable<int> currentGunIndex = new(0);

    private Camera camera;
    private Vector2 aimDirection = Vector2.zero;

    private bool isFiring = false;
    // Start is called before the first frame update

    void Start()
    {
        camera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        if (gunList.Count > 0)
        {
            InitialGunChangeServerRpc();
            GetComponent<SpriteRenderer>().sprite = gunList[currentGunIndex.Value].GunSprite;
        }
    }

    [ServerRpc]
    private void InitialGunChangeServerRpc()
    {
        currentGunIndex.Value = 0;
        GetComponent<SpriteRenderer>().sprite = gunList[currentGunIndex.Value].GunSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && camera)
        {
            aimDirection = camera.ScreenToWorldPoint(Input.mousePosition) -
                           transform.position;
            aimDirection.Normalize();

            Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - transform.position);
            

            if (Input.GetMouseButtonDown(0))
            {
                isFiring = true;
                StartCoroutine(Fire(aimDirection));
            }

            if (Input.GetMouseButtonUp(0))
            {
                isFiring = false;
                StopAllCoroutines();
            }
        }
    }

    IEnumerator Fire(Vector2 aimDirection)
    {
        while (isFiring)
        {
            FireServerRpc(aimDirection);
            yield return new WaitForSeconds(gunList[currentGunIndex.Value].FireRate);
        }
    }

    [ServerRpc]
    void FireServerRpc(Vector2 aimDirection)
    {
        // Get the projectile from the currently equipped gun
        var projectile = gunList[currentGunIndex.Value].Projectile;
        
        // Set projectile offset in aim direction
        Quaternion projectileRotation = Quaternion.LookRotation(Vector3.forward, aimDirection);
        Vector2 spawnPoint = (Vector2) transform.position + (aimDirection * projectileSpawnOffset);
        
        var obj = Instantiate(projectile, spawnPoint, projectileRotation);
        // Set direction (velocity multiplied in projectile script)
        obj.GetComponent<Rigidbody2D>().velocity = aimDirection;
        // Set ID of shooter
        obj.GetComponent<Projectile>().ShooterID = OwnerClientId;
        obj.GetComponent<NetworkObject>().Spawn();
    }
}