using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

	public Transform[] wayPoints;
	float vel = 10f;
	Transform Base;
	Transform target;
	int index;
	Enemy enemyScript;
	void Awake()
	{
		enemyScript = this.GetComponent<Enemy> ();
		vel = enemyScript.stats.movementVel;
	}

	void Start(){
		index = 0;
		LoadWaypoints ();
		if (wayPoints.Length > 0)
			target = wayPoints [index];

		Base = GameObject.Find ("Base").transform;
	}

	void LoadWaypoints(){
		GameObject wp = GameObject.Find ("WayPoints");

		Transform[] wps = new Transform[wp.transform.childCount];

		for (int i = 0; i < wps.Length; i++) {
			wps [i] = wp.transform.GetChild (i);
		}

		wayPoints = wps;

	}
	// Update is called once per frame
	void Update () {

        vel = enemyScript.stats.movementVel;

		CheckHealth ();
		if (wayPoints.Length > 0) {
			
			if (HasReachedTarget ())
				target = SelectNewTarget ();
			else
				Move ();
		}
	}

	bool HasReachedTarget(){

		float distance = Vector2.Distance (transform.position, target.position);

		return (distance < 0.1f);
	}

	Transform SelectNewTarget(){
		index++;
		if (index == wayPoints.Length) {
			return Base;
		} else if (index >= wayPoints.Length + 1) {
			//Llego a la base, llamar referencia y provoccar danio y destruir

			Destroy(this.gameObject);
			return transform;
		}
		return wayPoints [index];
	}

	void Move(){

		transform.position = Vector2.MoveTowards (transform.position, target.position, vel * Time.deltaTime);

	}

	void CheckHealth(){
		if (enemyScript.stats.hp <= 0) {
			//hacer graficos de puaj

			Destroy (this.gameObject);
		}
	}

	void OnDestroy(){
		GameObject.Find ("Spawner").GetComponent<Spawner> ().currentAmountOfEnemies--;

	}

}
