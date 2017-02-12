using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {
    public GameObject nodePrefab;
    public Vector3 dimentions;
  
    public Vector3 separation;
    private Vector3 carretPos;

    public List<Node> nodes;
    private GameObject AllNodes;

    [ContextMenu("Generate Blank Grid")]
    List<Node> GenerateBlankGrid()
    {
        AllNodes = new GameObject("Nodes");
        AllNodes.transform.SetParent(this.transform);
        nodes = new List<Node>();
        Node newNode = nodePrefab.GetComponent<Node>();
        GameObject newNodeGO;
        Vector3 carretInitalPos = transform.position + newNode.GetSize() / 2;
        carretPos = carretInitalPos;
        for (int k = 0; k < dimentions.z; k++)
        {
            for (int j = 0; j < dimentions.y; j++)
            {
                for (int i = 0; i < dimentions.x; i++)
                {
                    newNodeGO = Instantiate<GameObject>(nodePrefab, carretPos, Quaternion.identity,AllNodes.transform);
                    nodes.Add(newNodeGO.GetComponent<Node>());
                    carretPos += transform.right * (newNode.GetSize().x + separation.x);
                }
                carretPos = new Vector3(carretInitalPos.x,carretPos.y , carretPos.z);
                carretPos +=  transform.up * (newNode.GetSize().y + separation.y) ;
            }
            carretPos = new Vector3(carretInitalPos.x, carretInitalPos.y, carretPos.z );
            carretPos += transform.forward * (newNode.GetSize().z + separation.z);
        }
        return nodes;
    }

    [ContextMenu("Clean Grid")]
    void CleanGrid()
    {
        if(AllNodes != null)
        {
            DestroyImmediate(AllNodes);
        }
    }


}
