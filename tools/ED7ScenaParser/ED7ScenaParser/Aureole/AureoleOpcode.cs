using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using OpenCCNET;

namespace ED7ScenaParser.Aureole;

public abstract class AureoleContentFunc : AureoleFunc
{
    internal List<string> Content { get; set; } = new(10);

    internal static string ContentTrim(string content, int indentLevel)
    {
        indentLevel++;
        content = Regex.Replace(content.Replace("\r", ""), $"^{new string('\t', indentLevel)}", x => "", RegexOptions.Multiline);
        return content;
    }
}

public abstract partial class AureoleFunc : ICloneable
{
    protected int IndentLevel;
    public string? RawText;
    protected abstract byte OpCode { get; }
    protected abstract string Pattern { get; }
    private readonly Lazy<Regex> _regex;
    private Regex Regex => _regex.Value;

    protected AureoleFunc()
    {
        _regex = new Lazy<Regex>(() => new Regex(Pattern, RegexOptions.Compiled | RegexOptions.Multiline));
    }

    protected bool IsMatch(string input)
    {
        return Regex.IsMatch(input);
    }

    protected Match Match(string input)
    {
        return Regex.Match(input);
    }

    protected MatchCollection Matches(string input)
    {
        return Regex.Matches(input);
    }

    public static extern bool TryParse(string text, out AureoleFunc result);
    public abstract byte[] Encode(Encoding encoding);

    internal static byte[] ToBytes(string str, Encoding encoding)
    {
        str = ZhConverter.TWToHans(str,true);
        str = str
            .Replace("\n", "\x01")
            .Replace("{wait}", "\x02");
        var bytes = encoding.GetBytes(str);
        var matches = ColorRegex().Matches(str);
        var colors = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        colors.ForEach(x =>
        {
            byte[] replaced = [0x07, Convert.ToByte(x)];
            bytes = Bit.Replace(bytes, encoding.GetBytes($"{{color {x}}}"), replaced);
        });

        matches = ItemRegex().Matches(str);
        var items = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        items.ForEach(x =>
        {
            byte[] replaced = [0x1f, ..BitConverter.GetBytes(Convert.ToUInt16(x))];
            bytes = Bit.Replace(bytes, encoding.GetBytes($"{{item[{x}]}}"), replaced);
        });

        matches = HexRegex().Matches(str);
        var hex = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        hex.ForEach(x =>
        {
            byte[] replaced = [Convert.ToByte(x,16)];
            bytes = Bit.Replace(bytes, encoding.GetBytes($"{{0x{x}}}"), replaced);
        });

        return bytes;
    }

    internal static int GetIndentLevel(string line)
    {
        var level = 0;
        foreach (var t in line)
            if (t == '\t')
                level++;
            else break;
        return level;
    }

    [GeneratedRegex("\\{color (\\d+)\\}")]
    private static partial Regex ColorRegex();

    [GeneratedRegex("\\{item\\[(\\d+)\\]\\}")]
    private static partial Regex ItemRegex();

    [GeneratedRegex("\\{0x(\\d+)\\}")]
    private static partial Regex HexRegex();

    public object Clone()
    {
        return MemberwiseClone();
    }
}