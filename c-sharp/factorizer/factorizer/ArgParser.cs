using System.Reflection.Metadata.Ecma335;
using factorizer.Models;

namespace factorizer;

public class ArgParser
{
    public FuncCode[] Flags { get; init; }
    public string[] OriginalArgs { get; init; }
    public string[] ArgsNoFlags { get; init; }
    
    public ArgParser(string[] args)
    {
        List<FuncCode> codes = [];
        List<string> listArgsNoFlags = [];
        foreach (string arg in args)
        {
            FuncCode? code = MathEnums.FromArg(arg);
            if (code is null)
            {
                listArgsNoFlags.Add(arg);
                continue;
            }

            if (codes.Contains((FuncCode)code)) throw new ArgumentException($"got {arg} twice");
            codes.Add((FuncCode)code);
        }
        if (args.Length == 0) codes.Add(FuncCode.None);

        Flags = codes.ToArray();
        OriginalArgs = args;
        ArgsNoFlags = listArgsNoFlags.ToArray();
    }
}