global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Text;
global using DTLib;
global using DTLib.Extensions;
global using DTLib.Filesystem;
global using DTLib.Logging;
using DTLib.Console;

namespace ParadoxModMerger;

public static class Program
{
    static ConsoleLogger logger = new($"logs", "main");
    static void Log(params string[] msg) => logger.Log(msg);
    
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding=Encoding.UTF8;
            Console.InputEncoding=Encoding.UTF8;
            
            string outPath = "" ;
            
            new LaunchArgumentParser(
                new LaunchArgument(new []{"o", "out"}, 
                    "sets output path", 
                    p => outPath=p,
                    "out_path",
                    0),
                new LaunchArgument(new []{"clear"}, 
                    "Clear mod files and put them into separate dirs in output dir. Requires -o", 
                    wdir=>Workshop.ClearWorkshop(wdir, outPath), 
                    "workshop_dir", 
                    1),
                new LaunchArgument(new []{"diff"}, 
                    "Compares mod files by hash",
                    p=>Diff.DiffCommandHandler(p), 
                    "first_mod_directory:second_mod_directory:...", 
                    1),
                new LaunchArgument(new []{"diff-conflicts"},
                    "reads conflicts_XXX.dtsod file and shows text diff for each file",
                    p=>Diff.DiffConflictsCommandHandler(p),
                    "conflicts_dtsod_path", 
                    1
                ),
                new LaunchArgument(new []{"merge-subdirs"}, 
                    "Merges mods and shows conflicts. Requires -o", 
                    d => Merge.MergeAll(Directory.GetDirectories(d), outPath),
                    "dir_with_mods", 
                    1),
                new LaunchArgument(new []{"merge-into", "merge-single"}, 
                    "Merges one mod into output dir and shows conflicts. Requires -o",
                    mod=>Merge.MergeInto(mod, outPath), 
                    "mod_dir",
                    1),
                new LaunchArgument(new []{"gen-rus-locale"},
                    "Creates l_russian copy of english locale in output directory. Requires -o",
                    eng=>Localisation.GenerateRussian(eng, outPath),
                    "english_locale_path", 
                    1),
                new LaunchArgument(new []{"desc"}, 
                    "Downloads mod description from steam to new file in outDir. Requires -o",
                    id=>Workshop.CreateDescFile(id, outPath), 
                    "mod_id",
                    1)
            ).ParseAndHandle(args);
        }
        catch (LaunchArgumentParser.ExitAfterHelpException)
        { }
        catch (Exception ex)
        {
            Log("r", DTLib.Ben.Demystifier.ExceptionExtensions.ToStringDemystified(ex));
        }
        Console.ResetColor();
    }

    public static IOPath[] SplitStringToPaths(string connected_paths)
    {
        if (!connected_paths.Contains(':')) 
            throw new Exception($"<{connected_paths}> doesn't contain any separators (:)");
        string[] split = connected_paths.Split(':');
        IOPath[] split_iop = new IOPath[split.Length];
        for (int i = 0; i < split.Length; i++)
            split_iop[i] = new IOPath(split[i]);
        return split_iop;
    }
}