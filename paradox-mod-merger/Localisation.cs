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
}