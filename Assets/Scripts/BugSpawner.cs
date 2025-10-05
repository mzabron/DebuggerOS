using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BugSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject spawner;
    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject bugPrefab;
    [SerializeField] private int initialBugCount = 3;
    [SerializeField] private int maxBugCount = 20;
    
    [Header("Bug Behavior")]
    [SerializeField] private float baseMoveSpeed = 50f;
    [SerializeField] private float speedVariation = 20f;
    
    [Header("Spawning Timing")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnIntervalDecrease = 0.1f;
    [SerializeField] private float minSpawnInterval = 1f;
    
    private List<Bug> activeBugs = new List<Bug>();
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private int bugsDestroyed = 0;

    void Start()
    {
        currentSpawnInterval = spawnInterval;
        
        // Spawn initial bugs
        for (int i = 0; i < initialBugCount; i++)
        {
            SpawnBug();
        }
        
        // Start the spawning coroutine
        spawnCoroutine = StartCoroutine(SpawnBugsOverTime());
    }

    void Update()
    {
        // Clean up null references from destroyed bugs
        activeBugs.RemoveAll(bug => bug == null);
        
        // Debug info
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"Active bugs: {activeBugs.Count}, Bugs destroyed: {bugsDestroyed}");
        }
    }

    private void SpawnBug()
    {
        if (bugPrefab == null || uiObject == null) 
        {
            Debug.LogError("BugPrefab or UiObject not assigned!");
            return;
        }
        
        if (activeBugs.Count >= maxBugCount)
        {
            Debug.Log("Maximum bug count reached!");
            return;
        }

        // Instantiate the bug
        GameObject newBugObject = Instantiate(bugPrefab, uiObject.transform);
        
        // Get or add the Bug component
        Bug bugComponent = newBugObject.GetComponent<Bug>();
        if (bugComponent == null)
        {
            bugComponent = newBugObject.AddComponent<Bug>();
        }
        
        // Calculate random speed variation
        float randomSpeed = baseMoveSpeed + Random.Range(-speedVariation, speedVariation);
        
        // Initialize the bug
        bugComponent.Initialize(this, uiObject.GetComponent<RectTransform>(), randomSpeed);
        
        // Add to active bugs list
        activeBugs.Add(bugComponent);
        
        Debug.Log($"Bug spawned! Total active: {activeBugs.Count}");
    }

    private IEnumerator SpawnBugsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            
            // Only spawn if we haven't reached the maximum
            if (activeBugs.Count < maxBugCount)
            {
                SpawnBug();
                
                // Gradually decrease spawn interval (spawn faster over time)
                currentSpawnInterval = Mathf.Max(minSpawnInterval, 
                    currentSpawnInterval - spawnIntervalDecrease);
            }
        }
    }

    public void OnBugClicked(Bug clickedBug)
    {
        bugsDestroyed++;
        Debug.Log($"Bug clicked! Total destroyed: {bugsDestroyed}");
        
        // Remove from active bugs list
        activeBugs.Remove(clickedBug);
        
        // Optional: Slow down spawning when player is actively clicking
        currentSpawnInterval = Mathf.Min(spawnInterval, currentSpawnInterval + 0.2f);
    }

    // Public methods for external control
    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnBugsOverTime());
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ClearAllBugs()
    {
        foreach (Bug bug in activeBugs)
        {
            if (bug != null)
            {
                Destroy(bug.gameObject);
            }
        }
        activeBugs.Clear();
        bugsDestroyed = 0;
    }

    public void ResetSpawnRate()
    {
        currentSpawnInterval = spawnInterval;
    }

    // Getters for debugging or UI display
    public int GetActiveBugCount() => activeBugs.Count;
    public int GetBugsDestroyed() => bugsDestroyed;
    public float GetCurrentSpawnInterval() => currentSpawnInterval;
}