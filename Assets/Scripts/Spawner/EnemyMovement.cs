using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    Transform[] wayPoints; // Array of he points the enemy will move through
    float vel; // Velocity, set from the stats of the Enemy script
    Transform Base; // The objective, the players base
    Transform target; // The current target of the enemy
    int wayPointIndex; // Index that helps looping through waypoints
    Enemy enemyScript; // The enemy script corresponding to this object

    float distThreshold = 0.1f; // the distance threshold the enemy has to surpass to change target

    void Awake()
    {
        // Set the enemy script and velocity
        enemyScript = GetComponent<Enemy>();
        vel = enemyScript.stats.movementVel;
    }

    void Start()
    {
        wayPointIndex = 0;
        
        LoadWaypoints();

        if (wayPoints.Length > 0)
            target = wayPoints[wayPointIndex];

        Base = GameObject.Find("Base").transform;
    }

    void LoadWaypoints()
    {
        // For now it looks for the element called "Waypoints", later on it will be loaded from the Path

        GameObject wp = GameObject.Find("WayPoints");

        Transform[] wps = new Transform[wp.transform.childCount];

        for (int i = 0; i < wps.Length; i++)
        {
            wps[i] = wp.transform.GetChild(i);
        }

        wayPoints = wps;

    }

    // Update is called once per frame
    void Update()
    {
        vel = enemyScript.stats.movementVel;
        // If there are waypoints then check to see if the enemy has reached the current target, then select a new one, else move towards the target
        if (wayPoints.Length > 0)
        {
            if (HasReachedTarget())
                target = SelectNewTarget();
            else
                Move();
        }
    }

    bool HasReachedTarget()
    {
        // Calculates the distance between the enemy and the target, returns true if the distance threshold has been reached
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < distThreshold;
    }

    Transform SelectNewTarget()
    {
        // Moves to the next waypoint
        wayPointIndex++;

        // If there are no more waypoints, the target is the Base
        if (wayPointIndex == wayPoints.Length)
        {
            return Base;
        }
        else if (wayPointIndex > wayPoints.Length)
        {
            // If it has reached the base, destroy the object
            Destroy(this.gameObject);
            return transform;
        }
        return wayPoints[wayPointIndex];
    }

    void Move()
    {
        // Move towards the target using the velocity
        transform.position = Vector3.MoveTowards(transform.position, target.position, vel * Time.deltaTime);
    }
    
}
