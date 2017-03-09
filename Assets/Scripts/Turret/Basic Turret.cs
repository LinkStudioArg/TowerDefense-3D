using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BasicTurret : MonoBehaviour
{


    #region New Data Structures

    [System.Serializable]
    public struct Stats
    {
        public string name; // Name of the turret
        public string description; // A general descrption of the turret
        public float cost; // A cost in crystals for the turret
        public float damage; // The damage the turret causes in a shot
        public float fireRate; // The time between shots fired
        public float range; // The radius where enemies can be detected

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

    public enum State // The state of the turret, what part of the algorythm is being run
    {
        SEARCHING, // The turret is currently searching for a target
        STANDBY, // The turret is waiting between shots
        FIRING // The turret is shooting at the target
    }

    #endregion

    #region Variables

    public GameObject currentTarget; // Target enemy

    public MeshRenderer turretMesh; // Mesh, used for visual effects

    public Stats stats; // Turret stats

    public State state; // Current state of the turret

    public LayerMask enemyLayer; // Layer in which the enemies are stored

    #endregion


    // Coroutine that calls the FindEnemy method
    IEnumerator FindEnemyCR()
    {
        FindEnemy();
        yield return new WaitForSeconds(0.5f);
        if (state == State.SEARCHING)
        {
            StartCoroutine(FindEnemyCR());
        }
    }

    // Method that looks for enemies within the range
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

    // Method that is called every frame while firing
    protected virtual void PreFire()
    {

    }


    //Coroutine that calls the Fire method
        protected virtual IEnumerator BeginFire()
        {
            Fire();
            yield return new WaitForSeconds(stats.fireRate);
            if (state == State.FIRING)
                StartCoroutine(BeginFire());
        }

    // Method that fires a shot to the target
    protected virtual void Fire()
    {

    }



    protected virtual void Start()
    {
        // Initialization
        //enemyLayer = (LayerMask)LayerMask.NameToLayer("Enemy");
        turretMesh = gameObject.GetComponent<MeshRenderer>();
        state = State.STANDBY;
    }

    protected virtual void Update()
    {
        // Main algorythm
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
        // Drawing the range

        Gizmos.color = new Color(1, 1, 1, .5f);

        Gizmos.DrawWireSphere(transform.position, stats.range);

        Gizmos.color = new Color(1, 0, 1);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}