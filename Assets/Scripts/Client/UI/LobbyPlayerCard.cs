using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class LobbyPlayerCard : MonoBehaviour
    {
     
        [SerializeField] private GameObject playerDataPanel;
        
        [SerializeField] private TMP_Text playerDisplayNameText;
        [SerializeField] private Toggle isReadyToggle;

        public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
        {
            playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
            isReadyToggle.isOn = lobbyPlayerState.IsReady;
            playerDataPanel.SetActive(true);
        }

        public void DisableDisplay()
        {
            playerDataPanel.SetActive(false);
        }
    }
}
