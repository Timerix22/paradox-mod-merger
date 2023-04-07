namespace ParadoxModMerger;

static class Merge
{
    static ConsoleLogger logger = new("logs", "merge");
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
            if (file_basename=="descriptor.mod") // skip file
                continue;
            
            if (file_basename == modlist_filename) // append modlist
            {
                string subModlistText = File.ReadAllText(files[i]);
                File.AppendAllText(out_modlist_file, subModlistText);
                continue;
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
        File.AppendAllText(out_modlist_file, $"{moddir.LastName()}\n");
        ModDirCopy(moddir, outDir, out_modlist_file);
    }

    public static void ConsoleAskYN(string question, Action? yes, Action? no)
    {
        while (true)
        {
            Log("y", question + " [y/n]");
            string answ = Program.YesAll ? "y" : ColoredConsole.Read("w").ToLower();
            if (answ == "y")
            {
                Log("c",$"answer: {answ}");
                yes?.Invoke();
                break;
            }
            if (answ == "n") {
                Log("c",$"answer: {answ}");
                no?.Invoke();
                break;
            }
            Log("r", $"incorrect answer: {answ}");
        }
    }
    
    static void HandleConflicts(IOPath[] moddirs)
    {
        var conflicts = Diff.FindModConflicts(moddirs);
        conflicts = conflicts.Where(cfl => 
            !cfl.FilePath.EndsWith("descriptor.mod") &&
            !cfl.FilePath.EndsWith(modlist_filename)).ToList();
        if (conflicts.Count < 1) 
            return;
        
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

    
    public static void UpdateMods(IOPath updated_mods_dir, IOPath[] outdated_dirs, IOPath backup_dir)
    {
        var src_dir_mods = Directory.GetDirectories(updated_mods_dir).ToList();
        List<IOPath> not_found_mods = new List<IOPath>();
        List<IOPath> changed_mods = new List<IOPath>();
        List<IOPath> unchanged_mods = new List<IOPath>();
        
        foreach (IOPath outdated_mods_dir in outdated_dirs)
        foreach (var mod in Directory.GetDirectories(outdated_mods_dir))
        {
            Log("b", "updating mod ", "c", mod.LastName().Str);
            var mod_updated = mod.ReplaceBase(outdated_mods_dir, updated_mods_dir);
            int updated_index = src_dir_mods.IndexOf(mod_updated); // IOPath comparison doesnt work?
            
            if (updated_index == -1)
            {
                Log("m", $"mod {mod.LastName()} not found in {updated_mods_dir}");
                not_found_mods.Add(mod);
                continue;
            }

            var diff = Diff.DiffDirs(mod, mod_updated)
                .Where(d => d.State != DiffState.Equal)
                .ToList();
            if (!diff.Any())
            {
                Log("gray","unchanged");
                unchanged_mods.Add(mod);
                continue;
            }

            Log("file difference:");
            foreach (var fileD in diff)
            {
                string color = fileD.State switch
                {
                    DiffState.Added => "g",
                    DiffState.Changed => "y",
                    DiffState.Removed => "m",
                    _ => throw new ArgumentOutOfRangeException()
                };
                Log(color, fileD.Value.Str);
            }

            ConsoleAskYN("replace mod with its updated version?",
            () =>
                {
                    var mod_backup = Path.Concat(backup_dir, mod.LastName());
                    Log($"moving {mod} to {backup_dir}");
                    Directory.Move(mod, mod_backup, false);
                    Log($"copying {mod_updated} to {outdated_mods_dir}");
                    Directory.Copy(mod_updated, mod, false);
                    Log("g", $"mod {mod.LastName()} updated");
                    changed_mods.Add(mod);
                },
            ()=>unchanged_mods.Add(mod));
        }

        List<IOPath> added_mods = new List<IOPath>(src_dir_mods.Count - changed_mods.Count);
        var found_mods = changed_mods
            .Concat(unchanged_mods)
            .Select(m=>Path.Concat(updated_mods_dir, m.LastName()))
            .ToList();
        foreach (var modD in Diff.DiffCollections(found_mods, src_dir_mods))
        {
            if (modD.State == DiffState.Added)
                added_mods.Add(modD.Value);
        }

        Log("b", "mod update summary:");
        if (added_mods.Count > 0)
            Log("w", $"added {added_mods.Count}:\n",
                "g", added_mods.MergeToString('\n'));
        if (changed_mods.Count > 0)
            Log("w", $"changed {changed_mods.Count}:\n",
                "y", changed_mods.MergeToString('\n'));
        if (not_found_mods.Count > 0)
            Log("w", $"not found {not_found_mods.Count}:\n",
                "m", not_found_mods.MergeToString('\n'));
        if (unchanged_mods.Count>0)
            Log("w", $"unchanged {unchanged_mods.Count}");

        IOPath new_mods_copy_dir = Path.Concat(backup_dir.ParentDir(),
            $"!new_{DateTime.Now.ToString(MyTimeFormat.ForFileNames)}");
        ConsoleAskYN($"copy new mods to {new_mods_copy_dir}", () =>
        {
            foreach (var mod in added_mods)
            {
                Directory.Copy(mod, Path.Concat(new_mods_copy_dir, mod), false);
            } 
        }, 
        null);
    }

    public static void RenameModsCommandHandler(string dir_with_mods, string rename_pairs_str)
    {
        string[] split = Program.SplitArgToStrings(rename_pairs_str, false);
        int rename_pairs_length = split.Length / 2;
        if (split.Length % 2 != 0)
            throw new Exception($"rename_pairs length is not even ({rename_pairs_length})");
        var rename_pairs = new (string old_name, string new_name)[rename_pairs_length];
        for (int i = 0; i < rename_pairs_length; i++)
        {
            rename_pairs[i] = (split[i * 2], split[i * 2 + 1]);
        }
        
        RenameMods(dir_with_mods, rename_pairs);
    }
    
    public static void RenameMods(IOPath dir_with_mods, (string old_name,string new_name)[] rename_pairs)
    {
        foreach (var re in rename_pairs)
        {
            var old_mod_path = Path.Concat(dir_with_mods, re.old_name);
            var new_mod_path = Path.Concat(dir_with_mods, re.new_name);
            if (Directory.Exists(old_mod_path))
            {
                Log("b","renaming ", "c", re.old_name, "b", " to ", "c", re.new_name);
                Directory.Move(old_mod_path, new_mod_path, false);
            }
        }
    }
}