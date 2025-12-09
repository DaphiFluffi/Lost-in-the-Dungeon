using System;
using TMPro;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{

    [SerializeField] private int playerCharacterIndex;
    [SerializeField] private GameObject txtPlayerName;
    [SerializeField] private TextMeshProUGUI txtPlayerReady;

    private void Start()
    {
        GameMultiplayer.INSTANCE.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.INSTANCE.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
        UpdatePlayerCharacter();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayerCharacter();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayerCharacter();
    }

    private void UpdatePlayerCharacter()
    {
        if(GameMultiplayer.INSTANCE.IsPlayerCharacterIndexConnected(playerCharacterIndex))
        {
            ShowCharacterOnConnect();
            MultiplayerPlayerData playerData = GameMultiplayer.INSTANCE.GetPlayerDataFromPlayerIndex(playerCharacterIndex);

            if(CharacterSelectReady.INSTANCE.IsPlayerReady(playerData.clientID))
            {
                SelectPlayerReady();
            }
            else
            {
                SelectPlayerNotReady();
            }
        }
        else
        {
            HideCharacterOnDisconnect();
        }
    }

    private void ShowCharacterOnConnect()
    {
        gameObject.SetActive(true);
        txtPlayerName.SetActive(true);
        txtPlayerReady.gameObject.SetActive(true);
    }

    private void HideCharacterOnDisconnect()
    {
        gameObject.SetActive(false);
        txtPlayerName.SetActive(false);
        txtPlayerReady.gameObject.SetActive(false);
    }

    private void SelectPlayerReady()
    {
        txtPlayerReady.text = "READY";
        txtPlayerReady.color = Color.green;
    }

    private void SelectPlayerNotReady()
    {
        txtPlayerReady.text = "NOT READY";
        txtPlayerReady.color = Color.red;
    }
}
