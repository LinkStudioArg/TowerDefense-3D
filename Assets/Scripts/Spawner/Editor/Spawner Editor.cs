using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    Spawner mySpawner;

    protected static bool[] showSettings;
    public static bool[] showEnemies;

    private void Awake()
    {
        mySpawner = (Spawner)target;
        showSettings = new bool[mySpawner.waves.Count];
        showEnemies = new bool[mySpawner.waves.Count];

        for (int i = 0; i < showEnemies.Length; i++)
        {
            showSettings[i] = false;
            showEnemies[i] = false;
        }
    }

    public override void OnInspectorGUI()
    {

        mySpawner.state = (Spawner.State)EditorGUILayout.EnumPopup("Current state: ", mySpawner.state);
        mySpawner.timeBetweenWaves = EditorGUILayout.FloatField("Time between waves: ", mySpawner.timeBetweenWaves);


        for (int i = 0; i < mySpawner.waves.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Spawner.Wave wave = mySpawner.waves[i];

            showSettings[i] = EditorGUILayout.Foldout(showSettings[i], wave.name);

            if (showSettings[i])
            {
                wave.name = EditorGUILayout.TextField("Name: ",wave.name);
                wave.spawnRate      = EditorGUILayout.FloatField("Spawn Rate: ", wave.spawnRate);
                wave.enemyAmount    = EditorGUILayout.IntField("Enemies in wave: ", wave.enemyAmount);

                showEnemies[i] = EditorGUILayout.Foldout(showEnemies[i], "Enemy prefabs");

                if (showEnemies[i])
                {
                    for (int j = 0; j < wave.enemies.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (wave.enemies[j])
                            wave.enemies[j] = (GameObject)EditorGUILayout.ObjectField(wave.enemies[j].name, wave.enemies[j], typeof(GameObject), true);
                        else
                            wave.enemies[j] = (GameObject)EditorGUILayout.ObjectField("Empty", null, typeof(GameObject), false);

                        if (GUILayout.Button("Remove") && EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to remove this prefab?", "Yes", "No"))
                        {
                            wave.enemies.RemoveAt(j);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (GUILayout.Button("Add New Prefab"))
                    {
                        wave.enemies.Add(null);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mySpawner, "Modify Wave");
                mySpawner.waves[i] = wave;

                EditorUtility.SetDirty(mySpawner);
            }

            GUILayout.Space(10);
        }
        

        if (GUILayout.Button("Add Wave"))
        {
            Undo.RecordObject(mySpawner, "Add Wave");
            

            //Create a way to set the values
            mySpawner.waves.Add(new Spawner.Wave("new Wave", 1, 1));
            EditorUtility.SetDirty(mySpawner);
        }
    }
}
