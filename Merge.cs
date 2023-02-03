namespace ParadoxModMerger;

static class Merge
{
    static ConsoleLogger logger = new($"logs", "merge");
    static void Log(params string[] msg) => logger.Log(msg);

    public static void MergeAll(string[] moddirs, string outDir)
    {
        Log("b", $"found {moddirs.Length} mod dirs");
        for (short i = 0; i < moddirs.Length; i++)
        {
            Log("b", $"[{i + 1}/{moddirs.Length}] merging mod ", "c", $"{moddirs[i]}");
            Directory.Copy(moddirs[i], outDir, out List<string> _conflicts, true);
            Program.LogConflicts(_conflicts);
        }
    }

    public static void MergeSingle(string moddir, string outDir)
    {
        Directory.Copy(moddir, outDir, out List<string> _conflicts, true);
        Program.LogConflicts(_conflicts);
    }
}