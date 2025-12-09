using TMPro;
using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager INSTANCE { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public LevelManager levelManager;
    public GameObject Target1;
    public GameObject Target2;

    private GameObject Mirror1;
    private GameObject Mirror2;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private FreeMoveCamera freeMoveCam;
    [SerializeField] private CameraController playerCam;
    [SerializeField] private Camera mainCam;
    private bool _isFreeMoveCamActive = false;

    private bool _animationsPaused = false;

    private enum gameState
    {
        WaitingToStart,
        ReadyToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<gameState> state = new NetworkVariable<gameState>(gameState.WaitingToStart);
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private bool _isLocalGamePaused = false;
    private bool _isLocalPlayerReady;
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool _autoTestGamePauseState;

    private void Awake()
    {
        INSTANCE = this;

        // find all objects tagged mirrorobj
        GameObject[] mirrors = GameObject.FindGameObjectsWithTag("MirrorObj");
        Mirror1 = mirrors[0];
        Mirror2 = mirrors[1];
        levelManager = FindObjectOfType<LevelManager>();
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab);

            // Determine the spawn position for the player based on client ID
            int playerIndex = GameMultiplayer.INSTANCE.GetPlayerDataIndexFromClientID(clientId);
            Vector3 spawnPos = GameMultiplayer.INSTANCE.spawnPlayerPositions[playerIndex];

            // Set the player's position before spawning it as a player object
            player.transform.position = spawnPos;

            // Finally, spawn the player object with the correct position
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        _autoTestGamePauseState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (isGamePaused.Value)
        {
            if (!_animationsPaused)
                Time.timeScale = 0f;
            if (!_isLocalGamePaused)
                OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            if (!_isLocalGamePaused)
                OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(gameState previousValue, gameState newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void InitGameManager()
    {
        if (state.Value == gameState.WaitingToStart)
        {
            _isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("clientID " + clientId);

            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // This player is NOT ready
                allClientsReady = false;
                Debug.Log("allClientsReady " + allClientsReady);
                break;
            }
        }

        if (allClientsReady)
        {
            state.Value = gameState.ReadyToStart;
        }
    }

    public bool IsGameOver()
    {
        return state.Value == gameState.GameOver;
    }

    public bool IsGamePlaying()
    {
        return state.Value == gameState.GamePlaying;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == gameState.WaitingToStart;
    }

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }

    public void TogglePauseGame()
    {
        _isLocalGamePaused = !_isLocalGamePaused;
        if (_isLocalGamePaused)
        {
            PauseGameServerRpc();

            if (!_animationsPaused)
                OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePausedState();
    }

    /// <summary>
    /// Checking if player paused the game and then left
    /// LateUpdate cause pausing while disconnecting will still show both players
    /// we need to wait one frame at least for the player to really disconnect
    /// </summary>
    private void LateUpdate()
    {
        if (_autoTestGamePauseState)
        {
            _autoTestGamePauseState = false;
            TestGamePausedState();
        }
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                //This player paused the game
                isGamePaused.Value = true;
                return;
            }
        }

        //The other player is unpaused
        isGamePaused.Value = false;
    }

    [SerializeField] TextMeshProUGUI frameRateText;
    private bool _isFrameRateShown = false;

    // Start is called before the first frame update
    void Start()
    {
        frameRateText.enabled = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

        // Find all objects with the InteractableItem component
        InteractableObject[] interactableItems = FindObjectsOfType<InteractableObject>();
        int reflections = 0;

        // Log each interactable item found
        foreach (InteractableObject item in interactableItems)
        {
            if (item.GetIsBeamOnMirror())
                reflections++;

            item.SetIsBeamOnMirror(false);
        }

        Debug.Log("ref:" + reflections);
        if (reflections == 2 && Target1.GetComponent<Renderer>().material.color == Color.magenta)
        {
            SetLevel1DoneServerRpc();
        }

        if (reflections == 2 && Target2.GetComponent<Renderer>().material.color == Color.yellow)
        {
            levelManager.SetPlayerWonLevel2ServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseGame();

        if (Input.GetKeyDown(KeyCode.I))
        {
            _isFrameRateShown = !_isFrameRateShown;
            frameRateText.enabled = _isFrameRateShown;
        }

        if (_isFrameRateShown)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            frameRateText.text = "FPS: " + fps.ToString();
        }

        // Toggle free moving camera with mouse and WASD controls
        if (Input.GetKeyDown(KeyCode.U))
        {
            SwitchCamera();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleAnimations();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            SetLevel1DoneServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            levelManager.SetPlayerWonLevel2ServerRpc();
        }

        if (!IsServer) return;

        switch (state.Value)
        {
            case gameState.WaitingToStart:
                break;
            case gameState.ReadyToStart:
                state.Value = gameState.GamePlaying;
                break;
            case gameState.GameOver:
                break;
        }
    }

    private void SwitchCamera()
    {
        if (!_isFreeMoveCamActive)
        {
            _isFreeMoveCamActive = true;
            mainCam.orthographic = false;
            freeMoveCam.gameObject.SetActive(true);
            playerCam.GetLocalPlayer().GetComponent<PlayerController>().enabled = false;
            playerCam.gameObject.SetActive(false);
        }
        else
        {
            _isFreeMoveCamActive = false;
            mainCam.orthographic = true;
            playerCam.gameObject.SetActive(true);

            if (!_animationsPaused)
                playerCam.GetLocalPlayer().GetComponent<PlayerController>().enabled = true;

            freeMoveCam.gameObject.SetActive(false);
        }
    }

    private void ToggleAnimations()
    {
        Animator[] animators = FindObjectsOfType<Animator>();

        _animationsPaused = !_animationsPaused;
        TogglePauseGame();

        foreach (Animator animator in animators)
        {
            animator.enabled = !_animationsPaused;
        }

        if (!_isFreeMoveCamActive)
            playerCam.GetLocalPlayer().GetComponent<PlayerController>().enabled = !_animationsPaused;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLevel1DoneServerRpc()
    {
        SetLevel1DoneClientRpc();
        SetLevel1Done();
    }

    [ClientRpc]
    private void SetLevel1DoneClientRpc()
    {
        SetLevel1Done();
    }

    private void SetLevel1Done()
    {
        Target1.name = "Target1";
        Target2.name = "Target";
        Target1.GetComponent<TargetScript>().ChangeColor("white");

        Target2.GetComponent<TargetScript>().ChangeColor("white");

        Mirror1.transform.position = new Vector3(12.4899998f, -1f, 10.04f);
        Mirror2.transform.position = new Vector3(16f, -1f, 9.93000031f);
        Mirror1.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        Mirror2.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        levelManager.SetPlayerWonLevel1ServerRpc();
    }
}
