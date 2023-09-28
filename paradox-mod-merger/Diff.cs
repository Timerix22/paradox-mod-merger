using diff_text;
using DTLib.Dtsod;

namespace ParadoxModMerger; 


public enum DiffState
{
    Added, Equal, Removed, Changed
}
    
public record struct DiffPart<T>(T Value, DiffState State);
public record struct ConflictingModFile(string FilePath, string[] Mods);

static class Diff
{
    static ContextLogger logger = new (nameof(Diff), new FileLogger("logs", "diff"));
    static void Log(params string[] msg) => logger.LogColored(msg);

    public static void DiffCommandHandler(string connected_pathes)
    {
        IOPath[] moddirs = Program.SplitArgToPaths(connected_pathes, false);
        var conflicts = FindModConflicts(moddirs);
        LogModConflicts(conflicts);
    }

    public static void DiffDetailedCommandHandler(IOPath conflicts_dtsod_path)
    {
        var dtsod = new DtsodV23(File.ReadAllText(conflicts_dtsod_path));
        var conflicts = new ConflictingModFile[dtsod.Count];
        int i = 0;
        foreach (var p in dtsod)
        {
            conflicts[i]=new ConflictingModFile(p.Key, ((List<object>)p.Value).Select(m=>(string)m).ToArray());
            i++;
        }
        ShowConflictsTextDiff(conflicts);
    }
    
    public static void ShowConflictsTextDiff(ConflictingModFile[] conflicts)
    {
        int selected_confl_i = 0;
        int selected_mod0_i = 0;
        int selected_mod1_i = 1;
        int line_buffer_offset = 0;
        
        int line_i=0;
        for (int i = 0; i < conflicts.Length; i++) 
            line_i += 1 + conflicts[i].Mods.Length;

        var lines = new (ConsoleColor color, string text)[line_i];
        // set lines text
        line_i = 0;
        for (int confl_i = 0; confl_i < conflicts.Length; confl_i++)
        {
            lines[line_i].color = ConsoleColor.White;
            lines[line_i++].text = $"[{confl_i}]{conflicts[confl_i].FilePath}";
            for (int mod_i = 0; mod_i < conflicts[confl_i].Mods.Length; mod_i++)
            {
                lines[line_i].color = ConsoleColor.Gray;
                lines[line_i++].text = $"    [{mod_i}] {conflicts[confl_i].Mods[mod_i]}";
            }
        }
        
        while (true)
        {
            try
            {
                // set line colors
                line_i = 0;
                for (int confl_i = 0; confl_i < conflicts.Length; confl_i++)
                {
                    if (confl_i == selected_confl_i)
                        lines[line_i].color = ConsoleColor.Blue;
                    else lines[line_i].color = ConsoleColor.White;
                    line_i++;
                    
                    for (int mod_i = 0; mod_i < conflicts[confl_i].Mods.Length; mod_i++)
                    {
                        if (confl_i == selected_confl_i)
                        {
                            if (mod_i == selected_mod0_i)
                                lines[line_i].color = ConsoleColor.Yellow;
                            else if (mod_i == selected_mod1_i)
                                lines[line_i].color = ConsoleColor.Green;
                            else lines[line_i].color = ConsoleColor.Gray;
                        }
                        else lines[line_i].color = ConsoleColor.Gray;
                        line_i++;
                    }
                }

                // print lines
                Console.Clear();
                ColoredConsole.WriteLine("c",
                    "[Q]exit [Up/Down]select file [Left/Right][number]select mod [Enter]show diff");
                for (int i = line_buffer_offset;
                     i < lines.Length && i < Console.WindowHeight + line_buffer_offset - 2;
                     i++)
                    ColoredConsole.WriteLine(lines[i].color, lines[i].text);

                // read input
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        return;
                    case ConsoleKey.UpArrow:
                        if (selected_confl_i > 0)
                        {
                            line_buffer_offset -= conflicts[selected_confl_i].Mods.Length;
                            line_buffer_offset--;
                            selected_confl_i--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if (selected_confl_i < conflicts.Length - 1)
                        {
                            selected_confl_i++;
                            line_buffer_offset++;
                            line_buffer_offset += conflicts[selected_confl_i].Mods.Length;
                        }

                        break;
                    case ConsoleKey.Enter:
                    {
                        var conflict = conflicts[selected_confl_i];
                        var path0 = Path.Concat(conflict.Mods[selected_mod0_i], conflict.FilePath);
                        var path1 = Path.Concat(conflict.Mods[selected_mod1_i], conflict.FilePath);
                        if (path0.Extension() == "dds")
                        {
                            ColoredConsole.Write("r", $"file {path0} is not text file\n",
                                "c", "press enter to continue: ");
                            Console.ReadLine();   
                        }
                        var textDiff = DiffText.FileDiff(path0.Str, path1.Str);
                        Console.Clear();
                        DiffText.PrintDiff(textDiff, true);
                        ColoredConsole.Write("c", "\npress enter to continue: ");
                        Console.ReadLine();
                        break;
                    }
                    case ConsoleKey.LeftArrow:
                    {
                        ColoredConsole.Write("w", "enter left mod number: ");
                        string answ = Console.ReadLine()!;
                        selected_mod0_i = answ.ToInt();
                        break;
                    }
                    case ConsoleKey.RightArrow:
                    {
                        ColoredConsole.Write("w", "enter right mod number: ");
                        string answ = Console.ReadLine()!;
                        selected_mod1_i = answ.ToInt();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("r",ex.ToStringDemystified());
                ColoredConsole.Write("c", "\npress enter to continue: ");
                Console.ReadLine();
            }
        }
    }
    
    
    public static ICollection<ConflictingModFile> FindModConflicts(IOPath[] modpaths)
    {
        var all_files = new Dictionary<string, List<string>>();
        foreach (var modp in modpaths)
        {
            foreach (var _file in Directory.GetAllFiles(modp))
            {
                var file = _file.RemoveBase(modp);
                if (all_files.TryGetValue(file.Str, out var associated_mods))
                    associated_mods.Add(modp.Str);
                else all_files.Add(file.Str, new List<string>(1) { modp.Str });
            }
        }

        var output = new List<ConflictingModFile>();
        foreach (var p  in all_files)
            if (p.Value.Count > 1)
                output.Add(new ConflictingModFile(p.Key, p.Value.ToArray()));
        return output;
    }

    public static IEnumerable<DiffPart<T>> DiffCollections<T>(ICollection<T> col0, ICollection<T> col1)
    {
        foreach (var el in col0)
            yield return new DiffPart<T>(el, col1.Contains(el) ? DiffState.Equal : DiffState.Removed);
        foreach (var el in col1)
            if (!col0.Contains(el))
                yield return new DiffPart<T>(el, DiffState.Added);
    }

    public static IEnumerable<DiffPart<IOPath>> DiffDirs(IOPath dir0, IOPath dir1)
    {
        var files0 = Directory.GetAllFiles(dir0).Select(p=>p.RemoveBase(dir0)).ToList();
        var files1 = Directory.GetAllFiles(dir1).Select(p=>p.RemoveBase(dir1)).ToList();
        var filesMerged = DiffCollections(files0, files1);
        
        foreach (var filePathDiff in filesMerged)
        {
            if (filePathDiff.State == DiffState.Equal)
            {
                Hasher hasher = new Hasher();
                string hash0=hasher.HashFile(Path.Concat(dir0, filePathDiff.Value)).HashToString();
                string hash1=hasher.HashFile(Path.Concat(dir1, filePathDiff.Value)).HashToString();
                if (hash0 != hash1)
                    yield return filePathDiff with { State = DiffState.Changed };
                else yield return filePathDiff;
            }
            else yield return filePathDiff;
        }
    }
    
    
    // вывод конфликтующих файлов при -merge и -clear если такие есть
    public static void LogModConflicts(ICollection<ConflictingModFile> conflicts)
    {
        if(conflicts.Count==0) return;
        
        Log("m",$"found {conflicts.Count} conflicting files:");
        var dtsod = new DtsodV23();
        foreach (var cfl in conflicts)
        {
            Log("m", "file ","c", cfl.FilePath, "m", " in mods ", "m", cfl.Mods.MergeToString(", "));
            dtsod.Add(cfl.FilePath, cfl.Mods);
        }

        string timeStr = DateTime.Now.ToString(MyTimeFormat.ForFileNames);
        IOPath conflicts_dtsod_path = Path.Concat("conflicts", $"conflicts_{timeStr}.dtsod");
        File.WriteAllText(conflicts_dtsod_path, dtsod.ToString());
        conflicts_dtsod_path = Path.Concat("conflicts", "conflicts_latest.dtsod");
        File.WriteAllText(conflicts_dtsod_path, dtsod.ToString());
        Log("m",$"conflicts have written to {conflicts_dtsod_path}");
    }
}