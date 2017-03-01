using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTurret : MonoBehaviour
{
    //Auxiliares
    public float cooldownTime;

    [Space(10)]


    public GameObject currentTarget;

    //public MeshRenderer turretMesh;

    public Stats stats;

    public LayerMask enemyLayer;

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

    void FindEnemy()
    {
        if (currentTarget == null)
        {

            float dist = stats.range;
            int index = 0;
            Collider[] cols = Physics.OverlapSphere(transform.position, this.stats.range, enemyLayer.value);
            for (int i = 0; i < cols.Length; i++)
            {
                if (Mathf.Abs(Vector3.Distance(cols[i].transform.position, this.transform.position)) < dist)
                {
                    dist = Mathf.Abs(Vector3.Distance(cols[i].transform.position, this.transform.position));
                    index = i;
                }
            }
            if (cols.Length > 0)
                currentTarget = cols[index].gameObject;
            else
                currentTarget = null;
        }
        else
        {
            if (Mathf.Abs(Vector3.Distance(currentTarget.transform.position, this.transform.position)) > this.stats.range)
            {
                currentTarget = null;
            }
        }
    }

    void Move()
    {
        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;

        Quaternion rotateTo = Quaternion.LookRotation(dir);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotateTo, Time.deltaTime * stats.rotationSpeed);


    }

    void Fire()
    {
        if (cooldownTime >= stats.fireRate)
        {
            Ray r = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, Mathf.Infinity, enemyLayer))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == currentTarget)
                    {
                        currentTarget.GetComponent<Enemy>().stats.hp -= this.stats.damage;
                    }
                }
            }
            cooldownTime = 0;
        }
    }

    IEnumerator FindEnemyCR()
    {
        FindEnemy();
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FindEnemyCR());
    }


    void Start()
    {
        cooldownTime = stats.fireRate;
        StartCoroutine(FindEnemyCR());
    }

    void Update()
    {

        if (cooldownTime < stats.fireRate)
            cooldownTime += Time.deltaTime;

        if (currentTarget)
        {
            Move();

            Fire();

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
