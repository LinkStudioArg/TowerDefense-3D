using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTurret : BasicTurret
{
    public float rotationSpeed;

    void Move()
    {
        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;

        Quaternion rotateTo = Quaternion.LookRotation(dir);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotateTo, Time.deltaTime * rotationSpeed);
    }

    protected override void PreFire()
    {
        Move();
    }
    
    protected override void Fire()
    {
        Ray r = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, Mathf.Infinity, enemyLayer))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == currentTarget)
                {
                    currentTarget.GetComponent<Enemy>().TakeDamage(stats.damage);
                }
            }
        }
    }

}
