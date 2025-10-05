using System.Collections.Generic;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab that represents a DOUBLE pipe (top + bottom together).")]
    public GameObject pipePrefab;

    [Header("Spawn Control")]
    [Tooltip("Seconds between spawns.")]
    public float spawnInterval = 1.6f;
    [Tooltip("Start automatically when this GameObject (or its Canvas) becomes active.")]
    public bool autoStartOnEnable = true;
    [Tooltip("Delay before first pipe spawns.")]
    public float initialDelay = 0.75f;

    [Header("Positioning (UI pixel space)")]
    [Tooltip("X position where pipes appear (e.g. just off the right edge).")]
    public float spawnX = 900f;
    [Tooltip("Random Y min (vertical offset).")]
    public float minY = -412f;
    [Tooltip("Random Y max (vertical offset).")]
    public float maxY = -208f;

    [Header("Movement")]
    [Tooltip("Leftward speed (pixels/second if on UI Canvas; world units/sec otherwise).")]
    public float moveSpeed = 250f;
    [Tooltip("X position at which a pipe gets destroyed (compare with anchoredPosition.x or localPosition.x).")]
    public float destroyX = -1200f; // Changed from -900f to -1200f

    [Header("Runtime State (Read-Only)")]
    [SerializeField] private bool spawning;
    [SerializeField] private int activePipeCount;

    private float spawnTimer;
    private bool initialDelayDone;

    // Internal representation for spawned pipes
    private class PipeInstance
    {
        public GameObject go;
        public RectTransform rect; // null if world-space
    }

    private readonly List<PipeInstance> activePipes = new List<PipeInstance>();

    private void OnEnable()
    {
        if (autoStartOnEnable)
            BeginSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
        ClearAllPipes();
    }

    private void Update()
    {
        if (spawning)
            HandleSpawningTimer();

        MoveAndCullPipes();
    }

    // ----------------- Public Control -----------------

    public void BeginSpawning()
    {
        if (spawning || pipePrefab == null)
            return;

        spawning = true;
        spawnTimer = spawnInterval; // so after initial delay it spawns immediately
        initialDelayDone = (initialDelay <= 0f);
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    public void ClearAllPipes()
    {
        for (int i = 0; i < activePipes.Count; i++)
        {
            if (activePipes[i].go != null)
                Destroy(activePipes[i].go);
        }
        activePipes.Clear();
        activePipeCount = 0;
    }
    
    // New method to stop spawning and clear all pipes when score reaches 10
    public void StopAndClearPipes()
    {
        StopSpawning();
        ClearAllPipes();
    }

    // ----------------- Spawning Logic -----------------

    private void HandleSpawningTimer()
    {
        if (!initialDelayDone)
        {
            initialDelay -= Time.deltaTime;
            if (initialDelay <= 0f)
                initialDelayDone = true;
            else
                return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnPipe();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnPipe()
    {
        float y = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(spawnX, y, 0f);

        GameObject pipe = Instantiate(pipePrefab, transform);
        // For UI (under a Canvas) we use localPosition; if a RectTransform exists we prefer anchoredPosition
        RectTransform r = pipe.GetComponent<RectTransform>();
        if (r != null)
        {
            r.anchoredPosition = new Vector2(spawnPos.x, spawnPos.y);
        }
        else
        {
            pipe.transform.localPosition = spawnPos;
        }

        activePipes.Add(new PipeInstance { go = pipe, rect = r });
        activePipeCount = activePipes.Count;
    }

    // ----------------- Movement & Cleanup -----------------

    private void MoveAndCullPipes()
    {
        if (activePipes.Count == 0) return;

        float delta = moveSpeed * Time.deltaTime;
        for (int i = activePipes.Count - 1; i >= 0; i--)
        {
            PipeInstance p = activePipes[i];
            if (p.go == null)
            {
                activePipes.RemoveAt(i);
                continue;
            }

            if (p.rect != null)
            {
                // UI movement
                Vector2 pos = p.rect.anchoredPosition;
                pos.x -= delta;
                p.rect.anchoredPosition = pos;

                if (pos.x <= destroyX)
                {
                    Destroy(p.go);
                    activePipes.RemoveAt(i);
                }
            }
            else
            {
                // World-space fallback
                Vector3 pos = p.go.transform.localPosition;
                pos.x -= delta;
                p.go.transform.localPosition = pos;

                if (pos.x <= destroyX)
                {
                    Destroy(p.go);
                    activePipes.RemoveAt(i);
                }
            }
        }

        activePipeCount = activePipes.Count;
    }
}
