using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathFinding
{
	public class HeapCell 
	{
		public List<Cell>  closeList = new List<Cell>();
		public List<Cell>  openList  = new List<Cell>();
		public List<float> heapList	 = new List<float>();
		
		public void Manager(string action, Cell CellToInsert = null){
			bool loop = true;
			if (action == "insert") {
				openList.Add(CellToInsert);
				heapList.Add(CellToInsert.F); 
				int pos = heapList.Count-1;
				
				do{
					int parent = (pos-1)/2;
					if(heapList[pos] <= heapList[parent]){
						float tempF = heapList[pos];
						heapList[pos] = heapList[parent];
						heapList[parent] = tempF;
						Cell tempFCell = openList[pos];
						openList[pos] = openList[parent];
						openList[parent] = tempFCell;
						pos = parent;
					}else{
						loop = false;
					}
					if( pos <=0 ) { loop = false; }
				}while(loop);
				
			}else if (action == "remove0") {
				openList[0] = openList[openList.Count-1];
				openList.RemoveAt(openList.Count-1);
				heapList[0] = heapList[heapList.Count-1];
				heapList.RemoveAt(heapList.Count-1);
				int pos  = 0;
				
				do{
					int pos1 = pos*2+1;
					int pos2 = pos*2+2;
					int pos3 = pos1;
					
					if(pos2 < heapList.Count){
						if(heapList[pos1] >= heapList[pos2]){
							pos3 = pos2;
						}
					}
					if(pos3<heapList.Count && heapList[pos]>=heapList[pos3]){
						float tempF = heapList[pos];
						heapList[pos] = heapList[pos3];
						heapList[pos3] = tempF;
						Cell tempFCell = openList[pos];
						openList[pos] = openList[pos3];
						openList[pos3] = tempFCell;
						pos = pos3;
					}else{
						loop = false;
					}
					if( pos >= heapList.Count ) { loop = false; }
				}while(loop);
				
			}
		}
	
	}	
}
