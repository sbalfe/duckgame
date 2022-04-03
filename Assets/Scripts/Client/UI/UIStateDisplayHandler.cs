using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class UIStateDisplayHandler : NetworkBehaviour
{
    [SerializeField]
    bool m_DisplayHealth;

    [SerializeField]
    UIStateDisplay m_UIStatePrefab;

    // spawned in world (only one instance of this)
    UIStateDisplay m_UIState;

    [SerializeField]
    NetworkHealthState m_NetworkHealthState;

    Camera m_Camera;

    Transform m_CanvasTransform;

    void Start()
    {
        m_Camera = Camera.main;
        var canvasGameObject = GameObject.FindWithTag("GameCanvas");
        if (canvasGameObject)
        {
            m_CanvasTransform = canvasGameObject.transform;
        }

        Assert.IsNotNull(m_NetworkHealthState, "A NetworkHealthState component needs to be attached!");
       
        Assert.IsTrue(m_Camera != null && m_CanvasTransform != null);

        DisplayUIHealth();
    }

    void DisplayUIHealth()
    {
        if (m_NetworkHealthState == null)
        {
            return;
        }

        if (m_UIState == null)
        {
            SpawnUIState();
        }

        m_UIState.DisplayHealth(m_NetworkHealthState.m_HitPoints, m_NetworkHealthState.BaseHP);
    }

    void SpawnUIState()
    {
        if (IsOwner)
        {
            m_UIState = Instantiate(m_UIStatePrefab, m_CanvasTransform);
            // make in world UI state draw under other UI elements
            m_UIState.transform.SetAsFirstSibling();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (m_UIState != null)
        {
            Destroy(m_UIState.gameObject);
        }
    }
}
