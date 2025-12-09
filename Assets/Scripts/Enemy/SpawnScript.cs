using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{
    List<Vector3> Positions = new List<Vector3>();
    public GameObject Entity;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void spawnEnemy(int enemyID)
    {
        Instantiate(Entity, Positions.ElementAt(enemyID), Quaternion.identity);
    }

    private void spawnAllFromRange(int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            Instantiate(Entity, Positions.ElementAt(i), Quaternion.identity);
        }
    }
}
