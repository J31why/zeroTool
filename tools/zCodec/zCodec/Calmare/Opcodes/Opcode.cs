using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public abstract partial class Opcode
{
    protected byte Code { get; }
    public List<string> Param { get; } = new(20);
    public string RawText { get; }
    public int IndentLevel { get; }

    public abstract byte[] Encode(Encoding encoding);

    public Opcode(byte code,string text)
    {
        Code = code;
        RawText = text;
        IndentLevel = text.TakeWhile(x => x == '\t').Count();
        var span = text.AsSpan();
        var index = span.IndexOf('\n');
        var firstLine = index == -1 ? text : span[..span.IndexOf('\n')].ToString();
        var strs = firstLine.Split(' ');
        var hasDialog = false;
        foreach (var str in strs.Skip(1))
        {
            if (str == "{")
            {
                hasDialog = true;
                break;
            }
            if (str.StartsWith('\"') && str.EndsWith('\"'))
            {
                Param.Add(str[1..^1]);
                continue;
            }
            Param.Add(str);
        }
        if (index == -1)    //多行
            return;
        if (hasDialog && DialogReg.IsMatch(text))
        {
            var matches = DialogReg.Matches(text);
            Param.AddRange(matches.Select(x=>TrimContent(x.Value)));
        }
        else if(MenuOptionReg.IsMatch(text))
        {
            var matches = MenuOptionReg.Matches(text);
            Param.AddRange(matches.Select(x => x.Value));
        }
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)]out Opcode opcode)
    {
        if (!NameReg.IsMatch(text))
        {
            opcode = null;
            return false;
        }
        var name = NameReg.Match(text).Value.TrimStart('\t');
        switch (name)
        {
            case nameof(ED7MenuAdd):
                opcode = new ED7MenuAdd(text);
                return true;
            case nameof(Menu):
                opcode = new Menu(text);
                return true;
            case nameof(TextMessage):
                opcode = new TextMessage(text);
                return true;
            case nameof(TextSetName):
                opcode = new TextSetName(text);
                return true;
            case nameof(TextTalk):
                opcode = new TextTalk(text);
                return true;
            case nameof(TextTalkNamed):
                opcode = new TextTalkNamed(text);
                return true;
        }
        opcode = null;
        return false;
    }
    
    
    protected static byte[] EncodeCid(string text)
    {
        switch (text)
        {
            case "null":
                return [0xff, 0];
            case "self":
                return [0xfe, 0];
        }

        if (text.StartsWith("name"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value) + 0x101;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("char"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value) + 0x8;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("field_party"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value);
            return BitConverter.GetBytes(id);
        }

        throw new ArgumentException($"error cid : {text}");
    }

    protected static byte[] EncodeString(string text, Encoding encoding)
    {
        var texts = ContentSplitReg.Matches(text).Select(x => x.Value);
        var bytes = new List<byte>(0x200);
        foreach (var str in texts)
        {
            bytes.AddRange(str switch
            {
                "\n" => [1],
                "{}" => [],
                "{wait}" => [2],
                _ when str.StartsWith("{color") => [07, Convert.ToByte(NumReg.Match(str[6..]).Value)],
                _ when str.StartsWith("{item[") => 
                    [0x1F, ..BitConverter.GetBytes(Convert.ToUInt16(NumReg.Match(str[6..]).Value))],
                _ when str.StartsWith("{0x") => 
                    [Convert.ToByte(NumReg.Match(str[3..]).Value, 16)],
                _ => encoding.GetBytes(str)
            });
        }
        return bytes.ToArray();
    }

    protected string TrimContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;
        var indentLevel = IndentLevel + 1;
        var sb = new StringBuilder();
        var tabs = new string('\t', indentLevel);
        using var reader = new StringReader(content);
        while (reader.ReadLine() is { } line)
            if (line.StartsWith(tabs))
                sb.AppendLine(line[indentLevel..]);
            else if (line == "")
                sb.AppendLine("");
            else
                throw new Exception("unexpected line in calmare");
        if (sb.Length > 0 && sb[^1] == '\n')
            sb.Length--;
        return sb.ToString().Replace("\r", "");
    }


    private static Regex ContentSplitReg { get; } = ContentSplitRegex();
    protected static Regex NumReg { get; } = NumRegex();
    private static Regex DialogReg { get; } = DialogRegex();
    private static Regex MenuOptionReg { get; } = MenuOptionRegex();
    private static Regex NameReg { get; } = NameRegex();
    [GeneratedRegex("""^\t+.*?(?= |$)""",RegexOptions.Compiled)]
    private static partial Regex NameRegex();
    
    [GeneratedRegex("""(?<=").*?(?=")""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex MenuOptionRegex();
    [GeneratedRegex("""(?<={\n)[\s\S]+?(?=\n\t+})""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex DialogRegex();

    [GeneratedRegex("""\n|\{[\s\S]*?\}|[\s\S]+?(?=$|\n|{)""", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex ContentSplitRegex();

    [GeneratedRegex(@"\d+", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex NumRegex();
}