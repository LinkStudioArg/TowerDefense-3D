using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    static GameObject SelectedGo;
    static bool created = false;
    public override void OnInspectorGUI()
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

        DrawDefaultInspector();
        MapGenerator myScript = (MapGenerator)target;
        if (created == false)
        {
            if (GUILayout.Button("Generate Blank Grid"))
            {
                myScript.GenerateBlankGrid();
                created = true;
            }
        }
        if (created == true)
        {
            if (GUILayout.Button("Clear Grid"))
            {
                myScript.CleanGrid();
                created = false;
            }
        }
        if (SelectedGo)
        {
            if (GUILayout.Button("Select as Base"))
            {
                myScript.SelectBase(SelectedGo);
            }
            if (GUILayout.Button("Create Path"))
            {
                Path newPath = new Path();
                myScript.CreatePath(newPath);

                PathEditor window = ScriptableObject.CreateInstance<PathEditor>();
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
                window.Init(newPath);
                window.Show();
            }
        }
        

    }

    
}
