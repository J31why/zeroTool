#region

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Common;
using MiniExcelLibs;
using Models;
using OpenCCNET;
using zCodec.Calmare;
using zCodec.Dats;
using zCodec.Dats.As;

#endregion

namespace zCodec;

internal static class Program
{
    private enum zCodecCommands
    {
        None,
        Compile,
        Decompile,
        DecryptString,
        DecryptFile,
        Tw2s,
        Excel2MessString
    }

    private enum zCodecFlag
    {
        Scena,
        AS,
        TextDt
    }

    private static string _inputPath = "", _outPath = "", _calmare = "", _currentDir = "";
    private static zCodecCommands _command;
    private static zCodecFlag _flag = zCodecFlag.Scena;
    [NotNull]private static Encoding? _encoding;

    private static void ParseArgs(string[] args)
    {

        _currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
        _currentDir = Path.GetDirectoryName(_currentDir) ?? throw new DirectoryNotFoundException();
        _calmare = Path.Combine(_currentDir, "calmare.exe");
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg.ToLowerInvariant())
            {
                case "-o":
                    _outPath = args[++i];
                    if (!Directory.Exists(_outPath))
                        throw new DirectoryNotFoundException();
                    break;
                case "-c":
                    _command = zCodecCommands.Compile;
                    break;
                case "-d":
                    _command = zCodecCommands.Decompile;
                    break;
                case "-ds":
                    _command = zCodecCommands.DecryptString;
                    break;
                case "-df":
                    _command = zCodecCommands.DecryptFile;
                    break;
                case "-tw2s":
                    _command = zCodecCommands.Tw2s;
                    break;
                case "-excel2ms":
                    _command = zCodecCommands.Excel2MessString;
                    break;
                case "-as":
                    _flag = zCodecFlag.AS;
                    break;
                case "-scena":
                    _flag = zCodecFlag.Scena;
                    break;
                case "-dt":
                    _flag = zCodecFlag.TextDt;
                    break;
                case "-cp":
                    var cp = args[++i];
                    _encoding = CodePagesEncodingProvider.Instance.GetEncoding(Convert.ToInt32(cp));
                    break;
                default:
                    if (!string.IsNullOrEmpty(_inputPath) || string.IsNullOrWhiteSpace(arg))
                        throw new ArgumentException();
                    if (!Directory.Exists(arg) && !File.Exists(arg))
                        throw new DirectoryNotFoundException();
                    _inputPath = arg;
                    break;
            }
        }

        if (!Directory.Exists(_inputPath) && !File.Exists(_inputPath))
            throw new DirectoryNotFoundException();
        if (_encoding == null && _command is zCodecCommands.Compile or zCodecCommands.Decompile)
            throw new ArgumentException();
    }

    private static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            ParseArgs(args);
        }
        catch
        {
            OutHelp();
            return;
        }

        try
        {
            Console.WriteLine();
            if (_command == zCodecCommands.Compile && _flag == zCodecFlag.Scena)
            {
                if(!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码clm文件");
                CompileScenaScript(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码clm文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.AS)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码as文件");
                CompileActionScript(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码as文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.TextDt)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码dt文件");
                CompileTextDat(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码dt文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.Scena)
            {
                if(!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                Console.WriteLine("正在反编译Bin文件");
                DecompileScenaScript(_inputPath, _calmare);
                Console.WriteLine("已反编译Bin文件");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.AS)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? 
                    Directory.Exists(_inputPath)? _inputPath: Path.GetDirectoryName(_inputPath)
                    : _outPath;
                Console.WriteLine("正在反编译as.dat文件");
                DecompileActionScript(_inputPath, outPath ?? throw new DirectoryNotFoundException());
                Console.WriteLine($"已反编译as.dat文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.TextDt)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? 
                    Directory.Exists(_inputPath)? _inputPath: Path.GetDirectoryName(_inputPath)
                    : _outPath;
                Console.WriteLine("正在反编译dt文件");
                DecompileTextDat(_inputPath,outPath);
                Console.WriteLine($"已反编译dt文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.DecryptString)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "decrypted") : _outPath;
                Console.WriteLine("正在解密云豹字符串");
                DecryptStr(_inputPath, outPath);
                Console.WriteLine($"已解密云豹字符串到目录：{outPath}");
            }
            else if (_command == zCodecCommands.DecryptFile)
            {
                Console.WriteLine("正在解密云豹文件");
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "decrypted") : _outPath;
                DecryptFile(_inputPath, outPath);
                Console.WriteLine($"已解密云豹文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Tw2s)
            {
                Console.WriteLine("正在转换为大陆简体");
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "converted") : _outPath;
                ClmTw2s(_inputPath, outPath);
                Console.WriteLine($"已转换为大陆简体到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Excel2MessString)
            {
                Console.WriteLine("正在把excel转换为mess_strings_cn.txt");
                var outPath = string.IsNullOrEmpty(_outPath)
                    ? Path.GetDirectoryName(_inputPath) ?? throw new DirectoryNotFoundException()
                    : _outPath;
                Excel2MessString(_inputPath, outPath);
                Console.WriteLine($"已把excel转换为mess_strings_cn.txt到目录：{outPath}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void DecompileTextDat(string inputPath, string outPath)
    {
        var files = GetFiles(inputPath,"*._dt");
        foreach (var file in files)
        {
            DatHelper.ToCsv(file,outPath, _encoding);
        }
    }

    private static void CompileTextDat(string inputPath, string outPath)
    {
        var files = GetFiles(inputPath,"*.csv");
        foreach (var file in files)
        {
            DatHelper.ToDat(file,outPath, _encoding);
        }
    }

    private static void DecompileActionScript(string path, string outPath)
    {
        var codec = new AsCodec(_encoding);
        void decompile(string file, string outfile)
        {
            var script = codec.Decompile(file);
            File.WriteAllText(outfile, script);
        }
        var files = GetFiles(path,"as*.dat");
        foreach (var file in files.Where(x=>!x.EndsWith("as90000.dat")))
        {
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file)+".txt");
            decompile(file, outFile);
        }
    }

    private static void CompileActionScript(string path, string outPath)
    {
        var codec = new AsCodec(_encoding);
        void compile(string file, string outfile)
        {
            var text =  File.ReadAllText(file);
            var data = codec.Compile(text);
            File.AppendAllBytes(outfile, data);
        }
        var files = GetFiles(path,"as*.txt");
        foreach (var file in files)
        {
            var outFile  = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file)+".dat");
            compile(file, outFile);
        }
    }

    private static void Excel2MessString(string excelFile, string outPath)
    {
        if (!File.Exists(excelFile))
            throw new FileNotFoundException(excelFile);
        var outFile = Path.Combine(outPath, "mess_strings_cn.txt");
        var rows = MiniExcel.Query<MessString>(excelFile).ToList();
        var sb = new StringBuilder(
            "# This file contains the Chinese translation for display messages.\n" +
            "# Note that this file is GBK encoded.\n\n");
        foreach (var row in rows.Where(row => !string.IsNullOrEmpty(row.Key) && !string.IsNullOrEmpty(row.CN)))
            sb.AppendLine(row.ToLine());
        File.WriteAllText(outFile, sb.ToString(), ExtraEncoding.GBK);
    }

    private static void CompileScenaScript(string path, string outPath)
    {
        void compile(string file, string outfile)
        {
            try
            {
                var codec = new CalmareCodec();
                codec.ParseFromFile(file);
                codec.CompileToFile(outfile, _calmare, _encoding);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR]:{file}");
                Console.WriteLine(e.Message);
            }
        }
        var files = GetFiles(path,"*.clm");
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            compile(file, outFile);
        }
    }

    private static void ClmTw2s(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var content = File.ReadAllText(file);
                if (Path.GetFileNameWithoutExtension(file).Equals("e3500"))
                {
                    content=content.Replace("出發囉！", "我们出发了！");
                }
                content = CLEDecrypter.Tw2s(content);
                File.WriteAllText(outfile, content, Path.GetExtension(file) == ".csv"?Encoding.UTF8:ExtraEncoding.UTF8NoBOM);
            }
            catch (Exception e)
            {
                Console.WriteLine("转大陆简体失败: {0}", file);
            }
        }
        var files = GetFiles(path);
        files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")|| x.EndsWith(".csv")).ToArray();
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            decrypt(file, outFile);
        }
    }

    private static void DecompileScenaScript(string path, string calmare)
    {
        var files = GetFiles(path,"*.bin");
        foreach (var file in files) 
            Utils.RunExe(calmare, $"\"{file}\"", 1);
    }

    private static void DecryptStr(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var content = File.ReadAllText(file);
                content = CLEDecrypter.DecryptChar(content, out var warnList);
                if (warnList.Count > 0)
                {
                    Console.WriteLine("\n非法文字: {0}", string.Join(' ', warnList));
                    Console.WriteLine("文件有非法文字: {0}\n", file);
                }

                File.WriteAllText(outfile, content,
                    Path.GetExtension(file) == ".csv" ? Encoding.UTF8 : ExtraEncoding.UTF8NoBOM);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密字符串失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
        var files = GetFiles(path);
        files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")|| x.EndsWith(".csv")).ToArray();
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            decrypt(file, outFile);
        }
    }

    private static void DecryptFile(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var bytes = CLEDecrypter.DecryptFile(file);
                File.WriteAllBytes(outfile, bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密文件失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
        var files = GetFiles(path);
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            decrypt(file, outFile);
        }
    }

    private static IEnumerable<string> GetFiles(string path, string pattern="*.*")
    {
        if (Directory.Exists(path))
           return Directory.EnumerateFiles(path,pattern);
        if (File.Exists(path))
            return [path];
        return [];
    }
    
    private static string GetOutPath(string path, string dir)
    {
        var outPath = "";
        if (File.Exists(path))
            outPath = Path.Combine(Path.GetDirectoryName(path) ?? throw new DirectoryNotFoundException(), dir);
        else if (Directory.Exists(path)) outPath = Path.Combine(path, dir);

        if (outPath == string.Empty || outPath == dir)
            throw new DirectoryNotFoundException();
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);
        return outPath;
    }

    private static void OutHelp()
    {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("zCodec 工具帮助");
        Console.WriteLine("========================");
        Console.WriteLine("格式: zCodec [命令] <输入> -o [输出]\n");
        Dictionary<string, string> commands = new()
        {
            { "-c", "编译" },
            { "-d", "反编译" },
            { "-ds", "解密云豹加密字符串" },
            { "-df", "解密云豹加密文件" },
            { "-tw2s", "台湾文本转大陆简体" },
            { "-excel2ms", "Excel转mess_strings_cn.txt" },
            { "-o <dir>", "（可选）指定存在的输出目录，不支持反编译" },
            { "-cp <codepage>", "编译/反编译编码" }
        };
        Dictionary<string, string> flags = new()
        {
            { "-scena", "（默认）编译/反编译scena脚本文件标志，目录下必须有calmare.exe" },
            { "-as", "编译/反编译as脚本文件标志" },
            { "-dt", "编译/反编译text文件夹_dt文件" },
        };
        Console.WriteLine("命令列表:");
        foreach (var cmd in commands)
            Console.WriteLine($"  {cmd.Key,-15} {cmd.Value}");
        Console.WriteLine("标志列表:");
        foreach (var cmd in flags)
            Console.WriteLine($"  {cmd.Key,-15} {cmd.Value}");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  zCodec -c -cp 936 ./file.clm");
        Console.WriteLine("  zCodec -d -cp 932 ./bin/ -o ./output/");
    }

    private static void SearchTWPhrases(string dir)
    {
        //查看有多少个台湾IT用语
        var dic = new HashSet<string>(300);
        //opencc 原版TWPhrasesIT.txt
        var txt = File.ReadAllText("Dictionary\\TWPhrasesIT.txt");
        var matches = Regex.Matches(txt, """(?<=\s)(?!^)\S+""", RegexOptions.Multiline);
        var pattern = string.Join('|', matches.Select(x => x.Value).ToList());
        var reg = new Regex(pattern, RegexOptions.Multiline);
        var files = Directory.GetFiles(dir, "*.clm");
        foreach (var file in files)
        {
            matches = reg.Matches(File.ReadAllText(file));
            foreach (Match match in matches)
                dic.Add(match.Value);
        }

        var sb = new StringBuilder();
        foreach (var s in dic)
        {
            var p = @"^.*?\s" + s + @"(?: .*?$|$)";
            var r = new Regex(p, RegexOptions.Multiline);
            if (r.IsMatch(txt))
            {
                var match = r.Match(txt);
                if (!sb.ToString().Contains(match.Value))
                    sb.AppendLine(match.Value);
                else
                    Console.WriteLine(match.Value);
            }
            else
            {
                Console.WriteLine("error: " + s);
            }
        }

        File.WriteAllText("TWPhrasesIT.txt", sb.ToString());
        Console.WriteLine("dic.Count: " + dic.Count);
    }
}