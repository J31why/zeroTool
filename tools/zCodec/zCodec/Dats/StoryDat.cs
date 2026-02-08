using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using zCodec.Calmare;

namespace zCodec.Dats;

public class StoryDatItem
{
    [ExcelIgnore]public ushort pContent { get; set; }
    public string Content { get; set; } = string.Empty;
}
public class StoryDat : IDatCodec
{
    public IList<StoryDatItem>? Data { get; private set; }
    public bool IsAo => true;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file).StartsWith("b_story");

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<StoryDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<StoryDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var num = Path.GetFileNameWithoutExtension(file)[^1];
        var count = num switch
        {
            '0' => 0xB,
            '1' => 0x2C-1,
            '2' => 0x4C-1,
            _=> throw new Exception($"invalid file name: {file}")
        };
        var start = br.ReadUInt16();
        fs.Position = count * 2;
        itemList.Add(new StoryDatItem
        {
            Content = Convert.ToBase64String(br.ReadBytes((int)(start - fs.Position)))
        });
        while (fs.Position < fs.Length)
        {
            itemList.Add(new StoryDatItem
            {
                Content = br.ReadClmString(encoding)
            });
        }
        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(new byte[(Data.Count-1) * 2]);
        bw.Write(Convert.FromBase64String(Data.First().Content));
        foreach (var item in Data.Skip(1))
        {
            item.pContent = (ushort)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.Content,encoding));
            bw.Write((byte)0);
        }
        ms.Position = 0;
        foreach (var item in Data.Skip(1))
            bw.Write(item.pContent);
        return ms.ToArray();
    }
}