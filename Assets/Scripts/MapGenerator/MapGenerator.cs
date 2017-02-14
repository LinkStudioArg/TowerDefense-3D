using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MapGenerator : MonoBehaviour {
    public GameObject nodePrefab;

    public Vector3 nodeDimentions;

    

    public Vector3 dimentions;
        
    public Vector3 separation;

   
    

    [HideInInspector]
    public List<Node> nodes;

    public GameObject baseCell;
    [SerializeField]
    public List<Path> paths;
   
    private GameObject AllNodes;
    private Vector3 carretPos;
    [ContextMenu("Generate Blank Grid")]
    public List<Node> GenerateBlankGrid()
    {
        nodePrefab.transform.localScale = nodeDimentions;
        AllNodes = new GameObject("Nodes");
        AllNodes.transform.SetParent(this.transform);
        nodes = new List<Node>();
        Node newNode = nodePrefab.GetComponent<Node>();
        GameObject newNodeGO;
        Vector3 carretInitalPos = transform.position + newNode.GetSize() / 2;
        carretPos = carretInitalPos;
        for (int k = 0; k < (int)dimentions.z; k++)
        {
            for (int j = 0; j < (int)dimentions.y; j++)
            {
                for (int i = 0; i < (int)dimentions.x; i++)
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
    public void CleanGrid()
    {
        nodes.Clear();
        paths.Clear();
        baseCell = null;
        if (AllNodes != null)
        {
            
            DestroyImmediate(AllNodes);
        }
    }

    public void SelectBase(GameObject go)
    {
        baseCell = go;
    }
   

    public void CreatePath(Path path)
    {
        if(paths == null)
            paths = new List<Path>();        
        paths.Add(path);
    }

    
}
