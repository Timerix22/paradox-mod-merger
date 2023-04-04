namespace ParadoxModMerger;

static class Localisation
{
    static ConsoleLogger logger = new($"logs", "autoloc");
    static void Log(params string[] msg) => logger.Log(msg);
    
    public static void GenerateRussian(IOPath engDir, IOPath rusDir)
    {
        foreach (var enfFileName in Directory.GetAllFiles(engDir))
        {
            IOPath rusFileName = enfFileName
                .ReplaceBase(engDir, rusDir)
                .ReplaceAnywhere("l_english", "l_russian");
            if (!File.Exists(rusFileName))
            {
                string text = File.ReadAllText(enfFileName)
                    .Replace("l_english:", "l_russian: ");
                byte[] bytes = StringConverter.UTF8BOM.GetBytes(text);
                File.WriteAllBytes(rusFileName, bytes);
                Log("g", $"file {rusFileName} created");
            }
            else Log("y", $"file {rusFileName} already exists");
        }
    }

    // deletes all localisations except l_russian and l_english
    public static void Clean(IOPath _loc_dir)
    {
        Log("g", $"deleted {RemoveUnneededDirs(_loc_dir)} dirs");
        Log("g", $"deleted {RemoveUnneededFiles(_loc_dir)} files");
        
        
        int RemoveUnneededDirs(IOPath loc_dir)
        {
            int count = 0;
            foreach (var subdir in Directory.GetDirectories(loc_dir))
            {
                string dir_basename = subdir.LastName().Str;
                if (dir_basename == "russian" || dir_basename == "english")
                    continue;

                if (dir_basename == "replace")
                {
                    RemoveUnneededDirs(subdir);
                    RemoveUnneededFiles(subdir);
                    continue;
                }

                if (dir_basename.Contains("rus"))
                    Log("y", $"unexpected dir: {subdir}");
                Directory.Delete(subdir);
                count++;
            }

            return count;
        }

        int RemoveUnneededFiles(IOPath loc_dir)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(loc_dir))
            {
                if(file.EndsWith("l_russian") || file.EndsWith("l_enghish"))
                    continue;
                if (!file.Contains("_l_") || !file.EndsWith(".yml"))
                    Log("y",$"unexpected file: {file}");
                File.Delete(file);
                count++;
            }
            return count;
        }
    }
}