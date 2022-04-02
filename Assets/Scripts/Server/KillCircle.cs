using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KillCircle : NetworkBehaviour
{

    [SerializeField]
    private Transform m_CircleTransform;

    [SerializeField]
    private NetworkVariable<float> m_CircleRadius;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Starting");
            StartCoroutine(ScaleCircle());
        }
    }

    IEnumerator ScaleCircle()
    {
        //yield return new WaitForSeconds(30);
        while (true)
        {
            yield return new WaitForSeconds(0.001f);
            UpdateCircleRadiusServerRpc(0.0000005f);
            if (m_CircleTransform.localScale.magnitude <= 0.008)
            {
                yield break;
            }
        }
    }

    private void OnEnable()
    {
        m_CircleRadius.OnValueChanged += OnCircleRadiusChanged;
    }

    private void OnDisable()
    {
        m_CircleRadius.OnValueChanged -= OnCircleRadiusChanged;
    }
    private void OnCircleRadiusChanged(float oldValue, float newValue)
    {
        m_CircleTransform.localScale = Vector3.one * m_CircleRadius.Value;
    }

    [ServerRpc]
    void UpdateCircleRadiusServerRpc(float radiusChange)
    {
        m_CircleRadius.Value -= radiusChange;
    }
}
