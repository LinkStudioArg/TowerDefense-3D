using UnityEngine;
using System.Collections;
using PathFinding;

public class Maths {


	public static bool IsPointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		var s = (p0.z * p2.x - p0.x * p2.z + (p2.z - p0.z) * p.x + (p0.x - p2.x) * p.z);
		var t = (p0.x * p1.z - p0.z * p1.x + (p0.z - p1.z) * p.x + (p1.x - p0.x) * p.z);

		if (s <= 0 || t <= 0)
			return false;
		
		var A = (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
		
		return (s + t) < A;
	}


	public static Vector3 Vector3Limit( Vector3 vectorA , float magnitude)
	{
		if (vectorA.magnitude > magnitude) {
			vectorA = magnitude*vectorA.normalized;
		}
		return vectorA;
	}

	
}
