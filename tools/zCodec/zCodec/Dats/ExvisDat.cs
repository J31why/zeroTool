using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class ExvisDatItem
{
    public ushort W { get; set; }
    public ushort H { get; set; }
    public ushort cW { get; set; }
    public ushort cH { get; set; }
    [ExcelIgnore]public ushort pFile { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string File { get; set; }= string.Empty;
    public string Text { get; set; }= string.Empty;
}
public class AoExvisDatItem
{
    public ushort W { get; set; }
    public ushort H { get; set; }
    public ushort cW { get; set; }
    public ushort cH { get; set; }
    public int Unk1{ get; set; }
    [ExcelIgnore]public ushort pFile { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string File { get; set; }= string.Empty;
    public string Text { get; set; }= string.Empty;
}

public class ExvisDat(bool isAo) : IDatCodec
{
    public IEnumerable? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_exvis";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = !IsAo ? MiniExcel.Query<ExvisDatItem>(file) : MiniExcel.Query<AoExvisDatItem>(file);
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        if (IsAo) return DecompileAo(file,encoding);
        var itemList = new List<ExvisDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new ExvisDatItem
            {
                W = br.ReadUInt16(),
                H = br.ReadUInt16(),
                cW = br.ReadUInt16(),
            };
            if (item is not { W: 0, H: 0, cW: 0 })
                item.cH = br.ReadUInt16();
            item.pFile = br.ReadUInt16();
            item.pText = br.ReadUInt16();
            item.File = $"\"{br.ReadClmStringWithOffset(item.pFile, encoding)}\"";
            item.Text = $"\"{br.ReadClmStringWithOffset(item.pText, encoding)}\"";
            itemList.Add(item);
        }while (fs.Position < itemList.First().pFile);
        return itemList;
    }

    private object DecompileAo(string file, Encoding encoding)
    {
        var itemList = new List<AoExvisDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new AoExvisDatItem
            {
                W = br.ReadUInt16(),
                H = br.ReadUInt16(),
                cW = br.ReadUInt16(),
                cH = br.ReadUInt16()
            };
            if (item is not { W: 0, H: 0, cW: 0 ,cH:0})
                item.Unk1 =  br.ReadInt32();
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
        if (IsAo) return CompileAo(encoding);
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var data = Data.Cast<ExvisDatItem>().ToList();
        bw.Write(new byte[data.Count * 0xC - 2]);
        foreach (var item in data)
        {
            item.pFile = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.File.Trim('"'), encoding));
            bw.Write((byte)0);
            item.pText = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Text.Trim('"'), encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in data)
        {
            bw.Write(item.W);
            bw.Write(item.H);
            bw.Write(item.cW);
            if (item is not { W: 0, H: 0, cW: 0 })
                bw.Write(item.cH);
            bw.Write(item.pFile);
            bw.Write(item.pText);
        }        
        return ms.ToArray();
    }

    private byte[] CompileAo(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var data = Data.Cast<AoExvisDatItem>().ToList();
        bw.Write(new byte[data.Count * 0x10 - 4]);
        foreach (var item in data)
        {
            item.pFile = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.File.Trim('"'), encoding));
            bw.Write((byte)0);
            item.pText = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Text.Trim('"'), encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in data)
        {
            bw.Write(item.W);
            bw.Write(item.H);
            bw.Write(item.cW);
            bw.Write(item.cH);
            if (item is not { W: 0, H: 0, cW: 0 ,cH:0})
                bw.Write(item.Unk1);
            bw.Write(item.pFile);
            bw.Write(item.pText);
        }        
        return ms.ToArray();
    }
}