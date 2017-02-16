using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Wave[] waves;

    Wave currentWave;

    public int currentAmountOfEnemies;

    public enum State { STANDBY, WORKING, SPAWNING, FINISH }
    /*
     * STANDBY:  Waiting for next wave
     * WORKING:  At least one coroutine is working
     * SPAWNING: Spawning current wave
     * FINISH:   Done spawning last wave, level done
     */

    public State state;

    public int waveIndex = 0;

    public float timeBetweenWaves = 4f;

    void Start()
    {
        state = State.STANDBY;
        if (waves.Length > 0)
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

        if (waveIndex < waves.Length)
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
        public GameObject[] enemies;
        public float spawnRate;
        public int enemyAmount;

        public GameObject SelectGameObject()
        {
            int index = Random.Range(0, enemies.Length);
            return enemies[index];
        }
    }


}
