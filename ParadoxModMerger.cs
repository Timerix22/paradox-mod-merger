using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DTLib;
using DTLib.Filesystem;
using DTLib.Logging;
using DTLib.Extensions;

static class ParadoxModMerger
{
    
    // вывод лога в консоль и файл
    static ConsoleLogger logger = new ConsoleLogger("merger-logs", "merger");
    static void Log(params string[] msg) => logger.Log(msg);
    
    static void Main(string[] args)
    {
        try
        {
            PublicLog.LogEvent += Log;

            // хелп
            if (args.Length == 0 || args[0] == "/?" || args[0] == "-h")
            {
                Log(
                    "b", "paradox mod merger help:\n",
                    "c", "-clear \"steamworkshop dir\" -out \"utput dir\"   ",
                    "b", "clear mod files and put them into separate dirs in output dir\n",
                    "c", "-merge \"dir with mods\" -out \"output dir\"   ",
                    "b", "merge mods and show conflicts\n",
                    "c", "-merge-single \"dir with mod\" -out \"output dir\"   ",
                    "b", "copy single mod files and show conflicts\n",
                    "c", "-diff \"dir with mods\" \"another dir with mods\" -out \"output file\"   ",
                    "b", "compare mod files hashes for finding different files");
                Console.ResetColor();
                return;
            }

            string srcdir = "";
            string srcdir2 = "";
            string outdir = "";
            int mode = -1;
            // определение всех аргументов
            for (sbyte i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-clear":
                        srcdir = args[++i].Replace("\"", "").ИсправитьРазд();
                        mode = 0;
                        break;
                    case "-merge":
                        srcdir = args[++i].Replace("\"", "").ИсправитьРазд();
                        mode = 1;
                        break;
                    case "-diff":
                        srcdir = args[++i].Replace("\"", "").ИсправитьРазд();
                        srcdir2 = args[++i].Replace("\"", "").ИсправитьРазд();
                        mode = 2;
                        break;
                    case "-merge-single":
                        srcdir = args[++i].Replace("\"", "").ИсправитьРазд();
                        mode = 3;
                        break;
                    case "-out":
                        outdir = args[++i].Replace("\"", "").ИсправитьРазд();
                        break;
                    default:
                        throw new Exception($"invalid argument: <{args[i]}>");
                }
            }

            var conflicts = new List<string>();
            string[] moddirs = Directory.GetDirectories(srcdir);
            switch (mode)
            {
                case 0:
                    Log("b", $"found {moddirs.Length} mod dirs");
                    for (int i = 0; i < moddirs.Length; i++)
                    {
                        string modarch = "";
                        if (Directory.GetFiles(moddirs[i], "*.zip").Length != 0) modarch = Directory.GetFiles(moddirs[i], "*.zip")[0];
                        if (modarch.Length != 0)
                        {
                            Log("y", $"archive found: {modarch}");
                            var pr = new Process();
                            pr.StartInfo.CreateNoWindow = true;
                            pr.StartInfo.UseShellExecute = false;
                            pr.StartInfo.FileName = "7z{Путь.Разд}7z.exe";
                            pr.StartInfo.Arguments = $"x -y -o_TEMP \"{modarch}\"";
                            pr.Start();
                            pr.WaitForExit();
                            moddirs[i] = "_TEMP";
                            Log("g", "\tfiles extracted");
                        }
                        string modname = File.ReadAllText($"{moddirs[i]}{Путь.Разд}descriptor.mod");
                        modname = modname.Remove(0, modname.IndexOf("name=\"") + 6);
                        modname = modname.Remove(modname.IndexOf("\""))
                            .Replace($"{Путь.Разд}", "").Replace(":", "").Replace("?", "").Replace("\"", "").Replace("/", "")
                            .Replace("\'", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace("*", "");
                        Log("b", $"[{i + 1}/{moddirs.Length}] copying mod ", "c", $"{modname}");
                        string[] subdirs = Directory.GetDirectories(moddirs[i]);
                        for (sbyte n = 0; n < subdirs.Length; n++)
                        {
                            subdirs[n] = subdirs[n].Remove(0, subdirs[n].LastIndexOf(Путь.Разд) + 1);
                            switch (subdirs[n])
                            {
                                // stellaris
                                case "common":
                                case "events":
                                case "flags":
                                case "fonts":
                                case "gfx":
                                case "interface":
                                case "localisation":
                                case "localisation_synced":
                                case "map":
                                case "music":
                                case "prescripted_countries":
                                case "sound":
                                // hoi4
                                case "history":
                                case "portraits":
                                    Directory.Copy($"{moddirs[i]}{Путь.Разд}{{subdirs[n]}}",
                                        $"{outdir}{Путь.Разд}{{modname}}{Путь.Разд}{{subdirs[n]}}", out List<string> _conflicts, true);
                                    conflicts.AddRange(_conflicts);
                                    break;
                            }
                        }
                        if (Directory.Exists("_TEMP")) Directory.Delete("_TEMP");
                    }
                    break;
                case 1:
                    Log("b", $"found {moddirs.Length} mod dirs");
                    for (short i = 0; i < moddirs.Length; i++)
                    {
                        Log("b", $"[{i + 1}/{moddirs.Length}] merging mod ", "c", $"{moddirs[i]}");
                        Directory.Copy(moddirs[i], outdir, out List<string> _conflicts, true);
                        conflicts.AddRange(_conflicts);
                    }
                    break;
                case 2:
                    var hasher = new Hasher();
                    var diff = new Dictionary<string, byte[]>();
                    // добавление файлов из первой папки
                    List<string> files = Directory.GetAllFiles(srcdir);
                    var mods = new List<string>();
                    for (short i = 0; i < files.Count; i++)
                    {
                        byte[] hash = hasher.HashFile(files[i]);
                        files[i] = files[i].Replace(srcdir, "");
                        diff.Add(files[i], hash);
                        AddMod(files[i]);
                    }
                    // убирание совпадающих файлов
                    files = Directory.GetAllFiles(srcdir2);
                    for (short i = 0; i < files.Count; i++)
                    {
                        byte[] hash = hasher.HashFile(files[i]);
                        files[i] = files[i].Replace(srcdir2, "");
                        if (diff.ContainsKey(files[i]) && diff[files[i]].HashToString() == hash.HashToString()) diff.Remove(files[i]);
                        else
                        {
                            diff.Add(srcdir2 + files[i], hash);
                            AddMod(files[i]);
                        }
                    }
                    void AddMod(string mod)
                    {
                        mod = mod.Remove(0, 1);
                        mod = mod.Remove(mod.IndexOf(Путь.Разд));
                        if (!mods.Contains(mod)) mods.Add(mod);
                    }
                    // вывод результата
                    StringBuilder output = new StringBuilder();
                    output.Append($"[{DateTime.Now}]\n\n");
                    foreach (string mod in mods)
                    {
                        output.Append('\n').Append(mod).Append("\n{\n");
                        foreach (string file in diff.Keys)
                        {
                            if (file.Contains(mod))
                            {
                                output.Append('\t');
                                if (!file.Contains(srcdir2)) output.Append(srcdir).Append(file).Append('\n');
                                output.Append(file).Append('\n');
                            }
                        }
                        output.Append("}\n");
                        // не убирать, это полезное
                        if (output[output.Length - 4] == '{') output.Remove(output.Length - mod.Length - 5, mod.Length + 5);
                    }
                    // хоть называется outdir, в данном случае это путь к файлу
                    var _outStr = output.ToString();
                    File.WriteAllText(outdir, _outStr);
                    Log("g", $"output written to {outdir}");
                    break;
                case 3:
                    Directory.Copy(srcdir, outdir, out List<string> __conflicts, true);
                    conflicts.AddRange(__conflicts);
                    break;
            }
            // вывод конфликтующих файлов при -merge и -clear если такие есть
            if (conflicts.Count > 0)
            {
                Log("r", $"found {conflicts.Count} conflicts:\n",
                    "m", conflicts.MergeToString("\n"));
            }
        }
        catch (Exception ex)
        {
            Log("r", $"{ex.Message}\n{ex.StackTrace}");
        }
        Console.ResetColor();
    }
}