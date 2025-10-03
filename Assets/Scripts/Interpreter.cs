using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
    [System.Serializable]
    public class InterpreterCallbacks
    {
        public System.Func<string, GameObject> AddResponseLine;
        public System.Action ClearScreen;
        public System.Func<string, IEnumerator> PlayTypewriterEffect;
        public System.Action<IEnumerator> StartCoroutine;
        public System.Action<bool> SetUserInputLineActive;
    }

    private InterpreterCallbacks callbacks;
    private Dictionary<string, string> variables = new Dictionary<string, string>();
    private bool awaitingExitConfirmation = false;

    public void Initialize(InterpreterCallbacks interpreterCallbacks)
    {
        callbacks = interpreterCallbacks;

        variables["version"] = "4.2.0";
        variables["user"] = "developer";
        variables["system"] = "DebuggerOS";
    }

    public void ProcessCommand(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return;
        }

        // Check if we're awaiting exit confirmation
        if (awaitingExitConfirmation)
        {
            ProcessExitConfirmation(userInput.Trim().ToLower());
            return;
        }

        // Convert to lowercase and trim for case-insensitive commands
        string command = userInput.Trim().ToLower();
        string[] parts = command.Split(' ');
        string mainCommand = parts[0];

        switch (mainCommand)
        {
            case "help":
                ExecuteHelp();
                break;

            case "clear":
            case "cls":
                ExecuteClear();
                break;

            case "echo":
                ExecuteEcho(userInput);
                break;

            case "version":
            case "ver":
                ExecuteVersion();
                break;

            case "date":
                ExecuteDate();
                break;

            case "time":
                ExecuteTime();
                break;

            case "whoami":
                ExecuteWhoAmI();
                break;

            case "ls":
            case "dir":
                ExecuteListDirectory();
                break;

            case "exit":
            case "quit":
                ExecuteExit();
                break;

            default:
                ExecuteUnknownCommand(userInput);
                break;
        }
    }

    private void ExecuteHelp()
    {
        callbacks.AddResponseLine("Available commands:");
        callbacks.AddResponseLine("");
        callbacks.AddResponseLine("  help          - Show this help message");
        callbacks.AddResponseLine("  clear, cls    - Clear the terminal screen");
        callbacks.AddResponseLine("  echo [text]   - Display text");
        callbacks.AddResponseLine("  version, ver  - Show system version");
        callbacks.AddResponseLine("  date          - Show current date");
        callbacks.AddResponseLine("  time          - Show current time");
        callbacks.AddResponseLine("  whoami        - Show current user");
        callbacks.AddResponseLine("  ls, dir       - List directory contents");
        callbacks.AddResponseLine("  exit, quit    - Exit the terminal");
        callbacks.AddResponseLine("");
    }

    private void ExecuteClear()
    {
        callbacks.ClearScreen?.Invoke();
    }

    private void ExecuteEcho(string userInput)
    {
        if (userInput.Length > 5)
        {
            string textToEcho = userInput.Substring(5);
            callbacks.AddResponseLine(textToEcho);
        }
        else
        {
            callbacks.AddResponseLine("");
        }
    }

    private void ExecuteVersion()
    {
        callbacks.AddResponseLine($"DebuggerOS {variables["version"]} - Secure Developer Environment");
        callbacks.AddResponseLine("(c) 2025 Debugger Corporation. All rights reserved.");
    }

    private void ExecuteDate()
    {
        string currentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        callbacks.AddResponseLine($"Current date: {currentDate}");
    }

    private void ExecuteTime()
    {
        string currentTime = DateTime.Now.ToString("HH:mm:ss");
        callbacks.AddResponseLine($"Current time: {currentTime}");
    }

    private void ExecuteWhoAmI()
    {
        string windowsUsername = System.Environment.UserName;
        callbacks.AddResponseLine($"{windowsUsername}");
    }

    private void ExecuteListDirectory()
    {
        callbacks.AddResponseLine("Directory listing:");
        callbacks.AddResponseLine("");
        callbacks.AddResponseLine("  bin/          - System binaries");
        callbacks.AddResponseLine("  home/         - User directories");
        callbacks.AddResponseLine("  etc/          - Configuration files");
        callbacks.AddResponseLine("  var/          - Variable data");
        callbacks.AddResponseLine("  tmp/          - Temporary files");
        callbacks.AddResponseLine("");
        callbacks.AddResponseLine("5 directories, 0 files");
    }

    private void ExecuteExit()
    {
        callbacks.AddResponseLine("Are you sure you want to exit?");
        callbacks.AddResponseLine("<color=yellow>Warning: Progress will not be saved!</color>");
        callbacks.AddResponseLine("Type 'yes' to exit or 'no' to cancel.");

        // Set state to await confirmation
        awaitingExitConfirmation = true;
    }

    private void ProcessExitConfirmation(string userInput)
    {
        switch (userInput)
        {
            case "yes":
            case "y":
                awaitingExitConfirmation = false;
                callbacks.StartCoroutine?.Invoke(ExitAnimationCoroutine());
                break;

            case "no":
            case "n":
                awaitingExitConfirmation = false;
                break;

            default:
                callbacks.AddResponseLine("Please type 'yes' to exit or 'no' to cancel.");
                // Keep awaitingExitConfirmation = true to continue waiting
                break;
        }
    }

    private IEnumerator ExitAnimationCoroutine()
    {

        callbacks.SetUserInputLineActive?.Invoke(false);
        
        yield return callbacks.PlayTypewriterEffect("Shutting down DebuggerOS...");
        
        yield return new WaitForSeconds(0.5f);
        yield return callbacks.PlayTypewriterEffect("Goodbye!");
        yield return new WaitForSeconds(1f);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ExecuteUnknownCommand(string command)
    {
        callbacks.AddResponseLine($"'{command}' is not recognized as an internal or external command.");
        callbacks.AddResponseLine("Type 'help' for a list of available commands.");
    }
}