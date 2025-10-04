using System.Collections.Generic;
using UnityEngine;

public static class DirectoryInitializer
{
    public class DirectoryNode
    {
        public List<string> Subdirectories = new List<string>();
        public Dictionary<string, string> Files = new Dictionary<string, string>();
    }

    public static Dictionary<string, DirectoryNode> GetDirectories()
    {
        var directories = new Dictionary<string, DirectoryNode>();

        // ROOT "/"
        directories[""] = new DirectoryNode
        {
            Subdirectories = new List<string> { "dir0", "dir1", "dir2", "secured" },
            Files = new Dictionary<string, string> { { "quick_note.txt", "Matthias_08.04.2012\nIf you’re reading this… it means I failed. The System got me. You’re the only shot left. Don’t even think about what’s going on. Just… stop it. It’s watching.There’s only one way: find the script Shutdown.c, run it. That’s all. No shortcuts. If you mess up… it won’t let you leave.Good luck. I didn’t have much. I’m buried in this mess, and it's watching me too." } }

        };

        directories["dir0"] = new DirectoryNode
        {
            Subdirectories = new List<string> { "dir0" },
            Files = new Dictionary<string, string> { { "note.txt", "This folder may be recursive" } }
        };

        directories["dir1"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
        };

        directories["dir2"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Lucys_note.txt", "<i>1 1 3 7 14 27<i>" } }
        };

        directories["secured"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string>()
        };

        return directories;
    }
}
