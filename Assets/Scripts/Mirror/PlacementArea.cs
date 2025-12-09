using UnityEngine;

public class PlacementArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mirror"))
        {
            Debug.Log("Mirror placed in the correct area.");
        }
    }
}
