using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class PathEditor : EditorWindow {
    Path path;    
    GameObject SelectedGo;
    GameObject aux;
    public void Init(Path p)
    {
        path = p;
        if (Selection.activeTransform.gameObject.GetComponent<Node>() != null)
        {
            SelectedGo = Selection.activeTransform.gameObject;
            path.SetSpawnPoint(SelectedGo);
            aux = SelectedGo;
        }
    }
    private void OnSelectionChange()
    {
        this.Repaint();
    }

    
    private void OnGUI()
    {

        if (Selection.activeTransform != null)
        {
            if (Selection.activeTransform.gameObject.GetComponent<Node>() != null)
            {
                SelectedGo = Selection.activeTransform.gameObject;
            }
            else
            {
                SelectedGo = null;
            }
        }
        else
        {
            SelectedGo = null;
        }
        if (SelectedGo != null)
        {
            if (GUILayout.Button("Create Anchor"))
            {
                GeneratePathToObject(SelectedGo);
                SceneView.RepaintAll();
            }
        }

        
    }

    void GeneratePathToObject(GameObject selected)
    {
        if (aux != SelectedGo)
        {
            Vector3 direction = selected.transform.position - aux.transform.position;
            float distance = Vector3.Distance(aux.transform.position, selected.transform.position);
            RaycastHit[] hits = Physics.RaycastAll(aux.transform.position, direction, distance);

            foreach (RaycastHit hit in hits)
            {
                if(hit.collider.gameObject != aux)
                    path.SetWayPoint(hit.collider.gameObject);
            }
            aux = SelectedGo;
        }
    }
    
   
}
