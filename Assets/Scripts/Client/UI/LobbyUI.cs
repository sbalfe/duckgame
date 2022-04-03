using Server.Portals;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class LobbyUI : NetworkBehaviour
    {
        [Header("References")]
        
        /* Array storing each of the lobby player cards*/
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;
        
        /* Button to control the starting the game*/
        [SerializeField] private Button startGameButton;

        /* Stores the players within the lobby in a specific state.*/
        private NetworkList<LobbyPlayerState> lobbyPlayers;

        [SerializeField] private TMP_Text joinCode;

        /* awake runs before spawn initial values */
        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        /* When the lobby spawns*/
        public override void OnNetworkSpawn()
        {
            
            
            /* If We are a client*/
            if (IsClient)
            {
                /* new item added to the network list we attach the callback*/
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
            }

            if (IsHost)
            {
          
                joinCode.text = PlayerPrefs.GetString("joinCodeValue");
                startGameButton.gameObject.SetActive(true);

                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

                foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            /* Lobby closed.*/
            lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;

            /* if there is a network singleton */
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
        }

        /* check everyone readied up before starting the game*/
        private bool IsEveryoneReady()
        {
            /* dont start if incorrect count */
            if (lobbyPlayers.Count < 2)
            {
                return false;
            }

            /* dont run if any players are not readied*/
            foreach (var player in lobbyPlayers)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        /* call when client connects */
        private void HandleClientConnected(ulong clientId)
        {
            /* obtain the player data from server game net portal*/
            var playerData = ServerGameNetPortal.Instance.GetPlayerData(clientId);

            /* If this client id happens to have no data associated, then we return*/
            if (!playerData.HasValue)
            {
               
                return;
            }

            /* Add to the lobby players a new instantiation of the lobby player state, which essentially is a user.*/
            lobbyPlayers.Add(new LobbyPlayerState(
                clientId,
                playerData.Value.PlayerName,
                false
            ));
        }

        /* when some client leaves*/
        private void HandleClientDisconnect(ulong clientId)
        {
            /* find and remove them from the list of players in the lobby*/
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == clientId)
                {
                    lobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }

        /* When the user clicks ready, it runs on all local objects thus ownership not important.*/
        [ServerRpc(RequireOwnership = false)]
        private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
           
            /* Go through our players */
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
           

                /* Locate which ID corresponds to the caller of this  */
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    lobbyPlayers[i] = new LobbyPlayerState(
                        lobbyPlayers[i].ClientId,
                        lobbyPlayers[i].PlayerName,
                        !lobbyPlayers[i].IsReady
                    );
                }
            }
        }

        /* Start game only accessible via the client host on each object thus ownership not required.*/
        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("start game RPC");
            /* If the sender is not the client host */
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }
            
            
            /* checks if everyone has readied up */
            if (!IsEveryoneReady())
            {
                Debug.Log("network object");
            }

            /* Start the game on the server */
            ServerGameNetPortal.Instance.StartGame();
        }

        public void OnLeaveClicked()
        {
            /* Player requests to leave the host */
            GameNetPortal.Instance.RequestDisconnect();
        }

        public void OnReadyClicked()
        {
           
            ToggleReadyServerRpc();
        }

        public void OnStartGameClicked()
        {
            StartGameServerRpc();
        }

        /* when a new client connected, we pass in the lobby player state as a callback*/
        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
        {
            Debug.Log("changed");
            /* update display for when new player joins*/
       
           
            for (var i = 0; i < lobbyPlayerCards.Length; i++)
            {
              
                
                if (lobbyPlayers.Count > i)
                {
                  
                    lobbyPlayerCards[i].UpdateDisplay(lobbyPlayers[i]);
                }
                else
                {
                    lobbyPlayerCards[i].DisableDisplay();
                }
            }

            /* if we are he host*/
            if (IsHost)
            {
                /* set the start game to be bound to this function*/
                startGameButton.interactable = IsEveryoneReady();

            }
        }


    }
}
