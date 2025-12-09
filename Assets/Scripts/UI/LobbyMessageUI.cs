using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : IUIElement
{


    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;


    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        GameMultiplayer.INSTANCE.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        GameLobby.INSTANCE.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.INSTANCE.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.INSTANCE.OnQuickJoinFailed += GameLobby_OnQuickJoinFailed;
        GameLobby.INSTANCE.OnJoinStarted += GameLobby_OnJoinStarted;
        GameLobby.INSTANCE.OnJoinFailed += GameLobby_OnJoinFailed;

        Hide();
    }

    private void GameLobby_OnJoinFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to join Lobby!");
    }

    private void GameLobby_OnJoinStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void GameLobby_OnQuickJoinFailed(object sender, EventArgs e)
    {
        ShowMessage("Could not find Lobby to quick join!");
    }

    private void GameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to create Lobby!");
    }

    private void GameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if(NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void OnDestroy()
    {
        GameMultiplayer.INSTANCE.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
        GameLobby.INSTANCE.OnCreateLobbyStarted -= GameLobby_OnCreateLobbyStarted;
        GameLobby.INSTANCE.OnCreateLobbyFailed -= GameLobby_OnCreateLobbyFailed;
        GameLobby.INSTANCE.OnQuickJoinFailed -= GameLobby_OnQuickJoinFailed;
        GameLobby.INSTANCE.OnJoinStarted -= GameLobby_OnJoinStarted;
        GameLobby.INSTANCE.OnJoinFailed -= GameLobby_OnJoinFailed;
    }

}