using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;

using static zCodec.Calmare.CalmareCodec;
namespace zCodec.Dats;

public class RecordDatItem
{
    public int Id{ get; set; }
    public byte Type{ get; set; }
    public byte Point{ get; set; }
    public ushort Count{ get; set; }
    [ExcelIgnore]public ushort pName{ get; set; }
    [ExcelIgnore] public ushort pDesc{ get; set; }
    public string Name { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
}
public class RecordDat(bool isAo) : IDatCodec
{
    public IList<RecordDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_record";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<RecordDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<RecordDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do{
            var item = new RecordDatItem
            {
                Type = br.ReadByte(),
                Point = br.ReadByte(),
                Count = br.ReadUInt16(),
                Id = br.ReadInt32(),
                pName = br.ReadUInt16(),
                pDesc = br.ReadUInt16(),
            };
            itemList.Add(item);
            item.Name = $"\"{br.ReadClmStringWithOffset(item.pName,encoding)}\"";
            item.Desc = $"\"{br.ReadClmStringWithOffset(item.pDesc,encoding)}\"";
        } while (fs.Position < itemList.First().pName);
        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[Data.Count * 0xc]);
        foreach (var item in Data)
        {
            item.pName = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Name.Trim('\"'),encoding));
            bw.Write((byte)0);
        }
        foreach (var item in Data)
        {
            item.pDesc = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Desc.Trim('\"'),encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data)
        {
            bw.Write(item.Type);
            bw.Write(item.Point);
            bw.Write(item.Count);
            bw.Write(item.Id);
            bw.Write(item.pName);
            bw.Write(item.pDesc);
        }
        return ms.ToArray();
    }
}