using UnityEngine;
using System.Collections.Generic;
using PathFinding;



public struct Triangle 
{
	public Vector3 Origin;
	public Vector3 Left;
	public Vector3 Right;
}



public class Avoidance 
{
	public static void Execute(Agent agent, float weight=1)
	{
		agent.triangles.Clear (); 
		foreach (Agent other in agent.agentsDetected) {
			if (other.swarm != agent.swarm) {
				Triangle triangle = new Triangle ();
				Vector3 dir = other.transform.position - agent.transform.position;
				triangle.Origin = (agent.transform.position + other.transform.position) / 2;
				triangle.Origin.y = agent.transform.position.y;
				
				float hypotenuse = dir.magnitude;
				float opposyte = other.radius+agent.radius;
				float angleOtherToPointFromAgent = Mathf.Asin (opposyte / hypotenuse);
				float angleOtherFromAgent = Mathf.Asin (dir.z / hypotenuse);
				
				float anglePos = 0;
				float angleNeg = 0;
				float dist = agent.range * 4;
				float xr, yr, zr, xl, yl, zl;
				if (other.transform.position.x > agent.transform.position.x) {
					anglePos = angleOtherFromAgent + angleOtherToPointFromAgent;
					angleNeg = angleOtherFromAgent - angleOtherToPointFromAgent;
					
					xr = triangle.Origin.x + Mathf.Cos (angleNeg) * dist;
					yr = triangle.Origin.y;
					zr = triangle.Origin.z + Mathf.Sin (angleNeg) * dist;
					triangle.Right = new Vector3 (xr, yr, zr);
					
					xl = triangle.Origin.x + Mathf.Cos (anglePos) * dist;
					yl = triangle.Origin.y;
					zl = triangle.Origin.z + Mathf.Sin (anglePos) * dist;
					triangle.Left = new Vector3 (xl, yl, zl);
					
				} else {
					if (other.transform.position.z < agent.transform.position.z) {
						anglePos = (angleOtherFromAgent + angleOtherToPointFromAgent);
						angleNeg = (angleOtherFromAgent - angleOtherToPointFromAgent);
						
						xr = triangle.Origin.x - Mathf.Cos (angleNeg) * dist;
						yr = triangle.Origin.y;
						zr = triangle.Origin.z + Mathf.Sin (angleNeg) * dist;
						triangle.Right = new Vector3 (xr, yr, zr);
						
						xl = triangle.Origin.x - Mathf.Cos (anglePos) * dist;
						yl = triangle.Origin.y;
						zl = triangle.Origin.z + Mathf.Sin (anglePos) * dist;
						triangle.Left = new Vector3 (xl, yl, zl);
					} else {
						anglePos = 270 - (angleOtherFromAgent + angleOtherToPointFromAgent);
						angleNeg = 270 - (angleOtherFromAgent - angleOtherToPointFromAgent);
						
						xr = triangle.Origin.x - Mathf.Cos (angleNeg) * dist;
						yr = triangle.Origin.y;
						zr = triangle.Origin.z - Mathf.Sin (angleNeg) * dist;
						triangle.Right = new Vector3 (xr, yr, zr);
						
						xl = triangle.Origin.x - Mathf.Cos (anglePos) * dist;
						yl = triangle.Origin.y;
						zl = triangle.Origin.z - Mathf.Sin (anglePos) * dist;
						triangle.Left = new Vector3 (xl, yl, zl);	
					}
				}

				Vector3 Origin = agent.transform.position - triangle.Origin + 0.5f*(1/Time.deltaTime)*(new Vector3 (agent.velocity.x, 0, agent.velocity.z) + new Vector3 (other.velocity.x, 0, other.velocity.z) ); 

				triangle.Origin += Origin;
				triangle.Left += Origin;
				triangle.Right += Origin;
				
				triangle.Origin.y = agent.transform.position.y;
				triangle.Left.y = agent.transform.position.y;
				triangle.Right.y = agent.transform.position.y;
				
				agent.triangles.Add (triangle);
			}
		}
		Vector3 newVelocity = Avoidance.Correction (agent, agent.triangles); 
		agent.velocity = Vector3.zero;
		Steering.Seek (agent, agent.transform.position + newVelocity , weight);	
	}
	


	public static Vector3 Correction(Agent agent, List<Triangle> _triangles)
	{
		Vector3 v = agent.velocity*(1/Time.deltaTime);
		v.y = 0;
		int arc = 5;
		int Right = 0;
		int Left = 0;
		if (_triangles.Count > 0) {
			for (int i=1; i<100; i=i+arc) {
				Quaternion Rotation = Quaternion.AngleAxis (i, Vector3.up);
				Vector3 Direction = Rotation * v.normalized;
				Vector3 Point = agent.transform.position + Direction * v.magnitude;
				bool PointInTriangle = false;
				foreach (Triangle t in _triangles) {
					if (Maths.IsPointInTriangle (Point, t.Origin, t.Left, t.Right)) {
						PointInTriangle = true;
					}
				}
				if (!PointInTriangle) {
					Left = i+1;
					break;
				}
			}
			for (int i=1; i<100; i=i+arc) {
				Quaternion Rotation = Quaternion.AngleAxis (-i, Vector3.up);
				Vector3 Direction = Rotation * v.normalized;
				Vector3 Point = agent.transform.position + Direction * v.magnitude;
				bool PointInTriangle = false;
				foreach (Triangle t in _triangles) {
					if (Maths.IsPointInTriangle (Point, t.Origin, t.Left, t.Right)) {
						PointInTriangle = true;
					}
				}
				if (!PointInTriangle) {
					Right = i+1;
					break;
				}	
			}
		}
		if (Left > 0 && Right > 0) {
			if( Left < Right ){
				Right = 0;
			}else{
				Left = 0;
			}
		}
		if (Left > 0) {
			Quaternion Rotation = Quaternion.AngleAxis (Left, Vector3.up);
			Vector3 Direction = Rotation * v.normalized;
			return agent.maxSpeed * Direction.normalized;
		} else if (Right > 0) {
			Quaternion Rotation = Quaternion.AngleAxis (-Right, Vector3.up);
			Vector3 Direction = Rotation * v.normalized;
			return agent.maxSpeed * Direction.normalized;
		} else {
			return agent.maxSpeed * v.normalized;
		}
	}

}
