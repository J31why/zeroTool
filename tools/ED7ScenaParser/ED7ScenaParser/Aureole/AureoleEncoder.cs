using System.Text.RegularExpressions;

namespace ED7ScenaParser.Aureole;

public partial class AureoleEncoder
{
    public readonly List<(int index, List<AureoleFunc> func)> FnTexts = [];

    private readonly List<string> _functions = new(100);
    private const string FnPattern = "fn\\[\\d+\\]:$[\\s\\S]*?\\n$";

    private const string FnTextPattern =
        """
        \t+TextSetName ".{1,}"$|\t+(TextMessage|TextTalk |TextTalkNamed).*?{$[\s\S]*?\n\t+}$|\t+Menu .*?$[\s\S]*?\n\t+MenuWait
        """;

    public void Parse(string clmText)
    {
        var matches = FnRegex().Matches(clmText);
        _functions.AddRange(matches.Select(x => x.Value));
        ParseText();
    }

    private void ParseText()
    {
        for (var index = 0; index < _functions.Count; index++)
        {
            var func = _functions[index];
            var matches = FnTextRegex().Matches(func);
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
                throw new Exception($"Unknown func: {x.Value}");
            }).ToList()));
        }
    }

    [GeneratedRegex(FnPattern, RegexOptions.Multiline)]
    private static partial Regex FnRegex();

    [GeneratedRegex(FnTextPattern, RegexOptions.Multiline)]
    private static partial Regex FnTextRegex();
}