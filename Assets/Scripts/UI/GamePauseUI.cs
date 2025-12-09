using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : IUIElement
{

    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {

        mainMenuButton.onClick.AddListener(() => {
            //This also needs to be called in gameOver UI, when we will have SOME 
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        
    }

    private void Start()
    {
        GameManager.INSTANCE.OnLocalGamePaused += GameManager_OnLocalGamePaused;
        GameManager.INSTANCE.OnLocalGameUnpaused += GameManager_OnLocalGameUnpaused;

        Hide();
    }

    private void GameManager_OnLocalGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLocalGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }
}