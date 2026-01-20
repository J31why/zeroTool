#region

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using static zCodec.Calmare.CalmareCodec;

#endregion

namespace zCodec.Calmare.Opcodes;

public abstract partial class ScenaOpcode
{
    public static bool IsAo{ get; set; }
    protected abstract byte Code { get; }
    public List<string> Param { get; } = new(20);
    public string RawText { get; }
    public int IndentLevel { get; }

    public abstract byte[] Compile(Encoding encoding);

    public ScenaOpcode(string text)
    {
        RawText = text;
        IndentLevel = text.TakeWhile(x => x == '\t').Count();
        var span = text.AsSpan();
        var index = span.IndexOf('\n');
        var firstLine = index == -1 ? text : span[..span.IndexOf('\n')].ToString();
        var strs = firstLine.Split(' ');
        var hasDialog = false;
        for (var i = 1; i < strs.Length; i++)
        {
            var str = strs[i];
            if (str == "{")
            {
                hasDialog = true;
                break;
            }

            if (str.StartsWith('\"'))
            {
                if (str.EndsWith('\"'))
                {
                    Param.Add(str[1..^1]);
                    continue;
                }

                var temp = str[1..];
                while (i + 1 < strs.Length)
                {
                    str = strs[++i];
                    if (str.EndsWith('\"'))
                    {
                        temp += $" {str[..^1]}";
                        break;
                    }

                    temp += $" {str}";
                }

                Param.Add(temp);
                continue;
            }

            Param.Add(str);
        }

        if (index == -1) //多行
            return;
        if (hasDialog && DialogRegex().IsMatch(text))
        {
            var matches = DialogRegex().Matches(text);
            Param.AddRange(matches.Select(x => TrimContent(x.Value)));
        }
        else if (MenuOptionRegex().IsMatch(text))
        {
            var matches = MenuOptionRegex().Matches(text);
            Param.AddRange(matches.Select(x => x.Value));
        }
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out ScenaOpcode scenaOpcode)
    {
        if (!NameRegex().IsMatch(text))
        {
            scenaOpcode = null;
            return false;
        }

        var name = NameRegex().Match(text).Groups[1].Value;
        switch (name)
        {
            case nameof(ED7MenuAdd):
                scenaOpcode = new ED7MenuAdd(text);
                return true;
            case nameof(Menu):
                scenaOpcode = new Menu(text);
                return true;
            case nameof(TextMessage):
                scenaOpcode = new TextMessage(text);
                return true;
            case nameof(TextSetName):
                scenaOpcode = new TextSetName(text);
                return true;
            case nameof(TextTalk):
                scenaOpcode = new TextTalk(text);
                return true;
            case nameof(TextTalkNamed):
                scenaOpcode = new TextTalkNamed(text);
                return true;
            case nameof(ScMenuSetTitle):
                scenaOpcode = new ScMenuSetTitle(text);
                return true;
            case nameof(CharSetName):
                scenaOpcode = new CharSetName(text);
                return true;
        }

        scenaOpcode = null;
        return false;
    }


    protected static byte[] CompileCid(string text)
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
            var id = Convert.ToUInt16(NumRegex().Match(text).Value) + 0x101;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("char"))
        {
            var id = Convert.ToUInt16(NumRegex().Match(text).Value) + 0x8;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("field_party"))
        {
            var id = Convert.ToUInt16(NumRegex().Match(text).Value);
            return BitConverter.GetBytes(id);
        }

        throw new ArgumentException($"error cid : {text}");
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

    

    [GeneratedRegex("""^\t+(.*?)(?: |$)""", RegexOptions.Compiled)]
    private static partial Regex NameRegex();

    [GeneratedRegex("""(?<=").*?(?=")""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex MenuOptionRegex();

    [GeneratedRegex("""(?<={\n)[\s\S]+?(?=\n\t+})""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex DialogRegex();


}