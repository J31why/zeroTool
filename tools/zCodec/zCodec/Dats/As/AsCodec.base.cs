#region

using System.Text;

#endregion

namespace zCodec.Dats.As;

public partial class AsCodec(bool isAo,Encoding encoding)
{
    private string _fileName = "";
    private const string AddrFlag = "loc:";
    private bool _isRead;
    private Encoding Encoding { get; } = encoding;
    private bool IsAo { get; } = isAo;
    private void Reset()
    {
        _addrSet.Clear();
        _fileName = "";
        //writer
        _currentIns = default;
        _bWriter?.Dispose();
        _sReader?.Dispose();
        _addrDic?.Clear();
        _holderAddrDic?.Clear();
        //reader
        _bReader?.Dispose();
        _sBuilder?.Clear();

    }
    private static string ToAddr(ushort s)
    {
        return $"{AddrFlag}{s:X}";
    }

    private Dictionary<AsOpcodes, byte>? _opFuncCode;

    private Dictionary<AsOpcodes, byte> OpFuncCode
    {
        get
        {
            if (_opFuncCode != null) return _opFuncCode;
            _opFuncCode = new Dictionary<AsOpcodes, byte>();
            foreach (var op in _opFuncs)
                _opFuncCode.Add(op.Value.code, op.Key);
            return _opFuncCode;
        }
    }
}