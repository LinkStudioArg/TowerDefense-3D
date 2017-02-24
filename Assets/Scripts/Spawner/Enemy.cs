using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [System.Serializable]
    public struct Stats
    {
        public string name;
        public float movementVel;
        public float hp;
        public float damage;
        public float shield;
        //public float spawnRate;
    }

    public Stats stats;

     void Update()
    {
        CheckHealth();
    }

    void CheckHealth()
    {
        if (stats.hp <= 0)
        {
            //hacer graficos de puaj

            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        GameObject.Find("Spawner").GetComponent<Spawner>().currentAmountOfEnemies--;
    }
}