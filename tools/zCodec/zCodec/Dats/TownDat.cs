using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class TownDatItem
{
    [ExcelIgnore]public ushort pName { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TownDat(bool isAo) : IDatCodec
{
    public IList<TownDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_town";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<TownDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<TownDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        fs.Position = 2;
        do
        {
            var item = new TownDatItem
            {
                pName = br.ReadUInt16()
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
        bw.Write((ushort)(IsAo ? 0xDC : 0xAA));
        bw.Write(new byte[Data.Count * 2]);
        foreach (var item in Data)
        {
            item.pName = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Name.Trim('\"'),encoding));
            bw.Write((ushort)0);
        }
        ms.Position = 2;
        foreach (var item in Data)
            bw.Write(item.pName);
        return ms.ToArray();
    }
}