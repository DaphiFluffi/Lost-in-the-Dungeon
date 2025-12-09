using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FlashLightBeam : MonoBehaviour
{
    [SerializeField] private Material beamMaterail;
    [SerializeField] private float BEAM_START_WIDTH = 0.1f;
    [SerializeField] private float BEAM_END_WIDTH = 0.1f;
    [SerializeField] private Color BEAM_START_COLOR = Color.green;
    [SerializeField] private Color BEAM_END_COLOR = Color.green;
    [SerializeField] private float BEAM_LENGHT = 10f;
    [SerializeField] private enum FlashlightType { Green, Purple }
    [SerializeField] private FlashlightType type;

    public GameObject beamGO;
    private LineRenderer _beam;
    private List<Vector3> _beamIndices = new List<Vector3>();

    private GameObject _lastHitObject;
    private GameObject _currentHitObject;

    /*
     * THIS WILL NEED A REFACTOR FOR MULTIPLAYER DESTROYING >:(
     */

    private void Awake()
    {
        _beam = new LineRenderer();
        beamGO = new GameObject();
        //beamGO.name = "FlashLightBeam" + GameMultiplayer.INSTANCE.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId);

        _beam = beamGO.AddComponent(typeof(LineRenderer)) as LineRenderer;
        _beam.startWidth = BEAM_START_WIDTH;
        _beam.startColor = BEAM_START_COLOR;
        _beam.material = beamMaterail;
        _beam.endWidth = BEAM_END_WIDTH;
        _beam.endColor = BEAM_END_COLOR;
    }

    private void Update()
    {
        _beam.positionCount = 0;
        _beamIndices.Clear();
        _currentHitObject = null;
        CastBeam(gameObject.transform.position, gameObject.transform.forward, _beam);

        if (_lastHitObject != null && _currentHitObject != _lastHitObject)
        {
            Debug.Log("Beam stopped hitting: " + _lastHitObject.name);

            if (_lastHitObject.name == "Target")
            {
                if (this.type == FlashlightType.Green)
                {
                    BeamStateManager._greenIsHitting = false;
                    _lastHitObject.GetComponent<TargetScript>().ChangeColor("white");

                    Debug.Log("Green is NOT hitting target");
                }
                else if (this.type == FlashlightType.Purple)
                {
                    BeamStateManager._purpleIsHitting = false;
                    _lastHitObject.GetComponent<TargetScript>().ChangeColor("white");

                    Debug.Log("Purple is NOT hitting target");
                }
                if (!BeamStateManager._greenIsHitting && !BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = false;
                    _lastHitObject.GetComponent<TargetScript>().ChangeColor("white");

                    Debug.Log("Both are NOT hitting target");
                }
                if (BeamStateManager._greenIsHitting && !BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = false;
                    _lastHitObject.GetComponent<TargetScript>().ChangeColor("green");

                    Debug.Log("Both are NOT hitting target");
                }
                if (!BeamStateManager._greenIsHitting && BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = false;
                    _lastHitObject.GetComponent<TargetScript>().ChangeColor("purple");

                    Debug.Log("Both are NOT hitting target");
                }
            }

            _lastHitObject = null;
        }

        
        

        _lastHitObject = _currentHitObject;
    }

    public void CastBeam(Vector3 pos, Vector3 dir, LineRenderer beam)
    {
        _beamIndices.Add(pos);
        Ray ray = new Ray(pos, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, BEAM_LENGHT, 1))
        {
            _currentHitObject = hit.collider.gameObject;
            CheckBeamHit(hit, dir, beam);
        }
        else
        {
            _beamIndices.Add(ray.GetPoint(BEAM_LENGHT));
            _currentHitObject = null;
        }

        UpdateBeam();
    }

    private void UpdateBeam()
    {
        int count = 0;
        _beam.positionCount = _beamIndices.Count;

        foreach (Vector3 idx in _beamIndices)
        {
            _beam.SetPosition(count++, idx);
        }
    }

    private void CheckBeamHit(RaycastHit hitInfo, Vector3 direction, LineRenderer beam)
    {
        GameObject hitObject = hitInfo.collider.gameObject;

        if (hitObject != _lastHitObject)
        {
            Debug.Log("Beam started hitting: " + hitObject.name);
            if (hitObject.name == "Target")
            {
                if (this.type == FlashlightType.Green)
                {
                    BeamStateManager._greenIsHitting = true;
                    hitObject.GetComponent<TargetScript>().ChangeColor("green");
                    Debug.Log("Green is hitting target");
                }
                else if (this.type == FlashlightType.Purple)
                {
                    BeamStateManager._purpleIsHitting = true;
                    hitObject.GetComponent<TargetScript>().ChangeColor("purple");

                    Debug.Log("Purple is hitting target");
                }

                if (BeamStateManager._greenIsHitting && BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = true;
                    hitObject.GetComponent<TargetScript>().ChangeColor("yellow");

                    Debug.Log("Both are hitting target: " + BeamStateManager._bothAreHitting);
                }
                else if(BeamStateManager._greenIsHitting && !BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = false;
                    hitObject.GetComponent<TargetScript>().ChangeColor("green");

                    Debug.Log("Both are NOOOT hitting target: " + BeamStateManager._bothAreHitting);

                }
                else if (!BeamStateManager._greenIsHitting && BeamStateManager._purpleIsHitting)
                {
                    BeamStateManager._bothAreHitting = false;
                    hitObject.GetComponent<TargetScript>().ChangeColor("purple");

                    Debug.Log("Both are NOOOT hitting target: " + BeamStateManager._bothAreHitting);

                }
                else
                {
                    BeamStateManager._bothAreHitting = false;
                    hitObject.GetComponent<TargetScript>().ChangeColor("white");

                    Debug.Log("Both are NOOOThitting target: " + BeamStateManager._bothAreHitting);
                }
            }
        }

        if (hitObject.tag == "Mirror")
        {
            hitObject.GetComponentInParent<InteractableObject>().SetIsBeamOnMirror(true);
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);
            CastBeam(pos, dir, beam);
            // do something here 


        }
        else if (hitObject.tag == "Skeleton")
        {
            _beamIndices.Add(hitInfo.point);
            UpdateBeam();
            hitInfo.collider.gameObject.GetComponent<EnemyScript>().TakeDamage(1);

        }
        else if(hitInfo.collider.gameObject.tag == "Target")
        {
            _beamIndices.Add(hitInfo.point);
            UpdateBeam();
        }
        else
        {
            _beamIndices.Add(hitInfo.point);
            UpdateBeam();
        }
    }
}

public static class BeamStateManager
{
    public static bool _greenIsHitting = false;
    public static bool _purpleIsHitting = false;
    public static bool _bothAreHitting = false;
}
