using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : IUIElement
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        createPublicButton.onClick.AddListener(() =>
        {
            GameLobby.INSTANCE.CreateLobby(false, lobbyNameInputField.text);
        });

        createPrivateButton.onClick.AddListener(() =>
        {
            GameLobby.INSTANCE.CreateLobby(true, lobbyNameInputField.text);
        });

       closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }
}
