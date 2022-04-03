using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server.Portals
{

    /* Requires the game net portal to be made thus reqiured */
    [RequireComponent(typeof(GameNetPortal))]
    public class ClientGameNetPortal : MonoBehaviour
    {
        public static ClientGameNetPortal Instance => instance;
        private static ClientGameNetPortal instance;

        public DisconnectReason DisconnectReason { get; private set; } = new DisconnectReason();

        public event Action<ConnectStatus> OnConnectionFinished;

        public event Action OnNetworkTimedOut;

        private GameNetPortal gameNetPortal;

        /* waits for objects to be instantiated, called first as script is loaded*/
        private void Awake()
        {
            /* setups the instances before anything occurs and destroys the old objects */
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /* calls on first script frame*/
        private void Start()
        {
            /* Obtain game net portal*/
            gameNetPortal = GetComponent<GameNetPortal>();

            /* Attach required callbacks */
            gameNetPortal.OnNetworkReadied += HandleNetworkReadied;
            gameNetPortal.OnConnectionFinished += HandleConnectionFinished;
            gameNetPortal.OnDisconnectReasonReceived += HandleDisconnectReasonReceived;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        /* When script destroyed */
        private void OnDestroy()
        {

            /* if there was no game net portal instance*/
            if (gameNetPortal == null) { return; }

            /* disconnect callbacks */
            gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;
            gameNetPortal.OnConnectionFinished -= HandleConnectionFinished;
            gameNetPortal.OnDisconnectReasonReceived -= HandleDisconnectReasonReceived;

            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public async void StartClient()
        {
            Debug.Log("build index.0" + SceneManager.GetActiveScene().buildIndex );
            /* Convert object instantiation to JSON*/
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = Guid.NewGuid().ToString(),
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = PlayerPrefs.GetString("PlayerName", "Missing Name")
            });

            var joinCodeInput = PlayerPrefs.GetString("JoinCode");

            /* convert JSON to bytes */
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            /* attach this data to the network configuration*/
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput))
                await RelayManager.Instance.JoinRelay(joinCodeInput);

            if (NetworkManager.Singleton.StartClient())
                Debug.Log("test");
            else
                Debug.Log("test");
            
            /* start up a client instance*/
            NetworkManager.Singleton.StartClient();
        }

        /* handler for when the game net portal responds and connects*/
        private void HandleNetworkReadied()
        {
            
            /* if we are a pure server*/
            if (!NetworkManager.Singleton.IsClient) { return; }

            /* if we are a client */
            if (!NetworkManager.Singleton.IsHost)
            {
                /* attach disconnect request handler*/
                gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
            }
        }

        private void checkWin()
        {
            foreach (var item in NetworkManager.Singleton.ConnectedClients)
            {
                Debug.Log(item);
            }
        }

        private void HandleUserDisconnectRequested()
        {
            /* if user disconnects set disconnect reason*/
            DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);

            /* shutdown network manager locally of course*/
            NetworkManager.Singleton.Shutdown();

            /* Pass this into the client disconnection */
            HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

            /* load the main menu scene as they leave*/
            SceneManager.LoadScene("MainMenu");
        }

        /* When the user has already connected */
        private void HandleConnectionFinished(ConnectStatus status)
        {
            
            /* if status is a failure, we submit a reason based on this*/
            if (status != ConnectStatus.Success)
            {
                DisconnectReason.SetDisconnectReason(status);
            }

            /* otherwise invoke our connection finished callback */
            OnConnectionFinished?.Invoke(status);
        }

        private void HandleDisconnectReasonReceived(ConnectStatus status)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        private void HandleClientDisconnect(ulong clientId)
        {
           
            /* If we are not connected as a client and not a host */
            if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
            {
                /* detach user disconnect reasons*/
                gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;

                /* if the scene is not the main menu*/
                if (SceneManager.GetActiveScene().name != "MainMenu")
                {
                    /* if  connection is defined set it otherwise ignore */
                    if (!DisconnectReason.HasTransitionReason)
                    {
                        DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
                    }

                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    OnNetworkTimedOut?.Invoke(); /* no timeout callback bound currently*/
                }
            }
        }
    }
}
