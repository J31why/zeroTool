using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class DbmonDatItem
{
    public ushort Id { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string Data { get; set; } =  string.Empty;
    public string Text { get; set; } =  string.Empty;
    
}

public class DbmonDat(bool isAo) : IDatCodec
{
    public IList<DbmonDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_dbmon";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<DbmonDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<DbmonDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new DbmonDatItem
            {
                Id = br.ReadUInt16(),
                pText = br.ReadUInt16(),
                Data = Convert.ToBase64String(br.ReadBytes(0x10))
            };
            item.Text = $"\"{br.ReadClmStringWithOffset(item.pText, encoding)}\"";
            itemList.Add(item);
            if (item.Id == 999)
                break;
        }while (fs.Position < fs.Length);
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
            item.pText = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Text.Trim('"'), encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data)
        {
            bw.Write(item.Id);
            bw.Write(item.pText);
            bw.Write(Convert.FromBase64String(item.Data));
        }   
        return ms.ToArray();
    }
}