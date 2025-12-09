using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if(NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if(GameMultiplayer.INSTANCE != null)
        {
            Destroy(GameMultiplayer.INSTANCE.gameObject);
        }
    }
}
