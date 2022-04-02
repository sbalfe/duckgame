using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{

    [SerializeField]
    private List<GunSO> gunList;
    NetworkVariable<int> currentGunIndex = new(0);
    // Start is called before the first frame update
    void OnNetworkSpawn()
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
                StartCoroutine("Fire");
            }
            if (Input.GetMouseButtonUp(0))
            {
                StopCoroutine("Fire");
            }
        }
    }

    IEnumerator Fire()
    {
        FireServerRpc();
        yield return new WaitForSeconds(gunList[currentGunIndex.Value].FireRate);
    }
    
    [ServerRpc]
    void FireServerRpc()
    {
        var projectile = gunList[currentGunIndex.Value].Projectile;
        var obj = Instantiate(projectile, transform.position, transform.rotation);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}
