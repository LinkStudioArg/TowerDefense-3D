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
    protected static bool[] showEnemies;

    public List<GameObject> prefabs;
    public List<string> prefabNames;

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

        //Load prefabs
        prefabs = new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/Enemies"));
        prefabNames = new List<string>();

        for (int i = 0; i < prefabs.Count; i++)
        {
            prefabNames.Add(prefabs[i].name);
        }
    }

    public override void OnInspectorGUI()
    {

        // Draw the state and time between waves as usual
        mySpawner.state = (Spawner.State)EditorGUILayout.EnumPopup("Current state: ", mySpawner.state);
        mySpawner.timeBetweenWaves = EditorGUILayout.FloatField("Time between waves: ", mySpawner.timeBetweenWaves);

        GUILayout.Space(30);

        int prefabIndex = 0;
        prefabIndex = EditorGUILayout.Popup("Enemy prefabs in rotation: ", prefabIndex, prefabNames.ToArray());
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Remove Prefab") && EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to remove this prefab from the rotation?", "Yes", "No"))
        {
            prefabs.RemoveAt(prefabIndex);
            prefabNames.RemoveAt(prefabIndex);
        }

        if (GUILayout.Button("Add Prefab"))
        {
            AddNewEnemy();
        }
        EditorGUILayout.EndHorizontal();

        // Draw waves
        for (int i = 0; i < mySpawner.waves.Count; i++)
        {
            // Check to see if the wave was changed
            EditorGUI.BeginChangeCheck();

            // Auxiliar variable for simplicity
            Spawner.Wave wave = mySpawner.waves[i];

            // WaveFoldout
            showSettings[i] = EditorGUILayout.Foldout(showSettings[i], wave.name);


            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            if (showSettings[i])
            {
                // Draw the name, spawn rate and enemy amount as usual
                wave.name = EditorGUILayout.TextField("Name: ", wave.name);
                wave.spawnRate = EditorGUILayout.FloatField("Spawn Rate: ", wave.spawnRate);
                wave.enemyAmount = EditorGUILayout.IntField("Enemies in wave: ", wave.enemyAmount);

                // Enemy Foldout
                showEnemies[i] = EditorGUILayout.Foldout(showEnemies[i], "Enemy types");

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();

                if (showEnemies[i])
                {
                    // Draw enemy prefabs
                    for (int j = 0; j < wave.enemies.Count; j++)
                    {
                        // So the remove button appears to the left
                        EditorGUILayout.BeginHorizontal();

                        // Draw the enemy prefab selector
                        wave.enemies[j] = DrawEnemy(wave.enemies[j]);

                        // Remove button
                        if (GUILayout.Button("Remove") && EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to remove this enemy type from this wave?", "Yes", "No"))
                        {
                            wave.enemies.RemoveAt(j);
                        }
                        EditorGUILayout.EndHorizontal();
                    } // End of Draw enemy prefabs

                    // Add new prefab to the enemies list button
                    if (GUILayout.Button("Add New Enemy"))
                    {
                        wave.enemies.Add(ChooseEnemy());
                    }
                }// End of Enemy Foldout

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                // Remove Wave Button
                if (GUILayout.Button("Remove Wave"))
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
            }// End Wave Foldout

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

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


    GameObject DrawEnemy(GameObject enemy)
    {
        int chosenIndex = -1;

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (enemy.name == prefabs[i].name)
            {
                chosenIndex = i;
            }
        }

        if (chosenIndex != -1)
        {
            enemy = prefabs[EditorGUILayout.Popup("Prefab: ", chosenIndex, prefabNames.ToArray())];
        }
        else
        {
            //Error
        }

        return enemy;
    }

    GameObject ChooseEnemy()
    {
        return prefabs[0];
    }

    void AddNewEnemy()
    {
        Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/External Tools/Main/Resources/Prefabs/Enemies/newPrefab.prefab");

        GameObject go = new GameObject();

        go = EnemyCreationWindow.CreateEnemy();
    }
}
