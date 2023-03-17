using System;
using System.Collections.Generic;
using System.Text;
using DiffMatchPatch;
using DTLib.Filesystem;

namespace diff_text;

public static class DiffText
{
    internal static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length != 2)
        {
            Console.WriteLine("usage: [file0] [file1]");
            return;
        }

        var _diff = FileDiff(args[0], args[1]);
        PrintDiff(_diff);
    }

    public static List<Diff> FileDiff(string file0, string file1)
    {
        string fileText0 = File.ReadAllText(file0);
        string fileText1 = File.ReadAllText(file1);
        return TextDiff(fileText0, fileText1);
    }

    public static List<Diff> TextDiff(string text0, string text1)
    {
        var diff = Diff.Compute(text0, text1, checklines: true);
        diff.CleanupSemantic();
        return diff;
    }

    public static void PrintDiff(List<Diff> diff, bool ignoreWhitespaces = false)
    {
        Console.ResetColor();
        foreach (var d in diff)
        {
            bool whitespaceOnly = d.WhitespaceOnlyDiff;
            if (ignoreWhitespaces && whitespaceOnly)
                continue;
            
            string text;
            switch (d.Operation)
            {
                case Operation.Delete:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.Black;
                    text = whitespaceOnly ? d.FormattedText : d.Text;
                    break;
                case Operation.Insert:
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.Black;
                    text = whitespaceOnly ? d.FormattedText : d.Text;
                    break;
                case Operation.Equal:
                    text = d.Text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(d.Operation.ToString());
            }

            Console.Write(text);
            Console.ResetColor();
        }
    }
}