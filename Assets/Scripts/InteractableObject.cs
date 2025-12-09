using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractableObject : NetworkBehaviour
{
    // IF THIS IS 0 THEN HOST (BLUE) CAN INTERACT, IF 1 THEN CLIENT (GREEN) CAN
    // IF -1 THEN ALL CAN
    [SerializeField] private int playerIDAllowedToInteract;
    [SerializeField] private GameObject beam = null;
    public GameObject flashlightSource;
    private FollowTransform _followTransform;
    private IObjectParent _parent;

    private bool _isBeamOnMirror = false;

    public int GetPlayerIDAllowed()
    {
        return playerIDAllowedToInteract;
    }

    public GameObject GetFlashLighBeam()
    {
        return beam;
    }

    private void Start()
    {
        _followTransform = GetComponent<FollowTransform>();
        if(playerIDAllowedToInteract != -1 )
        {
            FindMyFlashSettings();

        }
    }

    public bool GetIsBeamOnMirror()
    {
        return _isBeamOnMirror;
    }

    public void SetIsBeamOnMirror(bool onMirror)
    {
        _isBeamOnMirror = onMirror;
    }

    public void LateUpdate()
    {
        //if (_isBeamOnMirror) _isBeamOnMirror = false;
    }

    private void FindMyFlashSettings()
    {
        beam = GetComponentInChildren<FlashLightBeam>().beamGO;
        flashlightSource = GetComponentInChildren<FlashLightBeam>().gameObject;
    }

    public void SetObjectParent(IObjectParent objectParent, bool allowRotation)
    {
        SetObjectParentServerRpc(objectParent.GetNetworkObject(), allowRotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetObjectParentServerRpc(NetworkObjectReference ObjectParentnetworkObjectRef, bool allowRotation)
    {
        SetObjectParentClientRpc(ObjectParentnetworkObjectRef, allowRotation);
    }

    [ClientRpc]
    private void SetObjectParentClientRpc(NetworkObjectReference objectParentNetworkRef, bool allowRotation)
    {
        objectParentNetworkRef.TryGet(out NetworkObject objectParentNetworkObject);

        IObjectParent objectParent;

        if (allowRotation)
             objectParent = objectParentNetworkObject.GetComponent<PlayerMirrorInteraction>();
        else
            objectParent = objectParentNetworkObject.GetComponent<PlayerFlashlightInteraction>();


        _parent = objectParent;
        _parent.SetObject(this);
        _followTransform.SetTartgetTransform(_parent.GetObjectFollowTransform(), allowRotation);
    }


    public void DropObject(Vector3 dropPosition)
    {
        DropObjectServerRpc(dropPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropObjectServerRpc(Vector3 dropPosition)
    {
        DropObjectClientRpc(dropPosition);
    }

    [ClientRpc]
    private void DropObjectClientRpc(Vector3 dropPosition)
    {
        _followTransform.SetTartgetTransform(null, false);
        transform.position = dropPosition;
        GetComponent<BoxCollider>().enabled = true;
        _parent = null;
    }

    public void RotateObject(Vector3 rotation)
    {
        RotateObjectServerRpc(rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RotateObjectServerRpc(Vector3 rotation)
    {
        RotateObjectClientRpc(rotation);
    }

    [ClientRpc]
    private void RotateObjectClientRpc(Vector3 rotation)
    {
        transform.eulerAngles = rotation;
    }
}
