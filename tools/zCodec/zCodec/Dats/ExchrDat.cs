using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class ExchrDatItem
{
    public ushort W { get; set; }
    public ushort H { get; set; }
    public ushort cW { get; set; }
    public ushort cH { get; set; }
    public int Id { get; set; }
    [ExcelIgnore]public ushort pFile { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string File { get; set; }= string.Empty;
    public string Text { get; set; }= string.Empty;
}

public class ExchrDat(bool isAo) : IDatCodec
{
    public IList<ExchrDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_exchr";
    public DatSaveFormat DatSaveFormat=> DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<ExchrDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<ExchrDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new ExchrDatItem
            {
                W = br.ReadUInt16(),
                H = br.ReadUInt16(),
                cW = br.ReadUInt16(),
                cH = br.ReadUInt16(),
            };
            if(item is not{W: 0, H: 0, cW: 0, cH: 0})
                item.Id = br.ReadInt32();
            item.pFile = br.ReadUInt16();
            item.pText = br.ReadUInt16();
            item.File = $"\"{br.ReadClmStringWithOffset(item.pFile, encoding)}\"";
            item.Text = $"\"{br.ReadClmStringWithOffset(item.pText, encoding)}\"";
            itemList.Add(item);
        }while (fs.Position < itemList.First().pFile);
        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[Data.Count * 0x10 - 4]);
        foreach (var item in Data)
        {
            item.pFile = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.File.Trim('"'), encoding));
            bw.Write((byte)0);
            item.pText = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Text.Trim('"'), encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data)
        {
            bw.Write(item.W);
            bw.Write(item.H);
            bw.Write(item.cW);
            bw.Write(item.cH);
            if (item is not { W: 0, H: 0, cW: 0,cH:0 })
                bw.Write(item.Id);
            bw.Write(item.pFile);
            bw.Write(item.pText);
        }        
        return ms.ToArray();
    }
}