using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer INSTANCE {  get; private set; }
    
    public const int MAX_PLAYER_AMOUNT = 2;
    private NetworkList<MultiplayerPlayerData> playerDataNetworkList;
    public List<Vector3> spawnPlayerPositions;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    private void Awake()
    {
        INSTANCE = this;
        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<MultiplayerPlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<MultiplayerPlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
        //GameManager.INSTANCE.InitGameManager();
    }



    private void Network_Server_OnClientDisconnectCallback(ulong clientID)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            MultiplayerPlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientID == clientID)
            {
                //this one disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientID)
    {
        playerDataNetworkList.Add(new MultiplayerPlayerData
        {
            clientID = clientID,
        });

    }


    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(Loader.GetActiveSceneName() != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "The selected game has already started.";
            return;
        }

        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "The selected game is full.";
            return;
        }


        response.Approved = true;
        /* NOT NEEDED FOR NOW
        if(GameManager.INSTANCE.IsWaitingToStart())
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            response.Approved = false;
        }*/
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
        //GameManager.INSTANCE.InitGameManager();
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientID)
    {
       // OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerCharacterIndexConnected(int playerCharacterIndex)
    {
        return playerCharacterIndex < playerDataNetworkList.Count;
    }

    public MultiplayerPlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public int GetPlayerDataIndexFromClientID(ulong clientID)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++) 
        {
            if (playerDataNetworkList[i].clientID == clientID)
            {
                return i;
            }
        }

        return -1;
    }

}
