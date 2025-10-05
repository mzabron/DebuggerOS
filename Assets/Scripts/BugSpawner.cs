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
    [SerializeField] private int maxBugCount = 30;
    
    [Header("Bug Behavior")]
    [SerializeField] private float baseMoveSpeed = 50f;
    [SerializeField] private float maxSpeedMultiplier = 1.8f; // 50% faster than base speed
    [SerializeField] private float maxSizeMultiplier = 4f; // 20% larger than base size
    
    [Header("Spawning Timing")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float spawnIntervalDecrease = 0.15f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    
    [Header("Punishment System")]
    [SerializeField] private float punishmentThreshold = 10f;
    [SerializeField] private float punishmentSpawnMultiplier = 2f;
    [SerializeField] private int punishmentBugBurst = 3;
    
    [Header("Victory Condition")]
    [SerializeField] private float victoryDelay = 2f; // Time to wait after clearing all bugs before declaring victory
    
    private List<Bug> activeBugs = new List<Bug>();
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private int bugsDestroyed = 0;
    private float timeSinceLastKill = 0f;
    private bool punishmentMode = false;
    private bool allBugsCleared = false;
    private float timeSinceLastBug = 0f;
    private bool isInitialized = false;

    void Start()
    {
        // Only initialize the spawn interval, but don't start spawning
        currentSpawnInterval = spawnInterval;
        
        // Mark as initialized but don't spawn bugs automatically
        isInitialized = true;
    }

    void Update()
    {
        // Only run update logic if spawning has been started
        if (!isInitialized || spawnCoroutine == null)
            return;
            
        // Clean up null references from destroyed bugs
        activeBugs.RemoveAll(bug => bug == null);
        
        // Check if all bugs are cleared
        if (activeBugs.Count == 0 && !allBugsCleared)
        {
            timeSinceLastBug += Time.deltaTime;
            
            // Wait for victory delay before stopping spawning
            if (timeSinceLastBug >= victoryDelay)
            {
                OnAllBugsCleared();
            }
        }
        else if (activeBugs.Count > 0)
        {
            // Reset the timer if bugs are still active
            timeSinceLastBug = 0f;
        }
        
        // Track time since last bug kill (only if spawning is still active)
        if (!allBugsCleared)
        {
            timeSinceLastKill += Time.deltaTime;
            
            // Check if we should enter punishment mode
            if (timeSinceLastKill >= punishmentThreshold && !punishmentMode)
            {
                EnterPunishmentMode();
            }
        }
        
        // Debug info
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"Active bugs: {activeBugs.Count}, Bugs destroyed: {bugsDestroyed}, Time since last kill: {timeSinceLastKill:F1}s, Punishment mode: {punishmentMode}, All cleared: {allBugsCleared}");
        }
    }

    private void SpawnBug()
    {
        // Don't spawn if all bugs have been cleared
        if (allBugsCleared)
        {
            return;
        }
        
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
        
        // Generate random variations
        float randomSpeedMultiplier = Random.Range(1f, maxSpeedMultiplier);
        float randomSizeMultiplier = Random.Range(1f, maxSizeMultiplier);
        
        // Calculate final speed
        float finalSpeed = baseMoveSpeed * randomSpeedMultiplier;
        
        // Initialize the bug with random speed and size
        bugComponent.Initialize(this, uiObject.GetComponent<RectTransform>(), finalSpeed, randomSizeMultiplier);
        
        // Add to active bugs list
        activeBugs.Add(bugComponent);
        
        Debug.Log($"Bug spawned! Speed: {finalSpeed:F1} (x{randomSpeedMultiplier:F2}), Size: x{randomSizeMultiplier:F2}, Total active: {activeBugs.Count}");
    }

    private IEnumerator SpawnBugsOverTime()
    {
        while (!allBugsCleared)
        {
            float currentInterval = punishmentMode ? 
                currentSpawnInterval / punishmentSpawnMultiplier : 
                currentSpawnInterval;
                
            yield return new WaitForSeconds(currentInterval);
            
            // Only spawn if we haven't reached the maximum and haven't cleared all bugs
            if (activeBugs.Count < maxBugCount && !allBugsCleared)
            {
                SpawnBug();
                
                // In punishment mode, spawn extra bugs in bursts
                if (punishmentMode && !allBugsCleared)
                {
                    for (int i = 0; i < punishmentBugBurst && activeBugs.Count < maxBugCount && !allBugsCleared; i++)
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
        
        Debug.Log("Spawning coroutine ended - All bugs cleared!");
    }

    private void OnAllBugsCleared()
    {
        allBugsCleared = true;
        
        // Stop the spawning coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // Exit punishment mode if active
        if (punishmentMode)
        {
            ExitPunishmentMode();
        }
        
        Debug.Log($"?? VICTORY! All bugs cleared! Total bugs destroyed: {bugsDestroyed}");
        
        // Optional: You can add victory effects here
        // For example: play victory sound, show victory UI, etc.
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
        if (spawnCoroutine == null && !allBugsCleared)
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
        allBugsCleared = false;
        timeSinceLastBug = 0f;
    }

    public void ResetSpawnRate()
    {
        currentSpawnInterval = spawnInterval;
        timeSinceLastKill = 0f;
        punishmentMode = false;
        allBugsCleared = false;
        timeSinceLastBug = 0f;
    }

    public void RestartBugSpawning()
    {
        // Method to restart the bug spawning system
        allBugsCleared = false;
        timeSinceLastBug = 0f;
        ResetSpawnRate();
        StartSpawning();
        
        // Spawn initial bugs again
        for (int i = 0; i < initialBugCount; i++)
        {
            SpawnBug();
        }
        
        Debug.Log("Bug spawning restarted!");
    }

    // Getters for debugging or UI display
    public int GetActiveBugCount() => activeBugs.Count;
    public int GetBugsDestroyed() => bugsDestroyed;
    public float GetCurrentSpawnInterval() => currentSpawnInterval;
    public bool IsPunishmentMode() => punishmentMode;
    public float GetTimeSinceLastKill() => timeSinceLastKill;
    public bool AreAllBugsCleared() => allBugsCleared;
}