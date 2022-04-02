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

        /* When script destroy */
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

        public void StartClient()
        {
            /* Convert object instantiation to JSON*/
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = Guid.NewGuid().ToString(),
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = PlayerPrefs.GetString("PlayerName", "Missing Name")
            });

            /* convert JSON to bytes */
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            /* attach this data to the network configuration*/
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            /* start up a client instance*/
            NetworkManager.Singleton.StartClient();
        }

        /* handdler for when the game net portal responds and connects*/
        private void HandleNetworkReadied()
        {
            /* if we are a pure server*/
            if (!NetworkManager.Singleton.IsClient) { return; }

            /* if we are a client*/
            if (!NetworkManager.Singleton.IsHost)
            {
                /* attach disconnect request handler*/
                gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
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

            /* load the main menu scen as they leave*/
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
                if (SceneManager.GetActiveScene().name != "Scene_Menu")
                {
                    /* if  connection is defined set it otherwise ignore */
                    if (!DisconnectReason.HasTransitionReason)
                    {
                        DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
                    }

                    SceneManager.LoadScene("Scene_Menu");
                }
                else
                {
                    OnNetworkTimedOut?.Invoke(); /* no timeout callback bound currently*/
                }
            }
        }
    }
}
