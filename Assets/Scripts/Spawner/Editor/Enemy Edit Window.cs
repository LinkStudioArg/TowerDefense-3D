using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyEditWindow : EditorWindow
{

    public GameObject go;
    Enemy enemy;
    Mesh mesh;
    Material material;

    public static void CreateEnemy(ref GameObject enemyObject)
    {
        EnemyEditWindow window = ScriptableObject.CreateInstance<EnemyEditWindow>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 300);

        window.go = enemyObject;

        window.Show();
    }

    public static void EditEnemy(GameObject enemyObject)
    {
        EnemyEditWindow window = ScriptableObject.CreateInstance<EnemyEditWindow>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 300);

        window.go = enemyObject;

        window.Show();
    }

    void OnGUI()
    {
        enemy = go.GetComponent<Enemy>();
        mesh = go.GetComponent<MeshFilter>().sharedMesh;
        material = go.GetComponent<MeshRenderer>().sharedMaterial;


        EditorGUILayout.LabelField("Enemy Stats: ");
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();

        EditorGUI.BeginChangeCheck();

        enemy.stats.name = EditorGUILayout.TextField("Name: ", enemy.stats.name);
        enemy.stats.hp = EditorGUILayout.FloatField("HP: ", enemy.stats.hp);
        enemy.stats.movementVel = EditorGUILayout.FloatField("Movement Velocity: ", enemy.stats.movementVel);
        enemy.stats.damage = EditorGUILayout.FloatField("Damage: ", enemy.stats.damage);
        enemy.stats.shield = EditorGUILayout.FloatField("Shield: ", enemy.stats.shield);

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(20);

        go.GetComponent<MeshFilter>().mesh = (Mesh)EditorGUILayout.ObjectField("Mesh: ", mesh, typeof(Mesh), false);
        go.GetComponent<MeshRenderer>().material = (Material)EditorGUILayout.ObjectField("Material: ", material, typeof(Material), false);

        bool objectChanged = EditorGUI.EndChangeCheck();

        if (GUILayout.Button("Done"))
        {
            if (IsValidEnemy())
            {
                go.layer = LayerMask.NameToLayer("Enemies");
                go.name = enemy.stats.name;
                string prefabPath = SpawnerEditor.enemyFolderPath + "/" + go.name + ".prefab";

                string[] test = System.IO.Directory.GetFiles(SpawnerEditor.enemyFolderPath, go.name + ".prefab", System.IO.SearchOption.AllDirectories);
                if (test.Length < 1)
                {
                    PrefabUtility.CreatePrefab(prefabPath, go);
                    GameObject.DestroyImmediate(go, true);
                }
                else
                {
                    if (objectChanged)
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
                this.Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Prefab", "The the stats are not valid, please correct them", "Ok");
            }
        }
    }

    bool IsValidEnemy()
    {
        return enemy.stats.name != "" && enemy.stats.damage > 0 && enemy.stats.hp > 0 && enemy.stats.movementVel > 0;
    }
}
