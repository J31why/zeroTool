using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using Newtonsoft.Json;
using zCodec.Calmare;

namespace zCodec.Dats;

public record ProcessContent(bool HasAddress, string Content);

public class QuestDatItem
{
    public byte Id{ get; set; }
    public byte Unk1{ get; set; }
    public ushort Mira{ get; set; }
    public byte Dp{ get; set; }
    public byte Unk2{ get; set; }
    public ushort Unk3{ get; set; }
    public ushort Unk4{ get; set; }
    public ushort Unk5{ get; set; }
    [JsonIgnore] public uint pQuestName{ get; set; }
    [JsonIgnore] public uint pClient{ get; set; }
    [JsonIgnore] public uint pReqBoardContent{ get; set; }
    [JsonIgnore] public uint ppProcessContent{ get; set; }
    public string QuestName{ get; set; }= string.Empty;
    public string Client{ get; set; }= string.Empty;
    public string ReqBoardContent{ get; set; }= string.Empty;
    public List<uint> pProcessContents{ get; set; }= new (40);
    public List<ProcessContent> ProcessContents{ get; set; }= new (40);
}

public class QuestDat(bool isAo) : IDatCodec
{
    public IList<QuestDatItem>? Data { get; private set; }
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_quest";
    public DatSaveFormat DatSaveFormat => DatSaveFormat.Json;
    public IDatCodec Load(string file)
    {
        var json = File.ReadAllText(file);
        Data = JsonConvert.DeserializeObject<IList<QuestDatItem>>(json);
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var itemList = new List<QuestDatItem>();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        do
        {
            var item = new QuestDatItem
            {
                Id = br.ReadByte(),
                Unk1 = br.ReadByte(),
                Mira = br.ReadUInt16(),
                Dp = br.ReadByte(),
                Unk2 = br.ReadByte(),
                Unk3 = br.ReadUInt16(),
                Unk4 = br.ReadUInt16(),
                Unk5 = br.ReadUInt16(),
                pQuestName = br.ReadUInt32(),
                pClient = br.ReadUInt32(),
                pReqBoardContent = br.ReadUInt32(),
                ppProcessContent = br.ReadUInt32(),
            };
            itemList.Add(item);
            item.QuestName = br.ReadClmStringWithOffset(item.pQuestName, encoding);
            item.Client = br.ReadClmStringWithOffset(item.pClient, encoding);
            item.ReqBoardContent = br.ReadClmStringWithOffset(item.pReqBoardContent, encoding);
            if (item.Id == 0xff)
                break;
        } while (br.BaseStream.Position < br.BaseStream.Length);
        for (var i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            var end = i + 1 < itemList.Count ? itemList[i + 1].ppProcessContent : br.BaseStream.Length;
            var addrList = new List<uint>(0x20);
            br.BaseStream.Position = item.ppProcessContent;
            while (br.BaseStream.Position < end)
                addrList.Add(br.ReadUInt32());

            for (var index = 0; index < addrList.Count; index++)
            {
                br.BaseStream.Position = addrList[index];
                var content = br.ReadClmString(encoding);
                item.ProcessContents.Add(new (true, content));
                var nextAddr = index + 1 < addrList.Count ? addrList[index + 1] :
                    i + 1 < itemList.Count ? itemList[i + 1].pQuestName :
                    0;
                while (nextAddr > br.BaseStream.Position)
                {
                    content = br.ReadClmString(encoding);
                    item.ProcessContents.Add(new(false, content));
                }
                
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
        bw.Write(new byte[Data.Count * 0x1C]);
        foreach (var item in Data)
        {
            item.pQuestName = (uint)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.QuestName, encoding));
            bw.Write((byte)0);
            item.pClient = (uint)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.Client, encoding));
            bw.Write((byte)0);
            item.pReqBoardContent = (uint)ms.Position;
            bw.Write(CalmareCodec.ClmStringToBytes(item.ReqBoardContent, encoding));
            bw.Write((byte)0);
            if (string.IsNullOrEmpty(item.Client) && string.IsNullOrEmpty(item.ReqBoardContent))
                bw.Write((byte)0);
            foreach (var content in item.ProcessContents)
            {
                if (content.HasAddress)
                    item.pProcessContents.Add((uint)ms.Position);
                bw.Write(CalmareCodec.ClmStringToBytes(content.Content, encoding));
                bw.Write((byte)0);
            }
        }

        foreach (var item in Data)
        {
            item.ppProcessContent = (uint)ms.Position;
            foreach (var p in item.pProcessContents)
                bw.Write(p);
        }
        
        ms.Position = 0;
        foreach (var item in Data)
        {
            bw.Write(item.Id);
            bw.Write(item.Unk1);
            bw.Write(item.Mira);
            bw.Write(item.Dp);
            bw.Write(item.Unk2);
            bw.Write(item.Unk3);
            bw.Write(item.Unk4);
            bw.Write(item.Unk5);
            bw.Write(item.pQuestName);
            bw.Write(item.pClient);
            bw.Write(item.pReqBoardContent);
            bw.Write(item.ppProcessContent);
        }
        return ms.ToArray();
    }
}