using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    [SerializeField]
    NetworkHealthState m_NetworkHealthState;

    [SerializeField] private ClientSoundController soundController;

    public NetworkHealthState NetworkHealth => m_NetworkHealthState;

    [SerializeField]
    private ClientCharacterVisualization m_CharacterVisualization;

    public ClientCharacterVisualization CharacterVisualization => m_CharacterVisualization;

    private NetworkAnimator networkAnimator;

    [SerializeField]
    private UIStateDisplayHandler m_NetworkStateDisplayHandler;

    // Start is called before the first frame update
    void Start()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        Debug.Log("scene changed");
        string currentName = next.name;
        Debug.Log("curr name" + currentName);

        if (currentName == "Game3")
        {
            if (IsOwner)
            {
                Debug.Log("Adding CameraController");
                gameObject.AddComponent<CameraController>();
                
            }  
        }
    }

    public override void OnNetworkSpawn()
    {
        NetworkHealth.IsAlive.OnValueChanged += OnLifeStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        NetworkHealth.IsAlive.OnValueChanged -= OnLifeStateChanged;
    }

    private void OnLifeStateChanged(bool prevLifeState, bool lifeState)
    {
        // If dead
        if (lifeState == false)
        {
            //SceneManager.LoadScene("EG", LoadSceneMode.Single);
            CharacterVisualization.DeathFXClientRPC();
            m_NetworkStateDisplayHandler.DisplayUIDeath();
            // Disable collider
            Collider2D m_Collider = GetComponent<Collider2D>();
            m_Collider.enabled = false;
        }
    }

   

    public void ReceiveHP(int HP)
    {

        if (!IsServer) return;

        // Receive Health
        if (HP > 0)
        {
            NetworkHealth.HitPoints = NetworkHealth.HitPoints + HP;
            Debug.Log("Receiving health");
            // Do a health effect?
        }
        // Take damage
        else
        {
            NetworkHealth.HitPoints = NetworkHealth.HitPoints + HP;
            // Do a damage effect?
            StartCoroutine(CharacterVisualization.TakeDamage());
        }

        // Stop health going below 0 and going above max health
        NetworkHealth.HitPoints = Mathf.Clamp(NetworkHealth.HitPoints, 0, NetworkHealth.BaseHP);
        Debug.Log(NetworkHealth.HitPoints);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (NetworkHealth.IsAlive.Value == false) return;
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Bullet hit");
            ReceiveHP(-5);
            CharacterVisualization.TakeDamageFXClientRpc();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (!NetworkHealth.IsAlive.Value) return;

        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        
        if (horizontalInput != 0)
        {
            transform.Translate(new Vector3(horizontalInput * Time.deltaTime * moveSpeed, 0, 0));
        }

        if (verticalInput != 0)
        {
            transform.Translate(new Vector3(0, verticalInput * Time.deltaTime * moveSpeed, 0));
        }
        
        SetAnimationParametersClientRpc(horizontalInput, verticalInput);
        SetAnimationParametersServerRpc(horizontalInput, verticalInput);
        networkAnimator.Animator.SetFloat("WalkDirectionX", horizontalInput);
        networkAnimator.Animator.SetFloat("WalkDirectionY", verticalInput);
    }

    [ClientRpc]
    public void SetAnimationParametersClientRpc(float horizontalInput, float verticalInput)
    {
        networkAnimator.Animator.SetFloat("WalkDirectionX", horizontalInput);
        networkAnimator.Animator.SetFloat("WalkDirectionY", verticalInput);
    }
    
    [ServerRpc]
    public void SetAnimationParametersServerRpc(float horizontalInput, float verticalInput)
    {
        networkAnimator.Animator.SetFloat("WalkDirectionX", horizontalInput);
        networkAnimator.Animator.SetFloat("WalkDirectionY", verticalInput);
    }
}
