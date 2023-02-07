namespace ParadoxModMerger;

static class Merge
{
    static ConsoleLogger logger = new($"logs", "merge");
    static void Log(params string[] msg) => logger.Log(msg);

    public static void MergeAll(IOPath[] moddirs, IOPath outDir)
    {
        Log("b", $"found {moddirs.Length} mod dirs");
        for (short i = 0; i < moddirs.Length; i++)
        {
            Log("b", $"[{i + 1}/{moddirs.Length}] merging mod ", "c", $"{moddirs[i]}");
            Directory.Copy(moddirs[i], outDir, true, out var _conflicts);
            Program.LogConflicts(_conflicts);
        }
    }

    public static void MergeSingle(IOPath moddir, IOPath outDir)
    {
        Directory.Copy(moddir, outDir, true, out var _conflicts);
        Program.LogConflicts(_conflicts);
    }
}