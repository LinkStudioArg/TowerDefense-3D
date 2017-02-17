using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    // Target
    Spawner mySpawner;

    // Bool arays used for the Wave and Enemy prefabs foldouts
    protected static bool[] showSettings;
    public static bool[] showEnemies;

    private void Awake()
    {
        // Set the target
        mySpawner = (Spawner)target;

        // Set length of foldout bool arrays
        showSettings = new bool[mySpawner.waves.Count];
        showEnemies = new bool[mySpawner.waves.Count];

        // Initialize foldout bool arrays
        for (int i = 0; i < showEnemies.Length; i++)
        {
            showSettings[i] = false;
            showEnemies[i] = false;
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the state and time between waves as usual
        mySpawner.state = (Spawner.State)EditorGUILayout.EnumPopup("Current state: ", mySpawner.state);
        mySpawner.timeBetweenWaves = EditorGUILayout.FloatField("Time between waves: ", mySpawner.timeBetweenWaves);

        // Draw waves
        for (int i = 0; i < mySpawner.waves.Count; i++)
        {
            // Check to see if the wave was changed
            EditorGUI.BeginChangeCheck();

            // Auxiliar variable for simplicity
            Spawner.Wave wave = mySpawner.waves[i];

            // WaveFoldout
            showSettings[i] = EditorGUILayout.Foldout(showSettings[i], wave.name);

            if (showSettings[i])
            {
                // Draw the name, spawn rate and enemy amount as usual
                wave.name = EditorGUILayout.TextField("Name: ", wave.name);
                wave.spawnRate = EditorGUILayout.FloatField("Spawn Rate: ", wave.spawnRate);
                wave.enemyAmount = EditorGUILayout.IntField("Enemies in wave: ", wave.enemyAmount);

                // Enemy Foldout
                showEnemies[i] = EditorGUILayout.Foldout(showEnemies[i], "Enemy prefabs");

                if (showEnemies[i])
                {
                    // Draw enemy prefabs
                    for (int j = 0; j < wave.enemies.Count; j++)
                    {
                        // So the remove button appears to the left
                        EditorGUILayout.BeginHorizontal();

                        // If the enemy is a defined prefab, draw it with its name, else draw it as "Empty"
                        if (wave.enemies[j])
                            wave.enemies[j] = (GameObject)EditorGUILayout.ObjectField(wave.enemies[j].name, wave.enemies[j], typeof(GameObject), true);
                        else
                            wave.enemies[j] = (GameObject)EditorGUILayout.ObjectField("Empty", null, typeof(GameObject), false);

                        // Remove button
                        if (GUILayout.Button("Remove") && EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to remove this prefab?", "Yes", "No"))
                        {
                            wave.enemies.RemoveAt(j);
                        }
                        EditorGUILayout.EndHorizontal();
                    } // End of Draw enemy prefabs

                    // Add new prefab to the enemies list button
                    if (GUILayout.Button("Add New Prefab"))
                    {
                        wave.enemies.Add(null);
                    }
                }// End of Enemy Foldout

                // Remove Wave Button
                if (GUILayout.Button("Remove"))
                {
                    mySpawner.waves.Remove(wave);
                }

                // If the wave was changed
                if (EditorGUI.EndChangeCheck())
                {
                    // Logic to undo the modifications
                    Undo.RecordObject(mySpawner, "Modify Wave");
                    mySpawner.waves[i] = wave;

                    EditorUtility.SetDirty(mySpawner);
                }
            }

            // Space between waves
            GUILayout.Space(10);

        }// End of Draw Enemies

        // Add Wave Button
        if (GUILayout.Button("Add Wave"))
        {
            Undo.RecordObject(mySpawner, "Add Wave");

            mySpawner.waves.Add(new Spawner.Wave("new Wave", .1f, 1));

            EditorUtility.SetDirty(mySpawner);
        }
    }// End of OnInspectorGUI
}
