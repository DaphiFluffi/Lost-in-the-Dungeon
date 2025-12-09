using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;
    private GameObject _localPlayer;

    private void Start()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Subscribe to the OnClientConnectedCallback event
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnect;

        // Check if any clients are already connected (in case this script starts after some clients are connected)
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            OnPlayerConnect(clientId);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnClientConnectedCallback event to prevent memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnect;
        }
    }

    private void OnPlayerConnect(ulong clientId)
    {
        Debug.Log("param clientID:" + clientId + " NetworkId:" + NetworkManager.Singleton.LocalClientId);

        // Check if this is the local player
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            StartCoroutine(AssignCameraToLocalPlayer());
        }
    }

    private IEnumerator AssignCameraToLocalPlayer()
    {
        _localPlayer = FindLocalPlayer();

        while (_localPlayer == null)
        {
            yield return new WaitForSeconds(0.5f);
            _localPlayer = FindLocalPlayer();
        }

        _virtualCamera.Follow = _localPlayer.transform;
        _virtualCamera.LookAt = _localPlayer.transform;
    }

    private GameObject FindLocalPlayer()
    {
        // This method should be implemented based on how your player prefab is instantiated.
        // Here, we assume that each player has a PlayerController script, which identifies the player.
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            Debug.Log("Is player local: " + player.IsLocalPlayer);

            if (player.IsLocalPlayer)
            {
                return player.gameObject;
            }
        }

        return null;
    }

    public GameObject GetLocalPlayer()
    {
        return _localPlayer;
    }
}
