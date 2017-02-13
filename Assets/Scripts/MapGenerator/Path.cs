using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    public GameObject spawnPoint;

    public List<GameObject> wayPoints;

    public void SetWayPoint(GameObject go)
    {
        if (wayPoints == null) {
            wayPoints = new List<GameObject>();
        }
        wayPoints.Add(go);
    }
    
    public void SetSpawnPoint(GameObject go)
    {
        spawnPoint = go;
    }
}
