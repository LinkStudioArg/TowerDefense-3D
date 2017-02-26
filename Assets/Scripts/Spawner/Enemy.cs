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

        public Stats(string _name = "", float _movVel = 1, float _hp = 1, float _dam = 0, float _shield = 0)
        {
            this.name = _name;
            this.movementVel = _movVel;
            this.hp = _hp;
            this.damage = _dam;
            this.shield = _shield;
        }
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