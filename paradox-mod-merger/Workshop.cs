using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DTLib.Ben.Demystifier;
using HtmlAgilityPack;

namespace ParadoxModMerger;
using Fizzler.Systems.HtmlAgilityPack;

static class Workshop
{
    
    static ConsoleLogger logger = new($"logs", "clear");
    static void Log(params string[] msg) => logger.Log(msg);
    
    public static void ClearWorkshop(IOPath workshopDir, IOPath outDir)
    {
        var moddirs = Directory.GetDirectories(workshopDir);
        Log("b", $"found {moddirs.Length} mod dirs");
        for (int i = 0; i < moddirs.Length; i++)
        {
            string modId = moddirs[i].LastName().ToString();
            IOPath modZip="";
            if (Directory.GetFiles(moddirs[i], "*.zip").Length != 0)
                modZip = Directory.GetFiles(moddirs[i], "*.zip")[0];
            if (modZip.Length != 0)
            {
                Log("y", $"archive found: {modZip}");
                if (Directory.Exists("_UNZIP")) Directory.Delete("_UNZIP");
                var pr = new Process();
                pr.StartInfo.CreateNoWindow = true;
                pr.StartInfo.UseShellExecute = false;
                pr.StartInfo.FileName = "7z";
                pr.StartInfo.Arguments = $"x -y -o_UNZIP \"{modZip}\"";
                Log("h",$"{pr.StartInfo.WorkingDirectory}$: {pr.StartInfo.FileName} {pr.StartInfo.Arguments}");
                pr.Start();
                pr.WaitForExit();
                moddirs[i] = "_UNZIP";
                Log("g", "\tfiles extracted");
            }

            var descriptorPath = Path.Concat(moddirs[i], "descriptor.mod");
            string descriptor = File.ReadAllText(descriptorPath);
            string modname = descriptor.Substring(descriptor.IndexOf("name=\"", StringComparison.Ordinal) + 6);
            modname = modname.Remove(modname.IndexOf("\"", StringComparison.Ordinal));
            Log("b", $"[{i + 1}/{moddirs.Length}] copying mod ", "c", $"({modId}) {modname}");
            
            IOPath outModDir=Path.Concat(outDir, Path.ReplaceRestrictedChars(modname));
            File.Copy(descriptorPath, Path.Concat(outModDir, "descriptor.mod"), true);
            
            CreateDescFile(modId, outModDir);
            
            
            
            var subdirs = Directory.GetDirectories(moddirs[i]);
            for (sbyte n = 0; n < subdirs.Length; n++)
            {
                subdirs[n] = subdirs[n].Remove(0, subdirs[n].LastIndexOf(Path.Sep) + 1);
                switch (subdirs[n].Str)
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
                            Path.Concat(outModDir, subdirs[n]),
                             true, out var _conflicts);
                        Log("y", $"found {_conflicts.Count} conflicts:\n{_conflicts.MergeToString('\n')}");
                        break;
                    }
                }
            }

            if (Directory.Exists("_UNZIP")) Directory.Delete("_UNZIP");
        }
    }

    private static HttpClient http = new HttpClient();

    public static async void CreateDescFile(string workshopId, IOPath outDir)
    {
        try
        {
            string desc = await DownloadModDescription(workshopId);
            var file = Path.Concat(outDir, $"desc_{workshopId}.txt");
            File.WriteAllText(file, desc);
            Log("g", $"downloaded {workshopId} description to {file}");
        }
        catch (Exception e)
        {
            Log("r", $"mod {workshopId} error: \n"+ e.ToStringDemystified());
        }
    }
    public static async Task<string> DownloadModDescription(string workshopId)
    {
        string url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + workshopId;
        var b = new StringBuilder(url);
        b.Append("\n\n");
        string pageText = await http.GetStringAsync(url);
        var page = new HtmlDocument();
        page.LoadHtml(pageText);
        var descNode=page.DocumentNode.QuerySelectorAll(".workshopItemDescription").FirstOrDefault();
        if (descNode is null)
            Log("y", $"no description found for mod {workshopId}");
        else processNodes(descNode.ChildNodes);

        return b.ToString().Replace("&quot;","\"");
        
        void processNodes(IEnumerable<HtmlNode> nodes)
        {
            foreach (var node in nodes)
            {
                b.Append(
                    node.Name switch
                    {
                        "br" => '\n',
                        "a" => $"{node.InnerText} ({node.GetAttributeValue("href", "NULL_ATTRIBUTE")}) ",
                        _ => node.InnerText
                    });
            }
        }
    }
}