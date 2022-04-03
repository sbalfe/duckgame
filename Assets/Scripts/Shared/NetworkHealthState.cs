using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkHealthState : NetworkBehaviour
{
    [SerializeField]
    private int m_BaseHP;

    public int BaseHP => m_BaseHP;

    [HideInInspector]
    public NetworkVariable<int> m_HitPoints = new NetworkVariable<int>();

    public int HitPoints
    {
        get { return m_HitPoints.Value; }
        set { m_HitPoints.Value = value; }
    }

    private NetworkVariable<bool> m_IsAlive = new NetworkVariable<bool>(true);

    public NetworkVariable<bool> IsAlive => m_IsAlive;

    void OnEnable()
    {
        HitPoints = BaseHP;
        m_HitPoints.OnValueChanged += HitPointsChanged;
    }

    void OnDisable()
    {
        m_HitPoints.OnValueChanged -= HitPointsChanged;
    }

    public void HitPointsChanged(int previousValue, int newValue)
    {
        if (previousValue > 0 && newValue <= 0)
        {
            // newly reached 0 HP
            HitPointsDepletedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership=false)]
    public void HitPointsDepletedServerRpc()
    {
        Debug.Log("DEADl");
        m_IsAlive.Value = false;
    }
}
