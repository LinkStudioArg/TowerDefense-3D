using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFinding;

public class Swarm
{
	static public List<Swarm> 	allSwarms 	= new List<Swarm>();
	public 	List<Agent> 		agents 		= new List<Agent>();
	public 	List<Sector> 		sectors 	= new List<Sector>();
	private	List<int> 			clusters 	= new List<int> ();
	public 	Grid				grid;
	public 	Vector3[,]			flow;
	public 	Vector3				targetPosWorld;
	public 	Cell				targetCell;
	public 	float 				time;
	public	bool				toUpdate	= false;


	public Swarm(Grid _grid)
	{
		grid = _grid;
		flow = new Vector3[grid.columns,grid.rows];
		allSwarms.Add (this);
		time = Time.time;
	}



	public void GoTo(Vector3 position)
	{
		targetPosWorld = position;
		targetCell = grid.GetCell (targetPosWorld);
		if ( !targetCell.walkable ) {
			targetCell = targetCell.SearchClosestWalkableCell();
		}

		time = Time.time;
		foreach (Agent agent in agents) {
			agent.targetPosWorld = targetPosWorld;
			agent.targetCell = targetCell;
			agent.state = StateAgent.Move;
		}

		foreach (Sector sector in sectors) {
			foreach (Cell c in sector.cells) {  
				flow [c.posGrid.x, c.posGrid.z] = Vector3.zero;
			}
		}
		sectors.Clear ();
		clusters.Clear (); 
		bool updateFlow = false;
		Cell HierNodeEnd = targetCell.SearchClosestHierNode ();
		foreach (Agent agent in agents) {
			if (grid == agent.grid) {                     
				agent.cell = agent.grid.GetCell (agent.transform.position);
				if (!clusters.Contains (agent.cell.idCluster)) {
					clusters.Add (agent.cell.idCluster);
					updateFlow = true;
					if( agent.cell.walkable ){
						Cell HierNodeStart = agent.cell.SearchClosestHierNode ();
						if (HierNodeStart != null && HierNodeEnd != null ){
							if (HierNodeStart.idCluster != HierNodeEnd.idCluster) { 
								AddToSectors(agent.cell.sector);
								Cell[] hierNodes = agent.swarm.FindHierachalPath (HierNodeStart, HierNodeEnd); 
								if ( hierNodes.Length == 0 ) {
									updateFlow = false;
									break;
								}else{
									foreach (Cell hNode in hierNodes) {
										AddToSectors(hNode.sector);
									}	
								}
							}else{
								AddToSectors(HierNodeStart.sector);
							}
						}else{
							AddToSectors(agent.cell.sector);
						}
					}else{
						AddToSectors(agent.cell.sector);
					}
				}
			}
		}	
		if (updateFlow) {
			CalculateIntegrationField (targetCell); 
			CalculateFlowField ();
		} else {
			sectors.Clear ();
			foreach (Agent agent in agents) {
				if (grid == agent.grid) {
					agent.cell = agent.grid.GetCell (agent.transform.position);
					AddToSectors(agent.cell.sector);
					agent.targetPosWorld = Mathf.Infinity*Vector3.up;
				}
			}
		}
	}



	private void AddToSectors( Sector sector )
	{
		if (!sectors.Contains (sector)) { 
			sector.swarms.Add (this);
			sectors.Add (sector);
		}
	}



	private void CalculateIntegrationField(Cell cell)
	{
		foreach (Sector sector in sectors) {
			foreach (Cell c in sector.cells) {  
				c.integrationField = 65535;
			}
		}
		List<Cell> OpenList = new List<Cell> ();
		OpenList.Add (cell);
		cell.integrationField = 0;
		bool loop = true;
		while (loop){
			List<Cell> CloseList = new List<Cell> ();
			for (int count1 = 0; count1 < OpenList.Count; count1++) {
				Cell current = OpenList[count1]; 
				foreach(Cell n in current.neighbours){ 
					if( sectors.Contains(n.sector) ){
						if( Mathf.Abs(n.posGrid.x - current.posGrid.x) != 1 ||  Mathf.Abs(n.posGrid.z - current.posGrid.z) != 1 ){ 
							//if( !n.Blocked() ){
							if( n.walkable ){	
								if(n.integrationField == 65535){
									n.integrationField = current.integrationField+1;
									CloseList.Add(n);
								}
							}
						}
					}
				}
				bool lineOfViewFree = grid.Bresenham(current.posGrid.x,current.posGrid.z,targetCell.posGrid.x,targetCell.posGrid.z);
				if( lineOfViewFree   ){
					Vector3 flowDir = (targetCell.posWorld-current.posWorld).normalized;
					RaycastHit hit;
					if(Physics.Raycast (current.posWorld + flowDir*grid.cellSize*0.45f + 2*Vector3.up, Vector3.down, out hit,4,grid.WalkableLayer)) {
						flowDir = (hit.point-current.posWorld).normalized;
					} 
					flow[current.posGrid.x,current.posGrid.z] = flowDir;
				}
			}
			if (CloseList.Count!=0) {
				OpenList = CloseList;
			}else{
				loop = false;
			} 
		}

		foreach (Sector sector in sectors) {
			foreach (Cell c in sector.cells) {
				if (c.inBorder) {
					c.integrationField = 65534;
				}
			}
		}
	}

	

	private void CalculateFlowField()
	{
		Cell nodeWithLowerIntegrationField = new Cell();
		int integrationField = int.MaxValue;
		foreach (Sector sector in sectors) {
			foreach (Cell node in sector.cells ) { 
				if( flow[node.posGrid.x,node.posGrid.z]== Vector3.zero ){
					if( !node.walkable || node.integrationField==65535 ){
						flow[node.posGrid.x,node.posGrid.z] = Vector3.zero;
					}else{
						integrationField = int.MaxValue;
						foreach( Cell neighbour in node.neighbours ){
							if( sectors.Contains( neighbour.sector) ){
								// Check if all neighbours are blocked
								bool allneigboursBlocked = false;
								int neigboursBlocked = 0;
								foreach( Cell neigbour in node.neighbours ){
									if( !neigbour.walkable ) {
										neigboursBlocked ++;
										neigbour.integrationField = 65535;
									}
								}
								if( neigboursBlocked == node.neighbours.Length ){
									flow[node.posGrid.x,node.posGrid.z] = Vector3.zero;
									allneigboursBlocked = true;
								}
								// If some neighbour is not blocked search the node with integration field lower.
								if( allneigboursBlocked == false ){
									// Axis neighbours
									if( Mathf.Abs(neighbour.posGrid.x - node.posGrid.x) != 1 ||  Mathf.Abs(neighbour.posGrid.z - node.posGrid.z) != 1 ){
										if( neighbour.integrationField < integrationField ){
											if( neighbour.walkable ){
												nodeWithLowerIntegrationField = neighbour;
												integrationField = neighbour.integrationField;
											}
										}
									// Diagonal neighbours
									}else{
										//Right Up
										if( neighbour.posGrid.x == node.posGrid.x+1 && neighbour.posGrid.z == node.posGrid.z+1 ){
											if( node.neighbours.Length>6 ) 
											if( node.neighbours[4].walkable && node.neighbours[6].walkable ){
												if( neighbour.integrationField < integrationField ){
													if( neighbour.walkable ){
														nodeWithLowerIntegrationField = neighbour;
														integrationField = neighbour.integrationField;
													}
												}
											}
										}
										//Right Down
										if( neighbour.posGrid.x == node.posGrid.x+1 && neighbour.posGrid.z == node.posGrid.z-1 ){
											if( node.neighbours.Length>6 )
											if( node.neighbours[3].walkable && node.neighbours[6].walkable ){
												if( neighbour.integrationField < integrationField ){
													if( neighbour.walkable ){
														nodeWithLowerIntegrationField = neighbour;
														integrationField = neighbour.integrationField;
													}
												}
											}
										}
										//Left Down
										if( neighbour.posGrid.x == node.posGrid.x-1 && neighbour.posGrid.z == node.posGrid.z-1 ){
											if( node.neighbours.Length>3 )
											if( node.neighbours[3].walkable && node.neighbours[1].walkable ){
												if( neighbour.integrationField < integrationField ){
													if( neighbour.walkable ){
														nodeWithLowerIntegrationField = neighbour;
														integrationField = neighbour.integrationField;
													}
												}
											}
										}
										//Left Up
										if( neighbour.posGrid.x == node.posGrid.x-1 && neighbour.posGrid.z == node.posGrid.z+1 ){
											if( node.neighbours.Length>4 ) 
											if( node.neighbours[4].walkable && node.neighbours[1].walkable ){
												if( neighbour.integrationField < integrationField ){
													if( neighbour.walkable ){
														nodeWithLowerIntegrationField = neighbour;
														integrationField = neighbour.integrationField;
													}
												}
											}
										}
									}
								}
						
								flow [node.posGrid.x, node.posGrid.z] = (nodeWithLowerIntegrationField.posWorld - node.posWorld).normalized ;

							}
						}
					}
				}
			}
		}
	}

	

	private Cell[] FindHierachalPath(Cell cell, Cell end)
	{
		foreach (Cell cell0 in cell.grid.hierNodes) {
			cell0.state = StateCell.Clear;
			cell0.parent = null;
			cell0.G = 0;
			cell0.F = 0;
			cell0.H = 0;
		}
		HeapCell heapCell = new HeapCell ();
		bool loop = true;
		while(loop){
			heapCell.closeList.Add(cell);
			cell.state = StateCell.Close;
			for(int i=0; i<cell.hierCellNeighbours.Count; i++){
				if( cell.hierCellNeighbours[i].walkable ){
					if( cell.hierCellNeighbours[i].state == StateCell.Clear ){
						cell.hierCellNeighbours[i].G = cell.G + cell.hierCellDistances[i] + cell.posWorld.y;
						cell.hierCellNeighbours[i].H = Vector3.Distance(cell.hierCellNeighbours[i].posWorld, end.posWorld);
						cell.hierCellNeighbours[i].F = cell.hierCellNeighbours[i].G + cell.hierCellNeighbours[i].H;
						cell.hierCellNeighbours[i].parent = cell;
					}else if( cell.hierCellNeighbours[i].state == StateCell.Open ){
						float tempG = cell.G + cell.hierCellDistances[i] + cell.posWorld.y;
						if( cell.hierCellNeighbours[i].G > tempG ){
							cell.hierCellNeighbours[i].parent = cell;
							cell.hierCellNeighbours[i].G = tempG;
							cell.hierCellNeighbours[i].F = cell.hierCellNeighbours[i].G + cell.hierCellNeighbours[i].H ;
						}
					}
				}
			}
			foreach(Cell neighbour in cell.hierCellNeighbours){
				if(neighbour.state == StateCell.Clear  &&  neighbour.walkable ) { 
					neighbour.state = StateCell.Open;
					heapCell.Manager("insert",neighbour);
				}
			}
			if(heapCell.openList.Count == 0) {
				loop=false;
			}else{ 
				cell = heapCell.openList[0];
				heapCell.Manager("remove0");
			}
			if(cell==end) {
				loop=false;
			}
		}
		List<Cell> cellsList = new List<Cell> ();
		if(heapCell.openList.Count != 0){ // If path is found.
			while(cell!=null){
				if( !cellsList.Contains(cell) ){
					cellsList.Add(cell);
				}
				cell = cell.parent;
			}
		}
		return cellsList.ToArray();
	}



	public bool IsWithin(Cell cell)
	{
		foreach (Sector sector in sectors) {
			if( sector == cell.sector ){
				foreach(Cell c in sector.cells){
					if( c.posWorld == cell.posWorld ){
						return true;
					}
				}
			}
		}
		return false;
	}




}
