using shriller.Core.Singletons;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class jamesRelay :Singleton<jamesRelay>
{
    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TMP_InputField joinCodeInput;


    private void Awake()
    {
        Cursor.visible = true;
    }

    void Update()
    {
    }

    void Start()
    {
        Debug.Log("started listenting");

        // START HOST
        // ReSharper disable once Unity.NoNullPropagation
        startHostButton?.onClick.AddListener(async () =>
        {

            if (RelayManager.Instance.IsRelayEnabled)
                await RelayManager.Instance.SetupRelay();

            NetworkManager.Singleton.StartHost();
        });


        startClientButton?.onClick.AddListener(async () =>
        {

            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            if (NetworkManager.Singleton.StartClient())
                Debug.Log("test");
            else
                Debug.Log("test");
        });


    }

}