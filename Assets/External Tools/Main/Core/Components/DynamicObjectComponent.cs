using UnityEngine;
using System.Collections.Generic;
using PathFinding;

public class DynamicObjectComponent : MonoBehaviour {

	[HideInInspector]	
	public 	Bounds			bounds;		
	public 	GridComponent 	Grid;		
	private Grid 			grid;
	private Vector3			posBefore;
	private float 			updating;

	void Awake()
	{
		grid = Grid.GetComponent<GridComponent> ().grid;
		posBefore = transform.position;
		updating = 1.0f / grid.updateFrequency;
		InvokeRepeating("BeDynamic", 0.001f, updating);
	}



	void BeDynamic()
	{
		bounds = UpdateBounds ((1 / updating) * (transform.position - posBefore));
		foreach (Cell cell in grid.cells) {
			if (bounds.Intersects (cell.bounds)) {
				if (!grid.propagators.Contains (cell)) {
					cell.unWalkableOrigin = new Vector3 (bounds.center.x, cell.posWorld.y, bounds.center.z);
					grid.propagators.Add (cell);
				}
			}
		}
		posBefore = transform.position;
	}



	private Bounds UpdateBounds(Vector3 velocity)
	{
		Bounds bounds = GetComponent<Collider> ().bounds;

		Vector3 boundMin = bounds.min;
		Vector3 boundMax = bounds.max;

		if (velocity.x < 0.0f){
			boundMin.x = bounds.min.x + velocity.x;
		}else if (velocity.x > 0.0f){
			boundMax.x = bounds.max.x + velocity.x;
		}

		if (velocity.z < 0.0f){
			boundMin.z = bounds.min.z + velocity.z;
		}else if (velocity.z > 0.0f){
			boundMax.z = bounds.max.z + velocity.z;
		}

		bounds.SetMinMax(boundMin, boundMax);	
	
		return bounds;
	}
}
