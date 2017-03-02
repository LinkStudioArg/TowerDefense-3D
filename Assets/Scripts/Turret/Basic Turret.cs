using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BasicTurret : MonoBehaviour
{
    #region Variables

    public GameObject currentTarget;

    public MeshRenderer turretMesh;

    public Stats stats;

    public State state;

    public LayerMask enemyLayer;

    #endregion

    #region New Data Structures

    [System.Serializable]
    public struct Stats
    {
        public string name;
        public string description;
        public float cost;
        public float damage;
        public float fireRate;
        public float range;

        public Stats(string newName = "New Turret", string newDescr = "", float newCost = 0, float newDamage = 1, float newFRate = .1f, float newRange = 1)
        {
            name = newName;
            description = newDescr;
            cost = newCost;
            damage = newDamage;
            fireRate = newFRate;
            range = newRange;
        }
    }

    public enum State
    {
        SEARCHING,
        STANDBY,
        FIRING
    }

    #endregion

    IEnumerator FindEnemyCR()
    {
        FindEnemy();
        yield return new WaitForSeconds(0.5f);
        if (state == State.SEARCHING)
        {
            StartCoroutine(FindEnemyCR());
        }
    }

    void FindEnemy()
    {
        float dist = stats.range;
        int index = 0;
        Collider[] cols = Physics.OverlapSphere(transform.position, this.stats.range, enemyLayer.value);
        for (int i = 0; i < cols.Length; i++)
        {
            float indexDist = Mathf.Abs(Vector3.Distance(cols[i].transform.position, this.transform.position));
            if (indexDist < dist)
            {
                dist = indexDist;
                index = i;
            }
        }
        if (cols.Length > 0)
        {
            //FOUND
            currentTarget = cols[index].gameObject;
            state = State.STANDBY;
        }
        else
            currentTarget = null;
    }



    protected virtual void PreFire()
    {

    }

    protected virtual void Fire()
    {

    }

    protected virtual IEnumerator BeginFire()
    {
        Fire();
        yield return new WaitForSeconds(stats.fireRate);
        if (state == State.FIRING)
            StartCoroutine(BeginFire());
    }



    protected virtual void Start()
    {
        //enemyLayer = LayerMask.NameToLayer("Enemies");
        turretMesh = gameObject.GetComponent<MeshRenderer>();
        state = State.STANDBY;
    }

    protected virtual void Update()
    {

        switch (state)
        {
            case State.STANDBY:
                if (currentTarget)
                {
                    state = State.FIRING;
                    StartCoroutine(BeginFire());
                }
                else
                {
                    state = State.SEARCHING;
                    StartCoroutine(FindEnemyCR());
                }
                break;
            case State.FIRING:
                if (currentTarget)
                    PreFire();
                else
                {
                    StopAllCoroutines();
                    state = State.STANDBY;
                }
                break;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, .5f);

        Gizmos.DrawWireSphere(transform.position, stats.range);

        Gizmos.color = new Color(1, 0, 1);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}