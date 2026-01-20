using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class BookMemoDatItem
{
    [ExcelIgnore]public ushort pContent { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class BookMemoDat(bool isAo) : IDatCodec
{
    public IList<BookMemoDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        return name.StartsWith("t_book") || name == "t_memo";
    }

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;

    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<BookMemoDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<BookMemoDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new BookMemoDatItem
            {
                pContent = br.ReadUInt16()
            };
            item.Content = $"\"{br.ReadClmStringWithOffset(item.pContent, encoding)}\"";
            itemList.Add(item);
        } while (fs.Position < itemList.First().pContent);

        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[Data.Count * 2]);
        foreach (var item in Data)
        {
            item.pContent = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Content.Trim('\"'),encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data)
            bw.Write(item.pContent);
        return ms.ToArray();
    }

}