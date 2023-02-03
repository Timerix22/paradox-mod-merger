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
            string outPath = "" ;
            
            new LaunchArgumentParser(
                new LaunchArgument(new []{"o", "out"}, 
                    "sets output path", 
                    p => outPath=p,
                    "out_path",
                    0),
                new LaunchArgument(new []{"clear"}, 
                    "Clear mod files and put them into separate dirs in output dir. Requires -o", 
                    wdir=>Clear.ClearWorkshop(wdir, outPath), 
                    "workshop_dir", 
                    1),
                new LaunchArgument(new []{"diff"}, 
                    "Compare mod files by hash",
                    p=>Diff.DiffMods(p), 
                    "first_mod_directory;second_mod_directory", 1),
                new LaunchArgument(new []{"merge-subdirs"}, 
                    "Merge mods and show conflicts. Requires -o", 
                    d => Merge.MergeAll(Directory.GetDirectories(d), outPath),
                    "dir_with_mods", 
                    1),
                new LaunchArgument(new []{"merge-single"}, 
                    "Merges one mod into output dir and shows conflicts. Requires -o",
                    mod=>Merge.MergeSingle(mod, outPath), 
                    "mod_dir",
                    1),
                new LaunchArgument(new []{"gen-rus-locale"},
                    "Creates l_russian copy of english locale in output directory. Requires -o",
                    eng=>Localisation.GenerateRussian(eng, outPath),
                    "english_locale_path", 1)
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


    // вывод конфликтующих файлов при -merge и -clear если такие есть
    public static void LogConflicts(List<string> conflicts)
    {
        Log("w", $"found {conflicts.Count}");
        if(conflicts.Count>0)
            Log("w","conflicts:\n", "m", conflicts.MergeToString("\n"));
    }
}