using System.Linq;
using DTLib.Console;

namespace ParadoxModMerger;

static class Merge
{
    static ConsoleLogger logger = new($"logs", "merge");
    static void Log(params string[] msg) => logger.Log(msg);

    private const string modlist_filename = "modlist.txt";

    /// analog of Directory.Copy(srcDir, outDir, true)
    /// has special behavior for some files
    private static void ModDirCopy(IOPath srcDir, IOPath outDir, IOPath out_modlist_file)
    {
        var files = Directory.GetAllFiles(srcDir);
        for (int i = 0; i < files.Count; i++)
        {
            string file_basename = files[i].LastName().Str;
            if(file_basename=="descriptor.mod") // skip file
                continue;
            if (file_basename == modlist_filename) // append modlist
            {
                File.AppendAllText(out_modlist_file, File.ReadAllText(files[i]));
            }
                
            var newfile = files[i].ReplaceBase(srcDir, outDir);
            File.Copy(files[i], newfile, true);
        }
    }
    
    public static void MergeAll(IOPath[] moddirs, IOPath outDir)
    {
        Log("b", $"found {moddirs.Length} mod dirs");
        HandleConflicts(moddirs);

        var modnamelist = new string[moddirs.Length];
        IOPath out_modlist_file = Path.Concat(outDir, modlist_filename);
        for (short i = 0; i < moddirs.Length; i++)
        {
            Log("b", $"[{i + 1}/{moddirs.Length}] merging mod ", "c", $"{moddirs[i]}");
            ModDirCopy(moddirs[i], outDir, out_modlist_file);
            modnamelist[i]=moddirs[i].LastName().Str;
        }

        File.AppendAllText(out_modlist_file, modnamelist.MergeToString('\n'));
        File.AppendAllText(out_modlist_file, "\n");
    }

    public static void MergeInto(IOPath moddir, IOPath outDir)
    {
        HandleConflicts(new[] { moddir, outDir });
        IOPath out_modlist_file = Path.Concat(outDir, modlist_filename);
        ModDirCopy(moddir, outDir, modlist_filename);
        File.AppendAllText(out_modlist_file, $"{moddir.LastName()}\n");
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