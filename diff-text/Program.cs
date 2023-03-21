using System;
using System.Collections.Generic;
using System.Text;
using DiffMatchPatch;
using DTLib.Console;
using DTLib.Filesystem;

namespace diff_text;

public static class DiffText
{
    internal static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length < 1)
        {
            Console.WriteLine("too few arguments, use -h to show help ");
            return;
        }

        try
        {
            List<Diff>? diff = null;
            bool noColors = false;
            new LaunchArgumentParser(
                new LaunchArgument(new[] { "s", "string" },
                    "shows difference of two strings",
                    (s0, s1) => diff=TextDiff(s0, s1),
                    "string0", "string1", 1),
                new LaunchArgument(new[] { "f", "file" },
                    "shows difference of two text files",
                    (f0,f1) => diff=FileDiff(f0, f1),
                    "file0", "file1", 1),
                new LaunchArgument(new []{"p", "plain-text","no-colors"},
                    "print diff in plain text format",
                        ()=> noColors=true, 0)
            ).ParseAndHandle(args);
            if (diff == null)
                throw new Exception("no action specified: use -s or -f");
            PrintDiff(diff, false, noColors);
        }
        catch (LaunchArgumentParser.ExitAfterHelpException)
        { }
        catch (Exception ex)
        {
            ColoredConsole.WriteLine("r", $"{ex.Message}\n{ex.StackTrace}");
        }
    }

    public static List<Diff> FileDiff(string file0, string file1)
    {
        string fileText0 = File.ReadAllText(file0);
        string fileText1 = File.ReadAllText(file1);
        return TextDiff(fileText0, fileText1);
    }

    public static List<Diff> TextDiff(string text0, string text1)
    {
        List<Diff>? diff = Diff.Compute(text0, text1, checklines: false);
        if (diff is null)
            throw new NullReferenceException("diff is null");
        diff.CleanupSemantic();
        return diff;
    }

    public static void PrintDiff(List<Diff> diff, bool ignoreWhitespaces = false, bool noColors = false)
    {
        Console.ResetColor();
        
        
        if (noColors)
        {
            StringBuilder b = new();
            foreach (var patch in Patch.FromDiffs(diff))
            {
                b.Append("@@ " + patch.Coordinates + " @@\n");
                foreach (var patchDiff in patch.Diffs)
                {
                    char opChar = patchDiff.Operation switch
                    {
                        Operation.Delete => '<',
                        Operation.Insert => '>',
                        Operation.Equal => ' ',
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    b.Append(opChar).Append(' ').Append(patchDiff.FormattedText).Append('\n');
                }
            }
            Console.WriteLine(b.ToString());
            return;
        }

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