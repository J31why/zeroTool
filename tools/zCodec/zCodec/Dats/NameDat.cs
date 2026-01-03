using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;

namespace zCodec.Dats;

public class NameDatItem
{
    public ushort Id { get; set; }
    [ExcelIgnore]public ushort pName { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public static class NameDat
{
    public static byte[] Serialize(this List<NameDatItem> items,Encoding encoding)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[items.Count * 0x14]);
        foreach (var item in items)
        {
            item.pName = (ushort)ms.Position;
            bw.Write([..encoding.GetBytes(item.Name), 0]);
        }
        ms.Position = 0;
        foreach (var item in items)
        {
            bw.Write(item.Id);
            bw.Write(item.pName);
            bw.Write(Convert.FromBase64String(item.Data));
        }
        return ms.ToArray();
    }

    public static void ToDat(string file, string outDir,Encoding encoding)
    {
        var list = MiniExcel.Query<NameDatItem>(file).ToList();
        var data = Serialize(list,encoding);
        var dtName = Path.GetFileNameWithoutExtension(file) + "._dt";
        File.WriteAllBytes(Path.Combine(outDir, dtName), data);
    }
    
    public static List<NameDatItem> Deserialize(string file, Encoding encoding)
    {
        var itemList = new List<NameDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new NameDatItem
            {
                Id = br.ReadUInt16(),
                pName =  br.ReadUInt16(),
                Data = Convert.ToBase64String(br.ReadBytes(0x10))
            };
            item.Name = br.ReadCStringWithOffset(item.pName, encoding) ?? throw new Exception();
            itemList.Add(item);
        } while (fs.Position < itemList.First().pName);
        return itemList;
    }

    public static void ToCsv(string file, string outDir,Encoding encoding)
    {
        var list = Deserialize(file, encoding);
        var csvName = Path.GetFileNameWithoutExtension(file) + ".csv";
        MiniExcel.SaveAs(Path.Combine(outDir,csvName), list,overwriteFile:true,excelType:ExcelType.CSV);
    }
}
