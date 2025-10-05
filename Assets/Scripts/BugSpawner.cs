using UnityEngine;
using UnityEngine.EventSystems;

public class BugSpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawner;
    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject bugPrefab;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float changeDirectionInterval = 2f;

    private GameObject bug;
    private Vector2 moveDirection;
    private float directionTimer;
    private RectTransform bugRectTransform;

    void Start()
    {
        // Instantiate the prefab to create a game object
        if (bugPrefab != null)
        {
            bug = Instantiate(bugPrefab, uiObject.transform);
            bug.transform.position = spawner.transform.position;

            bugRectTransform = bug.GetComponent<RectTransform>();
            SetRandomDirection();
        }
        else
        {
            Debug.LogError("BugPrefab not found in Resources folder!");
        }
    }

    void Update()
    {
        MoveBug();
    }

    private void MoveBug()
    {
        // Move the bug
        bugRectTransform.anchoredPosition += moveDirection * moveSpeed * Time.deltaTime;
        bugRectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg + 90);
        KeepBugInBounds();
    }

    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        // Rotate bug to face movement direction
        float rotationAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        bug.transform.rotation = Quaternion.Euler(0, 0, rotationAngle + 90); // +90 if your bug sprite faces up
    }

    private void KeepBugInBounds()
    {
        RectTransform parentRect = uiObject.GetComponent<RectTransform>();
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
}