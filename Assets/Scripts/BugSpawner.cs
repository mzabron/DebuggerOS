using UnityEngine;

public class BugSpawner : MonoBehaviour
{
    private GameObject spawner;
    private GameObject bugPrefab;
    private GameObject bug;

    void Start()
    {
        spawner = GameObject.Find("Spawner");
        bugPrefab = Resources.Load<GameObject>("Prefabs/Bug"); // Added missing semicolon

        if (spawner == null)
        {
            Debug.LogError("Spawner GameObject not found in scene!");
        }
        else
        {
            Debug.Log("Spawner found at position: " + spawner.transform.position);
        }

        if (bugPrefab == null)
        {
            Debug.LogError("Bug prefab not found in Resources folder!");
        }
        else
        {
            Debug.Log("Bug prefab loaded successfully: " + bugPrefab.name);
        }


        // Instantiate the prefab to create a game object
        if (bugPrefab != null)
        {
            bug = Instantiate(bugPrefab);
            // Optional: Set position relative to spawner
            bug.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogError("BugPrefab not found in Resources folder!");
        }
    }

    void Update()
    {
        
    }
}