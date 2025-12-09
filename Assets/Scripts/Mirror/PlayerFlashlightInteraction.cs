using UnityEngine;
using Unity.Netcode;

public class PlayerFlashlightInteraction : NetworkBehaviour, IObjectParent
{
    public Transform holdPoint;
    public GameObject flashLightBeam;
    private LayerMask interactableLayer;

    [SerializeField] private InteractableObject heldObject;
    private int playerId;

    private float maxRotationAngleUp = 25f;
    private float maxRotationAngleDown = -5f;
    private float maxRotationAngleLeft = -45f;
    private float maxRotationAngleRight = 45f;
    private float rotationIncrement = 5f;
    private float currentRotationAngleX = 0f;
    private float currentRotationAngleY = 0f;
    private Transform heldObjectInitialHeight;

    public void SetPlayerID(int id)
    {
        playerId = id;
    }

    private void Start()
    {
        interactableLayer = LayerMask.GetMask("Interactable");
    }

    private void Update()
    {
        if (!IsOwner) return; // Ensure only the owner of this object can interact with it

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!HasObject())
            {
                TryPickUp();
            }
            else
            {
                DropObject();
            }
        }

        if (heldObject != null)
        {
            HandleObjectInteraction();
        }
    }

    private void TryPickUp()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            if ((interactableLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
            {
                if (hit.collider.CompareTag("Flashlight"))
                {
                    InteractableObject interactableObject = hit.collider.GetComponent<InteractableObject>();
                    if (interactableObject.GetPlayerIDAllowed() == playerId)  // Check player ID
                    {
                        GetComponent<PlayerMirrorInteraction>().enabled = false;
                        heldObject = interactableObject;
                        heldObjectInitialHeight = heldObject.transform;
                        heldObject.GetComponent<BoxCollider>().enabled = false;
                        OnPickUp();

                        flashLightBeam = heldObject.GetFlashLighBeam();
                    }
                    else
                    {
                        Debug.Log("This flashlight cannot be picked up by this player.");
                    }
                }
            }
        }
    }

    private void OnPickUp()
    {
        heldObjectInitialHeight = heldObject.gameObject.transform;
        heldObject.SetObjectParent(this, false);
        UpdateFlashlightRotationServerRpc(currentRotationAngleX, currentRotationAngleY); // Sync rotation on pick up
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        Vector3 dropPosition = new Vector3(heldObject.transform.position.x, heldObjectInitialHeight.position.y, heldObject.transform.position.z);
        heldObject.DropObject(dropPosition);
        heldObject = null;
        GetComponent<PlayerMirrorInteraction>().enabled = true;
        Debug.Log("Dropped");
    }

    private void HandleObjectInteraction()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RotateObjectDown();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {

            RotateObjectUp();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            RotateObjectLeft();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {

            RotateObjectRight();
        }
    }

    private void RotateObjectUp()
    {
        if (heldObject == null) return;
        if (currentRotationAngleX < maxRotationAngleUp)
        {
            float newRotationAngle = currentRotationAngleX + rotationIncrement;
            newRotationAngle = Mathf.Min(newRotationAngle, maxRotationAngleUp);

            Vector3 rotation = heldObject.flashlightSource.transform.localEulerAngles;
            rotation.x -= (newRotationAngle - currentRotationAngleX);
            heldObject.flashlightSource.transform.localEulerAngles = rotation;
            currentRotationAngleX = newRotationAngle;

            UpdateFlashlightRotationServerRpc(currentRotationAngleX, currentRotationAngleY);
        }
    }

    private void RotateObjectDown()
    {
        if (heldObject == null) return;
        if (currentRotationAngleX > maxRotationAngleDown)
        {
            float newRotationAngle = currentRotationAngleX - rotationIncrement;
            newRotationAngle = Mathf.Max(newRotationAngle, maxRotationAngleDown);

            Vector3 rotation = heldObject.flashlightSource.transform.localEulerAngles;
            rotation.x -= (newRotationAngle - currentRotationAngleX);
            heldObject.flashlightSource.transform.localEulerAngles = rotation;
            currentRotationAngleX = newRotationAngle;

            UpdateFlashlightRotationServerRpc(currentRotationAngleX, currentRotationAngleY);
        }
    }

    private void RotateObjectLeft()
    {
        if (heldObject == null) return;
        if (currentRotationAngleY > maxRotationAngleLeft)
        {
            float newRotationAngle = currentRotationAngleY - rotationIncrement;
            newRotationAngle = Mathf.Max(newRotationAngle, maxRotationAngleLeft);

            Vector3 rotation = heldObject.flashlightSource.transform.localEulerAngles;
            rotation.y -= (newRotationAngle - currentRotationAngleY);
            heldObject.flashlightSource.transform.localEulerAngles = rotation;
            currentRotationAngleY = newRotationAngle;

            UpdateFlashlightRotationServerRpc(currentRotationAngleX, currentRotationAngleY);
        }
    }

    private void RotateObjectRight()
    {
        if (heldObject == null) return;
        if (currentRotationAngleY < maxRotationAngleRight)
        {
            float newRotationAngle = currentRotationAngleY + rotationIncrement;
            newRotationAngle = Mathf.Min(newRotationAngle, maxRotationAngleRight);

            Vector3 rotation = heldObject.flashlightSource.transform.localEulerAngles;
            rotation.y -= (newRotationAngle - currentRotationAngleY);
            heldObject.flashlightSource.transform.localEulerAngles = rotation;
            currentRotationAngleY = newRotationAngle;

            UpdateFlashlightRotationServerRpc(currentRotationAngleX, currentRotationAngleY);
        }
    }

    [ServerRpc]
    private void UpdateFlashlightRotationServerRpc(float rotationX, float rotationY)
    {
        currentRotationAngleX = rotationX;
        currentRotationAngleY = rotationY;
        UpdateFlashlightRotationClientRpc(rotationX, rotationY);
    }

    [ClientRpc]
    private void UpdateFlashlightRotationClientRpc(float rotationX, float rotationY)
    {
        if (heldObject == null) return;

        Vector3 rotation = heldObject.flashlightSource.transform.localEulerAngles;
        rotation.x = rotationX;
        rotation.y = rotationY;
        heldObject.flashlightSource.transform.localEulerAngles = rotation;
    }

    public Transform GetObjectFollowTransform()
    {
        return holdPoint;
    }

    public void SetObject(InteractableObject obj)
    {
        heldObject = obj;
    }

    public InteractableObject GetObject()
    {
        return heldObject;
    }

    public void ClearObject()
    {
        DropObject();
    }

    public bool HasObject()
    {
        return heldObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
