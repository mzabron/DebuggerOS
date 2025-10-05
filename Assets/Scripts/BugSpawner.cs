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
    [SerializeField] private int maxBugCount = 30; // Increased from 20 to 30
    
    [Header("Bug Behavior")]
    [SerializeField] private float baseMoveSpeed = 50f;
    [SerializeField] private float speedVariation = 20f;
    
    [Header("Spawning Timing")]
    [SerializeField] private float spawnInterval = 4f; // Decreased from 5f to 4f
    [SerializeField] private float spawnIntervalDecrease = 0.15f; // Increased from 0.1f to 0.15f
    [SerializeField] private float minSpawnInterval = 0.5f; // Decreased from 1f to 0.5f
    
    [Header("Punishment System")]
    [SerializeField] private float punishmentThreshold = 10f; // Time without killing bugs before punishment
    [SerializeField] private float punishmentSpawnMultiplier = 2f; // How much faster to spawn during punishment
    [SerializeField] private int punishmentBugBurst = 3; // Extra bugs to spawn during punishment
    
    private List<Bug> activeBugs = new List<Bug>();
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private int bugsDestroyed = 0;
    private float timeSinceLastKill = 0f;
    private bool punishmentMode = false;

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
        
        // Track time since last bug kill
        timeSinceLastKill += Time.deltaTime;
        
        // Check if we should enter punishment mode
        if (timeSinceLastKill >= punishmentThreshold && !punishmentMode)
        {
            EnterPunishmentMode();
        }
        
        // Debug info
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"Active bugs: {activeBugs.Count}, Bugs destroyed: {bugsDestroyed}, Time since last kill: {timeSinceLastKill:F1}s, Punishment mode: {punishmentMode}");
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
            float currentInterval = punishmentMode ? 
                currentSpawnInterval / punishmentSpawnMultiplier : 
                currentSpawnInterval;
                
            yield return new WaitForSeconds(currentInterval);
            
            // Only spawn if we haven't reached the maximum
            if (activeBugs.Count < maxBugCount)
            {
                SpawnBug();
                
                // In punishment mode, spawn extra bugs in bursts
                if (punishmentMode)
                {
                    for (int i = 0; i < punishmentBugBurst && activeBugs.Count < maxBugCount; i++)
                    {
                        yield return new WaitForSeconds(0.2f); // Small delay between burst spawns
                        SpawnBug();
                    }
                }
                
                // Gradually decrease spawn interval (spawn faster over time)
                currentSpawnInterval = Mathf.Max(minSpawnInterval, 
                    currentSpawnInterval - spawnIntervalDecrease);
            }
        }
    }

    private void EnterPunishmentMode()
    {
        punishmentMode = true;
        Debug.Log("PUNISHMENT MODE ACTIVATED! Bugs will spawn faster!");
        
        // Spawn immediate punishment bugs
        for (int i = 0; i < punishmentBugBurst && activeBugs.Count < maxBugCount; i++)
        {
            SpawnBug();
        }
    }

    private void ExitPunishmentMode()
    {
        punishmentMode = false;
        Debug.Log("Punishment mode deactivated. Good job!");
    }

    public void OnBugClicked(Bug clickedBug)
    {
        bugsDestroyed++;
        Debug.Log($"Bug clicked! Total destroyed: {bugsDestroyed}");
        
        // Remove from active bugs list
        activeBugs.Remove(clickedBug);
        
        // Reset punishment timer and exit punishment mode if active
        timeSinceLastKill = 0f;
        if (punishmentMode)
        {
            ExitPunishmentMode();
        }
        
        // Slightly slow down spawning when player is actively clicking
        currentSpawnInterval = Mathf.Min(spawnInterval, currentSpawnInterval + 0.1f);
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
        timeSinceLastKill = 0f;
        punishmentMode = false;
    }

    public void ResetSpawnRate()
    {
        currentSpawnInterval = spawnInterval;
        timeSinceLastKill = 0f;
        punishmentMode = false;
    }

    // Getters for debugging or UI display
    public int GetActiveBugCount() => activeBugs.Count;
    public int GetBugsDestroyed() => bugsDestroyed;
    public float GetCurrentSpawnInterval() => currentSpawnInterval;
    public bool IsPunishmentMode() => punishmentMode;
    public float GetTimeSinceLastKill() => timeSinceLastKill;
}