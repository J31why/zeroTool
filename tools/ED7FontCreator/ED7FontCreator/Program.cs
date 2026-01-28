using System;
using System.Text.Unicode;
using ITF;
using SkiaSharp;


namespace ED7FontCreator
{
    internal class Program
    {
        private static string _fontName = "";
        private static int _fontSize;
        private static SKFontStyleWeight _fontWeight;
        private static float _baselineOffset;
        private static string _unicodeRange = "";
        private static string _currentPath = "";

        static void Initialize()
        {
            _currentPath = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
            var file = Path.Combine(_currentPath ?? "", "font.cfg");
            if (!File.Exists(file))
                throw new ArgumentException("未找到font.cfg");
            using var reader = new StringReader(File.ReadAllText(file));
            var line = "";
            var count = 0;
            while (!string.IsNullOrEmpty(line = reader.ReadLine())) 
            {
                if (!line.Contains('=')) continue;
                var index = line.IndexOf('=');
                var p = line[..index].Trim();
                var v = line[(index + 1)..].Trim();
                switch(p)
                {
                    case "FontName":
                        _fontName = v;
                        count++;
                        break;
                    case "FontSize":
                        _fontSize = int.Parse(v);
                        count++;
                        break;
                    case "FontWeight":
                        _fontWeight = Enum.Parse<SKFontStyleWeight>(v);
                        count++;
                        break;
                    case "BaselineOffset":
                        _baselineOffset = float.Parse(v);
                        count++;
                        break;
                    case "UnicodeRange":
                        _unicodeRange = v;
                        count++;
                        break;
                }
            }
            if (count != 5)
                throw new ArgumentException("font.cfg设置出错");
        }
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("[INFO]解析font.cfg");
                Initialize();
                Console.WriteLine($@"
================================
FontName: {_fontName}
FontSize: {_fontSize}
FontWeight: {_fontWeight}
Baseline: {_baselineOffset}
Unicode: {_unicodeRange}
================================
                ");
                Console.WriteLine("[INFO]开始生成font.itp");
                using var ms = new MemoryStream();
                using var writer = new ITFWriter(ms);
                var res = writer.Build(_unicodeRange, _fontName, _fontSize, _fontWeight, _baselineOffset);
                if (!res)
                {
                    Console.WriteLine("[ERROR]生成失败");
                    return;
                }
                var file = Path.Combine(_currentPath, "font.itf");
                File.WriteAllBytes(file, ms.ToArray());
                Console.WriteLine($"[INFO]生成成功: {file}\n");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        
        }
    }
}
