using UnityEngine;

public class BugSpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawner;
    [SerializeField] private GameObject bugPrefab;
    private GameObject bug;

    void Start()
    {
        // Instantiate the prefab to create a game object
        if (bugPrefab != null)
        {
            bug = Instantiate(bugPrefab, spawner.transform);
            bug.transform.position = spawner.transform.position;
            //bug.transform.position = new Vector2(0, 0);
            Debug.Log("spawner transform position: " + spawner.transform.position);
            Debug.Log("Bug spawned at spawner position." + bug.transform.position);
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