using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;


namespace zCodec.Dats;


public class ExmovDatItem
{
    public int ID { get; set; }
    [ExcelIgnore]public ushort pFile { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string File { get; set; }= string.Empty;
    public string Text { get; set; }= string.Empty;

}



public class ExmovDat(bool isAo) : IDatCodec
{
    public IList<ExmovDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_exmov";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<ExmovDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<ExmovDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new ExmovDatItem
            {
                pFile = br.ReadUInt16(),
                pText = br.ReadUInt16(),
                ID = br.ReadInt32(),
            };
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
        bw.Write(new byte[Data.Count * 0x8]);
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
            bw.Write(item.pFile);
            bw.Write(item.pText);
            bw.Write(item.ID);
        }     
        return ms.ToArray();
    }
}