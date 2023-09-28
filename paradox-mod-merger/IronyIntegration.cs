namespace ParadoxModMerger;

public static class IronyIntegration
{
    static ContextLogger logger =  new(nameof(IronyIntegration), new FileLogger("logs", "irony-integration"));
    static void Log(params string[] msg) => logger.LogColored(msg);

    public static void GenerateIronyCollection(string dirs_with_mods_connected, IOPath out_json_file_path)
    {
        IOPath[] dirs_with_mods = Program.SplitArgToPaths(dirs_with_mods_connected, true);
        var mod_desc_values = new List<(string name, string steam_id)>();
        foreach (var dir in dirs_with_mods)
        {
            foreach (var mod_dir in Directory.GetDirectories(dir))
            {
                IOPath descriptor_path = Path.Concat(mod_dir, "descriptor.mod");
                if (!File.Exists(descriptor_path)) 
                    Log("y", "directory ", "c", mod_dir.Str, "y", " doesn't contain descriptor");
                else
                {
                    string? name=null, remote_file_id=null;
                    var lines = System.IO.File.ReadAllLines(descriptor_path.Str);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("name="))
                        {
                            // "name=\"".Length==6
                            name = line.Substring(6, line.LastIndexOf('\"')-6);   
                        }
                        else if (line.StartsWith("remote_file_id="))
                        {
                            // "remote_file_id=\"".Length==16
                            remote_file_id = line.Substring(16, line.LastIndexOf('\"')-16);
                        }
                    }

                    if (name.IsNullOrEmpty()) 
                        throw new NullReferenceException("name=null");
                    if (remote_file_id.IsNullOrEmpty())
                        throw new NullReferenceException("remote_file_id=null");
                    mod_desc_values.Add((name,remote_file_id)!);
                    Log("b",$"[{mod_desc_values.Count-1}] {{ ", "c", name!, "b", $", {remote_file_id!} }}");
                }
            }
        }

        
        using var out_json_stream = File.OpenWrite(out_json_file_path);
        using var stream_writer = new System.IO.StreamWriter(out_json_stream, StringConverter.UTF8);
        stream_writer.WriteLine($$"""
        {
            "game":"stellaris",
            "name":"{{out_json_file_path.LastName().AsSpan().BeforeLast('.')}}",
            "mods":[
        """);
        
        for (int mod_pos = 0; mod_pos < mod_desc_values.Count; mod_pos++)
        {
            stream_writer.Write($$"""
                    {
                        "displayName":"{{mod_desc_values[mod_pos].name}}",
                        "enabled":true,
                        "position":{{mod_pos}},
                        "steamId":"{{mod_desc_values[mod_pos].steam_id}}"
                    }
            """);
            if(mod_pos<mod_desc_values.Count-1)
                stream_writer.Write(',');
            stream_writer.Write('\n');
        }

        stream_writer.WriteLine("""
            ]
        }
        """);
        stream_writer.Flush();
        stream_writer.Close();
    }
}