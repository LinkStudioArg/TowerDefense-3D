using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    static GameObject SelectedGo;
    PathEditor window;
    static bool created = false;
    MapGenerator myScript;

    void OnLostFocus()
    {
        EditorPrefs.SetBool("created", created);
        EditorPrefs.SetBool("display", display);
    }

    void OnDestroy()
    {
        EditorPrefs.SetBool("created", created);
        EditorPrefs.SetBool("display", display);
    }
    void GetValues()
    {
        if (EditorPrefs.HasKey("created"))
            created = EditorPrefs.GetBool("created");
        if (EditorPrefs.HasKey("display"))
            display = EditorPrefs.GetBool("display");
    }

    public void Awake()
    {
        GetValues();

    }
    public override void OnInspectorGUI()
    {
        
        myScript = (MapGenerator)target;
        
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
        
        if (created == false)
        {
            if (GUILayout.Button("Generate Blank Grid"))
            {
                
                myScript.GenerateBlankGrid();
                created = true;
                EditorUtility.SetDirty(myScript);
            }
        }
        if (created == true)
        {
            if (GUILayout.Button("Clear Grid"))
            {
                myScript.CleanGrid();
                created = false;
                EditorUtility.SetDirty(myScript);
            }
        }
        if (SelectedGo)
        {
            if (GUILayout.Button("Select as Base"))
            {
                myScript.SelectBase(SelectedGo);
                EditorUtility.SetDirty(myScript);
            }
            if (GUILayout.Button("Create Path"))
            {
                
                //myScript.CreatePath();
                
                window = ScriptableObject.CreateInstance<PathEditor>();
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
                window.Init(myScript);
                window.Show();
                
                
            }
        }

        if (myScript.paths.Count > 0)
        {
            if (GUILayout.Button("Display PathLines"))
            {
                display = !display;
                
            }
        }
    }
    private void OnSceneGUI()
    {
        if (display)
        {
            foreach (Path path in myScript.paths)
            {
                if (path.wayPoints != null)
                {
                    for (int i = 0; i < path.wayPoints.Count - 1; i++)
                    {
                        Handles.color = Color.red;

                        Handles.DrawLine(path.wayPoints[i].transform.position, path.wayPoints[i + 1].transform.position);
                    }
                }
            }
            
        }
    }
    static bool display;


}
