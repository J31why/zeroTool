using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using zCodec.Calmare;

namespace zCodec.Dats;

public class NameDatItem
{
    public ushort Id { get; set; }
    [ExcelIgnore]public ushort pName { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class NameDat(bool isAo) : IDatCodec
{
    public IList<NameDatItem>? Data { get; private set; }

    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file) => Path.GetFileNameWithoutExtension(file) == "t_name";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<NameDatItem>(file).ToList();
        return this;
    }
    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<NameDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do{
            var item = new NameDatItem
            {
                Id = br.ReadUInt16(),
                pName =  br.ReadUInt16(),
                Data = Convert.ToBase64String(br.ReadBytes(0x10))
            };
            item.Name = $"\"{br.ReadClmStringWithOffset(item.pName, encoding)}\"";
            itemList.Add(item);
        } while (fs.Position < itemList.First().pName);
        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[Data.Count * 0x14]);
        foreach (var item in Data)
        {
            item.pName = (ushort)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.Name.Trim('\"'),encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data)
        {
            bw.Write(item.Id);
            bw.Write(item.pName);
            bw.Write(Convert.FromBase64String(item.Data));
        }
        return ms.ToArray();
    }
}
