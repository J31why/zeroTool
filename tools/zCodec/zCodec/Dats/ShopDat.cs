using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;
using zCodec.Calmare;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class ShopDatData
{
    public List<ShopDatItem> Shops { get; set; } = [];
    public List<int> Indexes { get; set; } = [];
}
public class ShopDatItem
{
    [JsonIgnore] public ushort pEntry{ get; set; }
    public short Id { get; set; }
    public string Name { get; set; }= string.Empty;
    public byte Type { get; set; }
    public byte ListCount { get; set; }
    public string Data { get; set; }= string.Empty;
    [JsonIgnore]public ushort pList { get; set; }
    [JsonIgnore]public ushort pName { get; set; }
    public string List { get; set; }= string.Empty;
}

public class ShopDat(bool isAo) : IDatCodec
{
    public ShopDatData? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file) => Path.GetFileNameWithoutExtension(file) == "t_shop";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Json;
    public IDatCodec Load(string file)
    {
        Data = JsonConvert.DeserializeObject<ShopDatData>(File.ReadAllText(file));
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<ShopDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var start = br.ReadUInt16();
        fs.Position= start;
        var data = new ShopDatData();
        do
        {
            var item = new ShopDatItem
            {
                pEntry = (ushort)fs.Position,
                Id = br.ReadInt16(),
                Type = br.ReadByte(),
                ListCount = br.ReadByte(),
                Data = Convert.ToBase64String(br.ReadBytes(0x8)),
                pList = br.ReadUInt16(),
                pName = br.ReadUInt16(),
                Name = br.ReadClmString(encoding),
            };
            if (item.ListCount < 100)
                item.List = Convert.ToBase64String(br.ReadBytes(item.ListCount * 2));
            data.Shops.Add(item);
        } while (fs.Position < fs.Length);

        fs.Position = 0;
        while (fs.Position < start)
        {
            var pos = br.ReadUInt16();
            data.Indexes.Add(pos == 0 ? -1 : data.Shops.FindIndex(x => x.pEntry == pos));
        }
        return data;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[Data.Indexes.Count * 2]);
        foreach (var item in Data.Shops)
        {
            item.pEntry = (ushort)ms.Position;
            bw.Write(item.Id);
            bw.Write(item.Type);
            bw.Write(item.ListCount);
            bw.Write(Convert.FromBase64String(item.Data));
            var nameBytes = ClmStringToBytes(item.Name, encoding);
            bw.Write((ushort)(ms.Position + 4 + nameBytes.Length + 1));
            bw.Write((ushort)(ms.Position + 2));
            bw.Write(nameBytes);
            bw.Write((byte)0);
            bw.Write(Convert.FromBase64String(item.List));
        }
        ms.Position = 0;
        foreach (var index in Data.Indexes)
            bw.Write((ushort)(index == -1 ? 0 : Data.Shops[index].pEntry));
        return ms.ToArray();
    }
}