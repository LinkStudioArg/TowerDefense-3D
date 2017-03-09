using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // This struct has the stats the enemy has at any given moment
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
    
    // This struct saves the stats the enemy has on start
    [System.Serializable]
    public struct BaseStats
    {

        // All variables are private, they are set in start
        private float movementVel;
        private float hp;
        private float damage;
        private float shield;

        public BaseStats(Stats s)
        {
            movementVel = s.movementVel;
            hp = s.hp;
            damage = s.damage;
            shield = s.shield;
        }

        // Get functions
        public float getVelocity() { return movementVel; }
        public float getHP() { return hp; }
        public float getDamage() { return damage; }
        public float getShield() { return shield; }
    }

    public Stats stats;

    public BaseStats baseStats;


    void CheckHealth()
    {
        if (stats.hp <= 0)
        {
            //hacer graficos de puaj

            Destroy(this.gameObject);
        }
    }
    
    public void TakeDamage(float dam)
    {
        stats.hp -= dam - stats.shield; //The shield gives the enemy a level of resistance to all damage
    }


    private void Start()
    {
        baseStats = new BaseStats(stats);
    }

    void Update()
    {
        CheckHealth();
    }

    void OnDestroy()
    {
        GameObject.Find("Spawner").GetComponent<Spawner>().currentAmountOfEnemies--;
    }
}