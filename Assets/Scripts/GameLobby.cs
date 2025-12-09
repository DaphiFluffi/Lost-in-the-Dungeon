using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class GameLobby : MonoBehaviour
{
    public static GameLobby INSTANCE { get; private set; }
    public const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";


    private Lobby _joinedLobby;
    private float _listLobbyTimer;
    private const float LIST_LOBBY_TIMER_MAX = 5f;
    private float _heartbeatTimer;
    // 20 seconds interval for sending the beats to keep the lobby alive
    private const float HEARTBEAT_TIMER_MAX = 20f;


    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;

    public event EventHandler<OnLobbyListChangeEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangeEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private void Awake()
    {
        INSTANCE = this;
        DontDestroyOnLoad(gameObject);
        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initOptions = new InitializationOptions();
            initOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); 
        }
    }

    private void Update()
    {
        KeepLobbyAlive();
        HandlePeriodicListLobbies();
    }

    private void HandlePeriodicListLobbies()
    {
        if(_joinedLobby == null && AuthenticationService.Instance.IsSignedIn &&
            SceneManager.GetActiveScene().name == Loader.Scene.LobbyScene.ToString())
        {
            _listLobbyTimer -= Time.deltaTime;
            if (_listLobbyTimer <= 0f)
            {
                _listLobbyTimer = LIST_LOBBY_TIMER_MAX;
                ListLobbies();
            }

        }
    }

    private void KeepLobbyAlive()
    {
        if(IsLobbyHost())
        {
            _heartbeatTimer -= Time.deltaTime;
            if( _heartbeatTimer <= 0f ) 
            {
                _heartbeatTimer = HEARTBEAT_TIMER_MAX;
                LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                // We want to filter only the lobbies that have space to join
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangeEventArgs
            {
                lobbyList = queryResponse.Results
            });
        } catch(LobbyServiceException lse)
        {
            Debug.LogError(lse);
        }
    }

    private bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.MAX_PLAYER_AMOUNT - 1);
            return alloc;

        } catch (RelayServiceException rse)
        {
            Debug.LogError(rse);
            return default;
        }
    }


    private async Task<string> GetRelayJoinCode(Allocation alloc)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            return relayJoinCode;

        } catch (RelayServiceException rse)
        {
            Debug.LogError(rse);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAlloc; 

        } catch (RelayServiceException rse)
        {
            Debug.LogError(rse);
            return default;
        }
    }

    public async void CreateLobby(bool isPrivate, string lobbyName = "Random lobby")
    {

        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions { IsPrivate = isPrivate }); ;
            
            Allocation allocation =  await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayer.INSTANCE.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException lse)
        {
            Debug.LogError(lse.Message);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }

    }

    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            
            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAlloc = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));

            GameMultiplayer.INSTANCE.StartClient();

        } catch(LobbyServiceException lse)
        {
            Debug.LogError(lse.Message);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAlloc = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));

            GameMultiplayer.INSTANCE.StartClient();
        } catch (LobbyServiceException lse) 
        {
            Debug.LogError(lse.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }

    }

    public async void JoinWithID(string lobbyID)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);

            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAlloc = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));

            GameMultiplayer.INSTANCE.StartClient();
        }
        catch (LobbyServiceException lse)
        {
            Debug.LogError(lse.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }

    }

    public Lobby GetLobby()
    {
        return _joinedLobby;
    }


    public async void DeleteLobby()
    {
        if(_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
                _joinedLobby = null;

            } catch(LobbyServiceException lse)
            {
                Debug.LogError(lse.Message);
            }
        }    
    }

    public async void LeaveLobby()
    {
        if(_joinedLobby != null)
        { 
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                _joinedLobby = null;

            } catch (LobbyServiceException lse)
            {
                Debug.LogError(lse.Message);
            }
        }
    }
}
