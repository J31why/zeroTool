using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json.Serialization;
using zCodec.Calmare;

namespace zCodec.Dats;

public enum MagicDatItemType
{
    None=0,
    Repeat=1,
    Empty=2,
    StrStart=3
}
public class MagicDatItem
{
    [ExcelIgnore]public ushort pEntry{ get; set; }
    public MagicDatItemType ItemType { get; set; }
    public string Data { get; set; }= string.Empty;
    [ExcelIgnore]public ushort pName { get; set; }
    [ExcelIgnore]public ushort pDesc { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
}

public class MagicDat(bool isAo) : IDatCodec
{
    public IList<MagicDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_magic";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<MagicDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<MagicDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var lastOffset = 0;
        var dataStart = br.ReadUInt16();
        fs.Position = 0;
        do
        {
            var item = new MagicDatItem
            {
                pEntry = br.ReadUInt16()
            };
            itemList.Add(item);
            if (item.pEntry == lastOffset)
            {
                item.ItemType = MagicDatItemType.Repeat;
                continue;
            }
            lastOffset = item.pEntry;
        } while (fs.Position < dataStart);

        var pStr = 0;
        for (var i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            if (item.ItemType is MagicDatItemType.Repeat)
                continue;
            var end = itemList.Skip(i + 1)
                          .FirstOrDefault(x => x.ItemType == MagicDatItemType.None)?.pEntry
                      ?? itemList.First().pName;
            var length = end - item.pEntry;
            if (pStr == item.pEntry)
            {
                item.ItemType = MagicDatItemType.StrStart;
                continue;
            }
            if (length <= 4)
            {
                item.ItemType = MagicDatItemType.Empty;
                continue;
            }
            fs.Position = item.pEntry;
            item.Data = Convert.ToBase64String(br.ReadBytes(!IsAo ? 0x1C : 0x18));
            item.pName = br.ReadUInt16();
            item.Name = $"\"{br.ReadClmStringWithOffset(item.pName, encoding)}\"";
            item.pDesc = br.ReadUInt16();
            item.Desc = $"\"{br.ReadClmStringWithOffset(item.pDesc, encoding)}\"";
            if (pStr == 0)
                pStr = item.pName;
        }

        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var emptyCount = Data.Count(x=>x.ItemType == MagicDatItemType.Empty);
        var itemCount = Data.Count(x=>x.ItemType == MagicDatItemType.None);
        bw.Write(new byte[Data.Count * 2 + emptyCount * 4 + itemCount * (!IsAo ? 0x20 : 0x1c)]);
        foreach (var item in Data)
        {
            if (item.ItemType is not MagicDatItemType.None)
                    continue;
            item.pName = (ushort)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.Name.Trim('"'),encoding));
            bw.Write((byte)0);
            item.pDesc = (ushort)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.Desc.Trim('"'),encoding));
            bw.Write((byte)0);
        }
        ms.Position = Data.Count * 2;
        foreach (var item in Data)
        {
            item.pEntry = (ushort)ms.Position;
            if (item.ItemType is MagicDatItemType.StrStart or MagicDatItemType.Repeat) continue;
            if (item.ItemType == MagicDatItemType.Empty)
            {
                bw.Write(0);
                continue;
            }

            bw.Write(Convert.FromBase64String(item.Data));
            bw.Write(item.pName);
            bw.Write(item.pDesc);
        }
        ms.Position = 0;
        ushort lastOffset = 0;
        foreach (var item in Data)
        {
            if (item.ItemType is MagicDatItemType.Repeat)
            {
                bw.Write(lastOffset);
                continue;
            }
            
            bw.Write(item.pEntry);
            lastOffset =  item.pEntry;
        }
        return ms.ToArray();
    }
}