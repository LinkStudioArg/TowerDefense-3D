using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ObjectNotesButton : EditorWindow
{

    [MenuItem("Notes/Add Note")]
    static void AddNoteComponent()
    {
        if (Selection.activeGameObject)
        {
            Selection.activeGameObject.AddComponent<ObjectNote>();

        }
    }
    [MenuItem("Notes/Remove Note")]
    static void RemoveNoteComponent()
    {
        if (Selection.activeGameObject )
        {
            ObjectNote objNote = Selection.activeGameObject.GetComponent<ObjectNote>();
            if (objNote)
            {
                DestroyImmediate(objNote);
            }

        }

    }
    
    [MenuItem("Notes/Remove All Notes")]
    static void RemoveAllNoteComponent()
    {
        foreach (ObjectNote obj in Resources.FindObjectsOfTypeAll<ObjectNote>())
        {
            DestroyImmediate(obj);
        }

    }
}
