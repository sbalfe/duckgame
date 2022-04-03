using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server.Portals
{
    public class GameNetPortal : MonoBehaviour
    {
        public static GameNetPortal Instance => instance;
        private static GameNetPortal instance;

        /* Custom actions to bind our own invoke methods on */
        public event Action OnNetworkReadied;
        public event Action<ConnectStatus> OnConnectionFinished;
        public event Action<ConnectStatus> OnDisconnectReasonReceived;

        public event Action<ulong, int> OnClientSceneChanged;

        public event Action OnUserDisconnectRequested;

        private void Awake()
        {
            /* Called to initialise values before the script is run and created after all objects are created*/

            /* If the instance of this portal is not empty or itself */
            if (instance != null && instance != this)
            {
                /* Destroy the game object , which wipes the object */
                Destroy(gameObject);
                return;
            }

            /* Set new instance to this game object*/
            instance = this;

            /* Preserve the object during level loading (scene transistion) */
            DontDestroyOnLoad(gameObject);
        }

        /* When Game Net Portal starts script*/
        private void Start()
        {

            /* On server started > attach the handlers*/
            NetworkManager.Singleton.OnServerStarted += HandleNetworkReady;

            /* Attach handlers for when the client joins*/
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        private void displayWinner()
        {
            
        }

        /* When this script is destroyed*/
        private void OnDestroy()
        {
            
            /* Check if the network manager exists */
            if (NetworkManager.Singleton != null)
            {
                /* Disconnect callbacks */
                NetworkManager.Singleton.OnServerStarted -= HandleNetworkReady;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;

                /* Check for scene manager too and disconnected its specific event*/
                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
                }

                /* if there is no custom messagin manager then exit*/
                if (NetworkManager.Singleton.CustomMessagingManager == null) { return; }

                UnregisterClientMessageHandlers();
            }
        }

        /* Call to start a host and register the client message handlers*/
        public async void StartHost()
        
        {
            
            if (RelayManager.Instance.IsRelayEnabled)
            {

                await RelayManager.Instance.SetupRelay();
            }

            NetworkManager.Singleton.StartHost();

            RegisterClientMessageHandlers();
        }

        public void RequestDisconnect()
        {
            /* Bound a callback for when user disconnects*/
            OnUserDisconnectRequested?.Invoke();
        }

        /* When client connects , fetch the ID*/
        private void HandleClientConnected(ulong clientId)
        {
            /* If the client is not the local client ID (the server) we ignore this*/
            if (clientId != NetworkManager.Singleton.LocalClientId) { return; }

            /*Call network ready as the client to say the client has connected all good*/
            HandleNetworkReady();

            /* Attach the handle scene event for this client */
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }

        /* Callback for any scene change in the netcode */
        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            /* If the scene has not loaded yet then return*/
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            /* invoke the manager for client scene changing, passing in the ID, and the specific scene name*/
            OnClientSceneChanged?.Invoke(sceneEvent.ClientId, SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
        }

        /* When the server starts up */
        private void HandleNetworkReady()
        {
            
          
            /* Are we a host */
            if (NetworkManager.Singleton.IsHost)
            {
              
                /* Connection finished invoke with succesful status*/
                OnConnectionFinished?.Invoke(ConnectStatus.Success);
            }

            /* Users can run the method on this function at this point. */

            /* This callback binds to when the network is ready to service the client */
            OnNetworkReadied?.Invoke();
        }

        #region Message Handlers

        private void RegisterClientMessageHandlers()
        {
            /* create a custom named message to read the connect status*/
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out ConnectStatus status);

                /* connection started giving the status back */
                OnConnectionFinished?.Invoke(status);
            });

            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out ConnectStatus status);

                /* disconnected with status saying why*/
                OnDisconnectReasonReceived?.Invoke(status);
            });
        }

        private void UnregisterClientMessageHandlers()
        {
            /* Unregister  */
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
        }

        #endregion

        #region Message Senders

        public void ServerToClientConnectResult(ulong netId, ConnectStatus status)
        {
            /* create a buffer writer */
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);

            /* write status to this */
            writer.WriteValueSafe(status);

            /* send this as a named message to our custom messaging client*/
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, writer);
        }


        /* Send disconnect reason from server to client */
        public void ServerToClientSetDisconnectReason(ulong netId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId, writer);
        }

        #endregion
    }
}
