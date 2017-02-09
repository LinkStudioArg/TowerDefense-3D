using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFinding;

public class GridComponent : MonoBehaviour 
{
	public Grid      		grid 				= new Grid ();
	public bool      		gizmos      		= true;
	public Color			gizmosColor			= new Color(0,255,255,125);
	public LayerMask 		walkable;
	public LayerMask 		unwalkable;
	public LayerMask 		agents;
	private List<Cell>		oldPropagators		= new List<Cell>();
	private List<Sector> 	sectors 			= new List<Sector>();
	private bool			updateGizmos		= false;


	// ************************************************************************************************************************************************************************





	void Awake()
	{
		grid.GenerateGrid (transform.position , walkable , unwalkable ,agents);
		Grid.AllGrids.Add (grid);
		InvokeRepeating("PropagationUpdate", 0.001f, 1.0f/grid.updateFrequency);
	}


	public void LateUpdate () 
	{
		updateGizmos = true;
		int agentsInGame = grid.agents.Count;
		for (var i = 0; i < agentsInGame; i++){
			grid.agents[i].Update();
		}
	}

	
	public void PropagationUpdate()
	{
		UpdateAndDeleteOldPropagators ();
		UpdateNewPropagators ();
		DeleteNewPropagators ();
	}
	
	

	void UpdateAndDeleteOldPropagators()
	{

		List<Cell> toDelete = new List<Cell> ();
		foreach (Cell cell in oldPropagators) {
			toDelete.Add(cell);	
		}

		foreach (Sector sector in sectors) {
			if( sector.swarms!=null ){
				foreach (Swarm s in sector.swarms) {
					s.toUpdate = false;
				}
			}
			sector.UpdateSector(null);
		}
		sectors.Clear ();

		foreach (Cell cell in toDelete){
			oldPropagators.Remove(cell);
		}
	}



	void UpdateNewPropagators()
	{
		foreach (Cell cell in grid.propagators) {
			oldPropagators.Add (cell);
			cell.walkable = false;

			if (!sectors.Contains (cell.sector)) {
				sectors.Add (cell.sector);
			}
		}
		foreach (Sector sector in sectors) {
			foreach (Swarm s in sector.swarms) {
				s.toUpdate = true;
			}
			sector.UpdateSector (null);
		}
	}


	void DeleteNewPropagators()
	{
		foreach (Cell cell in grid.cells) {
			if( cell.walkable != cell.walkableINI  && !grid.propagators.Contains(cell) ){
				cell.walkable = cell.walkableINI;
			}
		}
		List<Cell> toDelete = new List<Cell> ();
		foreach (Cell cell in grid.propagators){
			toDelete.Add(cell);
		}
		foreach (Cell cell in toDelete){
			grid.propagators.Remove(cell);
		}
	}


	public bool showCells = false;
	public bool	showCellLinks = false;
	public bool showSectors = false;
	public bool showHierNodes = false;
	public bool showHierNodeLinks = false;
	public bool showFlowField = false;
	public bool showBorders = false;

	void OnDrawGizmos()
	{

		if ( gizmos ) {
			// Define color 
			Gizmos.color = gizmosColor;
			// Mark origin grid.
			Gizmos.DrawCube( transform.position, grid.cellSize*Vector3.one );
			// Draw grid.
			float gridSideX = (grid.columns * grid.cellSize);
			float gridSideZ = (grid.rows * grid.cellSize);
			Gizmos.DrawWireCube (transform.position + new Vector3 (gridSideX * 0.5f, grid.height*0.5f, gridSideZ * 0.5f), new Vector3 (gridSideX, grid.height, gridSideZ));

			if (updateGizmos) {
				updateGizmos = false;
				if (grid.cells != null) {
					Vector3 Offset = Vector3.up * 0.15f;
					foreach (Cell cell in grid.cells) {
						if (showCellLinks || showCells) {
							if (cell != null) {
								if (showCells) {
									if (cell.walkable) {
										if (!showBorders) {
											Gizmos.color = gizmosColor;
											DrawCell (cell, Offset, true);
										} else {
											if (cell.inBorder) {
												Gizmos.color = Color.yellow;
												DrawCell (cell, Offset, false);
											} else {
												Gizmos.color = gizmosColor;
												DrawCell (cell, Offset, true);
											}
										}

									}

									if ((!cell.walkable && cell.walkableINI)) {
										Gizmos.color = Color.red;
										DrawCell (cell, Offset, false);
									}


								}
								if (showCellLinks) {
									Gizmos.color = gizmosColor * 0.75f;
									if (cell.neighbours != null) {
										foreach (Cell otherCell in cell.neighbours) {
											if (otherCell.walkable && cell.walkable) {
												Vector3 dir = (otherCell.posWorld - cell.posWorld).normalized;
												if (cell.posGrid.z == otherCell.posGrid.z || cell.posGrid.x == otherCell.posGrid.x) {
													Gizmos.DrawLine (cell.posWorld + Offset, cell.posWorld + Offset + dir * 0.4f * grid.cellSize);
												} else {
													Gizmos.DrawLine (cell.posWorld + Offset, cell.posWorld + Offset + dir * 0.5f * grid.cellSize);
												}
											}
										}
									}
								}
							}
						}
					}
					if (showSectors || showHierNodes || showHierNodeLinks || showFlowField) {
						foreach (Sector sector in grid.sectors) {
							if (sector.enabled) {
								if (showSectors) {
									Gizmos.color = gizmosColor;
									foreach (Cell cell in sector.cellsFront) {
										if (cell.walkableINI)
											Gizmos.DrawLine (cell.vertexPos [0] + Offset, cell.vertexPos [1] + Offset);
									}
									foreach (Cell cell in sector.cellsRight) {
										if (cell.walkableINI)
											Gizmos.DrawLine (cell.vertexPos [1] + Offset, cell.vertexPos [2] + Offset);
									}
									foreach (Cell cell in sector.cellsBack) {
										if (cell.walkableINI)
											Gizmos.DrawLine (cell.vertexPos [2] + Offset, cell.vertexPos [3] + Offset);
									}
									foreach (Cell cell in sector.cellsLeft) {
										if (cell.walkableINI)
											Gizmos.DrawLine (cell.vertexPos [3] + Offset, cell.vertexPos [0] + Offset);
									}
								}
								if (showHierNodes || showHierNodeLinks) {
									if (sector.hierCells != null) {
										foreach (Cell cell in sector.hierCells) {
											if (showHierNodes) {
												Gizmos.color = Color.cyan;
												DrawCell (cell, Offset, false);
											}
											if (showHierNodeLinks) {
												Gizmos.color = Color.green;
												foreach (Cell otherCell in cell.hierCellNeighbours) {
													Vector3 dir = (otherCell.posWorld - cell.posWorld).normalized;
													float mag = (otherCell.posWorld - cell.posWorld).magnitude;
													Gizmos.DrawLine (cell.posWorld + Offset, cell.posWorld + Offset + 0.49f * mag * dir);
												}
											}
										}
									}
								}
							}
						}
					}

					if (showFlowField && Swarm.allSwarms != null) {
						Gizmos.color = gizmosColor * 1.25f;
						if (Swarm.allSwarms.Count > 0) {
							Swarm swarm = Swarm.allSwarms [Swarm.allSwarms.Count - 1];
							for (int i = 0; i < grid.columns; i++) {
								for (int j = 0; j < grid.rows; j++) {
									Cell cell = grid.cells [i, j];
									if (cell.walkableINI) {
										Vector3 flow = swarm.flow [i, j];
										if (flow != Vector3.zero) {
											Gizmos.DrawLine (grid.cells [i, j].posWorld + Offset, grid.cells [i, j].posWorld + 0.5f * flow + Offset);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}



	void DrawCell( Cell cell,Vector3 Offset, bool normal=true, float size=0.45f )
	{
		if (normal) {
			Gizmos.DrawLine (cell.vertexPos [0] + Offset, cell.vertexPos [1] + Offset);
			Gizmos.DrawLine (cell.vertexPos [1] + Offset, cell.vertexPos [2] + Offset);
			if( cell.inBorder ){
				if( cell.posGrid.z > 0 ){
					Cell otherCell = grid.cells[cell.posGrid.x,cell.posGrid.z-1];
					if( !otherCell.walkable ){
						Gizmos.DrawLine (cell.vertexPos [2] + Offset, cell.vertexPos [3] + Offset);
					}else{
						if( !cell.IsANeighbourOf(otherCell) ){
								Gizmos.DrawLine (cell.vertexPos [2] + Offset, cell.vertexPos [3] + Offset);
						}
					}
				}
				if( cell.posGrid.x > 0 ){
					Cell otherCell = grid.cells[cell.posGrid.x-1,cell.posGrid.z];
					if( !otherCell.walkable ){
						Gizmos.DrawLine (cell.vertexPos [3] + Offset, cell.vertexPos [0] + Offset);
					}else{
						if( !cell.IsANeighbourOf(otherCell) ){
							Gizmos.DrawLine (cell.vertexPos [3] + Offset, cell.vertexPos [0] + Offset);
						}
					}
				}
			}else{
				if( showBorders ){
					if( cell.posGrid.z > 0 ){
						Cell otherCell = grid.cells[cell.posGrid.x,cell.posGrid.z-1];
						if( otherCell.inBorder ){
							Gizmos.DrawLine (cell.vertexPos [2] + Offset, cell.vertexPos [3] + Offset);
						}else{
							if( !cell.IsANeighbourOf(otherCell) ){
								Gizmos.DrawLine (cell.vertexPos [2] + Offset, cell.vertexPos [3] + Offset);
							}
						}
					}
					if( cell.posGrid.x > 0 ){
						Cell otherCell = grid.cells[cell.posGrid.x-1,cell.posGrid.z];
						if( otherCell.inBorder ){
							Gizmos.DrawLine (cell.vertexPos [3] + Offset, cell.vertexPos [0] + Offset);
						}else{
							if( !cell.IsANeighbourOf(otherCell) ){
								Gizmos.DrawLine (cell.vertexPos [3] + Offset, cell.vertexPos [0] + Offset);
							}
						}
					}
				}
			}
		} else {
			Vector3 vrtx0 = (cell.vertexPos[0] - cell.posWorld).normalized;
			Vector3 vrtx1 = (cell.vertexPos[1] - cell.posWorld).normalized;
			Vector3 vrtx2 = (cell.vertexPos[2] - cell.posWorld).normalized;
			Vector3 vrtx3 = (cell.vertexPos[3] - cell.posWorld).normalized;
			Gizmos.DrawLine(cell.posWorld + Offset + vrtx0*size*grid.cellSize , cell.posWorld + Offset + vrtx1*size*grid.cellSize );
			Gizmos.DrawLine(cell.posWorld + Offset + vrtx1*size*grid.cellSize , cell.posWorld + Offset + vrtx2*size*grid.cellSize );
			Gizmos.DrawLine(cell.posWorld + Offset + vrtx2*size*grid.cellSize , cell.posWorld + Offset + vrtx3*size*grid.cellSize );
			Gizmos.DrawLine(cell.posWorld + Offset + vrtx3*size*grid.cellSize , cell.posWorld + Offset + vrtx0*size*grid.cellSize );
		}
	}



}	


