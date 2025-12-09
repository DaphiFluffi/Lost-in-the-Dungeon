using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController localINSTANCE { get; private set; }

    // Network variable to store spawn position
    private NetworkVariable<Vector3> spawnPosition = new NetworkVariable<Vector3>();

    public bool IsLocal { get; private set; }

    public override void OnNetworkSpawn()
    {
        int playerIndex = -1;

        if (IsOwner)
        {
            localINSTANCE = this;
            IsLocal = IsOwner;
        }

        playerIndex = GameMultiplayer.INSTANCE.GetPlayerDataIndexFromClientID(OwnerClientId);
        GetComponent<PlayerFlashlightInteraction>().SetPlayerID(playerIndex);
        Vector3 spawnPos = GameMultiplayer.INSTANCE.spawnPlayerPositions[playerIndex];


        if (playerIndex == 0)
        {
            transform.Find("PlayerClient").gameObject.SetActive(false);
            _animator.avatar = mageAvatar;

            
            Gradient blueGradient = new Gradient();
            blueGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );


            GetComponent<PlayerHealthBatteryManager>().InitializeManager(blueGradient, new Color(79, 26, 151));
        
        }
        else
        {
            transform.Find("PlayerHost").gameObject.SetActive(false);
            _animator.avatar = rogueAvatar;

            Gradient greenGradient = new Gradient();
            greenGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );

            GetComponent<PlayerHealthBatteryManager>().InitializeManager(greenGradient, new Color(0, 117, 57));
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }


    public void SetInitialSpawnPosition(Vector3 position)
    {
        transform.position = position;
    }


    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        if (clientID == OwnerClientId)
        {
            Destroy(this.transform.Find("Flashlight").gameObject);
        }
    }

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private Avatar mageAvatar;
    [SerializeField] private Avatar rogueAvatar;

    [SerializeField] private LayerMask collisionsMask;

    private Vector3 _input;

    private void Update()
    {
        if (!IsOwner) return;

        GatherInput();
        Look();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
    }

    private void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void Look()
    {
        if (_input != Vector3.zero)
        {
            Vector3 lookDirection = _input.ToIso();
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }
    }

    private void Move()
    {
        _rb.isKinematic = _input == Vector3.zero;
        Vector3 moveDirection = transform.forward * _input.normalized.magnitude * _speed * Time.deltaTime;
        _rb.MovePosition(transform.position + moveDirection);
        _animator.SetFloat("Speed", _input.normalized.magnitude);
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
