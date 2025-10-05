using System;
using System.Collections;
using UnityEngine;

public class SecuredDirectoryManager : MonoBehaviour
{
    [System.Serializable]
    public class SecuredDirectoryCallbacks
    {
        public System.Func<string, GameObject> AddResponseLine;
        public System.Action<string> ProcessCommand;
        public System.Func<string, IEnumerator> PlayTypewriterEffect;
        public System.Func<string, IEnumerator> PlayGlitchTypewriterEffect;
        public System.Func<float, IEnumerator> PlayScreenGlitchEffect;
        public System.Action<IEnumerator> StartCoroutine;
        public System.Action<bool> SetUserInputLineActive;
        public System.Action UpdateScrollState;
        public System.Action SetUserInputLineAsLastSibling;
        public System.Action ActivateInputField;
    }

    private SecuredDirectoryCallbacks callbacks;
    private bool awaitingFirstConfirmation = false;
    private bool awaitingSecondConfirmation = false;
    private bool isSecuredDirectoryUnlocked = false; // Track if the secured directory has been unlocked
    private bool shouldShowWarnings = false; // Track if warning messages should be displayed
    private Coroutine warningCoroutine; // Reference to the warning coroutine
    private const string SECURED_PASSWORD = "hackyeah2025";

    public void Initialize(SecuredDirectoryCallbacks securedCallbacks)
    {
        callbacks = securedCallbacks;
    }

    public bool IsSecuredDirectoryUnlocked()
    {
        return isSecuredDirectoryUnlocked;
    }

    public bool ProcessPasswordInput(string password)
    {
        if (password.Trim() == SECURED_PASSWORD)
        {
            // Correct password - start confirmation sequence
            callbacks.AddResponseLine("Access granted.");
            callbacks.AddResponseLine("Are you sure that you want to enter this directory? [yes/no]");
            awaitingFirstConfirmation = true;
            return true; // Password was correct
        }
        else
        {
            // Incorrect password
            callbacks.AddResponseLine("Incorrect password.");
            return false; // Password was incorrect
        }
    }

    public bool IsAwaitingConfirmation()
    {
        return awaitingFirstConfirmation || awaitingSecondConfirmation;
    }

    public void ProcessConfirmationInput(string input)
    {
        if (awaitingFirstConfirmation)
        {
            ProcessFirstConfirmation(input);
        }
        else if (awaitingSecondConfirmation)
        {
            // Stop the warning messages when user responds
            StopWarningMessages();
            ProcessSecondConfirmation(input);
        }
    }

    private void ProcessFirstConfirmation(string input)
    {
        string response = input.Trim().ToLower();

        if (response == "yes" || response == "y")
        {
            // User confirmed first time - ask second confirmation with typewriter effect
            callbacks.StartCoroutine(PlaySecondConfirmationWithTypewriter());
            awaitingFirstConfirmation = false;
            awaitingSecondConfirmation = true;
        }
        else if (response == "no" || response == "n")
        {
            // User declined - cancel access
            callbacks.AddResponseLine("Access cancelled.");
            ResetState();
        }
        else
        {
            // Invalid response
            callbacks.AddResponseLine("Please type 'yes' or 'no'.");
        }
    }

    private IEnumerator PlaySecondConfirmationWithTypewriter()
    {
        // Disable user input during typewriter animation
        callbacks.SetUserInputLineActive(false);
        
        yield return callbacks.PlayTypewriterEffect("Are you definitely sure? [yes/no]");
        
        // After typewriter effect ends, start showing recurring warning messages
        StartWarningMessages();
    }

    private void StartWarningMessages()
    {
        shouldShowWarnings = true;
        warningCoroutine = StartCoroutine(ShowRecurringWarnings());
    }

    private void StopWarningMessages()
    {
        shouldShowWarnings = false;
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
    }

    private IEnumerator ShowRecurringWarnings()
    {
        // Wait for the first warning interval
        yield return new WaitForSeconds(0.75f);
        
        if (shouldShowWarnings) // Check if warnings should still be shown
        {
            // Show first warning
            callbacks.AddResponseLine("<color=red>WARNING: YOU MAY REGRET DOING THAT</color>");
            
            // Ensure user input line is always the last sibling after adding warning
            callbacks.SetUserInputLineAsLastSibling();
            callbacks.UpdateScrollState();
            
            // Enable user input after the first warning
            callbacks.SetUserInputLineActive(true);
            callbacks.ActivateInputField();
        }
        
        // Continue showing warnings every 0.75 seconds
        while (shouldShowWarnings)
        {
            yield return new WaitForSeconds(0.75f);
            
            if (shouldShowWarnings) // Check again after waiting in case it was stopped
            {
                callbacks.AddResponseLine("<color=red>WARNING: YOU MAY REGRET DOING THAT</color>");
                
                // Ensure user input line is always the last sibling after adding warning
                callbacks.SetUserInputLineAsLastSibling();
                
                callbacks.UpdateScrollState();
            }
        }
    }

    private void ProcessSecondConfirmation(string input)
    {
        string response = input.Trim().ToLower();

        if (response == "yes" || response == "y")
        {
            // User confirmed both times - trigger glitch effect and enter secured directory
            callbacks.StartCoroutine(PlayGlitchSequenceAndEnter());
            ResetState();
            callbacks.ActivateInputField();
        }
        else if (response == "no" || response == "n")
        {
            // User declined - cancel access
            callbacks.AddResponseLine("Access cancelled.");
            ResetState();
            callbacks.ActivateInputField();
        }
        else
        {
            // Invalid response - restart warning messages since user hasn't given valid response
            callbacks.AddResponseLine("Please type 'yes' or 'no'.");
            
            // Ensure user input line is positioned correctly after invalid response message
            callbacks.SetUserInputLineAsLastSibling();
            callbacks.UpdateScrollState();
            callbacks.ActivateInputField();

            StartWarningMessages();
        }
    }

    private IEnumerator PlayGlitchSequenceAndEnter()
    {
        // Hide user input line during glitch sequence
        callbacks.SetUserInputLineActive(false);
        
        // Trigger screen glitch effect for exactly 5 seconds
        yield return callbacks.PlayScreenGlitchEffect(5f);
        
        // Show entering message with typewriter effect
        yield return callbacks.PlayTypewriterEffect("Entering secured directory...");
        callbacks.UpdateScrollState();
        
        // Brief pause before entering
        yield return new WaitForSeconds(0.5f);
        
        // Mark the secured directory as unlocked after successful access
        isSecuredDirectoryUnlocked = true;
        
        // Enter the directory (this will show "Changed directory to secured" message)
        callbacks.ProcessCommand("cd secured");
        
        // Wait for the directory change to complete and all messages to appear
        yield return new WaitForSeconds(0.5f);
        
        // Update scroll state to ensure proper positioning
        callbacks.UpdateScrollState();
        
        // Show user input line again after everything is complete
        callbacks.SetUserInputLineActive(true);
        
        // Ensure user input line is positioned as the last child (at the bottom)
        callbacks.SetUserInputLineAsLastSibling();
        
        // Final scroll state update with a small delay
        yield return new WaitForSeconds(0.1f);
        callbacks.UpdateScrollState();
    }

    private void ResetState()
    {
        awaitingFirstConfirmation = false;
        awaitingSecondConfirmation = false;
        StopWarningMessages(); // Stop warnings when resetting state
    }

    public void CancelConfirmation()
    {
        ResetState();
    }
}