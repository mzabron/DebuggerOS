using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalManager : MonoBehaviour
{
    public GameObject directoryLine;
    public GameObject responseLine;
    public TMP_InputField inputField;
    public GameObject userInputLine;
    public ScrollRect scrollRect;
    public GameObject commandLineContainer;
    public GameObject userInputLinePrefab;

    private string lastUserInput = "";
    private int characterLimit = 78;
    private int maxDirectoryLines = 200;
    private Queue<GameObject> directoryLineQueue = new Queue<GameObject>();
    private TerminalAnimator terminalAnimator;
    private Interpreter interpreter;

    void Start()
    {
        InitializeAnimator();
        InitializeInterpreter();
        
        userInputLine.SetActive(false);
        StartCoroutine(terminalAnimator.PlayWelcomeBannerAnimation(userInputLine));

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnInputSubmit);
            inputField.characterLimit = characterLimit;
        }
        UpdateScrollState();
    }

    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnInputSubmit);
        }
    }
    private void InitializeAnimator()
    {
        terminalAnimator = GetComponent<TerminalAnimator>();

        var callbacks = new TerminalAnimator.AnimationCallbacks
        {
            AddResponseLine = AddResponseLine,
            RemoveSpecificLine = RemoveSpecificLine,
            GetInputField = (gameObject) => inputField
        };

        terminalAnimator.Initialize(callbacks);
    }

    private void InitializeInterpreter()
    {
        interpreter = GetComponent<Interpreter>();

        var callbacks = new Interpreter.InterpreterCallbacks
        {
            AddResponseLine = AddResponseLine,
            ClearScreen = ClearScreen,
            PlayTypewriterEffect = PlayTypewriterEffect,
            StartCoroutine = (coroutine) => StartCoroutine(coroutine),
            SetUserInputLineActive = SetUserInputLineActive
        };

        interpreter.Initialize(callbacks);
    }

    private void OnInputSubmit(string userInput)
    {
        lastUserInput = userInput;
        inputField.text = "";


        AddDirectoryLine();
        
        // Process the command through the interpreter
        interpreter.ProcessCommand(lastUserInput);

        userInputLine.transform.SetAsLastSibling();
        inputField.ActivateInputField();
        UpdateScrollState();
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private void ClearScreen()
    {
        GameObject originalUserInputLine = userInputLine;

        // Clear all lines from the queue
        while (directoryLineQueue.Count > 0)
        {
            GameObject line = directoryLineQueue.Dequeue();
            if (line != null)
            {
                Destroy(line);
            }
        }

        // Destroy the current userInputLine
        if (originalUserInputLine != null)
        {
            Destroy(originalUserInputLine);
        }

        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, 45);

        userInputLine = Instantiate(userInputLinePrefab, commandLineContainer.transform);
        RecreateUserInputLine();

        UpdateScrollState();
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private void RecreateUserInputLine()
    {
        if (userInputLine != null)
        {
            inputField = userInputLine.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnInputSubmit);
                inputField.characterLimit = characterLimit;
                inputField.ActivateInputField();
            }
        }
    }

    private void AddDirectoryLine()
    {
        if (directoryLineQueue.Count >= maxDirectoryLines)
        {
            RemoveOldestDirectoryLine();
        }

        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, commandLineContainerSize.y + 30);
        
        GameObject newDirectoryLine = Instantiate(directoryLine, commandLineContainer.transform);
        newDirectoryLine.GetComponentsInChildren<TMP_Text>()[1].text = lastUserInput;

        directoryLineQueue.Enqueue(newDirectoryLine);
    }

    private void RemoveOldestDirectoryLine()
    {
        if (directoryLineQueue.Count > 0)
        {
            GameObject oldestLine = directoryLineQueue.Dequeue();

            if (oldestLine != null)
            {
                Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
                commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, commandLineContainerSize.y - 30);
                Destroy(oldestLine);
            }
        }
    }

    // Methods used by the animator and interpreter
    public GameObject AddResponseLine(string text)
    {
        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, commandLineContainerSize.y + 30);

        GameObject newResponseLine = Instantiate(responseLine, commandLineContainer.transform);

        TMP_Text textComponent = newResponseLine.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }

        directoryLineQueue.Enqueue(newResponseLine);
        return newResponseLine;
    }

    public void RemoveSpecificLine(GameObject lineToRemove)
    {
        if (lineToRemove != null)
        {
            Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
            commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, commandLineContainerSize.y - 30);

            // Remove from queue (convert to list, remove, convert back)
            List<GameObject> tempList = new List<GameObject>(directoryLineQueue);
            tempList.Remove(lineToRemove);
            directoryLineQueue.Clear();
            foreach (GameObject obj in tempList)
            {
                directoryLineQueue.Enqueue(obj);
            }

            Destroy(lineToRemove);
        }
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if (scrollRect.vertical)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void UpdateScrollState()
    {
        RectTransform containerRect = commandLineContainer.GetComponent<RectTransform>();
        RectTransform viewportRect = scrollRect.viewport;

        bool needsScrolling = containerRect.sizeDelta.y > viewportRect.rect.height;
        scrollRect.vertical = needsScrolling;
    }

    // Public methods to access animations from outside
    public IEnumerator PlayTypewriterEffect(string text)
    {
        return terminalAnimator.TypewriterEffect(text);
    }

    public IEnumerator FadeInElement(GameObject element, float duration = 1f)
    {
        return terminalAnimator.FadeIn(element, duration);
    }

    public IEnumerator FadeOutElement(GameObject element, float duration = 1f)
    {
        return terminalAnimator.FadeOut(element, duration);
    }

    private void SetUserInputLineActive(bool active)
    {
        if (userInputLine != null)
        {
            userInputLine.SetActive(active);
        }
    }
}
