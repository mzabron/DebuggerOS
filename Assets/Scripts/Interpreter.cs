using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
    [System.Serializable]
    public class InterpreterCallbacks
    {
        public Func<string, GameObject> AddResponseLine;
        public Action ClearScreen;
        public Func<string, IEnumerator> PlayTypewriterEffect;
        public Action<IEnumerator> StartCoroutine;
        public Action<bool> SetUserInputLineActive;
        public Action<string> UpdateDirectoryPath;
    }

    private InterpreterCallbacks callbacks;
    private Dictionary<string, DirectoryInitializer.DirectoryNode> directories;
    private Dictionary<string, string> variables = new Dictionary<string, string>();
    private bool awaitingExitConfirmation = false;
    private List<string> currentPath = new List<string>();

    private int dir1VisitCount = 0;
    private int doublingLevel = 0;

    public void Initialize(InterpreterCallbacks interpreterCallbacks)
    {
        callbacks = interpreterCallbacks;

        variables["version"] = "4.2.0";
        variables["user"] = "developer";
        variables["system"] = "DebuggerOS";

        directories = DirectoryInitializer.GetDirectories();
    }

    // ============================ MAIN PROCESS ============================

    public void ProcessCommand(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return;

        if (awaitingExitConfirmation)
        {
            ProcessExitConfirmation(userInput.Trim().ToLower());
            return;
        }

        string command = userInput.Trim();
        string[] parts = command.Split(' ');
        string mainCommand = parts[0].ToLower();

        switch (mainCommand)
        {
            case "help": ExecuteHelp(); break;
            case "clear":
            case "cls": ExecuteClear(); break;
            case "echo": ExecuteEcho(userInput); break;
            case "version":
            case "ver": ExecuteVersion(); break;
            case "date": ExecuteDate(); break;
            case "time": ExecuteTime(); break;
            case "whoami": ExecuteWhoAmI(); break;
            case "ls":
            case "dir": ExecuteListDirectory(); break;
            case "cd": ExecuteChangeDirectory(parts); break;
            case "pwd": ExecutePrintWorkingDirectory(); break;
            case "cat": ExecuteCat(parts); break;
            case "exit":
            case "quit": ExecuteExit(); break;
            default: ExecuteUnknownCommand(userInput); break;
        }
    }

    // ============================ COMMANDS ============================

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
        callbacks.AddResponseLine("  cd [dir]      - Change directory");
        callbacks.AddResponseLine("  cd ..         - Go to parent directory");
        callbacks.AddResponseLine("  cat [file]    - Show contents of a file");
        callbacks.AddResponseLine("  pwd           - Print working directory");
        callbacks.AddResponseLine("  exit, quit    - Exit the terminal");
        callbacks.AddResponseLine("");
    }

    private void ExecuteClear() => callbacks.ClearScreen?.Invoke();

    private void ExecuteEcho(string userInput)
    {
        if (userInput.Length > 5)
        {
            string textToEcho = userInput.Substring(5);
            callbacks.AddResponseLine(textToEcho);
        }
        else callbacks.AddResponseLine("");
    }

    private void ExecuteVersion()
    {
        callbacks.AddResponseLine($"DebuggerOS {variables["version"]} - Secure Developer Environment");
        callbacks.AddResponseLine("(c) 2025 Debugger Corporation. All rights reserved.");
    }

    private void ExecuteDate() =>
        callbacks.AddResponseLine($"Current date: {DateTime.Now:dddd, MMMM dd, yyyy}");

    private void ExecuteTime() =>
        callbacks.AddResponseLine($"Current time: {DateTime.Now:HH:mm:ss}");

    private void ExecuteWhoAmI() =>
        callbacks.AddResponseLine(System.Environment.UserName);

    private bool IsDoublingMode()
    {
        return currentPath.Count > 0 && currentPath[0] == "dir1";
    }

    private void ExecuteListDirectory()
    {
        string currentPathKey = string.Join("/", currentPath);

        if (currentPathKey.StartsWith("dir0"))
            currentPathKey = "dir0";

        // Dynamiczne podwajanie katalogów tylko w drzewie dir1
        if (IsDoublingMode())
        {
            int count = (int)Math.Pow(2, doublingLevel + 1);
            callbacks.AddResponseLine("Directory listing:");
            callbacks.AddResponseLine("");
            for (int i = 1; i <= count; i++)
            {
                callbacks.AddResponseLine($"  dir{i}/");
            }
            callbacks.AddResponseLine("");
            callbacks.AddResponseLine($"{count} directories, 0 files");
            return;
        }

        // Statyczne katalogi z DirectoryInitializer
        if (directories.ContainsKey(currentPathKey))
        {
            var node = directories[currentPathKey];
            callbacks.AddResponseLine("Directory listing:");
            callbacks.AddResponseLine("");
            foreach (var sub in node.Subdirectories)
                callbacks.AddResponseLine($"  {sub}/");
            foreach (var file in node.Files)
                callbacks.AddResponseLine($"  <color=#00ffff>{file.Key}</color>");
            callbacks.AddResponseLine("");
            callbacks.AddResponseLine($"{node.Subdirectories.Count} directories, {node.Files.Count} files");
        }
        else
        {
            callbacks.AddResponseLine("Directory is empty.");
        }
    }

    private void ExecuteChangeDirectory(string[] parts)
    {
        if (parts.Length < 2)
        {
            callbacks.AddResponseLine("Usage: cd [directory]");
            callbacks.AddResponseLine("       cd ..  (to go back)");
            return;
        }

        string targetDirectory = parts[1];
        string currentPathKey = string.Join("/", currentPath);

        // Powrót do katalogu nadrzędnego
        if (targetDirectory == "..")
        {
            if (currentPath.Count > 0)
            {
                if (IsDoublingMode() && doublingLevel > 0)
                    doublingLevel--;
                currentPath.RemoveAt(currentPath.Count - 1);
                UpdateDirectoryPrompt();
                callbacks.AddResponseLine("Changed to parent directory.");
            }
            else
            {
                callbacks.AddResponseLine("Already at root directory.");
            }
            return;
        }

        if (targetDirectory == "dir0")
        {
            currentPath.Add("dir0");
            UpdateDirectoryPrompt();
            callbacks.AddResponseLine("Changed directory to dir0");
            return;
        }

        // Dynamiczne podwajanie tylko w drzewie dir1
        if (IsDoublingMode())
        {
            int count = (int)Math.Pow(2, doublingLevel + 1);
            for (int i = 1; i <= count; i++)
            {
                if (targetDirectory == $"dir{i}")
                {
                    currentPath.Add(targetDirectory);
                    doublingLevel++;
                    UpdateDirectoryPrompt();
                    callbacks.AddResponseLine($"Changed directory to {targetDirectory}");
                    return;
                }
            }
            callbacks.AddResponseLine($"Directory '{targetDirectory}' not found.");
            return;
        }

        // Statyczne katalogi z DirectoryInitializer
        if (directories.ContainsKey(currentPathKey) &&
            directories[currentPathKey].Subdirectories.Contains(targetDirectory))
        {
            currentPath.Add(targetDirectory);
            UpdateDirectoryPrompt();
            callbacks.AddResponseLine($"Changed directory to {targetDirectory}");
        }
        else
        {
            callbacks.AddResponseLine($"Directory '{targetDirectory}' not found.");
        }
    }

    private void ExecutePrintWorkingDirectory()
    {
        if (currentPath.Count == 0)
            callbacks.AddResponseLine("/");
        else
            callbacks.AddResponseLine("/" + string.Join("/", currentPath));
    }

    private void UpdateDirectoryPrompt()
    {
        string pathSuffix = "";
        if (currentPath.Count > 0)
        {
            if (currentPath.Count <= 3)
            {
                pathSuffix = "\\" + string.Join("\\", currentPath);
            }
            else
            {
                string firstFolder = currentPath[0];
                string lastFolder = currentPath[currentPath.Count - 1];
                pathSuffix = $"\\{firstFolder}\\...\\{lastFolder}";
            }
        }

        callbacks.UpdateDirectoryPath?.Invoke(pathSuffix);
    }

    private void ExecuteExit()
    {
        callbacks.AddResponseLine("Are you sure you want to exit?");
        callbacks.AddResponseLine("<color=yellow>Warning: Progress will not be saved!</color>");
        callbacks.AddResponseLine("Type 'yes' to exit or 'no' to cancel.");
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
                break;
        }
    }

    private void ExecuteCat(string[] parts)
    {
        if (parts.Length < 2)
        {
            callbacks.AddResponseLine("Usage: cat [file]");
            return;
        }

        string fileName = parts[1];
        string currentPathKey = string.Join("/", currentPath);

        if (!directories.ContainsKey(currentPathKey))
        {
            callbacks.AddResponseLine("Directory not found.");
            return;
        }

        var node = directories[currentPathKey];
        if (node.Files.ContainsKey(fileName))
        {
            string content = node.Files[fileName];
            string[] lines = content.Split('\n');

            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                string currentLine = "";

                foreach (string word in words)
                {
                    if ((currentLine + word).Length > 92)
                    {
                        callbacks.AddResponseLine("<i>" + currentLine.TrimEnd() + "</i>");
                        currentLine = "";
                    }
                    currentLine += word + " ";
                }

                if (!string.IsNullOrEmpty(currentLine))
                    callbacks.AddResponseLine("<i>" + currentLine.TrimEnd() + "</i>");
            }
        }
        else
        {
            callbacks.AddResponseLine($"File '{fileName}' not found.");
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
