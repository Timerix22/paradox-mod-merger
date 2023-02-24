using System;
using System.Collections.Generic;
using System.Text;
using DiffMatchPatch;
using DTLib.Filesystem;

Console.InputEncoding=Encoding.UTF8;
Console.OutputEncoding=Encoding.UTF8;

if (args.Length != 2)
{
    Console.WriteLine("usage: [file0] [file1]");
    return;
}

var _diff=FileDiff(args[0], args[1]);
PrintDiff(_diff);


List<Diff> FileDiff(string file0, string file1)
{
    string fileText0 = File.ReadAllText(file0);
    string fileText1 = File.ReadAllText(file1);
    return TextDiff(fileText0, fileText1);
}

List<Diff> TextDiff(string text0, string text1)
{
    var diff = Diff.Compute(text0, text1, checklines:true);
    diff.CleanupSemantic();
    return diff;
}

void PrintDiff(List<Diff> diff, bool ignoreWhitespaces=false)
{
    foreach (var d in diff)
    {
        bool whitespaceOnly = d.WhitespaceOnlyDiff;
        if(ignoreWhitespaces && whitespaceOnly)
            continue;
        
        switch(d.Operation)
        {
            case Operation.Delete:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(whitespaceOnly ? d.FormattedText : d.Text);
                Console.ResetColor();
                break;
            case Operation.Insert:
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(whitespaceOnly ? d.FormattedText : d.Text);
                Console.ResetColor();
                break;
            case Operation.Equal:
                Console.Write(d.Text);
                break;
            default:
                throw new ArgumentOutOfRangeException(d.Operation.ToString());
        }
    }
}