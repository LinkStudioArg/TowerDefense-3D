using UnityEngine;
using System.Collections.Generic;

namespace PathFinding
{


	public enum StateCell{Clear, Open, Close, assigned,unassigned};



	public class Cell 
	{
		public int			Id					{ get; set; }
		public Grid 		grid				{ get; set; }
		public Cell[] 		neighbours			{ get; set; }
		public VecXZ		posGrid				{ get; set; }
		public VecXZ		posSector			{ get; set; }
		public Vector3		posWorld			{ get; set; }
		public Vector3[] 	vertexPos			{ get; set; }
		public bool 		walkable			{ get; set; }
		public bool 		walkableINI			{ get; set; }
		public bool 		inBorder			{ get; set; }
		public Bounds		bounds				{ get; set; }
		public Sector		sector				{ get; set; }
		public bool			isAHierNode			{ get; set; }
		public int			idCluster			{ get; set; }
		public List<Cell> 	hierCellNeighbours	{ get; set; }
		public List<float> 	hierCellDistances	{ get; set; }
		public int 			integrationField	{ get; set; }
		public Vector3		unWalkableOrigin	{ get; set; }
		public Cell 		parent;
		public float 		G;
		public float 		H;
		public float 		F;
		public StateCell  	state = StateCell.Clear;




		public Cell(){}




		public Cell[] GetNeighbors()
		{
			int x = posGrid.x;
			int y = posGrid.z;
			List<Cell> list = new List<Cell>();
			// NON DIAGONALS
			// left
			if (x > 0){
				list.Add(grid.cells[x-1, y]); 
			}
			// right
			if (x < grid.cells.GetLength(0)-1){
				list.Add(grid.cells[x+1, y]);
			}
			// bottom
			if (y > 0){
				list.Add(grid.cells[x, y-1]);
			}
			// upper
			if (y < grid.cells.GetLength(1)-1){
				list.Add(grid.cells[x, y+1]);
			}
			// DIAGONALS
			// bottom-left
			if (x > 0 && y > 0){
				list.Add(grid.cells[x-1, y-1]);
			}
			// upper-right
			if (x < grid.cells.GetLength(0)-1 && y < grid.cells.GetLength(1)-1){
				list.Add(grid.cells[x+1, y+1]);
			}
			// upper-left
			if (x > 0 && y < grid.cells.GetLength(1)-1){
				list.Add(grid.cells[x-1, y+1]);
			}
			// bottom-right
			if (x < grid.cells.GetLength(0)-1 && y > 0){
				list.Add(grid.cells[x+1, y-1]);
			}
			return list.ToArray();
		}




		public bool IsInEdge()
		{
			if (posSector.x == grid.sectorSize - 1 || posSector.z == grid.sectorSize - 1 || posSector.x == 0 || posSector.z == 0) {
				return true;
			} else {
				return false;
			}
		}




		public void UpdateSector()
		{
			sector.UpdateSector(this);
		}




		public bool IsANeighbourOf( Cell other )
		{
			if (Id == other.Id) {
				return true;
			}
			foreach (Cell neighbour in neighbours) {
				if( neighbour.Id == other.Id ){
					return true;
				}
			}
			return false;
		}




		public bool SearchVertex(float size)
		{
			float cellSize = grid.cellSize;
			LayerMask walkableLayer = grid.WalkableLayer;
			RaycastHit hit;
			vertexPos = new Vector3[4];
			bool allVertexFounded = true;
			if(allVertexFounded){
				if(Physics.Raycast (new Vector3(posWorld.x-cellSize*size, posWorld.y+cellSize, posWorld.z+cellSize*size), Vector3.down, out hit,cellSize*2,walkableLayer)) { 
					vertexPos[0] = hit.point;
					if( Mathf.Abs(posWorld.y-hit.point.y)>cellSize ){
						allVertexFounded = false;
					}
				}else{
					allVertexFounded = false;
				}
			}
			if(allVertexFounded){
				if(Physics.Raycast (new Vector3(posWorld.x+cellSize*size, posWorld.y+cellSize, posWorld.z+cellSize*size), Vector3.down, out hit,cellSize*2,walkableLayer)) { 
					vertexPos[1] = hit.point;
					if( Mathf.Abs(posWorld.y-hit.point.y)>cellSize ){
						allVertexFounded = false;
					}
				}else{
					allVertexFounded = false;
				}
			}
			if(allVertexFounded){
				if(Physics.Raycast (new Vector3(posWorld.x+cellSize*size, posWorld.y+cellSize, posWorld.z-cellSize*size), Vector3.down, out hit,cellSize*2,walkableLayer)) {
					vertexPos[2] = hit.point;
					if( Mathf.Abs(posWorld.y-hit.point.y)>cellSize ){
						allVertexFounded = false;
					}
				}else{
					allVertexFounded = false;
				}
			}
			if(allVertexFounded){
				if(Physics.Raycast (new Vector3(posWorld.x-cellSize*size, posWorld.y+cellSize, posWorld.z-cellSize*size), Vector3.down, out hit,cellSize*2,walkableLayer)) { 
					vertexPos[3] = hit.point;
					if( Mathf.Abs(posWorld.y-hit.point.y)>cellSize ){
						allVertexFounded = false;
					}
				}else{
					allVertexFounded = false;
				}
			}
			if (!allVertexFounded) {
				vertexPos [0] = Vector3.zero;// North-West
				vertexPos [1] = Vector3.zero;// North-East
				vertexPos [2] = Vector3.zero;// South-East
				vertexPos [3] = Vector3.zero;// South-West
			}
			return allVertexFounded;
		}




		public Cell SearchClosestHierNode()
		{
			foreach (Cell c  in sector.hierCells) {
				if( c.idCluster == idCluster ){
					return c;
				}
			}
			return null;
		}




		public Cell SearchClosestWalkableCell(bool checkBorder=false )
		{
			float distance = grid.cellSize*10;
			Cell closestCell = this;
			if (walkable) {
				foreach (Cell cell in sector.cells) {
					if (cell.idCluster == idCluster) {
						bool ready = true;
						if (checkBorder) {
							if (cell.inBorder)
								ready = false;
						} 
						if (ready) {
							float distanceToCheck = Vector3.Distance (posWorld, cell.posWorld);
							if (distanceToCheck < distance) {
								distance = distanceToCheck;
								closestCell = cell;
							}
						}
					}
				}
			} else {
				foreach(Cell cell in grid.cells){
					bool ready = true;
					if (cell.walkable) {
						if (checkBorder) {
							if (cell.inBorder)
								ready = false;
						}
					} else {
						ready = false;
					}
					if( ready ){
						float distanceToCheck = Vector3.Distance(posWorld,cell.posWorld );
						if( distanceToCheck < distance ){
							distance = distanceToCheck;
							closestCell = cell;
						}
					}
				}
			}	
			if (closestCell != this) {
				return closestCell;
			} else {
				return null;
			}
		}
	



	}

}
