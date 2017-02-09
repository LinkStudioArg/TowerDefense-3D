using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathFinding
{
	[System.Serializable]
	public class Grid 
	{
		static public   	List<Grid> 		AllGrids = new List<Grid>();

		public int      	columns     	=   128;
		public int       	rows        	=   128;
		public float		height			=     1;
		public float      	cellSize    	=     1;	
		public int			maxSlope		=    45; //Maximum 45 degrees.
		public int			sectorSize		=    10;
		public bool			erosion			= false;
		public int			updateFrequency	=     5;
		public Cell[,] 		cells			{ get; set; }
		public Vector3 		posWorld		{ get; set; }
		public LayerMask 	WalkableLayer	{ get; set; }
		public LayerMask 	UnwalkableLayer	{ get; set; }
		public LayerMask 	AgentsLayer		{ get; set; }
		public Sector[,] 	sectors			{ get; set; }
		public List<Cell> 	hierNodes		{ get; set; }
		public List<Agent> 	agents 			{ get; set; }
		public List<Cell>	propagators		{ get; set; }




		public Grid(){}




		public Cell GetCell( Vector3 point )
		{
			int i = (int)((point.x - posWorld.x ) / cellSize);
			int k = (int)((point.z - posWorld.z ) / cellSize);
			if( i>-1 && i<columns && k>-1 && k<rows ) {	
				return cells [i,k];
			}
			return null;
		}




		public void GenerateGrid( Vector3 _posWorld , LayerMask _walkable, LayerMask _unwalkable , LayerMask _agents)
		{
			if (maxSlope > 45)
				maxSlope = 45;
			if (sectorSize < 3)
				sectorSize = 3;
			WalkableLayer = _walkable;
			UnwalkableLayer = _unwalkable;
			AgentsLayer = _agents;
			posWorld = _posWorld;
			hierNodes = new List<Cell> ();
			agents = new List<Agent> ();
			//Create and initialize a array of cells.
			cells = new Cell[columns,rows];
			//Calculate how many sectors will be and create its color;
			int secInX = columns / sectorSize;
			if (columns % sectorSize != 0)
				secInX = secInX + 1;
			int secInZ = rows / sectorSize;
			if (rows % sectorSize != 0)
				secInZ = secInZ + 1;
			sectors = new Sector[secInX,secInZ];
			for( int i=0 ; i<secInX ; i++ ){
				for( int k=0 ; k<secInZ ; k++ ){
					sectors[i,k] = new Sector();
					Sector sector = sectors[i,k];
					sector.posGrid = new VecXZ(i,k);
					sector.grid = this;
					int sizeX = sectorSize ;
					int sizeZ = sectorSize ;
					sectors[i,k].size = new VecXZ(sectorSize,sectorSize);
					if( i == secInX-1 ){
						sizeX = sectorSize - ((i+1)*sectorSize - columns) ;
					}
					if( k == secInZ-1 ){
						sizeZ = sectorSize - ((k+1)*sectorSize - rows) ;
					}
					sector.size = new VecXZ(sizeX,sizeZ);
					sector.cells = new Cell[sizeX,sizeZ];
				}	
			}
			//For all cells in array ...
			for (int i=0; i<columns; i++) {
				for (int k=0; k<rows; k++) {
					//Calculate position.
					float posx = posWorld.x + i * cellSize + cellSize * 0.5f;
					float posy = posWorld.y ;
					float posz = posWorld.z + k * cellSize + cellSize * 0.5f;
					//Initialize cell.
					cells[i,k] = new Cell();
					cells[i,k].Id = k * columns + i;
					cells[i,k].grid = this;
					cells[i,k].posWorld = new Vector3(posx, posy, posz);
					cells[i,k].posGrid = new VecXZ(i,k);
					//Initialize variables to ...
					bool walkable = false;
					RaycastHit hit;
					//... search a walkable place in cell position.
					if(Physics.Raycast (new Vector3(posx, posy + 0.9f*height, posz), Vector3.down, out hit,height,WalkableLayer)) {
						//Update cell position.
						cells[i,k].posWorld = new Vector3(posx, hit.point.y, posz);
						//Check if cell is inside a object.
						LayerMask mask = WalkableLayer | UnwalkableLayer;
						Vector3 Offset = Vector3.up  * 0.5f ;
						Collider[] hitColliders = Physics.OverlapSphere (hit.point + Offset, 0.24f,mask); 
						if( hitColliders.Length==0 ) {
							walkable = true;
							walkable = cells[i,k].SearchVertex(0.49f);
						}else{
							walkable = false;
						}
					}else if(Physics.Raycast (new Vector3(posx, posy + 0.9f*height, posz), Vector3.down, out hit,height,UnwalkableLayer)) {
						walkable = false;
					} 
					//Update walkable fields.
					cells[i,k].walkable = walkable;  
					cells[i,k].walkableINI = walkable;
					//
					if( walkable ){
						cells[i,k].hierCellNeighbours = new List<Cell>();
						cells[i,k].hierCellDistances = new List<float>();
						cells[i,k].bounds = new Bounds(cells[i,k].posWorld + Vector3.up*cellSize,new Vector3(cellSize,cellSize*2,cellSize));
					}
					//Attach sector to cell sector field.
					int secPosX = (i/sectorSize);
					int secPosZ = (k/sectorSize);
					cells[i,k].sector = sectors[secPosX,secPosZ];
					cells[i,k].sector.Id = secPosZ * secInX + secPosX;
					//Calculate cell position in sector.
					int posSectorX = (i+1)%sectorSize - 1;
					if( posSectorX == -1 )
						posSectorX = sectorSize-1;
					int posSectorZ = (k+1)%sectorSize - 1;
					if( posSectorZ == -1 )
						posSectorZ = sectorSize-1;
					cells[i,k].posSector =  new VecXZ(posSectorX,posSectorZ);
					//Attach this cell to its sector
					cells[i,k].sector.cells[ posSectorX,posSectorZ] = cells[i,k];
					cells[i,k].inBorder = false;
				}
			}
				
			SearchNeighbours ();
			if (erosion) { CreateErosion(); }

			propagators = new List<Cell> ();
			foreach (Cell cell in cells) {
				if( cell.neighbours.Length < 8 ){
					bool willBeInBorder = false;
					if( cell.posGrid.x == 0 || cell.posGrid.z == 0 || cell.posGrid.x == columns-1 || cell.posGrid.z == rows-1 ){
						willBeInBorder = true;
					}else{
						Cell[] neighbours = cell.GetNeighbors();
						foreach( Cell n in neighbours){
							if( !n.walkable || !cell.IsANeighbourOf(n) ){
								willBeInBorder = true;
								break;
							}
						}
					}
					if( willBeInBorder ){
						cell.inBorder = true;
					}
				}
			}

			SearchSectors ();
		}




		private void SearchSectors()
		{
			foreach (Sector sector in sectors) {
				bool enabled = false;
				foreach( Cell cell in sector.cells){
					if( cell.walkableINI ){
						enabled = true;
						break;
					}
				}
				sector.enabled = enabled;
				if ( sector.enabled ){
					List<Cell> 	cellsFront 	= new List<Cell> ();
					List<Cell> 	cellsRight 	= new List<Cell> ();
					List<Cell> 	cellsBack 	= new List<Cell> ();
					List<Cell> 	cellsLeft 	= new List<Cell> ();
					foreach( Cell cell in sector.cells ){
						if( cell.posSector.x==sector.size.x-1 ){
							cellsRight.Add(cell);
						}
						if( cell.posSector.z==sector.size.z-1 ){
							cellsFront.Add(cell);
						}	
						if( cell.posSector.x==0 ){
							cellsLeft.Add(cell);
						}
						if( cell.posSector.z==0 ){
							cellsBack.Add(cell);
						}
					}
					sector.cellsFront = cellsFront.ToArray();
					sector.cellsRight = cellsRight.ToArray();
					sector.cellsBack  = cellsBack.ToArray();
					sector.cellsLeft  = cellsLeft.ToArray();
					
					sector.UpdateSector(null);
				}
			}
		}




		private void SearchNeighbours()
		{
			for(int i=0; i<columns; i++){
				for(int k=0; k<rows; k++){
					List<Cell> neighbourList=new List<Cell>();
					cells[i,k].neighbours = neighbourList.ToArray();
					Cell cellFrom = cells[i,k];

					if( cellFrom.walkable ){
						Cell cellTo = cells[i,k];
						int lmin = -1;
						int lmax = 2;
						
						for(int l=lmin; l<lmax; l=l+1){
							if( k-1>-1){
								cellTo = cells[i,k-1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,0,-1);
							}
							if( k+1<rows ){
								cellTo = cells[i,k+1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,0,+1);
							}
							if( i-1>-1 ){
								cellTo = cells[i-1,k];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,-1,0);
							}
							if( i+1<columns ){
								cellTo = cells[i+1,k];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,+1,0);
							}
							if( i-1>-1  && k-1>-1  ){
								cellTo = cells[i-1,k-1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,-1,-1);
							}
							if( i+1<columns  && k+1<rows ){
								cellTo = cells[i+1,k+1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,+1,+1);
							}
							if( i-1>-1 && k+1<rows){
								cellTo = cells[i-1,k+1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,-1,+1);
							}
							if( i+1<columns && k-1>-1){
								cellTo = cells[i+1,k-1];
								neighbourList = AddNeighbour(neighbourList,cellFrom,cellTo,+1,-1);
							}
						}

						cellFrom.neighbours = neighbourList.ToArray(); 
						if( cellFrom.neighbours.Length == 0 ){
							cellFrom.walkable = false;
							cellFrom.walkableINI = false;
						}
					}
				}
			}
		}




		private void CreateErosion()
		{
			foreach (Cell cell in cells) {
				if( cell.neighbours.Length < 8 ){
					cell.walkable = false;
					cell.walkableINI = false;
				}
			}

			SearchNeighbours ();
		}




		private List<Cell> AddNeighbour( List<Cell> neighbourList, Cell From, Cell To, int ToX, int ToZ )
		{
			if ( To.walkable && From.walkable ){
				Vector3[] VertexPosFrom = From.vertexPos;
				Vector3[] VertexPosTo 	= To.vertexPos;

				float tangent 		= ( Vector3.Distance(  new Vector3(0,From.posWorld.y,0), new Vector3(0,To.posWorld.y,0)) ) / ( Vector3.Distance(  new Vector3(From.posWorld.x,0,From.posWorld.z), new Vector3(To.posWorld.x,0,To.posWorld.z)) );
				float maxTangent 	= Mathf.Abs( Mathf.Tan (Mathf.Deg2Rad*( maxSlope+1 ))) ;
			
				if( tangent < maxTangent ){
					float height = 0.51f;
					if( ToX != 0 && ToZ == 0){
						if( ToX == -1 ){
							float HeightA = Mathf.Abs(VertexPosFrom[0].y - VertexPosTo[1].y);
							float HeightB = Mathf.Abs(VertexPosFrom[3].y - VertexPosTo[2].y);
							if( HeightA<height && HeightB<height  &&  !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}else{
							float HeightA = Mathf.Abs(VertexPosFrom[1].y - VertexPosTo[0].y);
							float HeightB = Mathf.Abs(VertexPosFrom[2].y - VertexPosTo[3].y);
							if( HeightA<height && HeightB<height  &&  !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}
					}else if( ToX == 0 && ToZ != 0){
						if( ToZ == -1 ){
							float HeightA = Mathf.Abs(VertexPosFrom[2].y - VertexPosTo[1].y);
							float HeightB = Mathf.Abs(VertexPosFrom[3].y - VertexPosTo[0].y);
							if( HeightA<height && HeightB<height  &&  !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}else{
							float HeightA = Mathf.Abs(VertexPosFrom[1].y - VertexPosTo[2].y);
							float HeightB = Mathf.Abs(VertexPosFrom[0].y - VertexPosTo[3].y);
							if( HeightA<height && HeightB<height  &&  !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}
					}else{
						int PosX = To.posGrid.x;
						int PosZ = To.posGrid.z;
						if( ToX == 1 && ToZ == 1 && cells[PosX-ToX,PosZ].walkable && cells[PosX,PosZ-ToZ].walkable ){
							float HeightA = Mathf.Abs(VertexPosFrom[1].y - VertexPosTo[3].y);
							if( HeightA<height && !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}
						if( ToX == 1 && ToZ == -1 && cells[PosX-ToX,PosZ].walkable && cells[PosX,PosZ-ToZ].walkable ){
							float HeightA = Mathf.Abs(VertexPosFrom[2].y - VertexPosTo[0].y);
							if( HeightA<height && !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}
						if( ToX == -1 && ToZ == 1 && cells[PosX-ToX,PosZ].walkable && cells[PosX,PosZ-ToZ].walkable ){
							float HeightA = Mathf.Abs(VertexPosFrom[0].y - VertexPosTo[2].y);
							if( HeightA<height && !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}
						if( ToX == -1 && ToZ == -1 && cells[PosX-ToX,PosZ].walkable && cells[PosX,PosZ-ToZ].walkable ){
							float HeightA = Mathf.Abs(VertexPosFrom[3].y - VertexPosTo[1].y);
							if( HeightA<height && !neighbourList.Contains(To) ){
								neighbourList.Add(To);
							}
						}	
					}
				}
			}
			return neighbourList;
		}




		static public Grid[] GetGridsByPoint( Vector3 point )
		{
			List<Grid> gridList = new List<Grid> ();
			foreach (Grid grid in Grid.AllGrids) {
				float xLong = grid.columns * grid.cellSize;
				float yLong = grid.height;
				float zLong = grid.rows * grid.cellSize;
				Vector3 origin = grid.posWorld;
				if( origin.x<=point.x && origin.y<=point.y && origin.z<=point.z && origin.x+xLong>=point.x && origin.y+yLong>=point.y && origin.z+zLong>=point.z ){
					gridList.Add(grid);
				}
			}
			return gridList.ToArray ();
		}




		public bool Bresenham(int x0, int z0, int x1, int z1)
		{
			if (x0 == x1 && z0 == z1) {
				return true;
			}
			int dx = Mathf.Abs(x1-x0);
			int dy = Mathf.Abs(z1-z0);
			int sx = 0;
			int sy = 0;
			if (x0 < x1) {sx=1;}else{sx=-1;}
			if (z0 < z1) {sy=1;}else{sy=-1;}
			int rise = dx-dy;
			
			bool loop=true;
			Cell cellBefore = cells[x1,z1];
			if ( !cellBefore.walkable ||  cellBefore.inBorder ) {
				return false;
			}
			cellBefore = cells[x0,z0];
			if ( !cellBefore.walkable ||  cellBefore.inBorder ) {
				return false;
			}
			
			while (loop) {
				Cell cell = cells [x0, z0];
				bool nextStep = false;
				if (cell.Id != cellBefore.Id) {
					if (cell.IsANeighbourOf (cellBefore)) {
						nextStep = true;
					}
				} else {
					nextStep = true;
				}
				if (nextStep) {
					if ( !cell.walkable || cell.inBorder ) {
						return false;
					}
					if ((x0 == x1) && (z0 == z1))
						loop = false;
					int e2 = 2 * rise;
					if (e2 > -dy) {
						rise = rise - dy;
						x0 = x0 + sx;
					}
					if (e2 < dx) {
						rise = rise + dx;
						z0 = z0 + sy;
					}
					cellBefore = cell;
				} else {
					return false;
				}
			}
			return true;	
		}



	}
}

