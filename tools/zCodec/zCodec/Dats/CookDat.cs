using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Dats;

public class CookDatItem
{
    public ushort Id{ get; set; }
    public string Data { get; set; }= string.Empty;
    public ushort Unk { get; set; }
    [ExcelIgnore]public ushort pText { get; set; }
    public string Text { get; set; }= string.Empty;
}


public class CookDat(bool isAo): IDatCodec
{
    public IList<CookDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_cook";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<CookDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<CookDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        fs.Position = 2;
        var sec2Start = br.ReadUInt16();
        if (IsAo)
            fs.Position += 2;
        do
        {
            var item = new CookDatItem
            {
                Id = br.ReadUInt16(),
                Data = Convert.ToBase64String(br.ReadBytes(IsAo?0x38:0x2A)),
                pText = br.ReadUInt16(),
            };
            if(!IsAo)
                item.Unk = br.ReadUInt16();
            itemList.Add(item);
            if (item.Id == 999)
                break;
            item.Text = $"\"{br.ReadClmStringWithOffset(item.pText, encoding)}\"";
        }while (fs.Position < sec2Start);
        itemList.Add(new CookDatItem
        {
            Id = 0,
            Data = Convert.ToBase64String(br.ReadBytes(itemList.First().pText - sec2Start)),
        });
        return itemList;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        if (isAo)
        {
            bw.Write((ushort) 6);
            bw.Write((ushort)0x5e2);
            bw.Write((ushort)0x742);
            bw.Write(new byte[(Data.Count - 1) * 0x3c]);
        }
        else
        {
            bw.Write((ushort) 4);
            bw.Write((ushort)((Data.Count - 1) * 0x30+4));
            bw.Write(new byte[(Data.Count - 1) * 0x30]);
        }

        bw.Write(Convert.FromBase64String(Data.Last().Data));
        for (int i = 0; i < Data.Count - 2; i++)
        {
            var item = Data[i];
            item.pText = (ushort)ms.Position;
            bw.Write(ClmStringToBytes(item.Text.Trim('"'), encoding));
            bw.Write((byte)0);
        }
        ms.Position = isAo?6:4;
        for (int i = 0; i < Data.Count-1; i++)
        {
            var item = Data[i];
            bw.Write(item.Id);
            bw.Write(Convert.FromBase64String(item.Data));
            bw.Write(item.pText);
            if (!isAo)
                bw.Write(item.Unk);
        }
        return ms.ToArray();
    }
}