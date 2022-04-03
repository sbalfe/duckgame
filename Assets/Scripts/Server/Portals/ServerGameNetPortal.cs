using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server.Portals
{
    public class ServerGameNetPortal : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxPlayers = 4;

        public static ServerGameNetPortal Instance => instance;
        private static ServerGameNetPortal instance;

        /*Use the unique game identifier to identify the player data to locate */
        private Dictionary<string, PlayerData> clientData;

        /* Associate each local client ID to a unique Game identifier */
        private Dictionary<ulong, string> clientIdToGuid;

        /* store the ID > Map relation here */
        private Dictionary<ulong, int> clientSceneMap;

        /* Store whether the game is running here */
        private bool gameInProgress;

        /* Limit the connection payload limit */
        private const int MaxConnectionPayload = 1024;

        private GameNetPortal gameNetPortal;

        /*When the server script loads up and all objects activated generate a new instance of it */
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /* Ran when the object containing this script is created */
        private void Start()
        {
          
            /* Obtain the Game Net Portal script */
            gameNetPortal = GetComponent<GameNetPortal>();

            /* Attach a readied callback for when this game net portal server has started.*/
            gameNetPortal.OnNetworkReadied += HandleNetworkReadied;

            /* Add approval checks and server callbacks*/
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

            /* Init the server state for players joining */
            clientData = new Dictionary<string, PlayerData>();
            clientIdToGuid = new Dictionary<ulong, string>();
            clientSceneMap = new Dictionary<ulong, int>();
        }

        /* Server object destroyed */
        private void OnDestroy()
        {
            /* if there was no game net portal, return*/
            if (gameNetPortal == null) { return; }

            /* disconnect callback for readying network*/
            gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;

            /*If network manager singleton exists then remove approval checks and server starting callbacks*/
            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        }

        /* Fetch data associated with a specific client*/
        public PlayerData? GetPlayerData(ulong clientId)
        {

            /*Obtain GUID from Client ID*/
            if (clientIdToGuid.TryGetValue(clientId, out string clientGuid))
            {
                /*Obtain data from GUID*/
                if (clientData.TryGetValue(clientGuid, out PlayerData playerData))
                {
                    return playerData;
                }
                else
                {
                    Debug.LogWarning($"No player data found for client id: {clientId}");
                }
            }
            else
            {
                Debug.LogWarning($"No client guid found for client id: {clientId}");
            }

            return null;
        }

        /* Call to begin a game instance*/
        public void StartGame()
        {
            Debug.Log("running start game and switching to game2");
            /* game running */
            gameInProgress = true;

            /* change scene to first scene all players load in */
            NetworkManager.Singleton.SceneManager.LoadScene("Game3", LoadSceneMode.Single);
        }

        /* End of game switch scene back to the lobby*/
        public void EndRound()
        {
            gameInProgress = false;

            NetworkManager.Singleton.SceneManager.LoadScene("Scene_Lobby", LoadSceneMode.Single);
        }

        /* When the game net portal has declared its started.*/
        private void HandleNetworkReadied()
        {
            
            /* If we are not a server we cannot run this function thus return*/
            if (!NetworkManager.Singleton.IsServer) { return; }
            
            Debug.Log("handle network readied, loading the lobby");

            /* Connect the callbacks for the game net portal and network manager */
            gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            gameNetPortal.OnClientSceneChanged += HandleClientSceneChanged;

            /* When the server game portal is ready load the lobby*/
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            Debug.Log(PlayerPrefs.GetString("joinCodeValue"));

            /* If we are the host*/
            if (NetworkManager.Singleton.IsHost)
            {
                /* Set the Host ID > to be in the scene lobby returns the index */
                clientSceneMap[NetworkManager.Singleton.LocalClientId] = SceneManager.GetActiveScene().buildIndex;
            }
        }

        /* Called when client disconnects*/
        private void HandleClientDisconnect(ulong clientId)
        {
            /* remove them from the client dictionary */
            clientSceneMap.Remove(clientId);

            /* Obtain the client guid via client id */
            if (clientIdToGuid.TryGetValue(clientId, out string guid))
            {
                /* remove this link too*/
                clientIdToGuid.Remove(clientId);

                /* if the client id stored here is the client id*/
                if (clientData[guid].ClientId == clientId)
                {
                    /* remove this guid from client data detaching guid from player data */
                    clientData.Remove(guid);
                }
            }

            /* if the client is the client is the currently executing client id */
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                /* disconnect all the callbacks for dealing with players */
                gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
                gameNetPortal.OnClientSceneChanged -= HandleClientSceneChanged;
            }
        }

        /* When the scene changed for client and said index, called when scene management changes */
        private void HandleClientSceneChanged(ulong clientId, int sceneIndex)
        {
            /* update that client33 to be in this index*/
            clientSceneMap[clientId] = sceneIndex;
        }

        /* User disconnected from the game, clear data on them and shutdown the network manager*/
        private void HandleUserDisconnectRequested()
        {

            /* Handle client disconnected */
            HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

            NetworkManager.Singleton.Shutdown();

            ClearData();

            /* they go back to the main menu*/
            SceneManager.LoadScene("MainMenu");
        }

        private void HandleServerStarted()
        {
            /* If we arent the host, we dont have access to this functionality*/
            if (!NetworkManager.Singleton.IsHost) { return; }

            /* generate unique GUID and player name*/
            var clientGuid = Guid.NewGuid().ToString();
            var playerName = PlayerPrefs.GetString("PlayerName", "Missing Name");

            /* add to client data, link unique GUIDE to new instance of the player data and the ID of network in that */
            clientData.Add(clientGuid, new PlayerData(playerName, NetworkManager.Singleton.LocalClientId));
            clientIdToGuid.Add(NetworkManager.Singleton.LocalClientId, clientGuid);
        }

        /* wipes all the values from the server, after ending the game basically*/
        private void ClearData()
        {
            clientData.Clear();
            clientIdToGuid.Clear();
            clientSceneMap.Clear();

            gameInProgress = false;
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
        
            
          
            if (connectionData.Length > MaxConnectionPayload)
            {
                Debug.Log("approval checker2");
                Debug.Log("Could send the data as the payload volume was too high");
                /* callback after connection approval with false values*/
                callback(false, 0, false, null, null);
                return;
            }
           
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
              
                callback(true, null, true, null, null);
                return;
            }
            
            /* fetch the connection data as a string */
            var payload = Encoding.UTF8.GetString(connectionData);

            /* obtain the connection payload object from the JSON*/
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            /* update connection status to a success */
            ConnectStatus gameReturnStatus = ConnectStatus.Success;

            /* update more status values depending on the result of things.*/
            if (gameInProgress)
            {
                gameReturnStatus = ConnectStatus.GameInProgress;
            }
            else if (clientData.Count >= maxPlayers)
            {
                gameReturnStatus = ConnectStatus.ServerFull;
            }

           
            /* if connection was okay */
            if (gameReturnStatus == ConnectStatus.Success)
            {
                /* assign the scene they wish to enter to which map they start in*/
                clientSceneMap[clientId] = connectionPayload.clientScene;

                /* Assign the GUID to the associated GUID they entered*/
                clientIdToGuid[clientId] = connectionPayload.clientGUID;

                /* set the client data to link to a new instance of data stored on the player */
                clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);

                gameNetPortal.ServerToClientConnectResult(clientId, gameReturnStatus);
                /* does not create any*/
           
                callback(true, null, true, null, null);
              
                
            }
            else
            {
                StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
            }
        }

        private IEnumerator WaitToDisconnectClient(ulong clientId, ConnectStatus reason)
        {
            /* disconnect client for the reason specified in the connect status based on the settings in the handshake*/
            gameNetPortal.ServerToClientSetDisconnectReason(clientId, reason);

            /* run constantly checking*/
            yield return new WaitForSeconds(0);

            /* kick*/
            KickClient(clientId);
        }

        /* fuck off*/
        private void KickClient(ulong clientId)
        {
            /* obtain a network object reference to specified client to remove*/
            NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            /* if we found a client*/

            /* literally despawn the obejct.*/
            if (networkObject == false)
            {
                networkObject.Despawn();
            }

            /* disconnect client from the manager */
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
}
