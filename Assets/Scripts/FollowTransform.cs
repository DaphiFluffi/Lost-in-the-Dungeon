using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform _targetTransform;
    private bool _allowRotation = false;

    public void SetTartgetTransform(Transform targetTrans, bool allowRotation)
    {
        _targetTransform = targetTrans;
        _allowRotation = allowRotation;

    }

    private void LateUpdate()
    {
        if (_targetTransform == null) return;

        transform.position = _targetTransform.position;

        if (!_allowRotation)
            transform.rotation = _targetTransform.rotation;
        else
            transform.rotation = transform.rotation;
    }

}
