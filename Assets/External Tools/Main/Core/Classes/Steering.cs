using UnityEngine;
using System.Collections.Generic;
using PathFinding;


public class Steering {



	public static void ApplyForce( Agent agent, Vector3 force ){
		agent.acceleration += force*Time.deltaTime;
		agent.acceleration = Maths.Vector3Limit (agent.acceleration, agent.maxForce);
	}



	public static void Scanner (Agent agent, float _radius){
		Collider[] agentsInRadius =  Physics.OverlapSphere(agent.transform.position, _radius , agent.grid.AgentsLayer);
		List<Agent> agentsList = new List<Agent> ();  
		for (int i = 0; i < agentsInRadius.Length; i++){
			Agent other = agentsInRadius[i].gameObject.GetComponent<AgentComponent>().agent;
			if( other != agent ){
				agentsList.Add(other);    
			}
		}
		agent.agentsDetected = agentsList.ToArray ();
	}



	public static void Seek(Agent agent, Vector3 target, float weight=1){
		Vector3 desired = agent.maxSpeed * ( target - agent.transform.position ).normalized; 
		Vector3 steer = desired - agent.velocity; 
		steer = Maths.Vector3Limit(steer,agent.maxForce);
		Steering.ApplyForce( agent, weight*steer );
	}


	
	public static void Arrive(Agent agent, Vector3 target, float distanceToSlow, float distanceToStop, float weight=1){
		Vector3 desired = target - agent.transform.position ;
		float d = desired.magnitude;
		if (d < distanceToSlow) {
			float m = ((d+0.1f) * agent.maxSpeed / distanceToSlow );
			if( m > agent.maxSpeed ){
				m = agent.maxSpeed;
			}
			desired = m * desired.normalized;  
			if( d < distanceToStop  ){
				desired = agent.velocity;
			}
		} else {
			desired = agent.maxSpeed * desired.normalized;
		}
		Vector3 steer = desired - agent.velocity; 
		steer = Maths.Vector3Limit(steer,agent.maxForce);
		Steering.ApplyForce( agent, weight*steer );
	}
	


	public static void FlowField(Agent agent){
		Cell cell = agent.cell;

		if (agent.swarm != null && agent.state!=StateAgent.Wait) {
			if (agent.swarm.IsWithin (cell)) {
				if( agent.targetPosWorld == agent.swarm.targetPosWorld ){
					float distance = Vector3.Distance (agent.swarm.targetPosWorld, agent.transform.position);
					if (distance < agent.range) {
						if (agent.grid.Bresenham (cell.posGrid.x, cell.posGrid.z, agent.targetCell.posGrid.x, agent.targetCell.posGrid.z)) {
							if (distance < agent.grid.cellSize * 1.5f) {
								agent.state = StateAgent.Idle;
							} else {
								agent.state = StateAgent.Goal;
							}
							return;
						}
					}
					agent.state = StateAgent.Move;
				}else{
					agent.state = StateAgent.Wait;
				}
			} else {
				agent.swarm.toUpdate = true;
			}	
		}
	}
	


	public static void Separate (Agent agent, float weight=1){
		float desiredseparation = 0.0f;
		Vector3 steer = Vector3.zero;
		int counter = 0;
		foreach(Agent other in agent.agentsDetected) {
			float d = Vector3.Distance(agent.transform.position, other.transform.position);
			desiredseparation = agent.radius + other.radius + Mathf.Max(agent.merge,other.merge );
			if ( d > 0  &&  d < desiredseparation  &&  agent.mass <= other.mass ) {
				Vector3 diff = (agent.transform.position - other.transform.position).normalized;
				diff /= d;
				steer += diff;
				counter++;
			}
		}
		if (counter > 0) {
			steer /= counter;
			steer = agent.maxSpeed*steer.normalized - agent.velocity;
			steer = Maths.Vector3Limit(steer,agent.maxForce);
			Steering.ApplyForce( agent, weight*steer );
		}
	}



	public static void Cohesion (Agent agent, float weight=1) {
		Vector3 steer = agent.transform.position;
		int counter = 1;
		if (agent.agentsDetected.Length > 0) {
			foreach (Agent other in agent.agentsDetected) {
				if( other.swarm == agent.swarm ){
					steer += other.transform.position;
					counter++;
				}
			}
			steer.y = 0;
			steer = steer / counter;
			steer = Maths.Vector3Limit (steer, agent.maxForce);
			Steering.ApplyForce( agent, weight*steer );
		}
	}



}
