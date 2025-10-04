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
            Files = new Dictionary<string, string> { { "quick_note.txt", "Matthias_08.04.2012\nIf you're reading this… it means I failed. The System got me. You're the only shot left.Don't even think about what's going on. Just… stop it. It's watching.There's only one way: find the script Killcore.c, run it.That's all. No shortcuts.If you mess up… it won't let you leave.Good luck. I didn't have much. I'm buried in this mess, and it's watching me too." } }

        };

        directories["dir0"] = new DirectoryNode
        {
            Subdirectories = new List<string> { "dir0" },
            Files = new Dictionary<string, string> { { "note.txt", "NOTATKA W DIR 1" } }
        };

        // directories["folder1/sub1"] = new DirectoryNode
        // {
        //     Subdirectories = new List<string>(),
        //     Files = new Dictionary<string, string> { { "info.txt", "Information inside sub1 folder." } }
        // };

        directories["dir1"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "note.txt", "NOTATKA W DIR 3" } }
        };

        directories["dir2"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Lucy's_note", "<i>1 1 3 7 14 27<i>" } }
        };

        directories["secured"] = new DirectoryNode
        {
            Subdirectories = new List<string> { "usersDatabase" },
            Files = new Dictionary<string, string> { { "shutdown.c", "This is a restricted file." } }
        };

        directories["secured/usersDatabase"] = new DirectoryNode
        {
            Subdirectories = new List<string> { "Matthias", "Lucy", "Alice", "Lucas", "Mark" },
            Files = new Dictionary<string, string> { { "PLEASE_READ_THIS", "Hi, I'm Mark. I think I've figured out what's going on here… or I'm way too close to figuring it out. For months, strange things have been happening around me. You can see the news - people dying for no clear reason. The police do nothing, and emergency services can't explain the deaths. Every case is written off as a heart attack, but I didn't believe that.\n\nI started my own investigation — talking to the families, researching the victims' lives and I found something really strange: all of them had the same system installed - DebuggerOS. I borrowed one of the victims' devices and started testing it. It's like a virus you can't get rid of. That's why I created shutdown.c, the only way to erase the whole system and clean up a device.\n\nUnfortunately, one morning I woke up, opened my laptop, and saw that the system had installed itself. That's why I'm writing this note: if something happens to me, you'll find this and be able to run my script to deactivate it.\n\nTo compile the script, use:\n\ngcc shutdown shutdown.c\n\nThen run it with:\n\n./shutdown\n\nRemember this command, the system doesn't want you to know how to do it, so you won't find it anywhere else." } }
        };

        // User subdirectories with individual files
        directories["secured/usersDatabase/Matthias"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Matthias", "User deleted permanently. No remaining threat." } }
        };

        directories["secured/usersDatabase/Lucy"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Lucy", "User deleted permanently. No remaining threat." } }
        };

        directories["secured/usersDatabase/Alice"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Alice", "User deleted permanently. No remaining threat." } }
        };

        directories["secured/usersDatabase/Lucas"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Lucas", "User deleted permanently. No remaining threat." } }
        };

        directories["secured/usersDatabase/Mark"] = new DirectoryNode
        {
            Subdirectories = new List<string>(),
            Files = new Dictionary<string, string> { { "Mark", "User deleted permanently. No remaining threat." } }
        };

        return directories;
    }
}
