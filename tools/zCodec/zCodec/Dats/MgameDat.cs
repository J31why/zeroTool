using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using zCodec.Calmare;

namespace zCodec.Dats;


public class MgameDatItem
{
    [ExcelIgnore]public ushort pEntry { get; set; }
    public string Data1 { get; set; } = string.Empty;
    public string Data2 { get; set; }= string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class MgameDat(bool isAo) : IDatCodec
{
    public IList<MgameDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_mgame";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Csv;
    public IDatCodec Load(string file)
    {
        Data = MiniExcel.Query<MgameDatItem>(file).ToList();
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<MgameDatItem>(0x200);
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {  
            var item = new MgameDatItem
            {
                pEntry = br.ReadUInt16()
            };
            itemList.Add(item);

        } while (fs.Position< itemList.First().pEntry);
        
        for (var i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            var end = i + 1 < itemList.Count ? itemList[i + 1].pEntry : (ushort)fs.Length;
            var len = end - item.pEntry;
            fs.Position = item.pEntry;
            if(i is 0 || (isAo && i is 9 or 10 or >=290 and <307) || (!IsAo && i >= 290))
            {
                item.Content = $"\"{br.ReadClmString(encoding)}\"";
                item.Data2 = Convert.ToBase64String(br.ReadBytes((int)(end - fs.Position)));
                continue;
            }
            if (len <= 0xd)
            {
                item.Data1 = Convert.ToBase64String(br.ReadBytes(len));
                continue;
            }

            var v1 = br.ReadUInt16();
            fs.Position += 4;
            var v3 = br.ReadUInt16();
            fs.Position += 3;
            switch (v3)
            {
                case 0x01 when v1 is 0xffff or 0:
                case 0x02 when v1 is 0xffff or 0:
                case 0x03 when v1 is 0xffff or 0:
                    item.Name =  $"\"{br.ReadClmString(encoding)}\"";
                    item.Content =$"\"{br.ReadClmString(encoding)}\"";
                    item.Data2 = Convert.ToBase64String(br.ReadBytes((int)(end - fs.Position)));
                    fs.Position = item.pEntry;
                    item.Data1 = Convert.ToBase64String(br.ReadBytes(0xb));
                    break;
                default:
                    fs.Position = item.pEntry;
                    item.Data1 = Convert.ToBase64String(br.ReadBytes(len));
                    break;
            }
        }
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
            item.pEntry = (ushort)ms.Position;
            if (string.IsNullOrEmpty(item.Data1) && !string.IsNullOrEmpty(item.Content))
            {
                bw.Write(CalmareCodec.ClmStringToBytes(item.Content.Trim('"'), encoding));
                bw.Write((byte)0);
                bw.Write(Convert.FromBase64String(item.Data2));
            }
            else if (!string.IsNullOrEmpty(item.Data1) && !string.IsNullOrEmpty(item.Name))
            {
                bw.Write(Convert.FromBase64String(item.Data1));
                bw.Write(CalmareCodec.ClmStringToBytes(item.Name.Trim('"'), encoding));
                bw.Write((byte)0);
                bw.Write(CalmareCodec.ClmStringToBytes(item.Content.Trim('"'), encoding));
                bw.Write((byte)0);
                bw.Write(Convert.FromBase64String(item.Data2));
            }
            else
            {
                bw.Write(Convert.FromBase64String(item.Data1));
            }
        }
        ms.Position = 0;
        foreach (var item in Data)
            bw.Write(item.pEntry);
        return ms.ToArray();
    }
}