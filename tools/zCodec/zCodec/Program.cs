#region

using System.Text;
using System.Text.RegularExpressions;
using Common;
using MiniExcelLibs;
using Models;
using Newtonsoft.Json;
using zCodec.Calmare;
using zCodec.Calmare.Opcodes;
using zCodec.Dats;
using zCodec.Dats.As;

#endregion

namespace zCodec;

internal static class Program
{
    private enum zCodecCommands
    {
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
        ActionScript,
        MonsterStatus,
        TextData
    }

    private static string _inputPath = "", _outPath = "", _calmare = "", _currentDir = "";
    private static zCodecCommands _command;
    private static zCodecFlag _flag = zCodecFlag.Scena;
    private static bool _isAo;
    private static Encoding _encoding = null!;
    private static void ParseArgs(string[] args)
    {
        _currentDir = Utils.CurrentDir;
        _calmare = Path.Combine(_currentDir, "calmare.exe");
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg.ToLowerInvariant())
            {
                case "-o":
                    _outPath = args[++i];
                    if (!Directory.Exists(_outPath))
                        throw new InvalidOperationException();
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
                case "-scena":
                    _flag = zCodecFlag.Scena;
                    break;
                case "-as":
                    _flag = zCodecFlag.ActionScript;
                    break;
                case "-ms":
                    _flag = zCodecFlag.MonsterStatus;
                    break;
                case "-dt":
                    _flag = zCodecFlag.TextData;
                    break;
                case "-cp":
                    var cp = args[++i];
                    _encoding = CodePagesEncodingProvider.Instance.GetEncoding(Convert.ToInt32(cp)) 
                        ?? throw new InvalidOperationException();
                    break;
                case "-ao":
                    _isAo = true;
                    SetGame(_isAo);
                    break;
                default:
                    if (!string.IsNullOrEmpty(_inputPath) || string.IsNullOrWhiteSpace(arg) || !Directory.Exists(arg) && !File.Exists(arg))
                        throw new InvalidOperationException();
                    _inputPath = arg;
                    break;
            }
        }

        if (!Directory.Exists(_inputPath) && !File.Exists(_inputPath) ||
            _encoding == null && _command is zCodecCommands.Compile or zCodecCommands.Decompile)
            throw new InvalidOperationException();
    }

    private static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            ParseArgs(args);
            Console.WriteLine();
            DatCodecFactory.Initialize(_isAo);
            if (_command == zCodecCommands.Compile && _flag == zCodecFlag.Scena)
            {
                if (!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码scena文件");
                CompileScenaScript(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码scena文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.ActionScript)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码as文件");
                CompileActionScript(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码as文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.MonsterStatus)
            {
                //throw new InvalidDataException("63200??????63201????");
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码ms文件");
                CompileMonsterStatus(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码ms文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Compile && _flag == zCodecFlag.TextData)
            {
                var outPath = string.IsNullOrEmpty(_outPath) ? GetOutPath(_inputPath, "compiled") : _outPath;
                Console.WriteLine($"正在编译{_encoding.BodyName}编码dt文件");
                CompileTextDat(_inputPath, outPath);
                Console.WriteLine($"已编译{_encoding.BodyName}编码dt文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.Scena)
            {
                if (!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                Console.WriteLine("正在反编译scena文件");
                DecompileScenaScript(_inputPath, _calmare);
                Console.WriteLine("已反编译scena文件");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.ActionScript)
            {
                var outPath = string.IsNullOrEmpty(_outPath)
                    ? Directory.Exists(_inputPath) ? _inputPath : Path.GetDirectoryName(_inputPath)
                    : _outPath;
                Console.WriteLine("正在反编译as.dat文件");
                DecompileActionScript(_inputPath, outPath ?? throw new DirectoryNotFoundException());
                Console.WriteLine($"已反编译as.dat文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.MonsterStatus)
            {
                var outPath = string.IsNullOrEmpty(_outPath)
                    ? Directory.Exists(_inputPath) ? _inputPath : Path.GetDirectoryName(_inputPath)
                    : _outPath;
                Console.WriteLine("正在反编译ms.dat文件");
                DecompileMonsterStatus(_inputPath, outPath ?? throw new DirectoryNotFoundException());
                Console.WriteLine($"已反编译ms.dat文件到目录：{outPath}");
            }
            else if (_command == zCodecCommands.Decompile && _flag == zCodecFlag.TextData)
            {
                var outPath = string.IsNullOrEmpty(_outPath)
                    ? Directory.Exists(_inputPath) ? _inputPath : Path.GetDirectoryName(_inputPath)
                    : _outPath;
                Console.WriteLine("正在反编译dt文件");
                DecompileTextDat(_inputPath, outPath ?? throw new DirectoryNotFoundException());
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
                Tw2s(_inputPath, outPath);
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
        catch (InvalidOperationException)
        {
            OutHelp();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void CompileMonsterStatus(string inputPath, string outPath)
    {
        
        
        var file = GetFiles(inputPath,"*.json").FirstOrDefault(x=>Path.GetFileName(x) == "ms.json");
        if (string.IsNullOrEmpty(file))
            return;
        var codec = new MsCodec(_isAo,_encoding);
        codec.Load(file).Compile(outPath);
    }

    private static void DecompileMonsterStatus(string inputPath, string outPath)
    {
        var files = GetFiles(inputPath, "*.dat").Where(
            x => Path.GetFileName(x).StartsWith("ms")).ToList();
        var codec = new  MsCodec(_isAo,_encoding);
        var obj = codec.Decompile(files);
        var outFile = Path.Combine(outPath, "ms.json");
        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        File.WriteAllText(outFile, json);
    }

    private static void DecompileTextDat(string inputPath, string outPath)
    {
        var files = GetFiles(inputPath,"*._dt");
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file));
            var codec = DatCodecFactory.Get(file);
            if (codec == null)
                continue;
            var obj = codec.Decompile(file, _encoding);
            switch (codec.DatSaveFormat)
            {
                case DatSaveFormat.Csv:
                    outFile += ".csv";
                    MiniExcel.SaveAs(outFile, obj, overwriteFile: true, excelType: ExcelType.CSV);
                    break;
                case DatSaveFormat.Json:
                    outFile += ".json";
                    var json =JsonConvert.SerializeObject(obj,Formatting.Indented);
                    File.WriteAllText(outFile, json);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void CompileTextDat(string inputPath, string outPath)
    {
        var files = GetFiles(inputPath);
        files = files.Where(x =>  x.EndsWith(".csv")|| x.EndsWith(".json")).ToArray();
        foreach (var file in files)
        {
            var codec = DatCodecFactory.Get(file);
            if (codec == null)
                continue;
            var data = codec.Load(file).Compile(_encoding);
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file) + "._dt");
            File.WriteAllBytes(outFile, data);
        }
    }

    private static void DecompileActionScript(string path, string outPath)
    {
        var codec = new AsCodec(_isAo,_encoding);
        void decompile(string file, string outfile)
        {
            var script = codec.Decompile(file);
            File.WriteAllText(outfile, script);
        }
        var files = GetFiles(path,"as*.dat");
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileNameWithoutExtension(file)+".txt");
            decompile(file, outFile);
        }
    }

    private static void CompileActionScript(string path, string outPath)
    {
       
        void compile(string file, string outfile)
        {
            var codec = new AsCodec(_isAo,_encoding);
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
            var codec = new CalmareCodec();
            codec.ParseFromFile(file);
            var success =codec.CompileToFile(outfile, _calmare, _encoding);
            if(!success)
                Console.WriteLine("文件编译错误: {0}\n", file);
        }
        var files = GetFiles(path,"*.clm");
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            compile(file, outFile);
        }
    }

    private static void Tw2s(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var content = File.ReadAllText(file);
                var replaceDic = new Dictionary<string, string>
                {
                    ["⇒"]= "→",
                    ["≪"]="《",
                    ["≫"]="》",
                };
                foreach (var item in replaceDic.Where(item => content.Contains(item.Key)))
                    content=content.Replace(item.Key, item.Value);
                if (Path.GetFileNameWithoutExtension(file).Equals("e3500"))
                {
                    content=content.Replace("出發囉！", "我们出发了！");
                }
                ExtraEncoding.DoubleByteCharRegex().Matches(content).Select(x=>x.Value).ToList().ForEach(x =>
                {
                    if (CalmareCodec.RemapChars.ContainsKey(x))
                        return;
                    var unicount = Encoding.Unicode.GetByteCount(x);
                    var gbkcount = ExtraEncoding.GBK.GetByteCount(x);
                    if (unicount != gbkcount)
                    {
                        Console.WriteLine("错误 "+x);
                    }
                });
                content = CLEDecrypter.Tw2s(content).Replace("\r","");
                File.WriteAllText(outfile, content, Path.GetExtension(file) == ".csv"?Encoding.UTF8:ExtraEncoding.UTF8NoBOM);
            }
            catch 
            {
                Console.WriteLine(file);
                throw;
            }
        }
        var files = GetFiles(path);
        files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")|| x.EndsWith(".csv")|| x.EndsWith(".json")).ToArray();
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
            catch 
            {
                Console.WriteLine(file);
                throw;
            }
        }
        var files = GetFiles(path);
        files = files.Where(x => x.EndsWith(".clm") || x.EndsWith(".txt")|| x.EndsWith(".csv")|| x.EndsWith(".json")).ToArray();
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
            catch 
            {
                Console.WriteLine(file);
                throw;
            }
        }
        var files = GetFiles(path);
        foreach (var file in files)
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(file));
            decrypt(file, outFile);
        }
    }

    private static void SetGame(bool isAo)
    {
        CLEDecrypter.IsAo = isAo;
        ScenaOpcode.IsAo = true;
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
        var outPath = string.Empty;
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
            { "-o <dir>", "（可选）指定存在的输出目录，不支持scena反编译" },
            { "-cp <codepage>", "编译/反编译编码" }
        };
        Dictionary<string, string> flags = new()
        {
            { "-scena", "（默认）编译/反编译scena脚本文件标志，目录下必须有calmare.exe" },
            { "-as", "编译/反编译as脚本文件标志" },
            { "-ms", "编译/反编译ms文件标志" },
            { "-dt", "编译/反编译text文件夹_dt文件标志" },
            { "-ao", "碧之轨迹标志" }
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

}