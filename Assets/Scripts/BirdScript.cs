using UnityEngine;

public class BirdScript : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] public Rigidbody2D body;
    [SerializeField] public float flapStrength = 5f;
    
    [Header("Game State")]
    [SerializeField] public bool isAlive = true;
    [SerializeField] public PipeSpawner pipeSpawner; // Reference to PipeSpawner
    
    [Header("Score (Read Only)")]
    [SerializeField] private int score = 0;
    
    // Public property to access score
    public int Score { get { return score; } }

    void Start()
    {
        // Get the Rigidbody2D component if not assigned in inspector
        if (body == null)
            body = GetComponent<Rigidbody2D>();
            
        // Find PipeSpawner if not assigned
        if (pipeSpawner == null)
            pipeSpawner = FindFirstObjectByType<PipeSpawner>();
    }
    
    void Update()
    {
        // Check if bird is out of bounds
        if (transform.position.y > 350 || transform.position.y < -350)
        {
            isAlive = false;
            score = 0;
            body.linearVelocity = Vector2.zero; // Stop movement
        }
        
        // Handle space key input for jumping only if bird is alive
        if (Input.GetKeyDown(KeyCode.Space) && isAlive)
        {
            // Set the y velocity to create jump effect
            body.linearVelocity = Vector2.up * flapStrength;
        }
    }
    
    // Detect collision with pipes or other objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with score zone
        if (collision.gameObject.CompareTag("ScoreZone"))
        {
            // Add score logic here
            score += 1;
            
            // Check if score reached 10
            if (score >= 10 && pipeSpawner != null)
            {
                pipeSpawner.StopAndClearPipes();
            }
            
            return;
        }
        
        // Kill bird on collision with everything else
        isAlive = false;
        score = 0;
        body.linearVelocity = Vector2.zero; // Stop movement
    }
}