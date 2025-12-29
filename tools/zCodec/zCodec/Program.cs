#region

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Common;
using MiniExcelLibs;
using Models;
using zCodec.Calmare;
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
        AS
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
                case "-de":
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
                CompileScena(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码clm文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.AS)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码as文件");
                CompileAS(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码as文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.Scena)
            {
                if(!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                Console.WriteLine("正在反编译Bin文件");
                DecompileScena(_inputPath, _calmare);
                Console.WriteLine("已反编译Bin文件");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.AS)
            {
                Console.WriteLine("正在反编译as.dat文件");
                DecompileAS(_inputPath);
                Console.WriteLine("已反编译as.dat文件");
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

    private static void DecompileAS(string path)
    {
        var coder = new AsCoder(_encoding);
        void decompile(string file, string outfile)
        {
            var script = coder.Parse(file);
            File.WriteAllText(outfile, script);
        }
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path,"as*.dat");
            foreach (var file in files)
            {
                var outFile = Path.Combine(path, Path.GetFileNameWithoutExtension(file)+".txt");
                decompile(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outPath = Path.GetDirectoryName(path)?? throw new DirectoryNotFoundException();
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(path)+".txt");
            decompile(path, outFile);
        }
    }

    private static void CompileAS(string path, string outPath)
    {
        var coder = new AsCoder(_encoding);
        void compile(string file, string outfile)
        {
            var text =  File.ReadAllText(file);
            var data = coder.ToDat(text);
            File.AppendAllBytes(outfile, data);
        }

        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "as*.txt");
            foreach (var file in files)
            {
                var outFile  = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file)+".dat");
                compile(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(path)+".dat");
            compile(path, outFile);
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
        foreach (var row in rows)
            if (!string.IsNullOrEmpty(row.Key) && !string.IsNullOrEmpty(row.CN))
                sb.AppendLine(row.ToLine());
        File.WriteAllText(outFile, sb.ToString(), ExtraEncoding.GBK);
    }

    private static void CompileScena(string path, string outPath)
    {
        void compile(string file, string outfile)
        {
            try
            {
                var coder = new CalmareCoder();
                coder.ParseFile(file);
                coder.Encode2File(outfile, _calmare, _encoding);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR]:{file}");
                Console.WriteLine(e.Message);
            }
        }

        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                compile(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            compile(path, outFile);
        }
    }

    private static void ClmTw2s(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var clm = CLEDecrypter.Tw2s(File.ReadAllText(file));
                File.WriteAllText(outfile, clm);
            }
            catch (Exception e)
            {
                Console.WriteLine("转大陆简体失败: {0}", file);
            }
        }

        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")).ToArray();
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
        }
    }

    private static void DecompileScena(string path, string calmare)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.bin");
            foreach (var file in files) Utils.RunExe(calmare, $"\"{file}\"", 1);
        }
        else if (File.Exists(path))
        {
            Utils.RunExe(calmare, path, 2);
        }
    }

    private static void DecryptStr(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var clmText = File.ReadAllText(file);
                clmText = CLEDecrypter.DecryptChar(clmText, out var warnList);
                if (warnList.Count > 0)
                {
                    Console.WriteLine("\n非法文字: {0}", string.Join(' ', warnList));
                    Console.WriteLine("文件有非法文字: {0}\n", file);
                }

                File.WriteAllText(outfile, clmText);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密字符串失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }

        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")).ToArray();
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
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

        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
        }
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
        Console.WriteLine("格式: zCodec [命令] <输入> -cp 936 -o [输出]\n");

        Dictionary<string, string> _commands = new()
        {
            { "-c", "编译" },
            { "-de", "反编译" },
            { "-ds", "解密云豹加密字符串" },
            { "-df", "解密云豹加密文件" },
            { "-tw2s", "台湾文本转大陆简体" },
            { "-excel2ms", "Excel转mess_strings_cn.txt" },
            { "-o <dir>", "（可选）指定存在的输出目录，不支持反编译" },
            { "-cp <codepage>", "编译/反编译编码" }
        };
        Dictionary<string, string> _flags = new()
        {
            { "-scena", "（默认）脚本文件标志，用于编译/反编译，目录下必须有calmare.exe" },
            { "-as", "as文件标志，用于编译/反编译" },
        };
        Console.WriteLine("命令列表:");
        foreach (var cmd in _commands)
            Console.WriteLine($"  {cmd.Key,-15} {cmd.Value}");
        Console.WriteLine("标志列表:");
        foreach (var cmd in _flags)
            Console.WriteLine($"  {cmd.Key,-15} {cmd.Value}");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  zCodec -c -cp 936 ./file.clm");
        Console.WriteLine("  zCodec -de -cp 932 ./bin/ -o ./output/");
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