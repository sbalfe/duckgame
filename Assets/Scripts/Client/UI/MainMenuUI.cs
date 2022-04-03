using Server.Portals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField displayNameInputField;

        [SerializeField] private TMP_InputField relayCodeInput;

        private void Start()
        {
            PlayerPrefs.GetString("PlayerName");
        }

        public void OnHostClicked()
        {
            PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
            GameNetPortal.Instance.StartHost();
        }

        public void OnClientClicked()
        {
            PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
            PlayerPrefs.SetString("JoinCode", relayCodeInput.text);
            ClientGameNetPortal.Instance.StartClient();
        }
    }
}