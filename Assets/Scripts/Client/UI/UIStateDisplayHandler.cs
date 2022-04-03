using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

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

    Transform m_CanvasTransform;

    void Update()
    {

        if (m_CanvasTransform != null) return;
        
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        if (sceneName != "Game3") return;

        var canvasGameObject = GameObject.FindWithTag("GameCanvas");
        if (canvasGameObject)
        {
            m_CanvasTransform = canvasGameObject.transform;
        }

        Assert.IsNotNull(m_NetworkHealthState, "A NetworkHealthState component needs to be attached!");
       
        Assert.IsTrue(m_CanvasTransform != null);

        DisplayUIHealth();
    }

    public void DisplayUIDeath()
    {
        m_UIState.HideHealth();
        m_UIState.DisplayDeath();
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
        Debug.Log("SPAWNUISTATE");
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
