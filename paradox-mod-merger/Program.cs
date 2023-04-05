global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Text;
global using DTLib;
global using DTLib.Extensions;
global using DTLib.Filesystem;
global using DTLib.Logging;
using System.Linq;
using System.Security.Cryptography;
using DTLib.Console;

namespace ParadoxModMerger;

public static class Program
{
    static ConsoleLogger logger = new($"logs", "main");
    static void Log(params string[] msg) => logger.Log(msg);

    public static bool YesAll = false;
    
    static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding=Encoding.UTF8;
            Console.InputEncoding=Encoding.UTF8;
            
            string outPath = "" ;
            
            new LaunchArgumentParser(
            new LaunchArgument(new []{"o", "out"}, 
            "Sets output path", 
            p => outPath=p,
            "out_path",
            0),
            new LaunchArgument(new []{"y", "yes-all"}, 
            "Automatically answers [Y] to all questions", 
            ()=> YesAll=true,
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
            new LaunchArgument(new []{"diff-detailed"},
            "reads conflicts_XXX.dtsod file and shows text diff for each file",
            p=>Diff.DiffDetailedCommandHandler(p),
            "conflicts_dtsod_path", 
            1),
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
            id=>Workshop.CreateDescFile(id, outPath).GetAwaiter().GetResult(), 
            "mod_id",
            1),
            new LaunchArgument(new []{"rename"}, 
            "Renames mods in directory",
            (modsdir, replace_pairs)=>Merge.RenameModsCommandHandler(modsdir, replace_pairs),
            "dir_with_mods", "replace_pairs (old_name:new_name:...)",
            1),
            new LaunchArgument(new []{"update-mods"},
            "Updates mods in [outdated_dir0...outdated_dirN]  to new versions if found in updated_mods_dir. " +
            "Moves old mods to backup_dir defined by -o.",
            (updated, outdated)=>Merge.UpdateMods(updated, SplitArgToPaths(outdated, true), outPath),
            "updated_mods_dir", "outdated_dir OR outdated_dir0:...:outdated_dirN",
            1),
            new LaunchArgument(new []{"clean-locales"},
            "Deletes all localisations except l_russian and l_english.",
            locdir=> Localisation.Clean(locdir),
            "localisation_dir", 
            1)
            ).ParseAndHandle(args);
        }
        catch (LaunchArgumentParser.ExitAfterHelpException)
        { }
        catch (Exception ex)
        {
            Log("r", DTLib.Ben.Demystifier.ExceptionExtensions.ToStringDemystified(ex));
            Console.ResetColor();
            return 1;
        }

        return 0;
    }

    public static string[] SplitArgToStrings(string connected_parts, bool allow_one_part)
    {
        char part_sep;
        if (connected_parts.Contains(':'))
            part_sep = ':';
        else if (connected_parts.Contains(';'))
            part_sep = ';';
        else if (allow_one_part)
            return new []{connected_parts};
        else throw new Exception($"<{connected_parts}> doesn't contain any separators (:/;)");

        return connected_parts.Split(part_sep);
    }

    public static IOPath[] SplitArgToPaths(string connected_parts, bool allow_one_part) => 
        IOPath.ArrayCast(SplitArgToStrings(connected_parts, allow_one_part));
}