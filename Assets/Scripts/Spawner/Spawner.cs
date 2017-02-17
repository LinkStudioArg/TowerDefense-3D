using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum State { STANDBY, WORKING, SPAWNING, FINISH }
    /*
     * STANDBY:  Waiting for next wave
     * WORKING:  At least one coroutine is working
     * SPAWNING: Spawning current wave
     * FINISH:   Done spawning last wave, level done
     */

    public State state;

    public List<Wave> waves;

    public float timeBetweenWaves = 4;


    Wave currentWave;

    [HideInInspector()]
    public int currentAmountOfEnemies;

    int waveIndex = 0;


    void Start()
    {
        state = State.STANDBY;
        if (waves.Count > 0)
        {
            currentWave = waves[waveIndex];

            state = State.SPAWNING;
        }
    }

    int scenesAlreadySpawn;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            StopCoroutine(WaitForNextWave());

        if (state == State.SPAWNING)
            StartCoroutine(SpawnWave());

        if (state == State.STANDBY && currentAmountOfEnemies == 0)
            StartCoroutine(WaitForNextWave());

        /* if(Base.hp <= 0)
         *     state = State.FINISH
         */
    }


    void Spawn(GameObject go)
    {
        GameObject newGo = (GameObject)GameObject.Instantiate(go, transform.position, Quaternion.identity);
        newGo.transform.SetParent(transform);
    }


    IEnumerator SpawnWave()
    {
        state = State.WORKING;

        for (int i = 0; i < currentWave.enemyAmount; i++)
        {
            GameObject go = currentWave.SelectGameObject();
            Spawn(go);

            currentAmountOfEnemies++;

            yield return new WaitForSeconds(currentWave.spawnRate);
        }

        if (waveIndex < waves.Count)
            state = State.STANDBY;
        else
            state = State.FINISH;
    }

    IEnumerator WaitForNextWave()
    {
        state = State.WORKING;

        waveIndex++;
        currentWave = waves[waveIndex];

        yield return new WaitForSeconds(timeBetweenWaves);

        state = State.SPAWNING;
    }


    [System.Serializable]
    public struct Wave
    {
        public string name;
        public List<GameObject> enemies;
        public float spawnRate;
        public int enemyAmount;

        public GameObject SelectGameObject()
        {
            int index = Random.Range(0, enemies.Count);
            return enemies[index];
        }

        public Wave(string name, float spR, int enA)
        {
            this.name = name;
            this.spawnRate = spR;
            this.enemyAmount = enA;
            this.enemies = new List<GameObject>();
        }
    }


}
