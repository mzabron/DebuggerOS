using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationCallbacks
    {
        public System.Func<string, GameObject> AddResponseLine;
        public System.Action<GameObject> RemoveSpecificLine;
        public System.Func<GameObject, TMP_InputField> GetInputField;
    }

    private AnimationCallbacks callbacks;

    public void Initialize(AnimationCallbacks animationCallbacks)
    {
        callbacks = animationCallbacks;
    }

    public IEnumerator PlayWelcomeBannerAnimation(GameObject userInputLine)
    {
        // Initial lines that appear instantly
        string[] instantLines = {
            "DebuggerOS v4.2.0 - Secure Developer Environment",
            "(c) 2025 Debugger Corporation. All rights reserved.",
            "",
            "Initializing system modules"
        };

        // Add instant lines
        foreach (string line in instantLines)
        {
            callbacks.AddResponseLine(line);
        }

        // Loading animation with dots
        GameObject loadingLine = callbacks.AddResponseLine("");
        TMP_Text loadingText = loadingLine.GetComponentInChildren<TMP_Text>();

        /*
        // Animate dots 3 times
        for (int cycle = 0; cycle < 3; cycle++)
        {
            // Show dots one by one
            for (int dots = 1; dots <= 3; dots++)
            {
                loadingText.text = new string('.', dots);
                yield return new WaitForSeconds(0.5f);
            }

            // Clear dots
            loadingText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
        */

        // Remove the loading line
        callbacks.RemoveSpecificLine(loadingLine);

        // Lines that appear one by one
        string[] animatedLines = {
            "[<color=green> OK </color>] Kernel loaded",
            "[<color=green> OK </color>] Debug interface ready",
            "[<color=green> OK </color>] Virtual memory online"
        };

        // Add animated lines one by one
        foreach (string line in animatedLines)
        {
            callbacks.AddResponseLine(line);
            yield return new WaitForSeconds(0.8f);
        }

        // Empty line before final messages
        callbacks.AddResponseLine("");

        // Animated typewriter text
        string[] typewriterLines = {
            "Welcome to DebuggerOS.",
            "Type 'help' to begin."
        };

        foreach (string line in typewriterLines)
        {
            yield return StartCoroutine(TypewriterEffect(line));
        }

        // Fade in the userInputLine
        yield return StartCoroutine(FadeInUserInputLine(userInputLine));
    }

    public IEnumerator TypewriterEffect(string textToType)
    {
        GameObject typingLine = callbacks.AddResponseLine("");
        TMP_Text typingText = typingLine.GetComponentInChildren<TMP_Text>();

        string currentText = "";

        for (int i = 0; i < textToType.Length; i++)
        {
            currentText += textToType[i];
            typingText.text = currentText + "_";
            yield return new WaitForSeconds(0.05f); // Typing speed
        }

        // Remove caret and show final text
        typingText.text = currentText;
        yield return new WaitForSeconds(0.3f); // Brief pause after each line
    }

    public IEnumerator FadeInUserInputLine(GameObject userInputLine)
    {
        userInputLine.SetActive(true);
        userInputLine.transform.SetAsLastSibling();

        // Get all UI components that can be faded
        CanvasGroup canvasGroup = userInputLine.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = userInputLine.AddComponent<CanvasGroup>();
        }

        // Start with alpha 0 (invisible)
        canvasGroup.alpha = 0f;

        // Fade in over 1 second
        float fadeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            yield return null;
        }

        // Ensure it's fully visible
        canvasGroup.alpha = 1f;

        // Activate the input field
        TMP_InputField inputField = callbacks.GetInputField(userInputLine);
        if (inputField != null)
        {
            inputField.ActivateInputField();
        }
    }

    public IEnumerator FadeIn(GameObject target, float duration = 1f)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut(GameObject target, float duration = 1f)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}