using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Class containing references to UI children that we can display. Both are disabled by default on prefab.
/// </summary>
public class UIStateDisplay : MonoBehaviour
{

    [SerializeField]
    UIHealth m_UIHealth;

    [SerializeField]
    GameObject m_UIDeath;

    public void DisplayHealth(NetworkVariable<int> networkedHealth, int maxValue)
    {
        m_UIHealth.gameObject.SetActive(true);
        m_UIHealth.Initialize(networkedHealth, maxValue);
    }

    public void DisplayDeath()
    {
        m_UIDeath.SetActive(true);
    }

    public void HideHealth()
    {
        m_UIHealth.gameObject.SetActive(false);
    }
}
