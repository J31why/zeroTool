#region

using System.Text;
using Enums;
using static Enums.ParamType;

#endregion

namespace zCodec.Dats.As;

public partial class AsCoder
{
    private (AsOpcodes code, string[] param) _currentIns;
    private BinaryWriter? _bWriter;
    private StringReader? _sReader;
    private Dictionary<string, ushort>? _addrDic;
    private Dictionary<string, List<ushort>>? _holderAddrDic;

    public byte[] ToDat(string script)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(script))
                return [];
            _isRead = false;
            _addrDic = [];
            _holderAddrDic = [];
            _sReader = new StringReader(script);
            var ms = new MemoryStream();
            _bWriter = new BinaryWriter(ms);
            EncodeHeader();
            EncodeFns();
            foreach (var holder in _holderAddrDic)
            {
                if (!_addrDic.TryGetValue(holder.Key, out var addr))
                    addr = Convert.ToUInt16(holder.Key[AddrFlag.Length..], 16);
                foreach (var holderAddr in holder.Value)
                {
                    _bWriter.BaseStream.Position = holderAddr;
                    _bWriter.Write(addr);
                }
            }

            var result = ms.ToArray();
            return result;
        }
        finally
        {
            Reset();
        }
    }

    private void EncodeHeader()
    {
        string? line;
        while ((line = _sReader!.ReadLine()) != "Header:")
            if (line?.StartsWith("//as") == true)
                _fileName = line[2..];
        if (_fileName != "as90001")
            _bWriter!.Write(1);
       
        while (!string.IsNullOrEmpty(line = _sReader.ReadLine()))
            EncodeHeaderLine(line.Trim());
    }
    private void EncodeFns()
    {
        string? line;
        while (_sReader!.ReadLine() != "Func:")
        {
        }

        while ((line = _sReader.ReadLine()) != null)
            EncodeFnLine(line.Trim());
    }

    private void EncodeFnLine(string line)
    {
        var ms = _bWriter!.BaseStream;
        if (line.StartsWith(AddrFlag) && line.EndsWith(':'))
        {
            var addr = line[..^1];
            _addrDic![addr] = (ushort)ms.Position;
            return;
        }

        if (line.StartsWith("id ="))
        {
            EncodeProperty(line);
            return;
        }
        _currentIns = ParseInsLine(line);
        var code = OpFuncCode[_currentIns.code];
        var types = _opFuncs[code].Params(this);
        _bWriter.Write(OpFuncCode[_currentIns.code]);
        for (var index = 0; index < types.Length; index++)
        {
            var type = types[index];
            var p = _currentIns.param[index];
            Write(type, p);
        }
    }

    private void EncodeHeaderLine(string line)
    {
        var ms = _bWriter!.BaseStream;
        if (line.StartsWith("fn[") || line.StartsWith("table ="))
        {
            var addr = line[line.IndexOf(AddrFlag, StringComparison.Ordinal)..];
            if (addr == "loc:FFFF")
            {
                _bWriter!.Write((short)-1);
            }
            else
            {
                if (!_holderAddrDic!.TryGetValue(addr, out var list))
                {
                    list = new List<ushort>(10);
                    _holderAddrDic[addr] = list;
                }

                list.Add((ushort)ms.Position);
                _bWriter.Write((short)0);
            }
        }
        else if (line.StartsWith("Unk1 ="))
        {
            EncodeProperty(line);
            var pos = (ushort)ms.Position;
            ms.Position = 0;
            _bWriter.Write(pos);
            ms.Position = pos;
        }
        else if (line.StartsWith("Unk2 ="))
        {
            var pos = (int)ms.Position;
            ms.Position = 2;
            _bWriter.Write((ushort)pos);
            ms.Position = pos;
            EncodeProperty(line);
        }
    }

    private void EncodeProperty(string line)
    {
        var pos = line.IndexOf('"') + 1;
        var hex = line[pos..^1];
        var bytes = Convert.FromHexString(hex.Replace("-", ""));
        _bWriter!.Write(bytes);
    }

    private void Write(ParamType type, string p)
    {
        switch (type)
        {
            case b:
                _bWriter!.Write(Convert.ToByte(p, 16));
                break;
            case s:
                _bWriter!.Write(Convert.ToInt16(p, 16));
                break;
            case i:
                _bWriter!.Write(Convert.ToInt32(p, 16));
                break;
            case str:
                _bWriter!.Write([..Encoding.GetBytes(p.Trim('"')), 0]);
                break;
            case sp:
                if (!_holderAddrDic!.TryGetValue(p, out var list))
                {
                    list = new List<ushort>(10);
                    _holderAddrDic[p] = list;
                }

                list.Add((ushort)_bWriter!.BaseStream.Position);
                _bWriter!.Write((ushort)0);
                break;
            default:
                throw new Exception();
        }
    }

    private (AsOpcodes, string[]) ParseInsLine(string line)
    {
        line = line.Trim();
        var pos = line.IndexOf('(');
        if (pos == -1)
            return (Enum.Parse<AsOpcodes>(line), []);
        var op = Enum.Parse<AsOpcodes>(line[..pos++]);
        var paramStr = line[pos..^1].Trim();
        var param = ParseInsParam(paramStr);
        return (op, param);
    }

    private string[] ParseInsParam(string str)
    {
        var parameters = new List<string>(20);
        var inQuote = false;
        var currentParam = new StringBuilder();
        foreach (var c in str)
            switch (c)
            {
                case '\"':
                    inQuote = !inQuote;
                    break;
                case ',' when !inQuote:
                    parameters.Add(currentParam.ToString().Trim());
                    currentParam.Clear();
                    break;
                default:
                    currentParam.Append(c);
                    break;
            }

        parameters.Add(currentParam.ToString().Trim());
        return parameters.ToArray();
    }
}