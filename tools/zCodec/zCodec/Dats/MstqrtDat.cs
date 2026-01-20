using System.Text;
using Extensions;
using Newtonsoft.Json;
using zCodec.Calmare;

namespace zCodec.Dats;


public class MstqrtDatData
{
    public List<string> Strs { get; set; } = new(0x5d);
    public string Data { get; set; }=string.Empty;
}


public class MstqrtDat : IDatCodec
{
    private MstqrtDatData? Data { get; set; }
    
    public bool IsAo => true;
    public bool CanDecompile(string file)=> Path.GetFileNameWithoutExtension(file) == "t_mstqrt";

    public DatSaveFormat DatSaveFormat => DatSaveFormat.Json;
    public IDatCodec Load(string file)
    {
        Data = JsonConvert.DeserializeObject<MstqrtDatData>(File.ReadAllText(file));
        return this;
    }

    public object Decompile(string file, Encoding encoding)
    {
        var data = new MstqrtDatData();
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);
        data.Data = Convert.ToBase64String(br.ReadBytes(2200));
        for (var i = 0; i < 0x5d; i++)
        {
            var addr =  br.ReadUInt16();
            data.Strs.Add(br.ReadClmStringWithOffset(addr, encoding));
        }

        return data;
    }

    public byte[] Compile(Encoding encoding)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Convert.FromBase64String(Data.Data));
        var pos = ms.Position;
        bw.Write(new byte[0x5d * 2]);
        var list = new List<ushort>(0x5d);
        foreach (var str in Data.Strs)
        {
            list.Add((ushort)ms.Position);
            bw.Write(CalmareCodec.ClmStringToBytes(str,encoding));
            bw.Write((byte)0);
        }
        ms.Position = pos;
        foreach (var addr in list)
            bw.Write(addr);
        return ms.ToArray();
    }
}