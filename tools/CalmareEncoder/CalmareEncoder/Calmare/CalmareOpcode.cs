using System.Text;
using System.Text.RegularExpressions;
using Common;

namespace CalmareEncoder.Calmare;

public abstract partial class CalmareContentFunc : CalmareFunc
{
    internal List<string> Content { get; } = new(10);
    private static readonly Regex NameReg = NameRegex();
    private static readonly Regex CharReg = CharRegex();
    private static readonly Regex FieldPartyReg = FieldPartyRegex();
    internal static byte[] CidToBytes(string cid)
    {
        switch (cid)
        {
            case "null":
                return [0xff, 0];
            case "self":
                return [0xfe, 0];
        }

        if (NameReg.IsMatch(cid))
        {
            var id = Convert.ToUInt16(NameReg.Match(cid).Groups[1].Value) + 0x101;
            var idBytes = BitConverter.GetBytes((ushort)id);
            return idBytes;
        }

        if (CharReg.IsMatch(cid))
        {
            var id = Convert.ToUInt16(CharReg.Match(cid).Groups[1].Value) + 0x8;
            var idBytes = BitConverter.GetBytes((ushort)id);
            return idBytes;
        }
        if (FieldPartyReg.IsMatch(cid))
        {
            var id = Convert.ToUInt16(FieldPartyReg.Match(cid).Groups[1].Value);
            var idBytes = BitConverter.GetBytes(id);
            return idBytes;
        }
        throw new Exception($"Name not found : {cid}");
    }

    internal static string ContentTrim(string content, int indentLevel)
    {
        indentLevel++;
        content = Regex.Replace(content.Replace("\r", ""), $"^{new string('\t', indentLevel)}", x => "",
            RegexOptions.Multiline);
        return content;
    }

    [GeneratedRegex(@"name\[(\d+)\]")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"char\[(\d+)\]")]
    private static partial Regex CharRegex();
    
    [GeneratedRegex(@"field_party\[(\d+)\]")]
    private static partial Regex FieldPartyRegex();
}

public abstract partial class CalmareFunc : ICloneable
{
    protected int IndentLevel;
    public string? RawText;
    protected abstract byte OpCode { get; }
    protected abstract string Pattern { get; }
    private readonly Lazy<Regex> _regex;
    private Regex Regex => _regex.Value;
    private static readonly Regex ColorReg = ColorRegex();
    private static readonly Regex ItemReg = ItemRegex();
    private static readonly Regex HexReg = HexRegex();

    protected CalmareFunc()
    {
        _regex = new Lazy<Regex>(() => new Regex(Pattern, RegexOptions.Multiline));
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

    public static extern bool TryParse(string text, out CalmareFunc result);
    public abstract byte[] Encode(Encoding encoding);

    internal static byte[] ToBytes(string str, Encoding encoding)
    {
        str = str
            .Replace("{}","")
            .Replace("\n", "\x01")
            .Replace("{wait}", "\x02");
        var bytes = encoding.GetBytes(str);
        var matches = ColorReg.Matches(str);
        var colors = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        colors.ForEach(x =>
        {
            byte[] replaced = [0x07, Convert.ToByte(x)];
            bytes = BitHelper.Replace(bytes, encoding.GetBytes($"{{color {x}}}"), replaced);
        });

        matches = ItemReg.Matches(str);
        var items = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        items.ForEach(x =>
        {
            byte[] replaced = [0x1f, ..BitConverter.GetBytes(Convert.ToUInt16(x))];
            bytes = BitHelper.Replace(bytes, encoding.GetBytes($"{{item[{x}]}}"), replaced);
        });

        matches = HexReg.Matches(str);
        var hex = matches.Select(x => x.Groups[1].Value).Distinct().ToList();
        hex.ForEach(x =>
        {
            byte[] replaced = [Convert.ToByte(x, 16)];
            bytes = BitHelper.Replace(bytes, encoding.GetBytes($"{{0x{x}}}"), replaced);
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

    [GeneratedRegex(@"\{color (\d+)\}")]
    private static partial Regex ColorRegex();

    [GeneratedRegex(@"\{item\[(\d+)\]\}")]
    private static partial Regex ItemRegex();

    [GeneratedRegex(@"\{0x(\d+)\}")]
    private static partial Regex HexRegex();

    public object Clone()
    {
        return MemberwiseClone();
    }
}
