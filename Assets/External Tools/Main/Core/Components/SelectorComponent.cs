using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFinding;

public class SelectorComponent : MonoBehaviour 
{
	public Color 			RectangleSelectionColor 	= new Color (0.8f, 0.8f, 0.95f, 0.25f);
	public bool 			RectangleSelectionBorder 	= true;
	public bool 			RectangleSelectionFilling 	= true;
	public GameObject 		AgentSelectionCircle;
	public bool				SearchAlternativeTarget		= false;

	private bool 			isSelecting = false;
	private Vector3 		mousePosition;
	private Texture2D 		whiteTexture;
	private Swarm[]		 	swarms;
	private List<Grid>		grids = new List<Grid> ();
	private List<Agent>		agents = new List<Agent>();

	private bool			swarmCreatedBefore = false;

	void Awake()
	{
		whiteTexture = new Texture2D( 1, 1 );
		whiteTexture.SetPixel( 0, 0, Color.white );
		whiteTexture.Apply();
	}
	
	
	
	void Update()
	{
		StartSelection ();
		WorkingSelection ();
		EndSelection ();
		SetTargetSelection ();
	}
	
	

	public void StartSelection()
	{
		if (Input.GetMouseButtonDown (0)) {
			swarmCreatedBefore = false;
			swarms = new Swarm[1];
			isSelecting = true;
			mousePosition = Input.mousePosition;
			foreach (Agent _agent in Agent.allAgents) {
				if (_agent.selectionCircle != null) {
					Destroy (_agent.selectionCircle.gameObject);
					_agent.selectionCircle = null;
				}
			}
		}
	}



	public void WorkingSelection()
	{
		if (isSelecting) {
			foreach (Agent agent in Agent.allAgents) {
				if (IsWithinSelectionBounds (agent.transform.gameObject)) {
					if (agent.selectionCircle == null) {
						agent.selectionCircle = Instantiate (AgentSelectionCircle);
						agent.selectionCircle.transform.SetParent (agent.transform, false);
						agent.selectionCircle.transform.eulerAngles = new Vector3 (90, 0, 0);
						agent.selectionCircle.GetComponent<Projector> ().orthographicSize = agent.radius;
					}
				} else {
					if (agent.selectionCircle != null) {
						Destroy (agent.selectionCircle.gameObject);
						agent.selectionCircle = null;
					}
				}
			}
		}
	}



	public void EndSelection()
	{
		if (Input.GetMouseButtonUp (0)) {
			agents.Clear();
			grids.Clear();
			foreach (Agent agent in Agent.allAgents) {
				if (IsWithinSelectionBounds (agent.transform.gameObject)) {
					agents.Add(agent);
					if( !grids.Contains(agent.grid) ){
						grids.Add(agent.grid);
					}
				}
			}
			isSelecting = false;
		}
	}


	public void SetTargetSelection()
	{
		if ( Input.GetMouseButtonDown (1) && agents.Count>0 ) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				if( swarmCreatedBefore == false ){
					swarms = new Swarm[grids.Count];
					Grid[] _gridsArray = grids.ToArray();
					for(int i=0;i<grids.Count;i++){
						swarms[i] = new Swarm(_gridsArray[i]);
					}
					
					foreach(Swarm _swarm in swarms){
						_swarm.agents.Clear();
						foreach (Agent _agent in agents) {
							if(_swarm.grid == _agent.grid){
								_swarm.agents.Add (_agent);
								if (_agent.swarm != null) {
									Swarm swarmBefore = _agent.swarm;
									swarmBefore.agents.Remove (_agent);
									if (swarmBefore.agents.Count == 0) {
										Swarm.allSwarms.Remove (swarmBefore);
									}
								}
								_agent.swarm = _swarm;
							}
						}
					}
					swarmCreatedBefore = true;
				}
				foreach(Swarm swarm in swarms){
					Cell cellHitPoint = swarm.grid.GetCell(hit.point);
					if( cellHitPoint != null && cellHitPoint.walkable){// && !cellHitPoint.inBorder ){
						swarm.GoTo(hit.point);
					}else{
						if( SearchAlternativeTarget ){
							float distance = Mathf.Infinity;
							Vector3 newHitPoint = Vector3.zero;
							foreach(Cell c in swarm.grid.cells){
								if( c.walkable && !c.inBorder){
									float distanceToCheck = Vector3.Distance(hit.point,c.posWorld );
									if( distanceToCheck < distance ){
										distance = distanceToCheck;
										newHitPoint = c.posWorld;
									}
								}
							}
							if( distance < swarm.grid.cellSize*5 ){
								swarm.GoTo(newHitPoint);
							}
						}
					}
				}
			}
		}
	}



	public bool IsWithinSelectionBounds( GameObject gameObject )
	{
		if (!isSelecting) {
			return false;
		}
		Bounds viewportBounds = GetViewportBounds( Camera.main , mousePosition, Input.mousePosition );
		return viewportBounds.Contains( Camera.main.WorldToViewportPoint( gameObject.transform.position ) );
	}
	
	
	
	void OnGUI()
	{
		if ( isSelecting ){
			Rect rect = GetScreenRect( mousePosition, Input.mousePosition );
			if ( RectangleSelectionFilling ){ 
				DrawScreenRect( rect, RectangleSelectionColor );
			}
			if ( RectangleSelectionBorder ){
				DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
			}
		}
	}
	
	
	
	public Rect GetScreenRect( Vector3 screenPosition1, Vector3 screenPosition2 )
	{
		screenPosition1.y = Screen.height - screenPosition1.y;
		screenPosition2.y = Screen.height - screenPosition2.y;
		Vector3 topLeft = Vector3.Min( screenPosition1, screenPosition2 );
		Vector3 bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
		return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
	}
	
	
	
	public void DrawScreenRect( Rect rect, Color color )
	{
		GUI.color = color;
		GUI.DrawTexture( rect, whiteTexture );
		GUI.color = Color.white;
	}
	
	
	
	public void DrawScreenRectBorder( Rect rect, float thickness, Color color )
	{
		DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
		DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
		DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color );
		DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
	}
	
	
	
	public Bounds GetViewportBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
	{
		Vector3 v1 = camera.ScreenToViewportPoint( screenPosition1 );
		Vector3 v2 = camera.ScreenToViewportPoint( screenPosition2 );
		Vector3 min = Vector3.Min( v1, v2 );
		Vector3 max = Vector3.Max( v1, v2 );
		min.z = camera.nearClipPlane;
		max.z = camera.farClipPlane;
		Bounds bounds = new Bounds();
		bounds.SetMinMax( min, max );
		return bounds;
	}

}
