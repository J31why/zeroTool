using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using zCodec.Calmare;

namespace zCodec.Dats;



public class IttxtDatItem
{
    public int Table { get; set; }
    public bool IsTableRepeat { get; set; }
    public bool IsItemRepeat { get; set; }
    [ExcelIgnore]public ushort pId { get; set; }
    public uint Id { get; set; }
    [ExcelIgnore]public ushort pName { get; set; }
    [ExcelIgnore]public ushort pDesc { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
}


public class IttxtDat(bool isAo) : IDatCodec
{
    public IList<IttxtDatItem>? Data { get; private set; }
    ushort lastItemOffset = 0;
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file) => Path.GetFileNameWithoutExtension(file).StartsWith("t_ittxt");
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<IttxtDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        lastItemOffset = 0;
        var itemList = new List<IttxtDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var tableList = new List<ushort>(0x10);
        do{
            tableList.Add(br.ReadUInt16());
        } while (fs.Position < tableList.First());
        var itemDataStart = br.ReadUInt16();
        fs.Position -= 2;
        ushort lastOffset = 0;
        for (var index = 0; index < tableList.Count; index++)
        {
            var tableStart = tableList[index];
            if (tableStart == lastOffset)
            {
                itemList.Add(new IttxtDatItem
                {
                    Table = index,
                    IsTableRepeat = true
                });
                continue;
            }
            lastOffset = tableStart;
            fs.Position = tableStart;
            var tableEnd = index == tableList.Count - 1
                ? itemDataStart
                : tableList.Skip(index + 1).First(x => x != tableStart);
   
            GetTableItems(itemList,br,index, tableEnd,encoding);
        }
        return itemList;
    }

    private void GetTableItems(List<IttxtDatItem> list, BinaryReader br,int tableIndex,int end, Encoding encoding)
    {
        var fs = br.BaseStream;
        do
        {
            var item = new IttxtDatItem
            {
                Table = tableIndex,
                pId = br.ReadUInt16()
            };
            list.Add(item);
            item.IsItemRepeat = item.pId == lastItemOffset;
            if (item.IsItemRepeat)
                continue;
            lastItemOffset = item.pId;
            var pos = fs.Position;
            fs.Position = item.pId;
            item.Id = br.ReadUInt32();
            item.pName = br.ReadUInt16();
            item.pDesc = br.ReadUInt16();
            fs.Position = item.pName;
            item.Name = $"\"{br.ReadClmString(encoding)}\"";
            fs.Position = item.pDesc;
            item.Desc = $"\"{br.ReadClmString(encoding)}\"";
            fs.Position = pos;
        } while (fs.Position < end);
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        Dictionary<int, ushort> itemOffsets = [];
        // 占位
        var tableCount = Data.Last().Table + 1;
        bw.Write(new byte[(Data.Count(x => !x.IsTableRepeat) + tableCount) * 2]);
        // item
        foreach (var item in Data)
        {
            if (item.IsTableRepeat || item.IsItemRepeat)
                continue;
            item.pId = (ushort)ms.Position;
            bw.Write(item.Id);
            var nameBytes = CalmareCodec.ClmStringToBytes(item.Name.Trim('\"'),encoding);
            var descBytes = CalmareCodec.ClmStringToBytes(item.Desc.Trim('\"'),encoding);
            bw.Write((ushort)(ms.Position + 4));
            bw.Write((ushort)(ms.Position + 2 + nameBytes.Length + 1));
            bw.Write(nameBytes);
            bw.Write((byte)0);
            bw.Write(descBytes);
            bw.Write((byte)0);
        }
        // item偏移
        ms.Position = tableCount * 2;
        var lastTableOffset = 0;
        foreach (var item in Data)
        {
            if (!itemOffsets.ContainsKey(item.Table))
            {
                itemOffsets[item.Table] = (ushort)(item.IsTableRepeat ? lastTableOffset : ms.Position);
                if (!item.IsTableRepeat)
                    lastTableOffset = itemOffsets[item.Table];
            }
            if (item.IsTableRepeat)
                continue;
            if (item.IsItemRepeat)
            {
                bw.Write(lastItemOffset);
                continue;
            }
            lastItemOffset = item.pId;
            bw.Write(item.pId);
        }
        // 表偏移
        ms.Position = 0;
        for (var i = 0; i < itemOffsets.Count; i++)
            bw.Write(itemOffsets[i]);
        return ms.ToArray();
    }
}