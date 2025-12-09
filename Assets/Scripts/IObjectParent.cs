using Unity.Netcode;
using UnityEngine;

public interface IObjectParent
{
    public Transform GetObjectFollowTransform();
    public void SetObject(InteractableObject obj);
    public InteractableObject GetObject();
    public void ClearObject();
    public bool HasObject();
    public NetworkObject GetNetworkObject();
}
