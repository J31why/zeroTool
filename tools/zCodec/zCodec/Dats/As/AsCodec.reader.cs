#region

using System.Text;
using Enums;
using Extensions;
using static Enums.ParameterType;

#endregion

namespace zCodec.Dats.As;

public partial class AsCodec
{
    private readonly HashSet<ushort> _addrSet = [];
    private StringBuilder? _sBuilder;
    private BinaryReader? _bReader;

    public string Decompile(string file)
    {
        if (!File.Exists(file))
            return string.Empty;
        try
        {
            _fileName = Path.GetFileNameWithoutExtension(file);
            _isRead = true;
            _sBuilder = new StringBuilder();
            _bReader = new BinaryReader(new MemoryStream(File.ReadAllBytes(file)));
            _sBuilder.AppendLine($"//zero\n//{Path.GetFileNameWithoutExtension(file)}\n");
            _sBuilder.AppendLine(DecompileHeader());
            _sBuilder.AppendLine("Func:");
            var set = DecompileFns();
            foreach ((ushort addr, string line) tuple in set)
            {
                if (_addrSet.TryGetValue(tuple.addr, out var addr))
                    _sBuilder.AppendLine($"{ToAddr(addr)}:");
                _sBuilder.AppendLine(tuple.line);
            }

            return _sBuilder.ToString();
        }
        finally
        {
            Reset();
        }
    }

    private string DecompileHeader()
    {
        var sb = new StringBuilder("Header:\n");
        switch (_fileName)
        {
            case "as90000":
                sb.AppendLine($"\tfn[0] = {Read(sp)}");
                break;
            case "as90001":
            {
                sb.AppendLine($"\ttable = {Read(sp)}");
                for (var i = 0; i < 0x8A; i++) //暂定0x8a
                    sb.AppendLine($"\tfn[{i}] = {Read(sp)}");
                break;
            }
            default:
            {
                var funcStart = _bReader!.ReadUInt16();
                var funcEnd = _bReader.ReadUInt16();
                var bytes = _bReader.ReadBytes(funcStart - 4);
                sb.AppendLine($"\tUnk1 = \"{BitConverter.ToString(bytes)}\"");
                for (var i = 0; i < (funcEnd - funcStart) / 2; i++)
                    sb.AppendLine($"\tfn[{i}] = {Read(sp)}");
                bytes = _bReader.ReadBytes(0x10);
                sb.AppendLine($"\tUnk2 = \"{BitConverter.ToString(bytes)}\"");
                break;
            }
        }

        return sb.ToString();
    }

    private List<(ushort, string)> DecompileFns()
    {
        List<(ushort, string)> list = new(2000);

        if (_fileName == "as90001")
            return DecompileAs90001Fns();
        while (_bReader!.BaseStream.Position < _bReader.BaseStream.Length)
        {
            var pos = (ushort)_bReader.BaseStream.Position;
            var line = ReadInstruction();
            list.Add((pos, line));
            //Console.WriteLine($"{AddrFlag}{pos:X}\t{line}");
        }

        return list;
    }

    private List<(ushort, string)> DecompileAs90001Fns()
    {
        List<(ushort, string)> list = new(2000);
        var fnList = _addrSet.Skip(1).ToList();
        fnList.Sort();
        for (var index = 0; index < fnList.Count; index++)
        {
            var fn = fnList[index];
            var next = index + 1 < fnList.Count ? fnList[index + 1] : _bReader!.BaseStream.Length;
            while (_bReader!.BaseStream.Position < next)
            {
                var pos = (ushort)_bReader.BaseStream.Position;
                string line;
                if (pos == fn)
                {
                    line = $"\tid = \"{BitConverter.ToString(_bReader.ReadBytes(3))}\"";
                    list.Add((pos, line));
                    //Console.WriteLine($"{AddrFlag}{pos:X}\t{line}");
                    continue;
                }

                line = ReadInstruction();
                list.Add((pos, line));
                //Console.WriteLine($"{AddrFlag}{pos:X}\t{line}");
            }
        }

        return list;
    }

    private string ReadInstruction()
    {
        var opcode = _bReader!.ReadByte();
        if (!_opFuncs.TryGetValue(opcode, out var tuple)) throw new Exception($"Unknown opcode {opcode:X}");
        var param = Read(tuple.Params(this));
        return string.IsNullOrEmpty(param) ? $"\t{tuple.code}" : $"\t{tuple.code}({param})";
    }

    private string Read(ParameterType[] types)
    {
        var param = new string[types.Length];
        for (var i = 0; i < types.Length; i++) param[i] = Read(types[i]);
        return string.Join(", ", param);
    }

    private string Read(ParameterType type)
    {
        switch (type)
        {
            case b:
                return $"{_bReader!.ReadByte():X}";
            case s:
                return $"{_bReader!.ReadInt16():X}";
            case i:
                return $"{_bReader!.ReadInt32():X}";
            case str:
                return
                    $"\"{_bReader!.ReadCString(Encoding) ?? throw new Exception("read null string")}\"";
            case sp:
                var addr = _bReader!.ReadUInt16();
                _addrSet.Add(addr);
                return ToAddr(addr);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}