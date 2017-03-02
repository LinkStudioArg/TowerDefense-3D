using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTurret : BasicTurret
{
    public float freezeTime;

    protected override void Fire()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, stats.range, enemyLayer);

        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            StartCoroutine(Freeze(enemiesInRange[i].gameObject));
        }
    }

    IEnumerator Freeze(GameObject enemyObject)
    {
        float vel = enemyObject.GetComponent<Enemy>().baseStats.getVelocity();

        enemyObject.GetComponent<Enemy>().stats.movementVel = stats.damage * vel;

        yield return new WaitForSeconds(freezeTime);
        if (enemyObject)
            enemyObject.GetComponent<Enemy>().stats.movementVel = vel;
    }
}
