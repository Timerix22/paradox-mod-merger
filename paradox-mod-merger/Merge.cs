using System.Linq;
using DTLib.Console;

namespace ParadoxModMerger;

static class Merge
{
    static ConsoleLogger logger = new($"logs", "merge");
    static void Log(params string[] msg) => logger.Log(msg);

    public static void MergeAll(IOPath[] moddirs, IOPath outDir)
    {
        Log("b", $"found {moddirs.Length} mod dirs");
        HandleConflicts(moddirs);
        
        for (short i = 0; i < moddirs.Length; i++)
        {
            Log("b", $"[{i + 1}/{moddirs.Length}] merging mod ", "c", $"{moddirs[i]}");
            Directory.Copy(moddirs[i], outDir, true);
        }
    }

    public static void MergeInto(IOPath moddir, IOPath outDir)
    {
        HandleConflicts(new[] { moddir, outDir });
        Directory.Copy(moddir, outDir, true);
    }

    public static void ConsoleAskYN(string question, Action yes, Action no)
    {
        Log("y", question + " [y/n]");
        string answ = ColoredConsole.Read("w");
        if (answ == "y") yes();
        else no();
    }
    
    static void HandleConflicts(IOPath[] moddirs)
    {
        var conflicts = Diff.FindModConflicts(moddirs);
        if (conflicts.Count <= 0) return;
        
        Diff.LogModConflicts(conflicts);
        ConsoleAskYN("continue merge?",
            () =>Log("y", "merge continued"),
            () =>
            {
                Log("y", "merge interrupted"); 
                ConsoleAskYN("show text diff?", 
                    () => Diff.ShowConflictsTextDiff(conflicts.ToArray()),
                    () => {});
            });
    }
}