using UnityEngine;
using System.Collections.Generic;

namespace PathFinding
{
	public class Sector 
	{
		public int	 		Id				{ get; set; } 
		public Cell[,] 		cells			{ get; set; } 
		public bool 		enabled			{ get; set; } 
		public Grid			grid			{ get; set; } 
		public VecXZ		posGrid			{ get; set; } 
		public VecXZ		size			{ get; set; }  
		public Cell[] 		cellsFront		{ get; set; } 
		public Cell[] 		cellsRight		{ get; set; } 
		public Cell[] 		cellsBack		{ get; set; } 
		public Cell[] 		cellsLeft		{ get; set; }
		public List<Cell> 	hierCellsFront	{ get; set; } 
		public List<Cell> 	hierCellsRight	{ get; set; }
		public List<Cell> 	hierCellsBack	{ get; set; } 
		public List<Cell> 	hierCellsLeft	{ get; set; }
		public List<Cell> 	hierCells		{ get; set; }
		public List<Swarm>	swarms			= new List<Swarm>();




		public Sector(){}

		
		

		public void UpdateSector(Cell cell)
		{
			if (cell == null) {
				UpdateRight ();
				if (posGrid.x < grid.sectors.GetLength (0) - 1) {
					Sector otherSector = grid.sectors [posGrid.x + 1, posGrid.z];
					if (otherSector.enabled) {
						otherSector.Clustering ();
						otherSector.Linking ();
					}
				}
				UpdateLeft ();
				UpdateFront ();
				if (posGrid.z < grid.sectors.GetLength (1) - 1) {
					Sector otherSector = grid.sectors [posGrid.x, posGrid.z + 1];
					if (otherSector.enabled) {
						otherSector.Clustering ();
						otherSector.Linking ();
					}
				}
				UpdateBack ();
				Clustering ();
				Linking ();
			} else {
				if (cell.posSector.x > grid.sectorSize - 2) {
					UpdateRight ();
					if (posGrid.x < grid.sectors.GetLength (0) - 1) {
						Sector otherSector = grid.sectors [posGrid.x + 1, posGrid.z];
						if (otherSector.enabled) {
							otherSector.Clustering ();
							otherSector.Linking ();
						}
					}
				}
				if (cell.posSector.x < 1) {
					UpdateLeft ();
				}
				if (cell.posSector.z > grid.sectorSize - 2) {
					UpdateFront ();
					if (posGrid.z < grid.sectors.GetLength (1) - 1) {
						Sector otherSector = grid.sectors [posGrid.x, posGrid.z + 1];
						if (otherSector.enabled) {
							otherSector.Clustering ();
							otherSector.Linking ();
						}
					}
				}
				if (cell.posSector.z < 1) {
					UpdateBack ();
				}
				Clustering ();
				Linking ();
			}
		}



		public void UpdateLeft()
		{
			if( posGrid.x > 0 ){
				Sector otherSector = grid.sectors[posGrid.x-1,posGrid.z];
				if( otherSector.enabled ){
					otherSector.UpdateRight();
					otherSector.Clustering();
					otherSector.Linking ();
				}
			}
		}
		

		
		public void UpdateBack()
		{
			if( posGrid.z > 0 ){
				Sector otherSector = grid.sectors[posGrid.x,posGrid.z-1];
				if( otherSector.enabled ){
					otherSector.UpdateFront();
					otherSector.Clustering();
					otherSector.Linking ();
				}
			}
		}



		public void UpdateRight()
		{
			Cell firstCell = cellsRight [0];
			if (firstCell != null) {
				if ( firstCell.posGrid.x + 2 < grid.columns - 1 ){
					Sector nextSector = grid.cells[firstCell.posGrid.x+1,firstCell.posGrid.z].sector;
					if (hierCellsRight != null) {
						foreach (Cell cell in hierCellsRight) {
							grid.hierNodes.Remove(cell);
							cell.isAHierNode = false;
							cell.hierCellNeighbours = new List<Cell>();
						}
						hierCellsRight.Clear ();
						if ( posGrid.x < grid.sectors.GetLength(0)-1 ){
							Sector otherSector = grid.sectors[posGrid.x+1,posGrid.z];
							if (otherSector.hierCellsLeft != null) {
								foreach (Cell cell in otherSector.hierCellsLeft) {
									grid.hierNodes.Remove(cell);
									cell.isAHierNode = false;
									foreach(Cell p in cell.hierCellNeighbours){
										if( p.posGrid.x-1==cell.posGrid.x || p.sector==cell.sector ){
											p.hierCellNeighbours.Remove(cell);
										}
									}
									cell.hierCellNeighbours = new List<Cell>();
								}
							}
							otherSector.hierCellsLeft = new List<Cell> ();
						}
					} else {
						hierCellsRight = new List<Cell> ();
					}
					
					List<Cell> clist = new List<Cell> ();
					bool searchNewHierNode = false;
					bool addToNextSearch = false;
					Cell prev = null;

					foreach (Cell cell in cellsRight) {
						Cell B = grid.cells[firstCell.posGrid.x  ,cell.posGrid.z];
						Cell C = grid.cells[firstCell.posGrid.x+1,cell.posGrid.z];
						Cell Bprev = B;
						Cell Cprev = C;
						if (cell.posSector.z > 1) {
							Bprev = grid.cells[firstCell.posGrid.x  ,cell.posGrid.z-1];
							Cprev = grid.cells[firstCell.posGrid.x+1,cell.posGrid.z-1];
						}

						if( B.walkable && C.walkable && B.IsANeighbourOf(C) && B.IsANeighbourOf(Bprev) && C.IsANeighbourOf(Cprev) ){ 
							if (prev == null) {
								clist.Add (cell);
							} else {
								if (cell.IsANeighbourOf (prev)) {
									clist.Add (cell);
									if (cell.posSector.z == size.z - 1 ){
										searchNewHierNode = true;
									}
								} else {
									searchNewHierNode = true;
									addToNextSearch = true;
								}
							}
						} else {
							searchNewHierNode = true;
						}
						prev = cell;
						if (searchNewHierNode) {
							if (clist.Count > 0) {
								Cell newHierNodeA = clist [(clist.Count-1) / 2];
								Cell newHierNodeB = grid.cells[newHierNodeA.posGrid.x+1,newHierNodeA.posGrid.z];
								if (!hierCellsRight.Contains (newHierNodeA)) {
									hierCellsRight.Add (newHierNodeA);
									newHierNodeA.isAHierNode = true;
									if (!grid.hierNodes.Contains (newHierNodeA)) {
										grid.hierNodes.Add (newHierNodeA);
									}
									if ( nextSector.hierCellsLeft == null) {
										nextSector.hierCellsLeft = new List<Cell>();
									}
									nextSector.hierCellsLeft.Add(newHierNodeB);
									newHierNodeB.isAHierNode = true;
									if (!grid.hierNodes.Contains (newHierNodeB)) {
										grid.hierNodes.Add (newHierNodeB);
									}	
									CreateLink(newHierNodeA,newHierNodeB);
								}
							}
							clist = new List<Cell> ();
							if (addToNextSearch) {
								clist.Add (cell);
								addToNextSearch = false;
							}
							searchNewHierNode = false;
						}
					}
				}
			}
		}



		public void UpdateFront()
		{
			Cell firstCell = cellsFront[0];
			if (firstCell != null) {
				if ( firstCell.posGrid.z + 2 < grid.rows - 1 ){
					Sector nextSector = grid.cells[firstCell.posGrid.x,firstCell.posGrid.z+1].sector;
					if (hierCellsFront != null) {
						foreach (Cell cell in hierCellsFront) {
							grid.hierNodes.Remove(cell);
							cell.isAHierNode = false;
							cell.hierCellNeighbours = new List<Cell>();
							cell.hierCellDistances = new List<float>();
						}
						hierCellsFront.Clear ();
						if ( posGrid.z < grid.sectors.GetLength(1)-1 ){
							Sector otherSector = grid.sectors[posGrid.x,posGrid.z+1];
							if (otherSector.hierCellsBack != null) {
								foreach (Cell cell in otherSector.hierCellsBack) {
									grid.hierNodes.Remove(cell);
									cell.isAHierNode = false;
									foreach(Cell p in cell.hierCellNeighbours){
										if( p.posGrid.z-1==cell.posGrid.z || p.sector==cell.sector ){
											p.hierCellNeighbours.Remove(cell);
										}
									}
									cell.hierCellNeighbours = new List<Cell>();
									cell.hierCellDistances = new List<float>();
								}
							}
							otherSector.hierCellsBack = new List<Cell> ();
						}
					} else {
						hierCellsFront = new List<Cell> ();
					}
					
					List<Cell> clist = new List<Cell> ();
					bool searchNewHierNode = false;
					bool addToNextSearch = false;
					Cell prev = null;

					foreach (Cell cell in cellsFront) {
						Cell B = grid.cells[cell.posGrid.x,firstCell.posGrid.z  ];
						Cell C = grid.cells[cell.posGrid.x,firstCell.posGrid.z+1];
						Cell Bprev = B;
						Cell Cprev = C;
						if (cell.posSector.x > 0) {
							Bprev = grid.cells [cell.posGrid.x - 1, firstCell.posGrid.z];
							Cprev = grid.cells [cell.posGrid.x - 1, firstCell.posGrid.z + 1];
						}

						if( B.walkable && C.walkable && B.IsANeighbourOf(C) && B.IsANeighbourOf(Bprev) && C.IsANeighbourOf(Cprev) ){ 
							if (prev == null) {
								clist.Add (cell);
							} else {
								if (cell.IsANeighbourOf (prev)) {
									clist.Add (cell);
									
									if (cell.posSector.x == size.x - 1 ){
										searchNewHierNode = true;
									}
									
								} else {
									searchNewHierNode = true;
									addToNextSearch = true;
								}
							}
						} else {
							searchNewHierNode = true;
						}
						prev = cell;
						if (searchNewHierNode) {
							if (clist.Count > 0) {
								Cell newHierNodeA = clist [(clist.Count-1) / 2];
								Cell newHierNodeB = grid.cells[newHierNodeA.posGrid.x,newHierNodeA.posGrid.z+1];
								if (!hierCellsFront.Contains (newHierNodeA)) {
									hierCellsFront.Add (newHierNodeA);
									newHierNodeA.isAHierNode = true;
									if (!grid.hierNodes.Contains (newHierNodeA)) {
										grid.hierNodes.Add (newHierNodeA);
									}
									if ( nextSector.hierCellsBack == null) {
										nextSector.hierCellsBack = new List<Cell>();
									}
									nextSector.hierCellsBack.Add(newHierNodeB);
									newHierNodeB.isAHierNode = true;
									if (!grid.hierNodes.Contains (newHierNodeB)) {
										grid.hierNodes.Add (newHierNodeB);
									}	
									CreateLink(newHierNodeA,newHierNodeB);
								}
							}
							clist = new List<Cell> ();
							if (addToNextSearch) {
								clist.Add (cell);
								addToNextSearch = false;
							}
							searchNewHierNode = false;
						}	
					}
				}
			}
		}




		private Queue<Cell> queueA;
		private Queue<Cell> queueB;
		//
		public void Clustering ()
		{
			queueA = new Queue<Cell> ();
			Cell cell = new Cell ();
			bool found = false;
			foreach (Cell c in cells) {
				if( c.walkableINI ){
					c.idCluster = 0;
					//if( !c.Blocked() ){
					if( c.walkable ){
						cell = c;
						found = true;
					}
				}
			}
			if (found) {
				int clusterValor = 1;
				bool loop = true;
				while (loop) {
					queueA.Enqueue (cell);
					queueB = new Queue<Cell> ();
					cell.idCluster = clusterValor + 10 * cell.sector.Id;
					queueB.Enqueue (cell);
					while (queueB.Count != 0) {
						while (queueB.Count != 0) {
							Cell c = queueB.Dequeue ();
							if (c.neighbours != null) {
								foreach (Cell neighbour in c.neighbours) {
									if (Mathf.Abs (c.posGrid.z - neighbour.posGrid.z) == 0 || Mathf.Abs (c.posGrid.x - neighbour.posGrid.x) == 0) {
										if (neighbour.sector == this) {
											if ( neighbour.walkable ) { 
												if (neighbour.idCluster == 0) { 
													neighbour.idCluster = clusterValor + 10 * cell.sector.Id;
													queueA.Enqueue (neighbour);
												}
											}
										}
									}
								}
							}
						}
						while (queueA.Count != 0) {
							Cell n = queueA.Dequeue ();
							queueB.Enqueue (n);
						}
					}
					int counter = 0;
					foreach (Cell c in cells) {
						if ( c.walkable && c.idCluster == 0 ) {
							counter++;
							clusterValor++;
							cell = c;
							break;
						}
					}
					if (counter == 0) {
						loop = false;
					}
				}
			}
		}



		public void Linking()
		{
			hierCells = new List<Cell> ();
			if (hierCellsFront != null) {
				foreach (Cell cell in hierCellsFront) {
					hierCells.Add (cell);
				}
			}
			if (hierCellsRight != null) {
				foreach (Cell cell in hierCellsRight) {
					hierCells.Add (cell);
				}
			}
			if (hierCellsBack != null) {
				foreach (Cell cell in hierCellsBack) {
					hierCells.Add (cell);
				}
			}
			if (hierCellsLeft != null) {
				foreach (Cell cell in hierCellsLeft) {
					hierCells.Add (cell);
				}
			}

			foreach (Cell cell in hierCells) {
				List<Cell> cList = new List<Cell> ();
				foreach(Cell p in cell.hierCellNeighbours){
					if( p.sector!=cell.sector ){
						cList.Add(p);
					}
				}
				cell.hierCellNeighbours = new List<Cell>();
				cell.hierCellDistances = new List<float>();
				foreach(Cell l in cList){
					cell.hierCellNeighbours.Add(l);
					cell.hierCellDistances.Add( grid.cellSize );
				}
			}

			foreach (Cell A in hierCells) {
				foreach (Cell B in hierCells) {
					if ( A.Id != B.Id ){
						if( A.idCluster == B.idCluster ){
							CreateLink( A, B );
						}
					}
				}
			}
		}



		public void CreateLink( Cell A , Cell B )
		{
			if (A.hierCellNeighbours == null) {
				A.hierCellNeighbours = new List<Cell>();
			}
			if (B.hierCellNeighbours == null) {
				B.hierCellNeighbours = new List<Cell>();
			}
			A.hierCellNeighbours.Add (B);
			B.hierCellNeighbours.Add (A);
			if (A.sector.Id == B.sector.Id) {
				float dist = AstarDistance (A, B);
				A.hierCellDistances.Add (dist);
				B.hierCellDistances.Add (dist);
			}
		}




		/// <summary>
		/// Astar algorithm to calculate shortest distance between two cells A and B.
		/// </summary>
		public float AstarDistance(Cell A, Cell B)
		{
			foreach (Cell cell0 in cells) {
				cell0.state = StateCell.Clear;
				cell0.parent = null;
				cell0.G = 0;
				cell0.F = 0;
				cell0.H = 0;
			}
			HeapCell heapCell = new HeapCell ();
			bool loop = true;
			float distance = 0;
			while(loop){
				heapCell.closeList.Add(A);
				A.state = StateCell.Close;
				for(int i=0; i<A.neighbours.Length; i++){
					if( A.neighbours[i].walkable && A.neighbours[i].sector.Id == Id && A.idCluster == A.neighbours[i].idCluster ){
						if( A.neighbours[i].state == StateCell.Clear ){
							A.neighbours[i].G = A.G + Vector3.Distance(A.posWorld, A.neighbours[i].posWorld);
							A.neighbours[i].H = Vector3.Distance(A.neighbours[i].posWorld, B.posWorld);
							A.neighbours[i].F = A.neighbours[i].G + A.neighbours[i].H;
							A.neighbours[i].parent = A;
						}else if( A.neighbours[i].state == StateCell.Open ){
							float tempG = A.G + Vector3.Distance(A.posWorld, A.neighbours[i].posWorld);
							if( A.neighbours[i].G > tempG ){
								A.neighbours[i].parent = A;
								A.neighbours[i].G = tempG;
								A.neighbours[i].F = A.neighbours[i].G + A.neighbours[i].H;
							}
						}
					}
				}
				foreach(Cell neighbour in A.neighbours){
					if(neighbour.state == StateCell.Clear  &&  neighbour.walkable  && neighbour.sector.Id == Id && A.idCluster == neighbour.idCluster ){
						neighbour.state = StateCell.Open;
						heapCell.Manager("insert",neighbour);
					}
				}
				if ( heapCell.openList.Count == 0 ) {
					loop=false;
				}else{ 
					A = heapCell.openList[0];
					heapCell.Manager("remove0");
				}
				if ( A == B ) {
					loop=false;
				}
			}
			if(heapCell.openList.Count != 0){ // If path is found.
				Cell prev = A;
				while(A!=null){
					distance += Vector3.Distance(prev.posWorld,A.posWorld);
					prev = A;
					A = A.parent;
				}	
			}else{ // If path is not found.
				distance = -1;
			}
			return distance;
		}



		// ************************************************************************************************************************************************************************
	}
}