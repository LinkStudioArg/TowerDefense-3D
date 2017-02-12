using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node:MonoBehaviour {
    public Vector3 GetSize()
    {
        return transform.localScale;
    }

}
