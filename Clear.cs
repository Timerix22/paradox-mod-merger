namespace ParadoxModMerger;

static class Clear
{
    
    static ConsoleLogger logger = new($"logs", "clear");
    static void Log(params string[] msg) => logger.Log(msg);
    
    public static void ClearWorkshop(string workshopDir, string outDir)
    {
        string[] moddirs = Directory.GetDirectories(workshopDir);
        Log("b", $"found {moddirs.Length} mod dirs");
        for (int i = 0; i < moddirs.Length; i++)
        {
            string modarch = "";
            if (Directory.GetFiles(moddirs[i], "*.zip").Length != 0)
                modarch = Directory.GetFiles(moddirs[i], "*.zip")[0];
            if (modarch.Length != 0)
            {
                Log("y", $"archive found: {modarch}");
                var pr = new Process();
                pr.StartInfo.CreateNoWindow = true;
                pr.StartInfo.UseShellExecute = false;
                pr.StartInfo.FileName = Path.Concat("7z", "7z.exe");
                pr.StartInfo.Arguments = $"x -y -o _UNZIP \"{modarch}\"";
                pr.Start();
                pr.WaitForExit();
                moddirs[i] = "_UNZIP";
                Log("g", "\tfiles extracted");
            }

            string modname = File.ReadAllText(Path.Concat(moddirs[i], "descriptor.mod"));
            modname = modname.Remove(0, modname.IndexOf("name=\"", StringComparison.Ordinal) + 6);
            modname = Path.CorrectString(modname.Remove(modname.IndexOf("\"", StringComparison.Ordinal)));
            Log("b", $"[{i + 1}/{moddirs.Length}] copying mod ", "c", $"{modname}");
            string[] subdirs = Directory.GetDirectories(moddirs[i]);
            for (sbyte n = 0; n < subdirs.Length; n++)
            {
                subdirs[n] = subdirs[n].Remove(0, subdirs[n].LastIndexOf(Path.Sep) + 1);
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
                    {
                        Directory.Copy(Path.Concat(moddirs[i], subdirs[n]),
                            Path.Concat(outDir, modname, subdirs[n]),
                            out List<string> _conflicts, true);
                        Program.LogConflicts(_conflicts);
                        break;
                    }
                }
            }

            if (Directory.Exists("_UNZIP")) Directory.Delete("_UNZIP");
        }
    }
}