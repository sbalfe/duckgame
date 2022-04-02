using DapperDino.UMT.Lobby.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Cl.UI
{
    public class LobbyUI : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;
        [SerializeField] private Button startGameButton;

        private NetworkList<LobbyPlayerState> lobbyPlayers;

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        /* When the lobby spawns*/
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                /* new item added to the network list we attach the callback*/
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
            }

            if (IsServer)
            {
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
            if (!playerData.HasValue) { return; }

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
            /* If the sender is not the client host */
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            /* checks if everyone has readied up */
            if (!IsEveryoneReady()) { return; }

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

        /* when a new client connected*/
        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
        {
            /* update display for when new player joins*/
            for (int i = 0; i < lobbyPlayerCards.Length; i++)
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
