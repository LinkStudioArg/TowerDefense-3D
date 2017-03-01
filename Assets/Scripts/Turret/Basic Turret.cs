using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BasicTurret : MonoBehaviour
{
    GameObject currentTarget;

    public MeshRenderer turretMesh;

    public Stats stats;

    public LayerMask enemyLayer;

    public State state;

    public enum State
    {
        SEARCHING,
        FIRING,
        STANDBY,
        WORKING
    }
    /*
     * SEARCHING: Looking for next target
     * FIRING: Firing
     * STANDBY: Waiting
     * WORKING: Working in a process, no other process starts while working
     */

    [System.Serializable]
    public struct Stats
    {
        public string name;
        public string description;
        public float cost;
        public float damage;
        public float fireRate;
        public float range;
        public float rotationSpeed;

        public Stats(string newName = "New Turret", string newDescr = "", float newCost = 0, float newDamage = 1, float newFRate = .1f, float newRange = 1, float newRSpeed = 0)
        {
            name = newName;
            description = newDescr;
            cost = newCost;
            damage = newDamage;
            fireRate = newFRate;
            range = newRange;
            rotationSpeed = newRSpeed;
        }
    }

    protected void FindEnemy()
    {
        if (currentTarget == null)
        {
            float dist = stats.range;
            int index = 0;

            // Find all enemies in range
            Collider[] cols = Physics.OverlapSphere(transform.position, stats.range, enemyLayer.value);
            
            // If any
            if (cols.Length > 0)
            {
                // Find closest
                for (int i = 0; i < cols.Length; i++)
                {
                    if (Mathf.Abs(Vector3.Distance(cols[i].transform.position, this.transform.position)) < dist)
                    {
                        dist = Mathf.Abs(Vector3.Distance(cols[i].transform.position, this.transform.position));
                        index = i;
                    }
                }

                // Set closest as current
                currentTarget = cols[index].gameObject;
            }
            else
                // If no enemies in range, current set to null
                currentTarget = null;
        }
        else
        {
            // If current enemy no longer in range, set to null
            if (Mathf.Abs(Vector3.Distance(currentTarget.transform.position, this.transform.position)) > this.stats.range)
            {
                currentTarget = null;
            }
        }
    }

    // Use this for initialization
    protected void Start()
    {
        state = State.STANDBY;
        currentTarget = null;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (state == State.STANDBY)
        {
            if (currentTarget == null)
            {
                StartCoroutine(Search());
            }
            else
            {
                PrepareToFire();
            }
        }
        else
        {
            if (currentTarget == null)
            {

                state = State.STANDBY;
            }
            else
            {
                StopCoroutine(Search());
                state = State.STANDBY;
            }
        }
    }


    protected IEnumerator Search()
    {
        state = State.SEARCHING;
        FindEnemy();

        yield return new WaitForSeconds(.5f);
        state = State.STANDBY;
    }

    protected void PrepareToFire()
    {
        state = State.FIRING;


        return;
    }
    
}
