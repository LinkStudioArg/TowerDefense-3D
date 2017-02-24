using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyCreationWindow : EditorWindow
{



    public static GameObject CreateEnemy()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(EnemyCreationWindow));

        window.minSize = new Vector2(150, 100);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Enemy enemy = go.AddComponent<Enemy>();
        EnemyMovement eMov = go.AddComponent<EnemyMovement>();

        Rect preview = Rect.MinMaxRect(0, 0, 100, 100);

        

        return go;
    }

}
