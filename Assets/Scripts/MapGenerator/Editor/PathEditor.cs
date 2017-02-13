using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class PathEditor : EditorWindow {
    Path path;    
    GameObject SelectedGo;

    public void Init(Path p)
    {
        path = p;
        if (Selection.activeTransform.gameObject.GetComponent<Node>() != null)
        {
            SelectedGo = Selection.activeTransform.gameObject;
            path.SetSpawnPoint(SelectedGo);
        }
    }
    void OnGUI()
    {
        if (SelectedGo != null)
        {
           
        }
    }
}
