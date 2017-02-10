using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	
	public Wave[] waves;

	Wave currentWave;

	public int currentAmountOfEnemies;

	public enum State{STANDBY, SPAWNING, FINISH}

	public State state;

	public float time;

	public int waveIndex = 0;

	public float timeBetweenWaves = 4f;

	void Start(){
		scenesAlreadySpawn = 0;
		time = 0f;
		this.state = State.STANDBY;
		if (waves.Length > 0) {
			currentWave = waves [waveIndex];
			currentAmountOfEnemies = currentWave.enemyAmount;

			this.state = State.SPAWNING;
			
		}
	}

	int scenesAlreadySpawn;

	void Update(){
		

		if (scenesAlreadySpawn == waves.Length) {			
			state = State.FINISH;
		}
	 
		if (state != State.FINISH) {
			
			if (state == State.STANDBY && currentAmountOfEnemies <= 0) {
				time += Time.deltaTime;

				if (time >= timeBetweenWaves || Input.GetKeyDown (KeyCode.N)) {
					waveIndex++;
					currentWave = waves [waveIndex];
					time = 0f;
					this.state = State.SPAWNING;
					currentAmountOfEnemies = currentWave.enemyAmount;
				}
			}

			if (state == State.SPAWNING) {
				time += Time.deltaTime;
				if (time >= currentWave.spawnRate && currentWave.enemyAmount > 0) {
					time = 0f;
					GameObject go = currentWave.SelectGameObject ();
					Spawn (go);
				}
				if (currentWave.enemyAmount <= 0) {
					scenesAlreadySpawn++;
					state = State.STANDBY;
				}
			
			}

		}
	}



	void Spawn(GameObject go)
	{
		GameObject newGo = (GameObject)GameObject.Instantiate (go, this.transform.position, Quaternion.identity);
		newGo.transform.SetParent (this.transform);
		currentWave.enemyAmount--;
	}

	[System.Serializable]
	public struct Wave
	{
		public string name;
		public GameObject[] enemies;
		public float spawnRate;
		public int enemyAmount;

		public GameObject SelectGameObject(){
			int index = Random.Range (0, enemies.Length);
			return enemies [index];
		}
	}


}
