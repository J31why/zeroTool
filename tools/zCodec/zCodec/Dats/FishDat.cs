using System.Collections;
using System.Text;
using Extensions;
using MiniExcelLibs;
using Newtonsoft.Json;
using zCodec.Calmare;

namespace zCodec.Dats;

public class FishDatData
{
    public IList<FishDatItem> Fishes { get; } = [];
    public IList<FishDatUnk> Unk1 { get; } = [];
    public IList<FishDatUnk> Unk2 { get; } = [];
}

public class AoFishDatData
{
    public IList<FishDatItem> Fishes { get; } = [];
    public IList<FishDatUnk> Unk1 { get; } = [];
    public IList<FishDatUnk> Unk2 { get; } = [];
    public IList<string> Titles { get; } = [];
    public IList<string> Unk3 { get; } = [];
    public IList<(bool isRepeat, string text)> Dialogs { get; } = [];
}
public class FishDatItem
{
    public ushort Id{ get; set; }
    [JsonIgnore] public ushort pDesc;
    public string? Desc { get; set; } 
    public string Data { get; set; } =string.Empty;
}

public class FishDatUnk
{
    public ushort Id{ get; set; }
    public string Unk { get; set; } = string.Empty;
}


public class FishDat(bool isAo) : IDatCodec
{
    public object? Data { get; private set; } 
    public bool IsAo { get; } = isAo;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_fish";
    public DatSaveFormat DatSaveFormat=> DatSaveFormat.Json;
    public IDatCodec Load(string file)
    {
        var json = File.ReadAllText(file);
        Data = !IsAo
            ? JsonConvert.DeserializeObject<FishDatData>(json)
            : JsonConvert.DeserializeObject<AoFishDatData>(json);
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        if(IsAo) return DecompileAo(file, encoding);
        var dat = new FishDatData();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var pSeg1 = br.ReadUInt16();
        var pSeg2 = br.ReadUInt16();
        var pSeg3 = br.ReadUInt16();
        fs.Position = pSeg1;
        while (fs.Position < pSeg2)
        {
            var item = new FishDatItem
            {
                Id = br.ReadUInt16(),
                pDesc = br.ReadUInt16(),
            };
            dat.Fishes.Add(item);
            item.Data = Convert.ToBase64String(br.ReadBytes(0x38));
            if (item.pDesc != 0)
                item.Desc = br.ReadClmStringWithOffset(item.pDesc, encoding);
        }
        
        while (fs.Position < pSeg3)
        {
            dat.Unk1.Add(new FishDatUnk
            {
                Id = br.ReadUInt16(),
                Unk = Convert.ToBase64String(br.ReadBytes(0x3E))
            });
        }

        var end = dat.Fishes.First().pDesc;
        while (fs.Position < end)
        {
            dat.Unk2.Add(new FishDatUnk
            {
                Id = br.ReadUInt16(),
                Unk = Convert.ToBase64String(br.ReadBytes(0x12))
            });
        }
        return dat;
    }

    private object DecompileAo(string file, Encoding encoding)
    {
        var dat = new AoFishDatData();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        var pSeg1 = br.ReadUInt16();
        var pSeg2 = br.ReadUInt16();
        var pSeg3 = br.ReadUInt16();
        var pSeg4 = br.ReadUInt16();
        var pSeg5 = br.ReadUInt16();
        var pSeg6 = br.ReadUInt16();
        // 1
        fs.Position = pSeg1;
        while (fs.Position < pSeg2)
        {
            var item = new FishDatItem
            {
                Id = br.ReadUInt16(),
                pDesc = br.ReadUInt16(),
            };
            dat.Fishes.Add(item);
            item.Data = Convert.ToBase64String(br.ReadBytes(0x38));
            if (item.pDesc > 0)
                item.Desc = br.ReadClmStringWithOffset(item.pDesc, encoding);
        }
        // 2
        while (fs.Position < pSeg3)
        {
            dat.Unk1.Add(new FishDatUnk
            {
                Id = br.ReadUInt16(),
                Unk = Convert.ToBase64String(br.ReadBytes(0x16))
            });
        }
        // 3
        var end = dat.Fishes.First().pDesc;
        while (fs.Position < end)
        {
            dat.Unk2.Add(new FishDatUnk
            {
                Id = br.ReadUInt16(),
                Unk = Convert.ToBase64String(br.ReadBytes(0x12))
            });
        }
        // 4
        fs.Position = pSeg4;
        end = 0;
        do
        {
            var addr = br.ReadUInt16();
            dat.Titles.Add(br.ReadClmStringWithOffset(addr, encoding));
            if (end == 0)
                end = addr;
        } while (fs.Position < end);
        // 5
        fs.Position = pSeg5;
        end = br.ReadUInt16();
        var count = (end - fs.Position) / 2 + 1;
        fs.Position += count * 2 - 2;
        for (var i = 0; i < count; i++)
            dat.Unk3.Add(Convert.ToBase64String(br.ReadBytes(0x30)));
        // 6
        fs.Position = pSeg6;
        end = 0;
        var lastAddr = 0;
        do
        {
            var addr = br.ReadUInt16();
            if (lastAddr == addr)
            {
                dat.Dialogs.Add((true, ""));
                continue;
            }
            dat.Dialogs.Add((false,br.ReadClmStringWithOffset(addr, encoding)));
            if (end == 0)
                end = addr;
            lastAddr = addr;
        }while(fs.Position < end);
        return dat;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (IsAo) return CompileAo(encoding);
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        var data = (FishDatData)Data;
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        ushort pSeg1 = 6;
        var pSeg2 = (ushort)(pSeg1 + data.Fishes.Count * 0x3c);
        var pSeg3 = (ushort)(pSeg2 + data.Unk1.Count * 0x40);
        var pStr =  (ushort)(pSeg3 + data.Unk2.Count * 0x14);
        bw.Write(pSeg1);
        bw.Write(pSeg2);
        bw.Write(pSeg3);
        bw.Write(new byte[pStr - pSeg1]);
        
        foreach (var item in data.Fishes.Where(x=>x.Desc is not null))
        {
            item.pDesc = (ushort)ms.Position;
            var nameBytes = CalmareCodec.ClmStringToBytes(item.Desc!, encoding);
            bw.Write(nameBytes);
            bw.Write((byte)0);
        }
        ms.Position = pSeg1;
        foreach (var item in data.Fishes)
        {
            bw.Write(item.Id);
            bw.Write(item.pDesc);
            bw.Write(Convert.FromBase64String(item.Data));
        }

        foreach (var item in data.Unk1)
        {
            bw.Write(item.Id);
            bw.Write(Convert.FromBase64String(item.Unk));
        }
        foreach (var item in data.Unk2)
        {
            bw.Write(item.Id);
            bw.Write(Convert.FromBase64String(item.Unk));
        }
        return ms.ToArray();
    }

    private byte[] CompileAo(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        var data = (AoFishDatData)Data;
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        ushort pSeg1 = 0xC;
        var pSeg2 = (ushort)(pSeg1 + data.Fishes.Count * 0x3c);
        var pSeg3 = (ushort)(pSeg2 + data.Unk1.Count * 0x18);
        var pStr =  (ushort)(pSeg3 + data.Unk2.Count * 0x14);
        // str
        bw.Write(new byte[pStr]);
        foreach (var item in data.Fishes.Where(x=>x.Desc is not null))
        {
            item.pDesc = (ushort)ms.Position;
            var nameBytes = CalmareCodec.ClmStringToBytes(item.Desc!, encoding);
            bw.Write(nameBytes);
            bw.Write((byte)0);
        }
        // 4
        var pSeg4 = (ushort)ms.Position;
        bw.Write(new byte[data.Titles.Count * 2]);
        var addrList = new List<ushort>(0x10);
        foreach (var title in data.Titles)
        {
            addrList.Add((ushort)ms.Position);
            bw.Write(CalmareCodec.ClmStringToBytes(title, encoding));
            bw.Write((byte)0);
        }
        var pos = ms.Position;
        ms.Position = pSeg4;
        foreach (var addr in addrList)
            bw.Write(addr);
        ms.Position = pos;
        addrList.Clear();
        // 5
        var pSeg5 = (ushort)ms.Position;
        for (var i = 0; i < data.Unk3.Count; i++)
            bw.Write((ushort)(pSeg5 + i * 0x30 + data.Unk3.Count * 2));
        foreach (var item in data.Unk3)
            bw.Write(Convert.FromBase64String(item));
        // 6
        var pSeg6 = (ushort)ms.Position;
        bw.Write(new byte[data.Dialogs.Count * 2]);
        ushort lastAddr = 0;
        foreach (var dialog in data.Dialogs)
        {
            if (dialog.isRepeat)
            {
                addrList.Add(lastAddr);
                continue;
            }

            lastAddr = (ushort)ms.Position;
            addrList.Add(lastAddr);
            bw.Write(CalmareCodec.ClmStringToBytes(dialog.text, encoding));
            bw.Write((byte)0);
        }
        
        ms.Position = pSeg6;
        foreach (var addr in addrList)
            bw.Write(addr);
        //header
        ms.Position = 0;
        bw.Write(pSeg1);
        bw.Write(pSeg2);
        bw.Write(pSeg3);
        bw.Write(pSeg4);
        bw.Write(pSeg5);
        bw.Write(pSeg6);
        //fishes
        foreach (var fish in data.Fishes)
        {
            bw.Write(fish.Id);
            bw.Write(fish.pDesc);
            bw.Write(Convert.FromBase64String(fish.Data));
        }
        //2
        foreach (var item in data.Unk1)
        {
            bw.Write(item.Id);
            bw.Write(Convert.FromBase64String(item.Unk));
        }
        //3
        foreach (var item in data.Unk2)
        {
            bw.Write(item.Id);
            bw.Write(Convert.FromBase64String(item.Unk));
        }
        return ms.ToArray();
    }
}