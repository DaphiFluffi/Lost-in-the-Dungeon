using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForPlayerUI : IUIElement
{
    private void Start()
    {
        GameManager.INSTANCE.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.INSTANCE.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.INSTANCE.IsGamePlaying())
        {
            Hide();
        }
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.INSTANCE.IsLocalPlayerReady())
        {
            Show();
        }
    }
}
