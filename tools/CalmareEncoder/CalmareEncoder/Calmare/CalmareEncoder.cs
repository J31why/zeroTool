using System.Text.RegularExpressions;
using CalmareEncoder.Calmare.Common;
using Menu = CalmareEncoder.Calmare.Common.Menu;

namespace CalmareEncoder.Calmare;

public partial class CalmareEncoder
{
    public readonly List<string> NpcNames = new(20);
    public readonly List<(int index, List<CalmareFunc> func)> FnTexts = [];
    private readonly List<string> _functions = new(100);
    private static readonly Regex FnReg = FnRegex();
    private static readonly Regex FnTextReg = FnTextRegex();
    private static readonly Regex NpcNameReg = NpcNameRegex();


    private const string FnPattern = @"fn\[\d+\]:$[\s\S]*?\n$";

    private const string NpcNamePattern =
        """
        npc char[\s\S]+?name "(.*?)"
        """;

    private const string FnTextPattern =
        """
        \t+TextSetName ".{1,}"$|\t+(TextMessage|TextTalk |TextTalkNamed).*?{$[\s\S]*?\n\t+}$|\t+Menu .*?$[\s\S]*?\n[\s\S]*?(?=\n(?!\t+"))
        """;

    public void Parse(string clmText)
    {
        var matches = FnReg.Matches(clmText);
        _functions.AddRange(matches.Select(x => x.Value));
        ParseText();
        matches = NpcNameReg.Matches(clmText);
        NpcNames.AddRange(matches.Select(x => x.Groups[1].Value));
    }

    private void ParseText()
    {
        for (var index = 0; index < _functions.Count; index++)
        {
            var func = _functions[index];
            var matches = FnTextReg.Matches(func);
            if (matches.Count == 0)
                continue;
            FnTexts.Add((index, matches.Select(x =>
            {
                if (TextSetName.TryParse(x.Value, out var textSetName))
                    return textSetName;
                if (TextMessage.TryParse(x.Value, out var textMessage))
                    return textMessage;
                if (Menu.TryParse(x.Value, out var menu))
                    return menu;
                if (TextTalk.TryParse(x.Value, out var textTalk))
                    return textTalk;
                if (TextTalkNamed.TryParse(x.Value, out var textTalkNamed))
                    return textTalkNamed;
                throw new Exception($"Unknown func: {x.Value}");
            }).ToList()));
        }
    }

    [GeneratedRegex(FnPattern, RegexOptions.Multiline)]
    private static partial Regex FnRegex();

    [GeneratedRegex(FnTextPattern, RegexOptions.Multiline)]
    private static partial Regex FnTextRegex();

    [GeneratedRegex(NpcNamePattern, RegexOptions.Multiline)]
    private static partial Regex NpcNameRegex();
}