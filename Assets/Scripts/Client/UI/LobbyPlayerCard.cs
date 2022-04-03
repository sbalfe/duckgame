using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class LobbyPlayerCard : MonoBehaviour
    {
        
        [SerializeField] private TMP_Text playerDisplayNameText;
        [SerializeField] private Toggle isReadyToggle;

        public void Start()
        {
            isReadyToggle.interactable = false;
        }

        public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
        {
            Debug.Log("update display");
            playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
            isReadyToggle.isOn = lobbyPlayerState.IsReady;
         
        }

        public void DisableDisplay()
        {
            
        }
    }
}
