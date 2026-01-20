using System.Text;
using Extensions;
using Newtonsoft.Json;
using zCodec.Calmare;

namespace zCodec.Dats;

public class MsCraft
{
    public string Name { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class MsItem
{
    //ms*
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public List<MsCraft> Crafts { get; set; } = new(10);
    public string Data { get; set; } = string.Empty;
    public string Data2 { get; set; } = string.Empty;
    public string Data3 { get; set; } = string.Empty;
}

public class MsCodec(bool isAo,Encoding encoding)
{
    private List<MsItem>? Data { get; set; }
    private bool IsAo { get; set; } = isAo;
    public MsCodec Load(string file)
    {
        Data = JsonConvert.DeserializeObject<List<MsItem>>(File.ReadAllText(file));
        return this;
    }
    public object Decompile(List<string> files)
    {
        List<MsItem> itemList = new(files.Count);
        foreach (var file in files)
        {
            var item = new MsItem
            {
                Id = Path.GetFileNameWithoutExtension(file)[2..],
            };
            itemList.Add(item);
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);
            fs.Position = 0xA3;
            var count = 0;
            for (int i = 0; i < 4; i++) 
            {
                count = br.ReadByte();
                fs.Position += count * 0x18;
            }
            count = br.ReadByte();
            var len = fs.Position;
            fs.Position = 0;
            item.Data = Convert.ToBase64String(br.ReadBytes((int)len));
            for (var i = 0; i < count; i++)
            {
                item.Crafts.Add(new ()
                {
                    Data = Convert.ToBase64String(br.ReadBytes(IsAo?0x18:0x1c)),
                    Name = br.ReadClmString(encoding),
                    Desc = br.ReadClmString(encoding),
                });
            }
            item.Data2 = Convert.ToBase64String(br.ReadBytes(4));
            item.Name = br.ReadClmString(encoding);
            item.Desc = br.ReadClmString(encoding);
            item.Data3 = Convert.ToBase64String(br.ReadBytes((int)(fs.Length-fs.Position)));
        }
        return itemList;
    }

    public void Compile(string outdir)
    {
        if (Data == null)
            throw new ArgumentNullException(nameof(Data));
        var noteFile = Path.Combine(outdir, "monsnote.dt2");
        using var note = new FileStream(noteFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var noteBw = new BinaryWriter(note);
        foreach (var item in Data)
        {
            var fileName = Path.Combine(outdir, $"ms{item.Id}.dat");
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var bw = new BinaryWriter(fs);
            bw.Write(Convert.FromBase64String(item.Data));
            foreach (var craft in item.Crafts)
            {
                bw.Write(Convert.FromBase64String(craft.Data));
                bw.Write(CalmareCodec.ClmStringToBytes(craft.Name, encoding));
                bw.Write((byte)0);
                bw.Write(CalmareCodec.ClmStringToBytes(craft.Desc, encoding));
                bw.Write((byte)0);
            }
            bw.Write(Convert.FromBase64String(item.Data2));
            bw.Write(CalmareCodec.ClmStringToBytes(item.Name, encoding));
            bw.Write((byte)0);
            bw.Write(CalmareCodec.ClmStringToBytes(item.Desc, encoding));
            bw.Write((byte)0);
            bw.Write(Convert.FromBase64String(item.Data3));
            var buffer = new byte[fs.Length];
            fs.Position = 0;
            fs.ReadExactly(buffer, 0, (int)fs.Length);
            var headName = Convert.ToInt32(item.Id,16) | 0x30000000;
            noteBw.Write(headName);
            noteBw.Write((int)fs.Length);
            noteBw.Write(buffer);

            if (item.Id == "63200" && !IsAo)
            {
                var file_63200 = Path.Combine(outdir, "ms63201.dat");
                headName = 0x63201 | 0x30000000;
                noteBw.Write(headName);
                var d = File.ReadAllBytes(file_63200);
                noteBw.Write(d.Length);
                noteBw.Write(d);
            }
            
        }
        noteBw.Write(-1);
        noteBw.Write(-1);
    }
}