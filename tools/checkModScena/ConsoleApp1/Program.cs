using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1;

internal partial class Program
{
    [GeneratedRegex("""(?<= {\n)[\s\S]+?(?=\n\t+})""", RegexOptions.Multiline)]
    private static partial Regex dialogRegex();
    [GeneratedRegex("""#\d+[VIMF]""", RegexOptions.Multiline)]
    private static partial Regex tagRegex();

    private static Regex dialogReg = dialogRegex();
    private static Regex tagReg = tagRegex();
    private static string currentFile = "";

    static void Main(string[] args)
    {
        MatchMorePortrait_Scena_zero();
        return;
       

    }
    private static void Match_scena()
    {
        var replaceReg = new Regex(
            """
            (name ").*?(?=")|(?<=ED7MenuAdd menu\[\d\] ").*?(?=")|(?<=\t").*(?=" \/\/ \d)|(?<= {\n)[\s\S]+?(?=\n\t+})|(?<=TextSetName ").*?(?=")|(?<=ScMenuSetTitle \d+ \d+ \d+ ").*?(?=")|(?<=TextTalkNamed .*?").*?(?=")|(?<=CharSetName .*? ").*?(?=")
            """);

        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\zero\scena\cn_2_校对原版";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\zero\scena\jp";
        var count = 0;
        var cnFiles = Directory.GetFiles(cnDir);
        var kanaReg = new Regex("[\u3040-\u309F\u30A0-\u30FA\u30FC-\u30FF]");
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            var jpFile = Path.Combine(jpDir, fileName);
            var cnFileContent = File.ReadAllText(cnFile);
            if (kanaReg.IsMatch(cnFileContent))
            {
                Console.WriteLine($"日文: {fileName}");
            }
            var jpFileContent = File.ReadAllText(jpFile);
            var cnContent = replaceReg.Replace(cnFileContent.Replace("\r",""), "");
            var jpContent = replaceReg.Replace(jpFileContent, "");
        
            if(cnContent.GetHashCode()!= jpContent.GetHashCode())
            {
                Console.WriteLine($"有差异: {fileName}");
                count++;
            }
        }
        Console.WriteLine(count);

    }
    private static void TagMatch_Dat()
    {
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\text\cn";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\text\jp";
        var cnFiles = Directory.GetFiles(cnDir);
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            var jpFile = Path.Combine(jpDir, fileName);
            if (!File.Exists(jpFile))
            {
                Console.WriteLine($"JP file not found: {fileName}");
                continue;
            }
            var cnContent = File.ReadAllText(cnFile);
            var jpContent = File.ReadAllText(jpFile);
            var cnMatches = tagReg.Matches(cnContent);
            var jpMatches = tagReg.Matches(jpContent);
            if (cnMatches.Count != jpMatches.Count)
            {
                Console.WriteLine($"Mismatch in number of tags for file: {fileName}");
                continue;
            }
        }
    }

    private static void MatchMorePortrait_Scena_zero()
    {
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\zero\scena\jp_更多头像";
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\zero\scena\cn_2_校对原版";
        var outDir = @"F:\源码\C#\ed7zeroTool\trans\zero\scena\cn_3_校对更多头像";
        var jpFiles = Directory.GetFiles(jpDir);

        foreach (var jpFile in jpFiles)
        {
            var fileName = Path.GetFileName(jpFile);
            currentFile = fileName;
            var cnFile = Path.Combine(cnDir, fileName);
            if (!File.Exists(cnFile))
                continue;
            var jp_Content = File.ReadAllText(jpFile).Replace("\r", "");
            var cn_Content = File.ReadAllText(cnFile).Replace("\r", "");
            var jp_texts = dialogReg.Matches(jp_Content).Select(x => x.Value).ToList();
            var cn_texts = dialogReg.Matches(cn_Content).Select(x => x.Value).ToList();
            if (fileName == "c1030.clm")
            {
                cn_texts.Insert(868, "\t\t\t#11P對了，這個就\n\t\t\t作為對各位的謝禮吧。{wait}");
                cn_texts.Insert(869, "\t\t\t#11P謝謝你們，\n\t\t\t這樣一來，晚上就沒問題了！{wait}");
                cn_texts.Insert(872, "\t\t\t#11P對了，這個就\n\t\t\t作為對各位的謝禮吧。{wait}");
                cn_texts.Insert(873, "\t\t\t#11P謝謝你們，\n\t\t\t這樣一來，晚上就沒問題了！{wait}");
            }
            else if (fileName == "c1440.clm")
            {
                cn_texts.Insert(168, "\t\t\t你是軍人吧？{wait}");
            }

                if (jp_texts.Count != cn_texts.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mismatch in number of matches for file: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                continue;
            }
            for (int i = 0; i < jp_texts.Count; i++)
            {
                var jp_text = jp_texts[i];
                var cn_text = cn_texts[i];
                var fixedText = fixText(jp_text, cn_text);
                jp_Content = ReplaceFirst(jp_Content, jp_text, fixedText);
            }
            jp_Content = replaceOther_zero(cn_Content, jp_Content);
            File.WriteAllText(Path.Combine(outDir, fileName), jp_Content.Replace("\r", ""));
        }
    }


    private static void MatchMorePortrait_Scena_ao()
    {
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\jp_更多头像";
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\cn_2_校对原版";
        var outDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\cn_3_校对更多头像";
        var jpFiles = Directory.GetFiles(jpDir);
       
        foreach (var jpFile in jpFiles)
        {
            var fileName = Path.GetFileName(jpFile);
            currentFile = fileName;
            var cnFile = Path.Combine(cnDir, fileName);
            if (!File.Exists(cnFile))
            {
                continue;
            }
            var jp_Content = File.ReadAllText(jpFile).Replace("\r", "");
            var cn_Content = File.ReadAllText(cnFile).Replace("\r", "");
            var jp_texts = dialogReg.Matches(jp_Content).Select(x => x.Value).ToList();
            var cn_texts = dialogReg.Matches(cn_Content).Select(x => x.Value).ToList();
            if(fileName == "c0210.clm")
            {
                cn_texts.Insert(0x218, "\t\t\t啊，對喔，\n\t\t\t確實有這麼一回事。{wait}");
                cn_texts.Insert(0x219, "\t\t\t我想想喔，說到本店的麵包，\n\t\t\t每一種都值得推薦，不過……{wait}");
            }
            else if (fileName == "c1120.clm")
            {
                cn_texts.Insert(0xf5, "\t\t\t這是選美大會！\n\t\t\t男人不能參加啦！！{wait}");
                cn_texts.Insert(0xf6, "\t\t\t就這樣，\n\t\t\t快點選別人出來！！{wait}");
                cn_texts.Insert(0x13b, "\t\t\t那麼，終於到了最後一位！\n\t\t\t讓我們歡迎七號參賽者！{wait}");
                cn_texts.Insert(0x13e, "\t\t\t那麼，終於到了最後一位！\n\t\t\t讓我們歡迎七號參賽者！{wait}");
            }
            else if (fileName == "t2520.clm")
            {
                cn_texts.Insert(0x154, "\t\t\t#11P雖然我也想這麼做……\n\t\t\t不過就像我剛才說的，\n\t\t\t警備隊目前是高度戒備狀態。{wait}");
                cn_texts.Insert(0x155, "\t\t\t#11P被認為是克洛斯貝爾市襲擊犯的\n\t\t\t《紅色星座》也消失無蹤，\n\t\t\t連一點線索都沒有。{wait}");
                cn_texts.Insert(0x156, "\t\t\t#11P再加上，帝國跟共和國\n\t\t\t打算開始進行大規模演習的\n\t\t\t這個狀況……{wait}");
            }

            if (jp_texts.Count != cn_texts.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mismatch in number of matches for file: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                continue;
            }
            for (int i = 0; i < jp_texts.Count; i++)
            {
                var jp_text = jp_texts[i];
                var cn_text = cn_texts[i];
                var fixedText =fixText(jp_text, cn_text);
                jp_Content = ReplaceFirst(jp_Content, jp_text, fixedText);
            }
            jp_Content = replaceOther_ao(cn_Content, jp_Content);
            File.WriteAllText(Path.Combine(outDir,fileName),jp_Content.Replace("\r",""));
          
        }
    }
    private static string replaceOther_zero(string cn_Content, string jp_Content)
    {
        //name 
        var reg = NameRegex();
        var cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        var jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //menu
        reg = MenuRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //TextSetName
        reg = TextSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        switch (currentFile)
        {
            case "c0140.clm":
                cn_texts.Add("TextSetName \"溫蒂\"");
                cn_texts.Add("TextSetName \"溫蒂\"");
                cn_texts.Add("TextSetName \"溫蒂\"");
                break;
            case "c0210.clm":
                cn_texts.Add("TextSetName \"奥斯卡\"");
                cn_texts.Add("TextSetName \"奥斯卡\"");
                cn_texts.Add("TextSetName \"奥斯卡\"");
                cn_texts.Add("TextSetName \"貝奈特\"");
                break;
            case "c0240.clm":
                cn_texts.Add("TextSetName \"少年\"");
                break;
            case "c1010.clm":
                cn_texts.Insert(0, "TextSetName \"遊擊士斯克特\"");
                cn_texts.Insert(1, "TextSetName \"遊擊士溫蔡爾\"");
                cn_texts.Insert(2, "TextSetName \"遊擊士林\"");
                cn_texts.Insert(3, "TextSetName \"遊擊士艾歐莉雅\"");
                cn_texts.Insert(4, "TextSetName \"接待員蜜雪兒\"");
                cn_texts.Insert(5, "TextSetName \"接待員蜜雪兒\"");
                cn_texts.Add("TextSetName \"接待員蜜雪兒\"");
                break;
            case "c1150.clm":
                cn_texts.Insert(0, "TextSetName \"皮埃爾副局長\"");
                break;
            case "c1160.clm":
                cn_texts.Insert(0, "TextSetName \"皮埃爾副局長\"");
                cn_texts.Insert(2, "TextSetName \"皮埃爾副局長\"");
                break;
            case "c1410.clm":
                cn_texts.Insert(0, "TextSetName \"高大的光頭男子\"");
                cn_texts.Insert(1, "TextSetName \"阿巴斯\"");
                break;
            case "t2020.clm":
                cn_texts.Add("TextSetName \"米蕾優准尉\"");
                break;
        }
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //ED7MenuAdd
        reg = ED7MenuAddRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //ScMenuSetTitle
        reg = ScMenuSetTitleRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //CharSetName
        reg = CharSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //TextTalkNamed
        reg = TextTalkNamedRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();

        switch (currentFile)
        {
            case "c020c.clm":
                cn_texts.RemoveAt(cn_texts.Count - 1);
                cn_texts.RemoveAt(3);
                break;
            case "c0240.clm":
                cn_texts.RemoveAt(0);
                cn_texts.RemoveAt(0);
                for (int i = 0; i < 11; i++)
                    cn_texts.RemoveAt(cn_texts.Count - 1);
                break;
            case "c1400.clm":
                cn_texts.RemoveAt(19);
                break;
            case "c1410.clm":
                cn_texts.RemoveAt(19);
                cn_texts.RemoveAt(19);
                break;
        }

        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        return jp_Content;
    }
    private static string replaceOther_ao(string cn_Content, string jp_Content)
    {
        //name 
        var reg = NameRegex();
        var cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        var jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //menu
        reg = MenuRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //TextSetName
        reg = TextSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        switch (currentFile)
        {
            case "c0140.clm":
                cn_texts.Add("TextSetName \"溫蒂\"");
                break;
            case "c0400.clm":
                cn_texts.Add("TextSetName \"凱特巡警\"");
                break;
            case "c1010.clm":
                cn_texts.Insert(0, "TextSetName \"遊擊士林\"");
                cn_texts.Insert(1, "TextSetName \"遊擊士艾歐莉雅\"");
                cn_texts.Insert(2, "TextSetName \"遊擊士斯克特\"");
                cn_texts.Insert(3, "TextSetName \"遊擊士溫澤爾\"");
                break;
            case "c1030.clm":
                cn_texts.Add("TextSetName \"珊珊\"");
                break;
            case "c1440.clm":
                cn_texts.Add("TextSetName \"艾胥莉\"");
                break;
            case "t1650.clm":
                cn_texts.Add("TextSetName \"女醫生\"");
                break;
        }
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //ED7MenuAdd
        reg = ED7MenuAddRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //ScMenuSetTitle
        reg = ScMenuSetTitleRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //CharSetName
        reg = CharSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //TextTalkNamed
        reg = TextTalkNamedRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();

        switch (currentFile)
        {
            case "m1060.clm":
            case "m1140.clm":
                cn_texts.RemoveAt(cn_texts.Count - 1);
                break;
            case "t1650.clm":
                cn_texts.RemoveAt(cn_texts.Count - 1);
                cn_texts.RemoveAt(cn_texts.Count - 1);
                break;
        }

        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        return jp_Content;
    }

    static string fixText(string jp, string cn)
    {
        var cnTag = GetFaceTag(cn);
        var jpTag = GetFaceTag(jp);
        var cn_indentCount = GetIndentCount(cn);
        var jp_indentCount = GetIndentCount(jp);
        if (cn_indentCount == jp_indentCount && cnTag.Equals(jpTag))
            return cn;
     
        var indent = new string('\t', jp_indentCount);
        var strs = cn.Split('\n').Select(x => x[cn_indentCount..]).ToList();
        var firstLine = "";
        if (cnTag != "" && !jpTag.Equals(cnTag))
        {
            firstLine = $"{indent}{strs[0].Replace(cnTag, jpTag)}\n";
        }
        else
        {
            firstLine = $"{indent}{jpTag}{strs[0]}\n";
        }
        var sb = new StringBuilder(firstLine);
        for (int i = 1; i < strs.Count; i++)
        {
            sb.Append($"{indent}{strs[i]}\n");
        }
        sb.Length--;
        return sb.ToString();
    }
    static int GetIndentCount(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '\t')
                return i;
        }
        return 0;
    }
    static string ReplaceFirst(string source, string find, string replace)
    {
        int place = source.IndexOf(find);
        if (place == -1)
            throw new Exception();
        return source.Remove(place, find.Length).Insert(place, replace);
    }
    static Regex faceTagReg = new Regex(@"#\d+F");
    private static string GetFaceTag(string str)
    {
        return faceTagReg.IsMatch(str)? faceTagReg.Match(str).Value : "";
    }

    private static void TagMatch_Scena()
    {
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\jp";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\MorePortrait_jp";
        var cnFiles = Directory.GetFiles(cnDir);
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            var jpFile = Path.Combine(jpDir, fileName);
            if (!File.Exists(jpFile))
            {
                Console.WriteLine($"JP file not found: {fileName}");
                continue;
            }
            var cnContent = File.ReadAllText(cnFile).Replace("\r","");
            var jpContent = File.ReadAllText(jpFile);
            var cnMatches = dialogReg.Matches(cnContent);
            var jpMatches = dialogReg.Matches(jpContent);
            if (cnMatches.Count != jpMatches.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mismatch in number of matches for file: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                var list = new List<string>(jpMatches.Select(x => x.Value));
                foreach (Match cnMatch in cnMatches)
                {
                    var values = cnMatch.Value.Split("\n").Select(x=>x.Trim('\t'));
                    var item= list.FirstOrDefault(x => 
                    {
                        foreach (var v in values)
                        {
                            if (!x.Contains(v))
                                return false;
                        }
                        return true;
                    });
                    if(item != null)
                    {
                        list.Remove(item);
                    }
                }
                continue;
            }
            for (int i = 0; i < cnMatches.Count; i++)
            {
                var cnText = cnMatches[i].Value;
                var jpText = jpMatches[i].Value;
                var cnTags = tagReg.Matches(cnText);
                var jpTags = tagReg.Matches(jpText);
                if (cnTags.Count != jpTags.Count)
                {
                    Console.WriteLine(new string('-', 60));
                    Console.WriteLine($"Mismatch in number of tags for file: {fileName}, dialog index: {i}");
                    Console.WriteLine($"cn dialog:\n{cnText}");
                    Console.WriteLine($"jp dialog:\n{jpText}");
                    Console.WriteLine(new string('-', 60));
                    //Console.ReadKey();
                }
            }
        }
    }

    [GeneratedRegex("""
        (?<=\t)name ".+?"
        """)]
    private static partial Regex NameRegex();
    [GeneratedRegex("""
        (?<=\t)TextSetName ".+?"
        """)]
    private static partial Regex TextSetNameRegex();
    [GeneratedRegex("""
        (?<=\tED7MenuAdd .*? )".+?"
        """)]
    private static partial Regex ED7MenuAddRegex();
    [GeneratedRegex("""
        (?<=\tScMenuSetTitle .*? )".+?"
        """)]
    private static partial Regex ScMenuSetTitleRegex();
    [GeneratedRegex("""
        (?<=\tCharSetName .*? )".+?"
        """)]
    private static partial Regex CharSetNameRegex();
    [GeneratedRegex("""(?<=\t)".+?" \/\/""")]
    private static partial Regex MenuRegex();
    [GeneratedRegex("""
        (?<=\tTextTalkNamed .*? )".+?"
        """)]
    private static partial Regex TextTalkNamedRegex();
}
