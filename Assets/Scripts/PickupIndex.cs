using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupIndex : MonoBehaviour
{
    [SerializeField]
    private int m_AvailableGunIndex;

    public int AvailableGunIndex => m_AvailableGunIndex;
}
