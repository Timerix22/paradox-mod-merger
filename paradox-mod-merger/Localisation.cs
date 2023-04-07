namespace ParadoxModMerger;

static class Localisation
{
    static ConsoleLogger logger = new("logs", "autoloc");
    static void Log(params string[] msg) => logger.Log(msg);
    
    public static void GenerateRussian(IOPath _engDir, IOPath _rusDir)
    {
        int counter = 0;
        ProcessDir(_engDir, _rusDir);
        Log("g",$"created {counter} localisation files");

        void ProcessDir(IOPath engDir, IOPath rusDir)
        {
            foreach (var fileName in Directory.GetFiles(engDir))
            {
                if (!fileName.EndsWith("l_english.yml"))
                    continue;

                IOPath rusFileName = fileName
                    .ReplaceBase(engDir, rusDir)
                    .Replace("l_english", "l_russian");

                if (File.Exists(rusFileName))
                {
                    // Log("w", $"skipped {rusFileName.RemoveBase(rusDir)}");
                    continue;
                }

                string text = File.ReadAllText(fileName)
                    .Replace("l_english:", "l_russian: ");
                byte[] bytes = StringConverter.UTF8BOM.GetBytes(text);
                File.WriteAllBytes(rusFileName, bytes);
                Log("g", $"created {rusFileName}");
                counter++;
            }

            void ProcessSubdir(string subdirEngName, string subdirRusName)
            {
                var subdirEng = Path.Concat(engDir,subdirEngName);
                var subdirRus = Path.Concat(rusDir,subdirRusName);
                if (Directory.Exists(subdirEng))
                    ProcessDir(subdirEng, subdirRus);
            }

            ProcessSubdir("english", "russian");
            ProcessSubdir("replace", "replace");
            ProcessSubdir("name_lists", "name_lists");
            ProcessSubdir("random_names", "random_names");
        }
    }

    // deletes all localisations except l_russian and l_english
    public static void Clean(IOPath _loc_dir)
    {
        int deleted_files_count=0, deleted_dirs_count=0;
        DeleteUnneededDirs(_loc_dir);
        DeleteUnneededFiles(_loc_dir);
        Log("g", $"deleted {deleted_files_count} files");
        Log("g", $"deleted {deleted_dirs_count} dirs");
        
        void DeleteUnneededDirs(IOPath loc_dir)
        {
            foreach (var subdir in Directory.GetDirectories(loc_dir))
            {
                string dir_basename = subdir.LastName().Str;
                if (dir_basename is "russian" or "english")
                {
                    // Log("w",$"skipped {subdir}");
                    continue;
                }

                if (dir_basename is "replace" or "name_lists" or "random_names")
                {
                    DeleteUnneededDirs(subdir);
                    DeleteUnneededFiles(subdir);
                    continue;
                }
                
                // incorrect dirs, for example l_russian/
                if (dir_basename.ToLower().Contains("russian") || dir_basename.ToLower().Contains("english"))
                    Log("y", $"unexpected dir: {subdir}");

                Directory.Delete(subdir);
                Log("m", $"deleted {subdir}");
                deleted_dirs_count++;
            }
        }

        void DeleteUnneededFiles(IOPath loc_dir)
        {
            foreach (var file in Directory.GetFiles(loc_dir))
            {
                if(file.EndsWith("l_russian.yml") || file.EndsWith("l_english.yml"))
                {
                    // Log("w",$"skipped {file}");
                    continue;
                }
                
                if (!file.Contains("_l_") || !file.EndsWith(".yml"))
                    Log("y",$"unexpected file: {file}");
                File.Delete(file);
                Log("m",$"deleted {file}");
                deleted_files_count++;
            }
        }
    }
}