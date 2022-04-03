using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private float projectileSpawnOffset = 1f;
    [SerializeField] private List<GunSO> gunList;
    public NetworkVariable<int> currentGunIndex = new(0);

    [SerializeField] private ClientAudio clientAudio;

    private new Camera camera;

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
            Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - transform.position);


            if (Input.GetMouseButtonDown(0) && !isFiring)
            {
                isFiring = true;
                StartCoroutine(Fire());
            }

            if (Input.GetMouseButtonUp(0) && isFiring)
            {
                StopAllCoroutines();
                isFiring = false;
            }
        }
    }

    Vector2 GetAimVector()
    {
        Vector2 aimDirection = camera.ScreenToWorldPoint(Input.mousePosition) -
                       transform.position;
        aimDirection.Normalize();
        
        return aimDirection;
    }

    IEnumerator Fire()
    {
        while (isFiring)
        {
            FireServerRpc(GetAimVector());
            yield return new WaitForSeconds(1f / gunList[currentGunIndex.Value].FireRate);
        }
    }

    [ServerRpc]
    void FireServerRpc(Vector2 aimDirection)
    {
        // Get the projectile from the currently equipped gun
        var projectilePrefab = gunList[currentGunIndex.Value].Projectile;
        
        // Play sound
        clientAudio.PlaySoundClientRpc(gunList[currentGunIndex.Value].FireSoundIndex);

        // Set projectile offset in aim direction
        Quaternion projectileRotation = Quaternion.LookRotation(Vector3.forward, aimDirection);
        Vector2 spawnPoint = (Vector2)transform.position + (aimDirection * projectileSpawnOffset);

        var projectile = Instantiate(projectilePrefab, spawnPoint, projectileRotation);
        projectile.GetComponent<NetworkObject>().Spawn();
        // Set ID of shooter
        projectile.GetComponent<Projectile>().ShooterID = OwnerClientId;
        // Set direction (velocity multiplied in projectile script)
        projectile.GetComponent<Rigidbody2D>().velocity =
            aimDirection * projectile.GetComponent<Projectile>().ProjectileSpeed;
    }
}