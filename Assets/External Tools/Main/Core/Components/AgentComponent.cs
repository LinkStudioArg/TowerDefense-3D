using UnityEngine;
using System.Collections.Generic;
using PathFinding;


public class AgentComponent : MonoBehaviour 
{
	public Agent 		agent;
	public GameObject 	Grid;
	public float		radius 		= 0.4f;
	public float		height		= 2;
	public float		centerY		= 0;
	public float		merge		= 0.1f;
	public float		maxSpeed 	= 3;
	public float		maxForce 	= 3;
	public float		mass 		= 1;
	public float		range 		= 3;
	public GameObject	target;


	void Start () 
	{
		// Create Grid
		Grid grid = Grid.GetComponent<GridComponent> ().grid;
		// Create CharacterController
		CharacterController character = gameObject.GetComponent<CharacterController> ();
		if (character == null) character = gameObject.AddComponent<CharacterController> ();
		// Create Agent
		agent = new Agent (grid, gameObject, radius , height, centerY, merge, character, maxSpeed, maxForce, mass, range);
		if (target != null) {
			agent.swarm.GoTo (target.transform.position);
		} else {
			agent.swarm.GoTo (transform.position);
		}
	}		


	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		if (Application.isPlaying) {
			switch (agent.state) {
			case StateAgent.Idle:
				Gizmos.color = Color.red;
				break;
			case StateAgent.Goal:
				Gizmos.color = Color.blue;
				break;
			case StateAgent.Move:
				Gizmos.color = Color.green;
				break;
			}
		}
		DrawCircle(transform.position, range);
		Gizmos.color = Color.red;
		DrawCircle(transform.position, radius+merge);
		if( agent != null ){
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(agent.transform.position,new Vector3( agent.targetPosWorld.x,agent.transform.position.y,agent.targetPosWorld.z) );
			Gizmos.color = Color.green;
			Gizmos.DrawLine(agent.transform.position, agent.transform.position + (1/Time.deltaTime)*(new Vector3(agent.velocity.x,0,agent.velocity.z)) );
		}	
	}


	void DrawCircle(Vector3 pos, float dist)
	{
		for( int right=0; right<360; right=right+5 ){
			Quaternion leftRayRotationArc = Quaternion.AngleAxis( right+5, Vector3.up );
			Quaternion rightRayRotationArc = Quaternion.AngleAxis(right, Vector3.up );
			Vector3 leftRayDirectionArc = leftRayRotationArc * Vector3.forward;
			Vector3 rightRayDirectionArc = rightRayRotationArc * Vector3.forward;
			Gizmos.DrawLine( pos+leftRayDirectionArc * dist, pos+rightRayDirectionArc * dist );
		}
	}
}
