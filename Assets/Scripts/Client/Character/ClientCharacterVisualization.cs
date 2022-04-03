using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientCharacterVisualization : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if (!IsClient || transform.parent == null)
        {
            enabled = false;
            return;
        }
    }

    public IEnumerator TakeDamage()
    {
        TakeDamageFXClientRpc();
        yield return new WaitForSeconds(0.2f);
        CancelFXClientRpc();
    }

    [ClientRpc]
    public void TakeDamageFXClientRpc()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    [ClientRpc]
    public void CancelFXClientRpc()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    [ClientRpc]
    public void DeathFXClientRPC()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }
}
