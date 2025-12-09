using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public NetworkVariable<bool> playerWonLevel1 = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> playerWonLevel2 = new NetworkVariable<bool>(false);

    public GameObject chestLevel1;
    public GameObject doorLevel1;
    public GameObject greenFlashlightForPlayer2;
    public GameObject chestLevel2;
    public GameObject doorLevel2;
    public GameObject hammer;
    public GameObject navMesh;

    private void Awake()
    {
        doorLevel1.GetComponent<Animator>().enabled = false;
        doorLevel2.GetComponent<Animator>().enabled = false;
    }

    void Start()
    {
        chestLevel1.SetActive(false);
        chestLevel2.SetActive(false);
        hammer.SetActive(false);
        navMesh.SetActive(false);

        // Register to listen to changes on the NetworkVariables
        playerWonLevel1.OnValueChanged += OnPlayerWonLevel1Changed;
        playerWonLevel2.OnValueChanged += OnPlayerWonLevel2Changed;
    }

    void OnDestroy()
    {
        // Unregister the listeners when the object is destroyed
        playerWonLevel1.OnValueChanged -= OnPlayerWonLevel1Changed;
        playerWonLevel2.OnValueChanged -= OnPlayerWonLevel2Changed;
    }

    private void OnPlayerWonLevel1Changed(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Player won level 1");
            chestLevel1.SetActive(true);
            greenFlashlightForPlayer2.transform.position = new Vector3(greenFlashlightForPlayer2.transform.position.x, 0.0f, greenFlashlightForPlayer2.transform.position.z);
            doorLevel1.GetComponent<Animator>().enabled = true;
            navMesh.SetActive(true);
        }
    }

    private void OnPlayerWonLevel2Changed(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Player won level 2");
            chestLevel2.SetActive(true);
            doorLevel2.GetComponent<Animator>().enabled = true;
            hammer.SetActive(true);
        }
    }

    // Server-side method to set the win conditions
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerWonLevel1ServerRpc()
    {
        playerWonLevel1.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerWonLevel2ServerRpc()
    {
        playerWonLevel2.Value = true;
    }
}
