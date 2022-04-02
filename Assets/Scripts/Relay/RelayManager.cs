
using System.Threading.Tasks;
using DilmerGames.Core.Singletons;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : Singleton<RelayManager>
{

    /* production environment */
    [SerializeField]
    private string environment = "production";

    [SerializeField]
    private int maxNumberOfConnections = 10;

    /* check to see if we have relay enabled before accessing it.*/
    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    /* obtain the unity transport object */
    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    /* async function that returns a task of type relay host data which defines the data required to communicate over relay */
    public async Task<RelayHostData> SetupRelay()
    {

        /* setup our environment we are attempting to setup our options for.*/
        InitializationOptions options = new InitializationOptions().SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        /* allocate the data for the relay host data */
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

        RelayHostData relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        /* generate a join code for the relay instance */
        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

        /* set the relay server data */
        Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes,
                relayHostData.Key, relayHostData.ConnectionData);


        Debug.Log("game created. code: " + relayHostData.JoinCode);
        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {

        Debug.Log("join game");
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        /* Allocate data and generate the join code.*/
        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        RelayJoinData relayJoinData = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4,
            JoinCode = joinCode
        };

        Transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes,
            relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

        return relayJoinData;
    }
}

