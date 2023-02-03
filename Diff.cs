namespace ParadoxModMerger; 

static class Diff
{
    static ConsoleLogger logger = new($"logs", "diff");
    static void Log(params string[] msg) => logger.Log(msg);

    public static void DiffMods(string connectedPathes)
    {
        string[] split = connectedPathes.Split(';');
        DiffMods(split[0], split[1]);
    }
    
    public static void DiffMods(string moddir0, string moddir1)
    {
        var hasher = new Hasher();
        var diff = new Dictionary<string, byte[]>();
        // добавление файлов из первой папки
        List<string> files = Directory.GetAllFiles(moddir0);
        var mods = new List<string>();
        for (short i = 0; i < files.Count; i++)
        {
            byte[] hash = hasher.HashFile(files[i]);
            files[i] = files[i].Replace(moddir0, "");
            diff.Add(files[i], hash);
            AddMod(files[i]);
        }

        // убирание совпадающих файлов
        files = Directory.GetAllFiles(moddir1);
        for (short i = 0; i < files.Count; i++)
        {
            byte[] hash = hasher.HashFile(files[i]);
            files[i] = files[i].Replace(moddir1, "");
            if (diff.ContainsKey(files[i]) && diff[files[i]].HashToString() == hash.HashToString())
                diff.Remove(files[i]);
            else
            {
                diff.Add(moddir1 + files[i], hash);
                AddMod(files[i]);
            }
        }

        void AddMod(string mod)
        {
            mod = mod.Remove(0, 1);
            mod = mod.Remove(mod.IndexOf(Path.Sep));
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
                    if (!file.Contains(moddir1)) output.Append(moddir0).Append(file).Append('\n');
                    output.Append(file).Append('\n');
                }
            }

            output.Append("}\n");
            // не убирать, это полезное
            if (output[output.Length - 4] == '{') 
                output.Remove(output.Length - mod.Length - 5, mod.Length + 5);
        }

        var _outStr = output.ToString();
        Log("w", _outStr);
    }
}