using UnityEngine;
using System.Collections.Generic;
using PathFinding;




public enum StateAgent{Idle,Move,Goal,Wait};




public class Agent
{
	static public List<Agent> 	allAgents 		= new List<Agent>();

	public List<Triangle> 		triangles 		= new List<Triangle> ();
	public StateAgent			state	  		= StateAgent.Idle;
	public GameObject			selectionCircle	{ get; set; }
	public Transform			transform	 	{ get; set; }
	public Grid					grid			{ get; set; }
	public Swarm				swarm			{ get; set; }
	public Cell					cell			{ get; set; }
	public CharacterController	character		{ get; set; }
	public Animator 			animator 		{ get; set; }
	public Agent[] 				agentsDetected;
	public float				radius;
	public float				height;
	public float				merge;
	public float 				maxSpeed;
	public float				maxForce;
	public float				mass;
	public float				range;
	public Vector3 				velocity;
	public Vector3				acceleration;
	public Vector3				targetPosWorld;
	public Cell					targetCell;



	
	public Agent(Grid _grid, GameObject _gameObject , float _radius, float _height, float _centerY, float _merge, CharacterController _character, float _maxSpeed, float _maxForce, float _mass, float _range)
	{
		transform  				= _gameObject.transform;
		grid 					= _grid;
		swarm 					= new Swarm (_grid);
		RaycastHit hit;
		if (Physics.Raycast (transform.position, Vector3.down, out hit, grid.height, grid.WalkableLayer)) {
			swarm.targetPosWorld = hit.point;
			targetPosWorld = hit.point;
		}
		cell 					= _grid.GetCell (targetPosWorld);
		character				= _character;
		character.radius		= _radius;
		character.height 		= _height;
		character.slopeLimit	= _grid.maxSlope+1;
		character.center 		= new Vector3(0,_centerY,0);
		radius					= _radius;
		height					= _height;
		merge					= _merge;
		maxSpeed				= _maxSpeed;
		maxForce				= _maxForce;
		mass					= _mass;
		range					= _range;
		velocity				= Vector3.zero;
		acceleration			= Vector3.zero;
		animator 				= transform.GetComponent<Animator>();
		swarm.agents.Add (this);
		grid.agents.Add (this);
		allAgents.Add (this);
	}




	public void Update()
	{
		if (swarm.toUpdate) {
			if (Time.time - swarm.time > (1.0f/grid.updateFrequency)) {
				swarm.GoTo(swarm.targetPosWorld);
			}
		}
		Scan ();
		Behaviours ();
		Move ();
	}




	/// <summary>
	/// Detect all agents within a specific distance from our agent.
	/// </summary>
	private void Scan()
	{
		cell = grid.GetCell (transform.position);
		Steering.Scanner (this,range);
	}




	private void Behaviours()
	{
		Steering.FlowField	(this);
		switch (state) {
			case StateAgent.Idle:
				state = StateAgent.Idle;
				Steering.Arrive 	( this, targetPosWorld, 2*grid.cellSize, 0.1f, 0.25f );
				Steering.Separate 	( this, 0.15f );
				transform.LookAt ( new Vector3(targetPosWorld.x,transform.position.y,targetPosWorld.z) );
			break;
			case StateAgent.Goal:
				Steering.Arrive 	( this, targetPosWorld, grid.cellSize, 0.1f, 0.5f );
				transform.LookAt 	( transform.position + new Vector3(velocity.x,0,velocity.z) );
				Steering.Separate 	( this, 0.1f );
				Avoidance.Execute 	( this, 1.0f );
			break;
			case StateAgent.Move:
				Steering.Seek 		( this, transform.position + swarm.flow[cell.posGrid.x,cell.posGrid.z],0.5f);
				transform.LookAt 	( transform.position + new Vector3(velocity.x,0,velocity.z) );
				Steering.Separate 	( this, 0.25f );
				Avoidance.Execute 	( this, 1.25f );
			break;
			case StateAgent.Wait:
				Steering.Arrive 	( this, transform.position, grid.cellSize, 0.1f, 0.25f );
				Steering.Separate 	( this, 0.15f );
				swarm.toUpdate = true;
			break;
		}
	}
	



	private void AnimationManager ()
	{
		switch (state) {
		case StateAgent.Idle:
			if( animator !=null)
				animator.SetFloat("Speed", 0);
			break;
		case StateAgent.Goal:
			if( animator !=null)
				animator.SetFloat("Speed", (velocity.magnitude));
			break;
		case StateAgent.Move:
			if( animator !=null)
				animator.SetFloat("Speed", (velocity.magnitude));
			break;
		case StateAgent.Wait:
			if( animator !=null)
				animator.SetFloat("Speed", 0);
			break;
		}
	}




	private void Move()
	{
		velocity += acceleration / mass + 0.98f*Vector3.down ;
		velocity = Maths.Vector3Limit (velocity, maxSpeed);
		CorrectionPosWorld ();
		if (animator == null) {
			character.Move (velocity);
		} else {
			AnimationManager ();
		}
		acceleration = Vector3.zero;
	}
	



	private void CorrectionPosWorld()
	{
		// Y position
		RaycastHit hit;
		if (Physics.Raycast (transform.position, Vector3.down, out hit, grid.height, grid.WalkableLayer)) {
			if( Mathf.Abs(transform.position.y - hit.point.y) > height){
				transform.position = hit.point;
			}
		}
		// If agent is on no walkable position
		if (!cell.walkable) {
			Cell close = cell.SearchClosestWalkableCell();
			velocity += ((close.posWorld-cell.posWorld).normalized)*Time.deltaTime;
			velocity = Maths.Vector3Limit (velocity, maxSpeed);
		}
	}

	


}
