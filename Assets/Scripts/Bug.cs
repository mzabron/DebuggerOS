using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bug : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float changeDirectionInterval = 8f;
    
    private Vector2 moveDirection;
    private float directionTimer;
    private RectTransform bugRectTransform;
    private RectTransform parentRect;
    private BugSpawner spawner;

    public void Initialize(BugSpawner bugSpawner, RectTransform parent, float speed, float sizeMultiplier)
    {
        spawner = bugSpawner;
        parentRect = parent;
        moveSpeed = speed;
        bugRectTransform = GetComponent<RectTransform>();
        
        // Add Image component if it doesn't exist (needed for click detection)
        if (GetComponent<Image>() == null)
        {
            gameObject.AddComponent<Image>();
        }
        
        // Make sure raycast target is enabled for clicking
        GetComponent<Image>().raycastTarget = true;
        
        // Apply random size
        ApplyRandomSize(sizeMultiplier);
        
        SetRandomDirection();
        SetRandomStartPosition();
    }

    private void ApplyRandomSize(float sizeMultiplier)
    {
        if (bugRectTransform != null)
        {
            // Apply the size multiplier to the bug's scale
            Vector3 currentScale = bugRectTransform.localScale;
            bugRectTransform.localScale = currentScale * sizeMultiplier;
        }
    }

    void Update()
    {
        MoveBug();
        UpdateDirection();
    }

    private void MoveBug()
    {
        if (bugRectTransform == null) return;
        
        // Move the bug
        bugRectTransform.anchoredPosition += moveDirection * moveSpeed * Time.deltaTime;
        
        // Rotate bug to face movement direction
        float rotationAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        bugRectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle + 90);
        
        KeepBugInBounds();
    }

    private void UpdateDirection()
    {
        directionTimer += Time.deltaTime;
        if (directionTimer >= changeDirectionInterval)
        {
            SetRandomDirection();
            directionTimer = 0f;
        }
    }

    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private void SetRandomStartPosition()
    {
        if (parentRect == null) return;
        
        Vector2 parentSize = parentRect.rect.size;
        
        // Choose a random edge to spawn from (0=top, 1=right, 2=bottom, 3=left)
        int edge = Random.Range(0, 4);
        Vector2 startPos = Vector2.zero;
        
        switch (edge)
        {
            case 0: // Top edge
                startPos = new Vector2(Random.Range(-parentSize.x/2, parentSize.x/2), parentSize.y/2);
                break;
            case 1: // Right edge
                startPos = new Vector2(parentSize.x/2, Random.Range(-parentSize.y/2, parentSize.y/2));
                break;
            case 2: // Bottom edge
                startPos = new Vector2(Random.Range(-parentSize.x/2, parentSize.x/2), -parentSize.y/2);
                break;
            case 3: // Left edge
                startPos = new Vector2(-parentSize.x/2, Random.Range(-parentSize.y/2, parentSize.y/2));
                break;
        }
        
        bugRectTransform.anchoredPosition = startPos;
    }

    private void KeepBugInBounds()
    {
        if (parentRect == null) return;
        
        Vector2 minPosition = parentRect.rect.min;
        Vector2 maxPosition = parentRect.rect.max;
        Vector2 currentPos = bugRectTransform.anchoredPosition;

        // Bounce off edges
        if (currentPos.x <= minPosition.x || currentPos.x >= maxPosition.x)
        {
            moveDirection.x = -moveDirection.x;
        }
        if (currentPos.y <= minPosition.y || currentPos.y >= maxPosition.y)
        {
            moveDirection.y = -moveDirection.y;
        }

        // Clamp position within bounds
        currentPos.x = Mathf.Clamp(currentPos.x, minPosition.x, maxPosition.x);
        currentPos.y = Mathf.Clamp(currentPos.y, minPosition.y, maxPosition.y);
        bugRectTransform.anchoredPosition = currentPos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Notify spawner that this bug was clicked
        if (spawner != null)
        {
            spawner.OnBugClicked(this);
        }
        
        // Destroy this bug
        Destroy(gameObject);
    }
}