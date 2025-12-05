using System.Text;
using System.Text.RegularExpressions;
using ED7ScenaParser;
using ED7ScenaParser.Scena;
using ED7ScenaParser.Scena.Struct;
using Newtonsoft.Json;
using OpenCCNET;
using AureoleEncoder = ED7ScenaParser.Aureole.AureoleEncoder;


Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var sjis = CodePagesEncodingProvider.Instance.GetEncoding(932)
               ?? throw new ArgumentException("编码不存在");
var gbk = CodePagesEncodingProvider.Instance.GetEncoding(936)
           ?? throw new ArgumentException("编码不存在");
ZhConverter.Initialize();


/*
var bytes = File.ReadAllBytes("E:\\Games\\The Legend of Heroes Trails from Zero\\data\\scena\\m0000.bin");
var ms2 = new MemoryStream(bytes);
var reader = new ScenaReader(ms2);
reader.Encoding = sjis;
reader.Parse();
Console.WriteLine(reader.Script.Out());
*/

var gbkEncoder = new AureoleEncoder();
var file = "C:\\Users\\Jelly\\Downloads\\Programs\\m0000.clm";
var test_placeholder_clmfile = @"C:\Users\Jelly\Downloads\Programs\output\m0000.clm";
var test_placeholder_scena_file = @"C:\Users\Jelly\Downloads\Programs\output\m0000.bin";
var clmText = File.ReadAllText(file);


// var utf8 = @"E:\Games\The Legend of Heroes Trails from Zero\data\localization\utf8.table.ce";
// var utf8table = File.ReadAllBytes(utf8);
// clmText=Regex.Replace(clmText, "[\u2000-\uffff]", x =>
// {
//     var bytes = sjis.GetBytes(x.Value);
//     var index = (bytes[0] << 8) - 0x8900 + bytes[1];
//     index *= 3;
//     index += 4;
//     bytes = [utf8table[index], utf8table[index +1], utf8table[index+ 2 ]];
//     var text = Encoding.UTF8.GetString(bytes).Trim('\0');
//     if (gbk.GetByteCount(text) !=2)
//     {
//         throw new Exception("错误: 长度错误");
//     }
//     return text;
// },RegexOptions.Multiline);
// SaveToHolder(file, clmText);

//
gbkEncoder.Parse(clmText);
var placeholderText = File.ReadAllText(test_placeholder_clmfile);
var placeholderEncoder = new AureoleEncoder();
placeholderEncoder.Parse(placeholderText);

if (placeholderEncoder.FnTexts.Count != gbkEncoder.FnTexts.Count)
{
    Console.WriteLine("错误: FN解析出错");
    return;
}

var binBytes = File.ReadAllBytes(test_placeholder_scena_file);
using var ms = new MemoryStream(binBytes);
using var br = new BinaryReader(ms);
ms.Seek(0x42, SeekOrigin.Begin);
var pFunc = br.ReadUInt16();
var nFunc = br.ReadUInt16() / 4;
var pFunctions = new uint[nFunc];
ms.Seek(pFunc, SeekOrigin.Begin);
for (int i = 0; i < nFunc; i++)
    pFunctions[i] =  br.ReadUInt32();


for (int i = 0; i < placeholderEncoder.FnTexts.Count; i++)
{
   var pfnText = placeholderEncoder.FnTexts[i];
   var gbkfnText = gbkEncoder.FnTexts[i];
   var start = (int)pFunctions[pfnText.index];
   var end = pfnText.index+1 <= pFunctions.Length - 1 ? (int)pFunctions[pfnText.index+1] : -1;

   for (int j = 0; j < pfnText.func.Count; j++)
   {
       var sjisBytes =  pfnText.func[j].Encode(sjis);
       var gbkBytes =  gbkfnText.func[j].Encode(gbk);
       if (sjisBytes.Length != gbkBytes.Length)
           throw new Exception("字节长度不一致");
       var result = Bit.Replace(binBytes, sjisBytes, gbkBytes, start, end, 1);
       if (!result.replaced)
           throw new Exception("未找到字节");
       binBytes = result.result;
   }
}
File.WriteAllBytes(test_placeholder_scena_file+".bin", binBytes);
Console.WriteLine("done.");
string SaveToHolder(string oriFile,string text)
{
    var placeholder = Regex.Replace(text,"[\u2000-\uffff]", "果");
    var outPath = Path.Combine(Path.GetDirectoryName(oriFile)?? throw new ArgumentException("路径获取错误"),"output");
    if(!Directory.Exists(outPath))
        Directory.CreateDirectory(outPath);
    outPath = Path.Combine(outPath, Path.GetFileName(oriFile));
    File.WriteAllText(outPath, placeholder);
    return outPath;
}