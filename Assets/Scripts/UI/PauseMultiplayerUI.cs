using System;
using UnityEngine;

public class PauseMultiplayerUI : IUIElement
{
    private void Start()
    {
        GameManager.INSTANCE.OnMultiplayerGamePaused += GameManager_OnMultiplayerGamePaused;
        GameManager.INSTANCE.OnMultiplayerGameUnpaused += GameManager_OnMultiplayerGameUnpaused;
        Hide();
    }

    private void GameManager_OnMultiplayerGameUnpaused(object sender, EventArgs e)
    {
        //Cursor.lockState = CursorLockMode.Locked;
        Hide();
    }

    private void GameManager_OnMultiplayerGamePaused(object sender, EventArgs e)
    {
        //Cursor.lockState = CursorLockMode.None;
        Show();
    }
}
