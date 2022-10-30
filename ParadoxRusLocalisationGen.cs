﻿using System;
using DTLib;
using DTLib.Extensions;
using DTLib.Filesystem;
using DTLib.Logging;

static class ParadoxRusLocalisationGen
{
    static ConsoleLogger logger = new ConsoleLogger("autoloc-logs", "merger");
    
    static void Main(string[] args)
    {
        if (args.Length != 2 || args[0] == "/?" || args[0] == "help" || args[0] == "--help")
        {
            Console.WriteLine("[dir with eng localisation] [dir with rus localisation]");
            return;
        }

        string engDir = args[0];
        string rusir = args[1];
        foreach (string enfFileName in Directory.GetAllFiles(engDir))
        {
            string rusFileName = enfFileName
                .Replace(engDir, rusir)
                .Replace("l_english", "l_russian");
            if(!File.Exists(rusFileName))
            {
                string text = File.ReadAllText(enfFileName)
                    .Replace("l_english:", "l_russian: ");
                byte[] bytes = StringConverter.UTF8BOM.GetBytes(text);
                File.WriteAllBytes(rusFileName, bytes);
                ColoredConsole.Write("g",$"file {rusFileName} created\n");
            }
            else ColoredConsole.Write("y", $"file {rusFileName} already exists\n");
        }
    }
}