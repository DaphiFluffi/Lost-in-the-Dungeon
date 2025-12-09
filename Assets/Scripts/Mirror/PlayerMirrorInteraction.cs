using UnityEngine;
using Unity.Netcode;
using Unity.Properties;

public class PlayerMirrorInteraction : NetworkBehaviour, IObjectParent
{
    public Transform holdPoint;

    private Animator animator;
    private LayerMask interactableLayer;

    [SerializeField] private InteractableObject heldObject;
    private PlayerController playerController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        interactableLayer = LayerMask.GetMask("Interactable");
    }

    private void Update()
    {
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
                if (hit.collider.CompareTag("MirrorObj"))
                {
                    GetComponent<PlayerFlashlightInteraction>().enabled = false;
                    SetObject(hit.collider.GetComponent<InteractableObject>());
                    heldObject.GetComponent<BoxCollider>().enabled = false;
                    OnPickUp();
                }
            }
        }
    }

    private void OnPickUp()
    {
        heldObject.SetObjectParent(this, true);

    }

    private void DropObject()
    {
        if (heldObject == null) return;

        Vector3 dropPosition = new Vector3(heldObject.transform.position.x, -1, heldObject.transform.position.z);
        heldObject.DropObject(dropPosition);
        heldObject = null;
        GetComponent<PlayerFlashlightInteraction>().enabled = true;
        Debug.Log("Dropped");
    }

    private void HandleObjectInteraction()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateObject();
        }
    }

    private void RotateObject()
    {
        if (heldObject == null)
        {
            return;
        }

        Vector3 rotation = heldObject.transform.eulerAngles;
        rotation.y += 45f;
        heldObject.RotateObject(rotation);  // Synchronize the rotation across the network
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
