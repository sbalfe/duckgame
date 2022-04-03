using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gun : NetworkBehaviour
{
    [SerializeField] private ClientSoundController soundController;
    [SerializeField] private float projectileSpawnOffset = 1f;
    [SerializeField] private List<GunSO> gunList;
    public NetworkVariable<int> currentGunIndex = new(0);

    //private Camera camera;
    private Vector2 aimDirection = Vector2.zero;

    private bool isFiring = false;
    // Start is called before the first frame update

    /*void Start()
    {
        camera = Camera.main;
    }*/

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

        NetworkHealthState networkHealth = GetComponentInParent<NetworkHealthState>();
        if (networkHealth.IsAlive.Value == false) return;

        if (IsOwner && SceneManager.GetActiveScene().name == "Game3")
        {
            var camera = GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>().OutputCamera;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, GetAimDirection());


            if (Input.GetMouseButtonDown(0))
            {
                isFiring = true;
                StartCoroutine(Fire());
            }

            if (Input.GetMouseButtonUp(0))
            {
                isFiring = false;
                StopAllCoroutines();
            }
        }
    }

    IEnumerator Fire()
    {
        while (isFiring)
        {
            if (IsOwner)
            {
                FireServerRpc(GetAimDirection());
                yield return new WaitForSeconds(1 / gunList[currentGunIndex.Value].FireRate);
            }
        }
    }

    private Vector2 GetAimDirection()
    {
        var camera = GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>().OutputCamera;
        Vector2 aimDirection = camera.ScreenToWorldPoint(Input.mousePosition) -
                               transform.position;
        aimDirection.Normalize();
        return aimDirection;
    }

    [ServerRpc]
    void FireServerRpc(Vector2 aimDirection)
    {
        soundController.PlaySoundClientRpc(gunList[currentGunIndex.Value].SoundIndex);
        // Get the projectile from the currently equipped gun
        var projectile = gunList[currentGunIndex.Value].Projectile;

        // Set projectile offset in aim direction
        Quaternion projectileRotation = Quaternion.LookRotation(Vector3.forward, aimDirection);
        Vector2 spawnPoint = (Vector2)transform.position + (aimDirection * projectileSpawnOffset);

        var obj = Instantiate(projectile, spawnPoint, projectileRotation);
        // Set direction (velocity multiplied in projectile script)
        obj.GetComponent<Rigidbody2D>().velocity = aimDirection;
        // Set ID of shooter
        obj.GetComponent<Projectile>().ShooterID = OwnerClientId;
        obj.GetComponent<NetworkObject>().Spawn();
    }
}