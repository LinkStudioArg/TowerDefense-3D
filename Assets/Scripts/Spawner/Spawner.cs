using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum State { STANDBY, WORKING, SPAWNING, FINISH }
    /*
     * STANDBY:  Waiting for next wave
     * WORKING:  At least one coroutine is working, no other coroutine should be called
     * SPAWNING: Spawning current wave
     * FINISH:   Done spawning last wave, level done
     */

    public State state; // Current state of the spawner

    public List<Wave> waves; // List of the waves to spawn

    public float timeBetweenWaves = 4; // Time between one wave being done and the next spawining


    Wave currentWave; // Auxiliar variable to define the wave that is being worked on

    [HideInInspector()]
    public int currentAmountOfEnemies; // Current amount of enemies on screen at any given moment

    int waveIndex = 0; // Auxiliar variable to define the index of the wave that is being worked on


    void Start()
    {
        // At start the state is set to STANDBY, if there are waves, 
        state = State.STANDBY;
        if (waves.Count > 0)
        {
            currentWave = waves[waveIndex];
            state = State.SPAWNING;
        }
        else
            state = State.FINISH;
    }

    void Update()
    {
        if (PressedNextWaveButton()) // Function that returns if the player pressed the button to start next wave early
            StopCoroutine(WaitForNextWave());

        switch (state)
        {
            case State.STANDBY:
                if (currentAmountOfEnemies == 0)
                    StartCoroutine(WaitForNextWave());
                break;

            case State.SPAWNING:
                StartCoroutine(SpawnWave());
                break;

            case State.FINISH:
                Debug.Log("Level end");
                break;
        }

        /* if(Base.hp <= 0)
         *     state = State.FINISH
         */
    }


    void Spawn(GameObject go)
    {
        // Creates a new object using the corresponding enemy prefab and sets it as child of the spawner
        GameObject newGo = (GameObject)GameObject.Instantiate(go, transform.position, Quaternion.identity);
        newGo.transform.SetParent(transform);
    }


    IEnumerator SpawnWave()
    {
        state = State.WORKING;

        for (int i = 0; i < currentWave.enemyAmount; i++)
        {
            // Selects an enemy prefab and spawns an enemy
            GameObject go = currentWave.SelectGameObject();
            Spawn(go);

            currentAmountOfEnemies++;

            yield return new WaitForSeconds(currentWave.spawnRate);
        }

        // If there are more waves to spawn go to standby, else the level is done
        if (waveIndex < waves.Count - 1)
            state = State.STANDBY;
        else
            state = State.FINISH;
    }

    IEnumerator WaitForNextWave()
    {
        state = State.WORKING;

        // Moves to next wave
        waveIndex++;
        currentWave = waves[waveIndex];

        // Waits the corresponding time and sets the state to spawning
        yield return new WaitForSeconds(timeBetweenWaves);

        // If there are more waves to spawn go to spawning, else the level is done
        if (waveIndex < waves.Count)
            state = State.SPAWNING;
        else
            state = State.FINISH;
    }

    bool PressedNextWaveButton()
    {
        // The input N is a placeholder for the player pressing the "Next Wave" button
        return Input.GetKeyDown(KeyCode.N);
    }

    [System.Serializable]
    public struct Wave
    {
        // Wave name, used for identifying in level design
        public string name;

        // List of enemy prefabs that can appear in this wave
        public List<GameObject> enemies;

        // Time to wait between spawns
        public float spawnRate;

        // Amount of enemies that spawn this wave
        public int enemyAmount;

        // Function to select a random prefab from the enemies list
        public GameObject SelectGameObject()
        {
            int index = Random.Range(0, enemies.Count);
            return enemies[index];
        }

        // Constructor
        public Wave(string name, float spR, int enA)
        {
            this.name = name;
            this.spawnRate = spR;
            this.enemyAmount = enA;
            this.enemies = new List<GameObject>();
        }
    }
}
