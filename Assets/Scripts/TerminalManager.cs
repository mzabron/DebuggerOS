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


    public GameObject flappyBirdCanvas;

    private string lastUserInput = "";
    private int characterLimit = 40;
    private int maxDirectoryLines = 200;
    private Queue<GameObject> directoryLineQueue = new Queue<GameObject>();
    private TerminalAnimator terminalAnimator;
    private Interpreter interpreter;
    private SecuredDirectoryManager securedDirectoryManager;
    private string currentDirectoryPath = "";
    private string basePrompt = "C:\\SYSTEM_32>";
    
    private bool awaitingPassword = false;

    void Start()
    {

        InitializeAnimator();
        InitializeInterpreter();
        InitializeSecuredDirectoryManager();
        
        userInputLine.SetActive(false);
        StartCoroutine(terminalAnimator.PlayWelcomeBannerAnimation(userInputLine));

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnInputSubmit);
            inputField.characterLimit = characterLimit;
        }
        UpdateScrollState();
        UpdateUserInputLineDirectoryText();

        // Ensure Flappy Bird canvas starts hidden
        if (flappyBirdCanvas != null)
            flappyBirdCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        if (inputField != null)
            inputField.onSubmit.RemoveListener(OnInputSubmit);
    }

    private void InitializeAnimator()
    {
        terminalAnimator = GetComponent<TerminalAnimator>();

        var callbacks = new TerminalAnimator.AnimationCallbacks
        {
            AddResponseLine = AddResponseLine,
            RemoveSpecificLine = RemoveSpecificLine,
            GetInputField = (go) => inputField
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
            StartCoroutine = (c) => StartCoroutine(c),
            SetUserInputLineActive = SetUserInputLineActive,
            UpdateDirectoryPath = UpdateDirectoryPath,

            ActivateFlappyBirdCanvas = () => StartCoroutine(ActivateFlappyBirdCanvasSequence())
        };

        interpreter.Initialize(callbacks);
    }

    private void InitializeSecuredDirectoryManager()
    {
        securedDirectoryManager = GetComponent<SecuredDirectoryManager>();
        if (securedDirectoryManager == null)
            securedDirectoryManager = gameObject.AddComponent<SecuredDirectoryManager>();

        var callbacks = new SecuredDirectoryManager.SecuredDirectoryCallbacks
        {
            AddResponseLine = AddResponseLine,
            ProcessCommand = (command) => interpreter.ProcessCommand(command),
            PlayTypewriterEffect = PlayTypewriterEffect,
            PlayGlitchTypewriterEffect = (t) => terminalAnimator.GlitchTypewriterEffect(t),
            PlayScreenGlitchEffect = (d) => terminalAnimator.ScreenGlitchEffect(d),
            StartCoroutine = (c) => StartCoroutine(c),
            SetUserInputLineActive = SetUserInputLineActive,
            UpdateScrollState = () => {
                UpdateScrollState();
                StartCoroutine(ScrollToBottomCoroutine());
            },
            SetUserInputLineAsLastSibling = () => {
                if (userInputLine != null)
                    userInputLine.transform.SetAsLastSibling();
            },
            ActivateInputField = () => {
                if (inputField != null)
                    inputField.ActivateInputField();
            }
        };

        securedDirectoryManager.Initialize(callbacks);
    }

    private void OnInputSubmit(string userInput)
    {
        lastUserInput = userInput;
        inputField.text = "";
        AddDirectoryLine();
        
        if (awaitingPassword)
        {
            ProcessPasswordInput(lastUserInput);
        }
        else if (securedDirectoryManager.IsAwaitingConfirmation())
        {
            securedDirectoryManager.ProcessConfirmationInput(lastUserInput);
        }
        else
        {
            if (IsSecuredDirectoryCommand(lastUserInput))
            {
                if (securedDirectoryManager.IsSecuredDirectoryUnlocked())
                    interpreter.ProcessCommand(lastUserInput);
                else
                {
                    AddResponseLine("Enter the password:");
                    awaitingPassword = true;
                    SetPasswordMode(true);
                }
            }
            else
            {
                interpreter.ProcessCommand(lastUserInput);
            }
        }

        if (userInputLine.activeSelf && inputField != null)
        {
            userInputLine.transform.SetAsLastSibling();
            inputField.ActivateInputField();
        }
        UpdateScrollState();
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private bool IsSecuredDirectoryCommand(string input)
    {
        string[] parts = input.Trim().Split(' ');
        if (parts.Length >= 2)
        {
            string command = parts[0].ToLower();
            string directory = parts[1].ToLower();
            return (command == "cd") && (directory == "secured");
        }
        return false;
    }

    private void ProcessPasswordInput(string password)
    {
        bool passwordCorrect = securedDirectoryManager.ProcessPasswordInput(password);
        awaitingPassword = false;
        SetPasswordMode(false);
    }

    private void SetPasswordMode(bool enabled)
    {
        if (inputField != null)
        {
            inputField.inputType = enabled ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;
            inputField.ForceLabelUpdate();
        }
    }

    private void UpdateDirectoryPath(string pathSuffix)
    {
        currentDirectoryPath = pathSuffix;
        UpdateUserInputLineDirectoryText();
    }

    private void UpdateUserInputLineDirectoryText()
    {
        if (userInputLine != null)
        {
            TMP_Text[] textComponents = userInputLine.GetComponentsInChildren<TMP_Text>();
            if (textComponents.Length > 0)
            {
                TMP_Text directoryText = textComponents[0];
                string promptWithPath = basePrompt.Substring(0, basePrompt.Length - 1) + currentDirectoryPath + ">";
                directoryText.text = promptWithPath;
            }
        }
    }

    private void ClearScreen()
    {
        GameObject originalUserInputLine = userInputLine;

        while (directoryLineQueue.Count > 0)
        {
            GameObject line = directoryLineQueue.Dequeue();
            if (line != null)
                Destroy(line);
        }

        if (originalUserInputLine != null)
            Destroy(originalUserInputLine);

        securedDirectoryManager.CancelConfirmation();
        awaitingPassword = false;
        SetPasswordMode(false);

        StartCoroutine(ClearScreenCoroutine());
    }

    private IEnumerator ClearScreenCoroutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandLineContainer.GetComponent<RectTransform>());
        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(commandLineContainerSize.x, 45);
        yield return null;
        userInputLine = Instantiate(userInputLinePrefab, commandLineContainer.transform);
        userInputLine.transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandLineContainer.GetComponent<RectTransform>());
        RecreateUserInputLine();
        UpdateUserInputLineDirectoryText();
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
                SetPasswordMode(false);
                inputField.ActivateInputField();
            }
        }
    }

    private void AddDirectoryLine()
    {
        if (directoryLineQueue.Count >= maxDirectoryLines)
            RemoveOldestDirectoryLine();

        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta =
            new Vector2(commandLineContainerSize.x, commandLineContainerSize.y + 30);

        GameObject newDirectoryLine = Instantiate(directoryLine, commandLineContainer.transform);

        TMP_Text[] textComponents = newDirectoryLine.GetComponentsInChildren<TMP_Text>();
        if (textComponents.Length > 1)
        {
            string promptWithPath = basePrompt.Substring(0, basePrompt.Length - 1) + currentDirectoryPath + ">";
            textComponents[0].text = promptWithPath;
            textComponents[1].text = awaitingPassword ? new string('*', lastUserInput.Length) : lastUserInput;
        }

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
                commandLineContainer.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(commandLineContainerSize.x, commandLineContainerSize.y - 30);
                Destroy(oldestLine);
            }
        }
    }

    public GameObject AddResponseLine(string text)
    {
        Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
        commandLineContainer.GetComponent<RectTransform>().sizeDelta =
            new Vector2(commandLineContainerSize.x, commandLineContainerSize.y + 30);

        GameObject newResponseLine = Instantiate(responseLine, commandLineContainer.transform);

        TMP_Text textComponent = newResponseLine.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
            textComponent.text = text;

        directoryLineQueue.Enqueue(newResponseLine);
        return newResponseLine;
    }

    public void RemoveSpecificLine(GameObject lineToRemove)
    {
        if (lineToRemove != null)
        {
            Vector2 commandLineContainerSize = commandLineContainer.GetComponent<RectTransform>().sizeDelta;
            commandLineContainer.GetComponent<RectTransform>().sizeDelta =
                new Vector2(commandLineContainerSize.x, commandLineContainerSize.y - 30);

            List<GameObject> tempList = new List<GameObject>(directoryLineQueue);
            tempList.Remove(lineToRemove);
            directoryLineQueue.Clear();
            foreach (GameObject obj in tempList)
                directoryLineQueue.Enqueue(obj);

            Destroy(lineToRemove);
        }
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if (scrollRect.vertical)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    private void UpdateScrollState()
    {
        RectTransform containerRect = commandLineContainer.GetComponent<RectTransform>();
        RectTransform viewportRect = scrollRect.viewport;
        bool needsScrolling = containerRect.sizeDelta.y > viewportRect.rect.height;
        scrollRect.vertical = needsScrolling;
    }

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
            userInputLine.SetActive(active);
    }

    // New coroutine to activate and fade in Flappy Bird canvas
    private IEnumerator ActivateFlappyBirdCanvasSequence()
    {
        if (flappyBirdCanvas == null)
        {
            AddResponseLine("FlappyBird canvas not assigned.");
            yield break;
        }

        flappyBirdCanvas.SetActive(true);
        yield return StartCoroutine(FadeInElement(flappyBirdCanvas, 1f));
    }
}
