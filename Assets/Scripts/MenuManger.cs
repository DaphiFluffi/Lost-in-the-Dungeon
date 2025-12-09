using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManger : MonoBehaviour
{
    // Quit the game on ESC
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    // Pressing Start means we go to the next scene
    public void StartGame()
    {
        // Load Scene 1
        Loader.Load(Loader.Scene.LobbyScene);
    }

    public void ShowInstructions()
    {
        //TODO: Show instructions
    }

    public void QuitGame()
    {
        // Quit the game
        Application.Quit();
    }

    public void ShowSettings()
    { 
        //TODO: Show settings
    }

    private void Start()
    {
        GameObject Instructions = GameObject.Find("InstructionsUI");
        GameObject Settings = GameObject.Find("SettingsUI");

        Debug.Log("Finded items:" + Instructions + ", " + Settings);
        Instructions.SetActive(false);
        Settings.SetActive(false);
    }
}
