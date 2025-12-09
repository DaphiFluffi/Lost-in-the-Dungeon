using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : IUIElement
{
    private void Start()
    {
        GameMultiplayer.INSTANCE.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.INSTANCE.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }
}
