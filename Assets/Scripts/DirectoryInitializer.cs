using System.Collections.Generic;
using UnityEngine;

public static class DirectoryInitializer
{
    public class DirectoryNode
    {
        public List<string> Subdirectories = new List<string>();
        public Dictionary<string, string> Files = new Dictionary<string, string>();
    }

    // Funkcja zwraca gotową strukturę katalogów
    public static Dictionary<string, DirectoryNode> GetDirectories()
    {
        var directories = new Dictionary<string, DirectoryNode>();

        // ROOT "/"
        directories[""] = new DirectoryNode
        {
            Subdirectories = new List<string> { "folder1", "folder2", "secured" },
            Files = new Dictionary<string, string> { { "readme.txt", "Welcome to DebuggerOS!\nThis is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file.This is the root readme file." } }
        };

        directories["folder1"] = new DirectoryNode
        {
            Subdirectories = new List<string> { "sub1" },
            Files = new Dictionary<string, string> { { "notes.txt", "These are some notes in folder1." } }
        };

        directories["folder1/sub1"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "info.txt", "Information inside sub1 folder." } }
        };

        directories["folder2"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "data.txt", "Data stored in folder2." } }
        };

        directories["secured"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string>()
        };

        return directories;
    }
}
