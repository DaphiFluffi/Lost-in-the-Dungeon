using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetwokManagerUI : IUIElement
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            Hide();
        });

        hostBtn.onClick.AddListener(() =>
        {
            GameMultiplayer.INSTANCE.StartHost();
            Hide();
        });

        clientBtn.onClick.AddListener(() =>
        {
            GameMultiplayer.INSTANCE.StartClient();
            Hide();
        });
    }
}
